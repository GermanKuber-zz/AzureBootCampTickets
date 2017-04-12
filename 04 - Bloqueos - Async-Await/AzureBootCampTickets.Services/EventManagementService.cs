using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureBootCampTickets.Contracts;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Services
{
    public class EventManagementService : IEventManagementService
    {
        private readonly AzureBootCampTicketsContext _ctx;
        private readonly IAzureBootCampTicketsCloudContext _cloudContext;

        public EventManagementService(AzureBootCampTicketsContext dbContext,
            IAzureBootCampTicketsCloudContext cloudContext)
        {
            _ctx = dbContext;
            _cloudContext = cloudContext;
        }

        public bool CreateNewEvent(string name, string description, DateTime eventDate, int totalSeats, double ticketPrice, string userId)
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
                EventDate = eventDate,
                Status = EventStatus.Draft,
                Organizer = userId
            };
            try
            {
                _ctx.Events.Add(newEvent);
                _ctx.SaveChanges();
                result = true;

        
                _cloudContext.AddEvent(newEvent);
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

            _cloudContext.MakeEventLive(ev);
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


     
            _cloudContext.DeleteEvent(ev);

            return result;
        }
     
        //TODO : 05 - Convierto metodo Async
        public async Task<List<Event>> GetMyEventsAsync(string userId)
        {
            return await _cloudContext.GetMyEventsAsync(userId);
        }
        
        public async Task<List<Event>> GetLiveEventsAsync(DateTime currentDate)
        {
            return await _cloudContext.GetLiveEventsAsync(DateTime.Now);
        }
    }
}