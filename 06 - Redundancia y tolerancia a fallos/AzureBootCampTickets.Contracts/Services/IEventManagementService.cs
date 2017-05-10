using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Contracts.Services
{
    public interface IEventManagementService
    {
        bool CreateNewEvent(string name, string description, DateTime eventDate, int totalSeats, double ticketPrice, string userId);
        bool DeleteEvent(Guid eventId);
        bool MakeEventLive(Guid eventId);
        Task<List<Event>> GetMyEventsAsync(string userId);
        Task<List<Event>> GetLiveEventsAsync(DateTime currentDate);
    }
}