namespace Application.Services.contract.LocalizationService
{
    /// <summary>
    /// Resolves a message key to localized text based on the current request's language.
    /// Falls back to returning the input unchanged if the key isn't found — this is what lets
    /// legacy literal-Arabic messages pass straight through untouched.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>"ar" or "en" — resolved from Accept-Language, falling back to the user's stored preference, falling back to "ar".</summary>
        string CurrentLanguage { get; }

        /// <summary>Resolves a key. If not found in the dictionary, returns <paramref name="keyOrLiteral"/> as-is.</summary>
        string Resolve(string keyOrLiteral, params object[] args);
    }
}