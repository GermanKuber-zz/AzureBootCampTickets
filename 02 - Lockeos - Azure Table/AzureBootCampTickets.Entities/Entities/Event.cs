using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureBootCampTickets.Entities.Entities
{
    public enum EventStatus
    {
        Draft,
        Live
    }

    public class Event
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [NotMapped]
        public EventStatus Status 
        {
            get
            {
                return (EventStatus)this.StatusId;
            }
            set
            {
                this.StatusId = (int)value;
            }
        }
        public int StatusId { get; set; }

        public int TotalSeats { get; set; }
        public double TicketPrice { get; set; }
        public int AvailableSeats { get; set; }

        public string Organizer { get; set; }
        public List<Ticket> Tickets { get; set; }
        //TODO : 05 - Agregamos fecha a los eventos
        public DateTime EventDate { get; set; }
    }
}