using System;
using System.Collections.Generic;
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
        List<Event> GetLiveEvents(DateTime currentDate);
        List<Event> GetMyEvents(string userId);
        List<Ticket> GetMyTickets(string userId);
        Ticket GetTicket(string userId, Guid ticketId);
        void MakeEventLive(Event eventObj);
        void UpdateEventSeats(Event eventObj);
    }
}