using System;
using System.Linq;
using AzureBootCampTickets.Contracts.Services;
using StackExchange.Redis;
using System.Configuration;

//TODO : 02 Implemento servicio de cache
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

        public void InvalidateCache(string key)
        {
            IDatabase cache = Connection.GetDatabase();
            cache.KeyDelete(key);
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            //TODO : 08 - Agrego endpoint de Redis Cache
            var multiplex =  ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["CacheConnectionString"].ConnectionString);
            //TODO: Eliminar cache de redis
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
