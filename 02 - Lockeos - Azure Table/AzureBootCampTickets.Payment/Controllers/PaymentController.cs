using System.Web.Mvc;
using AzureBootCampTickets.Payment.Models;

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
}
