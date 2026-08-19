using Application.Services.contract.LocalizationService;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Infrastructure.Services.LocalizationServices
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Lazy<string> _resolvedLanguage;

        public LocalizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _resolvedLanguage = new Lazy<string>(ResolveLanguage);
        }

        public string CurrentLanguage => _resolvedLanguage.Value;

        public string Resolve(string keyOrLiteral, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(keyOrLiteral))
                return keyOrLiteral;

            if (!LocalizationResources.Messages.TryGetValue(keyOrLiteral, out var pair))
                return keyOrLiteral; // not a known key -> treat as legacy literal message

            var template = CurrentLanguage == "en" ? pair.En : pair.Ar;
            return args is { Length: > 0 } ? string.Format(template, args) : template;
        }

        private string ResolveLanguage()
        {
            var header = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrWhiteSpace(header) && header.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return "en";

            // Fallback: the user's stored preference (see ApplicationUser.PreferredLanguage below),
            // read from the JWT claim so no DB round-trip is needed on every request.
            var claimLang = _httpContextAccessor.HttpContext?.User?.FindFirst("PreferredLanguage")?.Value;
            if (!string.IsNullOrWhiteSpace(claimLang))
                return claimLang!;

            return "ar"; // app default — matches the current all-Arabic UI
        }
    }
}