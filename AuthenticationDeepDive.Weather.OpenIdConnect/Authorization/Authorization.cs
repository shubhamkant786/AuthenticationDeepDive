namespace AuthenticationDeepDive.Weather.OpenIdConnect.Authorization
{
    public static class AuthPolicies
    {
        public const string ReadAccess = "LocationsReadAccess";
        public const string ReadWriteAccess = "LocationsReadWriteAccess";
    }

    public static class AuthRoles
    {
        public const string LocationsReadOnly = "locations_access_ro";
        public const string LocationsReadWrite = "locations_access_rw";
    }

    public class Authorization
    {
    }
}
