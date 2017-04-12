namespace AzureBootCampTickets.Entities.Entities
{
    //TODO : 02 - Creo una clase Wrapper para la informacion que voy a enviar mediante la cola
    public class OrderDetails
    {
        public string UserId { get; set; }
        public string EventId { get; set; }
        public string TicketId { get; set; }

        // Infrastructure detail
        public string MessageId { get; set; }
        public string PopReceipt { get; set; }
    }
}