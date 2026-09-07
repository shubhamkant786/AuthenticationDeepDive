using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;

namespace AuthenticationDeepDive.Weather.Cookie
{
    public sealed class DistributedCacheTicketStore : ITicketStore
    {
        private const string KeyPrefix = "AuthTicket:";
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(12);

        private readonly IDistributedCache _cache;

        public DistributedCacheTicketStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = $"{KeyPrefix}{Guid.NewGuid():N}";

            await RenewAsync(key, ticket);

            return key;
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new DistributedCacheEntryOptions();

            if (ticket.Properties.ExpiresUtc is { } expires)
            {
                options.AbsoluteExpiration = expires;
            }
            else
            {
                options.AbsoluteExpirationRelativeToNow = DefaultExpiration;
            }

            await _cache.SetAsync(key, TicketSerializer.Default.Serialize(ticket), options);
        }

        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            var bytes = await _cache.GetAsync(key);

            return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
        }

        public Task RemoveAsync(string key)
        {
            return _cache.RemoveAsync(key);
        }
    }
}
