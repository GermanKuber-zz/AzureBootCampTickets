using System;
using System.Threading.Tasks;


namespace AzureBootCampTickets.Contracts.Services
{
    public interface ICacheService
    {
        T GetFromCache<T>(string key, Func<T> missedCacheCall);
        T GetFromCache<T>(string key, Func<T> missedCacheCall, TimeSpan timeToLive);
        void InvalidateCache(string key);
        Task<T> GetFromCacheAsync<T>(string key, Func<Task<T>> missedCacheCall);
        Task<T> GetFromCacheAsync<T>(string key, Func<Task<T>> missedCacheCall, TimeSpan timeToLive);
 
    }
}