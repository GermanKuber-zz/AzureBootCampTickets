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
using AzureBootCampTickets.Data;

namespace AzureBootCampTickets.Services
{
    public class OrderService : IOrderService
    {
        private readonly AzureBootCampTicketsContext _ctx;
        private readonly IAzureBootCampTicketsCloudContext _cloudContext;

        //TODO : 03 - Utilizo mi manejador de Reintentos
        private static CircuitBreaker _circuitBreaker;

        public OrderService(AzureBootCampTicketsContext dbContext,
              IAzureBootCampTicketsCloudContext cloudContext)
        {
            _ctx = dbContext;
            _cloudContext = cloudContext;
            if (_circuitBreaker == null)
            {
                _circuitBreaker = new CircuitBreaker();
            }
        }
       
        public async Task<Guid> PlaceOrder(Guid eventId, string userId)
        {
            var parentEvent = _ctx.Events.Single(e => e.Id == eventId);
            return await _cloudContext.PlaceOrderInQueue(eventId, userId);
        }
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

        //TODO : 04 - Envuelvo la llamada a mi servicio en el Manejador de Reintentos
        public async Task<List<Ticket>> GetMyTicketsAsync(string userId)
        {
            List<Ticket> result = new List<Ticket>();

            try
            {
                await _circuitBreaker.ExecuteAsync(async () =>
                {
                    result = await _cloudContext.GetMyTicketsAsync(userId);
                });
            }
            catch (CircuitBreakerOpenException cboe)
            {
                // Log the method, return empty list
                // or get the list from a local cache
                // or surface the error to the user
                throw new Exception("Couldn't contact the ticket store, please try again.", cboe);
            }

            return result;
        }


        public async Task<Ticket> GetTicketAsync(string userId, Guid ticketId)
        {
            return await _cloudContext.GetTicketAsync(userId, ticketId);
        }
    }
}