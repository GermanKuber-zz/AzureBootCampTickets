using System;

namespace AzureBootCampTickets.Entities.Models
{
    public class TicketSummary
    {
        public Guid TicketId { get; set; }
        public string TicketDescription { get; set; }
        public double TicketPrice { get; set; }
        public bool IsPending { get; set; }
    }
}