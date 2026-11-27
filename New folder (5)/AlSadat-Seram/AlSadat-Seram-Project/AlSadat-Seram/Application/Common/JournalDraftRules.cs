using Application.DTOs.FinanceDtos.Posting;
using Domain.Entities.Finance;
using Domain.Enums;

namespace Application.Common;

/// <summary>
/// The shape rules of a journal entry (F6), pure so they are unit-tested without a database.
/// A broken draft is a bug in the calling service, not something the user typed, so
/// <see cref="EnsureValid"/> throws instead of returning a <c>Result</c>.
/// </summary>
public static class JournalDraftRules
{
    /// <summary>An entry needs a debit side and a credit side.</summary>
    public const int MinLines = 2;

    /// <summary>Amounts are stored as <c>decimal(18,2)</c>: the caller rounds, the engine never does.</summary>
    public const int AmountDecimals = 2;

    /// <summary>Lists what is wrong with <paramref name="draft"/>; empty when it can be posted.</summary>
    /// <param name="draft">The entry to check.</param>
    /// <returns>One English sentence per problem, for the log.</returns>
    public static IReadOnlyList<string> Violations(JournalDraft draft)
    {
        ArgumentNullException.ThrowIfNull(draft);
        var problems = new List<string>();

        if (draft.EntryDate == default)
            problems.Add("EntryDate is not set.");
        if (string.IsNullOrWhiteSpace(draft.Description))
            problems.Add("Description is empty.");
        else if (draft.Description.Length > JournalEntries.DescriptionMaxLength)
            problems.Add($"Description is longer than {JournalEntries.DescriptionMaxLength} characters.");

        if (!Enum.IsDefined(draft.SourceType))
            problems.Add($"SourceType {draft.SourceType} is not defined.");
        else if (draft.SourceType == JournalSourceType.Manual && draft.SourceId is not null)
            problems.Add("A manual entry has no SourceId.");
        else if (draft.SourceType != JournalSourceType.Manual && draft.SourceId is not > 0)
            problems.Add($"A {draft.SourceType} entry needs the document's SourceId.");

        var lines = draft.Lines ?? [];
        if (lines.Count < MinLines)
            problems.Add($"An entry needs at least {MinLines} lines.");

        for (var i = 0; i < lines.Count; i++)
            problems.AddRange(LineViolations(lines[i]).Select(p => $"Line {i + 1}: {p}"));

        var debit = lines.Sum(l => l.Debit);
        var credit = lines.Sum(l => l.Credit);
        if (debit != credit)
            problems.Add($"Not balanced: debit {debit} ≠ credit {credit}.");

        return problems;
    }

    /// <summary>Throws when <paramref name="draft"/> breaks any rule in <see cref="Violations"/>.</summary>
    /// <param name="draft">The entry to check.</param>
    /// <exception cref="ArgumentException">The draft can't be posted; the message lists why.</exception>
    public static void EnsureValid(JournalDraft draft)
    {
        var problems = Violations(draft);
        if (problems.Count > 0)
            throw new ArgumentException("Invalid journal draft: " + string.Join(" ", problems), nameof(draft));
    }

    /// <summary>
    /// The lines of the reversal entry: the same accounts and amounts with debit and credit swapped (D37).
    /// </summary>
    /// <param name="lines">The original entry's lines.</param>
    public static IReadOnlyList<JournalDraftLine> Reversed(IEnumerable<JournalDraftLine> lines)
        => lines.Select(l => l with { Debit = l.Credit, Credit = l.Debit }).ToList();

    private static IEnumerable<string> LineViolations(JournalDraftLine line)
    {
        if (line.SystemAccount is null == line.AccountId is null)
            yield return "set exactly one of SystemAccount or AccountId.";
        else if (line.SystemAccount is { } code && !Enum.IsDefined(code))
            yield return $"SystemAccount {code} is not defined.";
        else if (line.AccountId is <= 0)
            yield return "AccountId must be positive.";

        if (line.Debit < 0 || line.Credit < 0)
            yield return "amounts can't be negative.";
        else if ((line.Debit > 0) == (line.Credit > 0))
            yield return "a line is either a debit or a credit, above zero.";

        if (decimal.Round(line.Debit, AmountDecimals) != line.Debit || decimal.Round(line.Credit, AmountDecimals) != line.Credit)
            yield return $"amounts have at most {AmountDecimals} decimals.";

        if (line.Description is { Length: > JournalEntryDetails.DescriptionMaxLength })
            yield return $"description is longer than {JournalEntryDetails.DescriptionMaxLength} characters.";
    }
}
