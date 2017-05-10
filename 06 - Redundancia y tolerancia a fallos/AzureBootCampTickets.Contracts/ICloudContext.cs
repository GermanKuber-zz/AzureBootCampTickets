using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Contracts
{
    public interface IAzureBootCampTicketsCloudContext
    {
        void AddEvent(Event eventObj);
        void AddTicket(Ticket ticket);
        void ConfirmTicket(Ticket ticket);
        void DeleteEvent(Event eventObj);
        void DeleteTicket(Ticket ticket);
        Task<List<Event>> GetLiveEventsAsync(DateTime currentDate);
        
        Task<List<Ticket>> GetMyTicketsAsync(string userId);
        Task<Ticket> GetTicketAsync(string userId, Guid ticketId);
        void MakeEventLive(Event eventObj);
        void UpdateEventSeats(Event eventObj);
        Task<List<Event>> GetMyEventsAsync(string userId);
        Task<Ticket> GetTicket(string userId, Guid ticketId);
        Task<Guid> PlaceOrderInQueue(Guid eventId, string userId);
    }
}