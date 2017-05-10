namespace AzureBootCampTickets.Entities.Entities
{
    public class OrderDetails
    {
        public string UserId { get; set; }
        public string EventId { get; set; }
        public string TicketId { get; set; }
        public string MessageId { get; set; }
        public string PopReceipt { get; set; }
    }
}