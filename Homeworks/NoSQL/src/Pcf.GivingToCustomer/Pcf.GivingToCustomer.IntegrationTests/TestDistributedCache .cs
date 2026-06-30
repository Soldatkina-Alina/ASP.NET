using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.IntegrationTests
{
    public class TestDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _cache = new();

        public byte[]? Get(string key)
            => _cache.TryGetValue(key, out var value) ? value : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            => Task.FromResult(Get(key));

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            => _cache[key] = value;

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }

        public void Refresh(string key) { }

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public void Remove(string key)
            => _cache.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }
    }
}
