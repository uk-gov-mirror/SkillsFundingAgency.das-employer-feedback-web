using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerClaims = SFA.DAS.EmployerFeedback.Infrastructure.Configuration.EmployerClaims;

namespace SFA.DAS.EmployerFeedback.Web.Models.Home
{
    [ExcludeFromCodeCoverage]
    public class SignedInStubViewModel
    {
        public const string EncodedAccountIdPlaceholder = "{{EncodedAccountId}}";
        private readonly ClaimsPrincipal _claimsPrinciple;

        public SignedInStubViewModel(IHttpContextAccessor httpContextAccessor, string returnUrl)
        {
            _claimsPrinciple = httpContextAccessor.HttpContext.User;
            ReturnUrl = returnUrl;
        }

        public string StubEmail => _claimsPrinciple.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;
        public string StubId => _claimsPrinciple.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value;

        public string ReturnUrl { get; }

        public bool HasEncodedAccountIdPlaceholder()
        {
            return Uri.UnescapeDataString(ReturnUrl).Contains(EncodedAccountIdPlaceholder);
        }

        public string ReplaceEncodedAccountIdPlaceholderUrl(string EncodedAccountId)
        {
            string replacedUrl = Regex.Replace(Uri.UnescapeDataString(ReturnUrl), 
                EncodedAccountIdPlaceholder, 
                EncodedAccountId, 
                RegexOptions.None, 
                TimeSpan.FromMilliseconds(25));

            return replacedUrl;
        }

        public List<EmployerUserAccountItem> GetAccounts()
        {
            var associatedAccountsClaim = _claimsPrinciple.Claims.FirstOrDefault(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier))?.Value;
            if (string.IsNullOrEmpty(associatedAccountsClaim))
                return new List<EmployerUserAccountItem>();

            try
            {
                var accountsDictionary = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim);
                return accountsDictionary?.Values.ToList() ?? new List<EmployerUserAccountItem>();
            }
            catch (JsonException)
            {
                return new List<EmployerUserAccountItem>();
            }
        }
    }
}
