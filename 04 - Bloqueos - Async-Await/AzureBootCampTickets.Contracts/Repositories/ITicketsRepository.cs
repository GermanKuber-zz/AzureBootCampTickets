using System;
using System.Collections.Generic;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Contracts.Repositories
{
    public interface ITicketsRepository
    {
        Ticket GetTicket(Guid ticketId);
        List<Ticket> MyTickets();
    }
}