using Pcf.GivingToCustomer.Core.Abstractions;
using Pcf.GivingToCustomer.Core.Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using System.Text.Json;


namespace Pcf.GivingToCustomer.WebHost.Services
{
    public class PreferenceCacheService : IPreferenceCacheService
    {
        const string keyCache = "preference:all";
        static readonly TimeSpan cacheTimeSpan = TimeSpan.FromMinutes(30);
        IRepository<Preference> _repositoryPreference;
        IDistributedCache _distributedCache;

        public PreferenceCacheService(IRepository<Preference> repository, IDistributedCache distributedCache)
        {
            _repositoryPreference = repository;
            _distributedCache = distributedCache;
        }

        public async Task<IEnumerable<Preference>> GetAllPreference()
        {
            var cach = await _distributedCache.GetStringAsync(keyCache);

            if (cach != null) { 
                return JsonSerializer.Deserialize<List<Preference>>(cach);
            }

            var preference = (await _repositoryPreference.GetAllAsync()).ToList();
            await SaveCache(preference);

            return preference;
        }

        private async Task SaveCache(IEnumerable<Preference> preferences)
        {
            var serializedPreference = JsonSerializer.Serialize(preferences);
            var option = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = cacheTimeSpan
            };

            await _distributedCache.SetStringAsync(keyCache, serializedPreference, option);
        }

        public async Task<Preference> GetPreferenceById(Guid id)
        {
            var prefernce = await GetAllPreference();

            return prefernce.FirstOrDefault(x=> x.Id == id);
        }

        public async Task<IEnumerable<Preference>> GetRangePreference(List<Guid> ids)
        {
            var prefernce = await GetAllPreference();

            return prefernce.Where(x => ids.Contains(x.Id)).ToList();
        }
    }
}
