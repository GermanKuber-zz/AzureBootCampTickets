using System;
using System.Linq;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Services
{
    public class EventManagementService : IEventManagementService
    {
        private readonly AzureBootCampTicketsContext _ctx;

        public EventManagementService(AzureBootCampTicketsContext dbContext)
        {
            _ctx = dbContext;
        }

        public bool CreateNewEvent(string name, string description, int totalSeats, double ticketPrice, string userId)
        {
            var result = false;
            var newEvent = new Event()
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                TotalSeats = totalSeats,
                TicketPrice = ticketPrice,
                AvailableSeats = totalSeats,
                Status = EventStatus.Draft,
                Organizer = userId
            };
            try
            {
                _ctx.Events.Add(newEvent);
                _ctx.SaveChanges();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public bool MakeEventLive(Guid eventId)
        {
            var result = false;
            var ev = _ctx.Events.Single(e => e.Id == eventId);
            if (ev == null || ev.Status != EventStatus.Draft)
            {
                return false;
            }
            ev.Status = EventStatus.Live;
            _ctx.SaveChanges();
            result = true;
            return result;
        }

        public bool DeleteEvent(Guid eventId)
        {
            var result = false;
            var ev = _ctx.Events.Single(e => e.Id == eventId);
            if (ev == null || ev.Status != EventStatus.Draft)
            {
                return false;
            }
            _ctx.Events.Remove(ev);
            _ctx.SaveChanges();
            result = true;
            return result;
        }

    }
}