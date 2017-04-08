using System;
using System.Collections.Generic;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Contracts.Services
{
    public interface IEventManagementService
    {
        bool CreateNewEvent(string name, string description, DateTime eventDate, int totalSeats, double ticketPrice, string userId);
        bool DeleteEvent(Guid eventId);
        bool MakeEventLive(Guid eventId);
        List<Event> GetMyEvents(string userId);
        List<Event> GetLiveEvents(DateTime currentDate);
    }
}