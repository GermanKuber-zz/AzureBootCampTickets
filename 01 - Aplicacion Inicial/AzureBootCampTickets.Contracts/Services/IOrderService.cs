using System;
using AzureBootCampTickets.Entities.Models;

namespace AzureBootCampTickets.Contracts.Services
{
    public interface IOrderService
    {
        bool ConfirmTicket(Guid ticketId);
        bool DeleteTicket(Guid ticketId);
        TicketSummary PlaceOrder(Guid eventId, string userId);
    }
}