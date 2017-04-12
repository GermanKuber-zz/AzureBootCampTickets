using System;
using System.Threading.Tasks;
using AzureBootCampTickets.Contracts.Services;
using StackExchange.Redis;

namespace AzureBootCampTickets.Cache
{
    public class CacheService : ICacheService
    {
        public T GetFromCache<T>(string key, Func<T> missedCacheCall, TimeSpan timeToLive)
        {
            IDatabase cache = Connection.GetDatabase();
            
            var obj = cache.Get<T>(key);
            if (obj == null)
            {
                obj = missedCacheCall();
                if (obj != null)
                {
                    cache.Set(key, obj);
                }
            }
            return obj;
        }
        public T GetFromCache<T>(string key, Func<T> missedCacheCall)
        {
            return GetFromCache<T>(key, missedCacheCall, TimeSpan.FromMinutes(5));
        }

        public async Task<T> GetFromCacheAsync<T>(string key, Func<Task<T>> missedCacheCall, TimeSpan timeToLive)
        {
            IDatabase cache = Connection.GetDatabase();
            var obj = await cache.GetAsync<T>(key);
            if (obj == null)
            {
                obj = await missedCacheCall();
                if (obj != null)
                {
                    cache.Set(key, obj);
                }
            }
            return obj;
        }
        //TODO : 03 - Agrego métodos Async
        public async Task<T> GetFromCacheAsync<T>(string key, Func<Task<T>> missedCacheCall)
        {
            return await GetFromCacheAsync<T>(key, missedCacheCall, TimeSpan.FromMinutes(5));
        }
        public void InvalidateCache(string key)
        {
            IDatabase cache = Connection.GetDatabase();
            cache.KeyDelete(key);
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            var multiplex =  ConnectionMultiplexer.Connect("azurebootcampstorageapi.redis.cache.windows.net:6380,password=ZBCYrzO8BlGp3doiB6PS5nHhG4ozGi/gFC5GWGJ56z4=,ssl=True,abortConnect=False,allowAdmin=true");
       
            //var endpoints = multiplex.GetEndPoints();
            //var server = multiplex.GetServer(endpoints.First());
            //server.FlushDatabase();
            return multiplex;
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}
