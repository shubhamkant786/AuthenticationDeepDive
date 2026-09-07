namespace AuthenticationDeepDive.Weather.OpenIdConnect.Authentication
{
    public class AzureAdOptions
    {
        public const string Name = "AzureAd";

        public string Instance { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = string.Empty;
        public string SignedOutCallbackPath { get; set; } = string.Empty;
        public string[] Scopes { get; set; } = [];
        public string DownstreamApiScope { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string CookieDomain { get; set; } = string.Empty;

        public string Authority => $"{Instance.TrimEnd('/')}/{TenantId}/v2.0";
        public string MetadataAddress => $"{Authority}/.well-known/openid-configuration";

    }
}
