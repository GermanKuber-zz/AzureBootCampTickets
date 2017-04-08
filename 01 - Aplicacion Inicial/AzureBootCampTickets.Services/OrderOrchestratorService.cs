using System;
using System.Linq;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.Models;

namespace AzureBootCampTickets.Services
{
    public class OrderService : IOrderService
    {
        private readonly AzureBootCampTicketsContext _ctx;

        public OrderService(AzureBootCampTicketsContext dbContext)
        {
            _ctx = dbContext;
        }
        public TicketSummary PlaceOrder(Guid eventId, string userId)
        {
            var parentEvent = _ctx.Events.Single(e => e.Id == eventId);

            var ticket = new Ticket()
            {
                AccessCode = Ticket.GenerateRandomAccessCode(),
                Attendee = userId,
                TotalPrice = parentEvent.TicketPrice,
                Status = TicketStatus.Pending,
                Id = Guid.NewGuid(),
                ParentEvent = parentEvent
            };

            _ctx.Tickets.Add(ticket);
            _ctx.SaveChanges();

            var ticketSummary = new TicketSummary()
            {
                TicketId = ticket.Id,
                TicketDescription = "Ticket for " + parentEvent.Name,
                TicketPrice = ticket.TotalPrice
            };
            return ticketSummary;
        }

        public bool ConfirmTicket(Guid ticketId)
        {
            var result = false;
  
            var ticket = _ctx.Tickets.Include("ParentEvent").Single(t => t.Id == ticketId);

            if (ticket.ParentEvent.AvailableSeats > 0)
            {
                result = true;
                ticket.Status = TicketStatus.Paid;
                ticket.ParentEvent.AvailableSeats--;
            }
            else
            {
                _ctx.Tickets.Remove(ticket);
            }
            _ctx.SaveChanges();
            return result;
        }

        public bool DeleteTicket(Guid ticketId)
        {
            var result = false;

            var ticket = _ctx.Tickets.Include("ParentEvent").Single(t => t.Id == ticketId);

            if (ticket != null)
            {
                if (ticket.Status == TicketStatus.Paid)
                {
                    // Increase available tickets
                    ticket.ParentEvent.AvailableSeats++;
                }
                _ctx.Tickets.Remove(ticket);
                _ctx.SaveChanges();
                result = true;
            }
            return result;
        }

    }
}