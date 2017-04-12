using System.Collections.Generic;
using System.Linq;
using AzureBootCampTickets.Contracts.Repositories;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Entities.Entities;


namespace AzureBootCampTickets.Data.Repositories
{
    public class EventsRepository : IEventsRepository
    {
        private readonly AzureBootCampTicketsContext _dbContext;
        private readonly IIdentiService _identiService;

        public EventsRepository(AzureBootCampTicketsContext dbContext,
            IIdentiService identiService)
        {
            _dbContext = dbContext;
            _identiService = identiService;
        }
        public List<Event> MyEvents()
        {
            var id = _identiService.GetUserId();
            return _dbContext.Events.Where(e => e.Organizer == id).ToList();
        }
        public List<Event> EventsLive()
        {
            return _dbContext.Events.Where(e => e.StatusId == (int)EventStatus.Live).ToList();
        }
    }
}