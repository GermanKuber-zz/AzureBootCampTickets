using System;

namespace AzureBootCampTickets.Contracts.Services
{
    public interface IEventManagementService
    {
        bool CreateNewEvent(string name, string description, int totalSeats, double ticketPrice, string userId);
        bool DeleteEvent(Guid eventId);
        bool MakeEventLive(Guid eventId);
    }
}