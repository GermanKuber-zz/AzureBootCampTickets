using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using AzureBootCampTickets.Contracts;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.Models;

namespace AzureBootCampTickets.Services
{
    public class OrderService : IOrderService
    {
        private readonly AzureBootCampTicketsContext _ctx;
        private readonly IAzureBootCampTicketsCloudContext _cloudContext;

        public OrderService(AzureBootCampTicketsContext dbContext,
              IAzureBootCampTicketsCloudContext cloudContext)
        {
            _ctx = dbContext;
            _cloudContext = cloudContext;
        }
        //public TicketSummary PlaceOrder(Guid eventId, string userId)
        //{
        //    var parentEvent = _ctx.Events.Single(e => e.Id == eventId);

        //    var ticket = new Ticket()
        //    {
        //        AccessCode = Ticket.GenerateRandomAccessCode(),
        //        Attendee = userId,
        //        TotalPrice = parentEvent.TicketPrice,
        //        Status = TicketStatus.Pending,
        //        Id = Guid.NewGuid(),
        //        ParentEvent = parentEvent
        //    };

        //    _ctx.Tickets.Add(ticket);
        //    _ctx.SaveChanges();

        //    _cloudContext.AddTicket(ticket);

        //    var ticketSummary = new TicketSummary()
        //    {
        //        TicketId = ticket.Id,
        //        TicketDescription = "Ticket for " + parentEvent.Name,
        //        TicketPrice = ticket.TotalPrice
        //    };
        //    return ticketSummary;
        //}
        //TODO : 10 - creo un mensaje en la cola - Retorno solo el GUID 
        public async Task<Guid> PlaceOrder(Guid eventId, string userId)
        {
            var parentEvent = _ctx.Events.Single(e => e.Id == eventId);
            return await _cloudContext.PlaceOrderInQueue(eventId, userId);
        }
        //TODO : 04 - Agrego metodo que consulta el ticketsummary
        public async Task<TicketSummary> GetTicketSummary(Guid ticketId, string userId)
        {
            var ticket = await _cloudContext.GetTicket(userId, ticketId);
            if (ticket != null)
            {
                var ticketSummary = new TicketSummary()
                {
                    TicketId = ticket.Id,
                    TicketDescription = "Ticket for " + ticket.ParentEvent.Name,
                    TicketPrice = ticket.TotalPrice,
                    IsPending = ticket.Status == TicketStatus.Pending
                };
                return ticketSummary;
            }
            return new TicketSummary();
        }
        public bool ConfirmTicket(Guid ticketId)
        {
            var result = false;
            bool hasBeenConfirmed = false;

            var ticket = _ctx.Tickets.Include("ParentEvent").Single(t => t.Id == ticketId);

            if (ticket.ParentEvent.AvailableSeats > 0)
            {
                result = true;
                ticket.Status = TicketStatus.Paid;
                ticket.ParentEvent.AvailableSeats--;
                hasBeenConfirmed = true;
            }
            else
            {
                _ctx.Tickets.Remove(ticket);
            }
            _ctx.SaveChanges();
            
            if (hasBeenConfirmed)
            {
                _cloudContext.ConfirmTicket(ticket);
                _cloudContext.UpdateEventSeats(ticket.ParentEvent);
            }
            else
            {
                _cloudContext.DeleteTicket(ticket);
            }

            return result;
        }

        public bool DeleteTicket(Guid ticketId)
        {
            var result = false;

            var ticket = _ctx.Tickets.Include("ParentEvent").Single(t => t.Id == ticketId);
            var parentEvent = ticket.ParentEvent;

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
    
            if (result)
            {
                _cloudContext.DeleteTicket(ticket);
                _cloudContext.UpdateEventSeats(parentEvent);
            }
            return result;
        }

        
        public async Task<List<Ticket>> GetMyTicketsAsync(string userId)
        {
            return await _cloudContext.GetMyTicketsAsync(userId);
        }


        public async Task<Ticket> GetTicketAsync(string userId, Guid ticketId)
        {
            return await _cloudContext.GetTicketAsync(userId, ticketId);
        }
    }
}