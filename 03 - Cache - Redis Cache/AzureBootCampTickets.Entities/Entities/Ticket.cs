using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AzureBootCampTickets.Entities.Entities
{
    public enum TicketStatus
    {
        Pending,
        Paid
    }
    [Serializable]
    public class Ticket
    {
        public Guid Id { get; set; }

        public virtual Event ParentEvent { get; set; }

        public string Attendee { get; set; }
        public double TotalPrice { get; set; }
        [NotMapped]
        public TicketStatus Status
        {
            get
            {
                return (TicketStatus)this.TicketStatusId;
            }
            set
            {
                this.TicketStatusId = (int)value;
            }
        }
        public int TicketStatusId { get; set; }
        public string AccessCode { get; set; }

        public static string GenerateRandomAccessCode() => Common.CodeGenerator.Generate(6);
    }
}