using Application.Common;
using Application.DTOs.FinanceDtos.Posting;
using Domain.Enums;
using Xunit;

namespace Application.UnitTests.Common;

/// <summary>Unit tests for <see cref="JournalDraftRules"/> — pure, no DB.</summary>
public class JournalDraftRulesTests
{
    private static readonly DateTime Today = new(2026, 11, 27);

    private static JournalDraft Draft(params JournalDraftLine[] lines)
        => new(Today, "تأكيد فاتورة INV-0040", JournalSourceType.SalesInvoice, 40, lines);

    private static JournalDraft SalesInvoice() => Draft(
        JournalDraftLine.DebitTo(101, 28_750m, "الحاج سيد"),
        JournalDraftLine.CreditTo(SystemAccountCode.SalesRevenue, 28_750m),
        JournalDraftLine.DebitTo(SystemAccountCode.CostOfGoodsSold, 19_400.55m),
        JournalDraftLine.CreditTo(SystemAccountCode.Inventory, 19_400.55m));

    [Fact]
    public void A_balanced_sales_entry_is_valid()
    {
        Assert.Empty(JournalDraftRules.Violations(SalesInvoice()));
        JournalDraftRules.EnsureValid(SalesInvoice());
    }

    [Fact]
    public void An_unbalanced_entry_is_rejected()
    {
        var draft = Draft(
            JournalDraftLine.DebitTo(SystemAccountCode.Cash, 5_040m),
            JournalDraftLine.CreditTo(SystemAccountCode.Capital, 4_920m));

        var problem = Assert.Single(JournalDraftRules.Violations(draft));
        Assert.Contains("Not balanced", problem);
        Assert.Throws<ArgumentException>(() => JournalDraftRules.EnsureValid(draft));
    }

    [Fact]
    public void One_line_is_not_an_entry()
    {
        var draft = Draft(JournalDraftLine.DebitTo(SystemAccountCode.Cash, 0m));
        Assert.Contains(JournalDraftRules.Violations(draft), p => p.Contains("at least 2 lines"));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 100)]
    [InlineData(-100, 0)]
    public void A_line_is_one_side_above_zero(decimal debit, decimal credit)
    {
        var bad = new JournalDraftLine { SystemAccount = SystemAccountCode.Cash, Debit = debit, Credit = credit };
        var draft = Draft(bad, bad with { Debit = credit, Credit = debit });
        Assert.Contains(JournalDraftRules.Violations(draft), p => p.StartsWith("Line 1:"));
    }

    [Fact]
    public void Amounts_with_more_than_two_decimals_are_rejected()
    {
        var draft = Draft(
            JournalDraftLine.DebitTo(SystemAccountCode.Cash, 10.005m),
            JournalDraftLine.CreditTo(SystemAccountCode.Capital, 10.005m));
        Assert.Contains(JournalDraftRules.Violations(draft), p => p.Contains("at most 2 decimals"));
    }

    [Fact]
    public void A_line_names_exactly_one_account()
    {
        var both = new JournalDraftLine { SystemAccount = SystemAccountCode.Cash, AccountId = 5, Debit = 10 };
        var none = new JournalDraftLine { Credit = 10 };
        var problems = JournalDraftRules.Violations(Draft(both, none));
        Assert.Contains(problems, p => p.StartsWith("Line 1:") && p.Contains("exactly one"));
        Assert.Contains(problems, p => p.StartsWith("Line 2:") && p.Contains("exactly one"));
    }

    [Fact]
    public void A_system_entry_needs_its_document_and_a_manual_one_has_none()
    {
        var lines = SalesInvoice().Lines;
        Assert.NotEmpty(JournalDraftRules.Violations(new JournalDraft(Today, "x", JournalSourceType.Receipt, null, lines)));
        Assert.NotEmpty(JournalDraftRules.Violations(new JournalDraft(Today, "x", JournalSourceType.Manual, 7, lines)));
        Assert.Empty(JournalDraftRules.Violations(new JournalDraft(Today, "x", JournalSourceType.Manual, null, lines)));
    }

    [Fact]
    public void Date_and_description_are_required()
    {
        var problems = JournalDraftRules.Violations(new JournalDraft(default, " ", JournalSourceType.SalesInvoice, 40, SalesInvoice().Lines));
        Assert.Contains(problems, p => p.Contains("EntryDate"));
        Assert.Contains(problems, p => p.Contains("Description"));
    }

    [Fact]
    public void Reversal_swaps_every_line_and_stays_balanced()
    {
        var original = SalesInvoice().Lines;
        var reversed = JournalDraftRules.Reversed(original);

        Assert.Equal(original.Count, reversed.Count);
        for (var i = 0; i < original.Count; i++)
        {
            Assert.Equal(original[i].Debit, reversed[i].Credit);
            Assert.Equal(original[i].Credit, reversed[i].Debit);
            Assert.Equal(original[i].SystemAccount, reversed[i].SystemAccount);
            Assert.Equal(original[i].AccountId, reversed[i].AccountId);
        }
        Assert.Empty(JournalDraftRules.Violations(Draft([.. reversed])));
    }
}
