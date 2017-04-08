using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace AzureBootCampTickets.Payment.Controllers
{
    public class PaymentController : Controller
    {
        public ActionResult Index()
        {
            var model = new PaymentViewModel
            {

                CallbackUrl = HttpContext.Request.Form["callbackUrl"],
                Concept = HttpContext.Request.Form["concept"],
                Amount = HttpContext.Request.Form["amount"],
                TransactionKey = HttpContext.Request.Form["transactionKey"]
            };


            return View(model);
        }
        [HttpPost]
        public ActionResult ProcessPayment(PaymentViewModel model)
        {
            return Redirect($"{model.CallbackUrl}?transactionKey={model.TransactionKey}&result=success");
        }

    }

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
