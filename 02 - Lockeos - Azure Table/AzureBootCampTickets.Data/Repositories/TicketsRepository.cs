using System;
using System.Collections.Generic;
using System.Linq;
using AzureBootCampTickets.Contracts.Repositories;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Entities.Entities;


namespace AzureBootCampTickets.Data.Repositories
{
    public class TicketsRepository : ITicketsRepository
    {
        private readonly AzureBootCampTicketsContext _dbContext;
        private readonly IIdentiService _identiService;

        public TicketsRepository(AzureBootCampTicketsContext dbContext,
            IIdentiService identiService)
        {
            _dbContext = dbContext;
            _identiService = identiService;
        }
        public List<Ticket> MyTickets()
        {
            var id = _identiService.GetUserId();
            return _dbContext.Tickets.Include("ParentEvent").Where(t => t.Attendee == id).ToList();
        }
        public Ticket GetTicket(Guid ticketId)
        {
            return _dbContext.Tickets.Include("ParentEvent").Single(t => t.Id == ticketId);
        }
    }
}