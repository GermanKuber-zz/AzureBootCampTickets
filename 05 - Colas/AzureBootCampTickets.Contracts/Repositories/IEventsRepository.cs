using System.Collections.Generic;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Contracts.Repositories
{
    public interface IEventsRepository
    {
        List<Event> EventsLive();
        List<Event> MyEvents();
    }
}