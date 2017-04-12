using System;

namespace AzureBootCampTickets.Entities.Models
{
    public class TicketSummary
    {
        public Guid TicketId { get; set; }
        public string TicketDescription { get; set; }
        public double TicketPrice { get; set; }

        //TODO : 07 - Agrego propiedad que me indica el estado
        public bool IsPending { get; set; }
    }
}