using System;
using System.Collections.Generic;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.Models;

namespace AzureBootCampTickets.Contracts.Services
{
    public interface IOrderService
    {
        bool ConfirmTicket(Guid ticketId);
        bool DeleteTicket(Guid ticketId);
        TicketSummary PlaceOrder(Guid eventId, string userId);
        Ticket GetTicket(string userId, Guid ticketId);
        List<Ticket> GetMyTickets(string userId);
    }
}