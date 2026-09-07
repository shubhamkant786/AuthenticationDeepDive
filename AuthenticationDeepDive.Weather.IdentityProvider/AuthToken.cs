namespace AuthenticationDeepDive.Weather.IdentityProvider
{
    public class AuthToken
    {       
        public string Token { get; set; }

        public string? RefreshToken { get; set; }

        public int ExpiresIn { get; set; } = 1;
    }
}
