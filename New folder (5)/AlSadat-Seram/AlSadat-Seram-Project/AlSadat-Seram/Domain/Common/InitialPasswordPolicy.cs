namespace Domain.Common;

/// <summary>
/// First password of a customer account created by staff (decision 2026-11-17):
/// the phone number followed by the first letter of the full name, exactly as
/// written — e.g. phone <c>01012345678</c> and name «محمود علي» give <c>01012345678م</c>.
/// The account is flagged so the customer must change it at the first login.
/// </summary>
public static class InitialPasswordPolicy
{
    /// <summary>Builds the initial password from the phone number and the full name.</summary>
    /// <exception cref="ArgumentException">When the phone number or the name is empty.</exception>
    public static string Build(string phoneNumber, string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        return string.Concat(phoneNumber.Trim(), fullName.Trim()[0].ToString());
    }
}
