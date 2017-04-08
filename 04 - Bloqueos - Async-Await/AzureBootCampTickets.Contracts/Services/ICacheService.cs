using System;

//TODO : 01 creo un nuevo servicio de cache
namespace AzureBootCampTickets.Contracts.Services
{
    public interface ICacheService
    {
        T GetFromCache<T>(string key, Func<T> missedCacheCall);
        T GetFromCache<T>(string key, Func<T> missedCacheCall, TimeSpan timeToLive);
        void InvalidateCache(string key);
    }
}