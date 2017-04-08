using System;
using System.ComponentModel.DataAnnotations;

namespace AzureBootCampTickets.Payment.Models
{
    public class PaymentViewModel
    {
        public string CallbackUrl { get; set; }
        public string Concept { get; set; }
        public string Amount { get; set; }
        public string TransactionKey { get; set; }
        [MinLength(9)]
        [Required]
        public string CardKey { get; set; }
        [Required]
        public DateTime CardExpiration { get; set; }
    }
}