namespace Domain.Common;

/// <summary>
/// Message keys (and error <c>code</c>s, D22) returned by <c>IJournalPostingService</c> (F6).
/// Each one has its ar/en text in <c>LocalizationResources</c>.
/// </summary>
public static class JournalErrorCodes
{
    /// <summary>A line hits an inactive or parent account.</summary>
    public const string AccountNotPostable = "Journal.AccountNotPostable";

    /// <summary>The entry to reverse doesn't exist.</summary>
    public const string EntryNotFound = "Journal.EntryNotFound";

    /// <summary>Only a posted entry can be reversed; a draft is edited or deleted instead.</summary>
    public const string NotPosted = "Journal.NotPosted";

    /// <summary>The entry already has its reversal (at most one, D37).</summary>
    public const string AlreadyReversed = "Journal.AlreadyReversed";

    /// <summary>A reversal entry is never reversed again; the document is posted anew instead.</summary>
    public const string CannotReverseReversal = "Journal.CannotReverseReversal";
}
