using System;

namespace AzureBootCampTickets.Entities.Models
{
    public class PaymentCallbackInfo
    {
        public Guid TransactionKey { get; set; }
        public string Result { get; set; }
    }
}