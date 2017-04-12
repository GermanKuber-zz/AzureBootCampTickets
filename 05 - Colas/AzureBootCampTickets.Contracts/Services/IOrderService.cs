using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.Models;

namespace AzureBootCampTickets.Contracts.Services
{
    public interface IOrderService
    {
        bool ConfirmTicket(Guid ticketId);
        bool DeleteTicket(Guid ticketId);
        Task<Guid> PlaceOrder(Guid eventId, string userId);
        Task<Ticket> GetTicketAsync(string userId, Guid ticketId);
        Task<List<Ticket>> GetMyTicketsAsync(string userId);
        Task<TicketSummary> GetTicketSummary(Guid ticketId, string userId);
    }
}