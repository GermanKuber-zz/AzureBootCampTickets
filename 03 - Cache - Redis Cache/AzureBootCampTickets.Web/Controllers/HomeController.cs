using System;
using System.Configuration;
using System.Web.Mvc;
using AzureBootCampTickets.Contracts.Repositories;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.Models;


namespace AzureBootCampTickets.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly IIdentiService _identiService;

        private readonly IEventsRepository _eventsRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IOrderService _orderService;
        private readonly IEventManagementService _eventManagementService;


        public HomeController(IIdentiService identiService, IEventsRepository eventsRepository, ITicketsRepository ticketsRepository,
            IOrderService orderService, IEventManagementService eventManagementService) : base()
        {
            _identiService = identiService;
            _eventsRepository = eventsRepository;
            _ticketsRepository = ticketsRepository;
            _orderService = orderService;
            _eventManagementService = eventManagementService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult MyEvents()
        {

            ViewBag.MyEvents = _eventManagementService.GetMyEvents(this._identiService.GetUserId());
            return View();
        }

        public ActionResult Events()
        {
            ViewBag.Events = _eventManagementService.GetLiveEvents(DateTime.Now);
            return View();
        }
        [Authorize]
        public ActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateEventConfirmed(Event newEvent)
        {
            var isCreated = _eventManagementService.CreateNewEvent(newEvent.Name, newEvent.Description, newEvent.EventDate, newEvent.TotalSeats, newEvent.TicketPrice, _identiService.GetUserId());
            if (isCreated)
                return RedirectToAction("MyEvents");
            else
                return View();

        }

        [Authorize]
        public ActionResult MyTickets()
        {

            ViewBag.MyTickets = _orderService.GetMyTickets(_identiService.GetUserId());
            return View();
        }

        [Authorize]
        public ActionResult OrderTicket(Guid eventId)
        {

            var temporaryTicketSummary = _orderService.PlaceOrder(eventId, _identiService.GetUserId());
            ViewBag.TicketSummary = temporaryTicketSummary;
            return View();
        }

        [Authorize]
        public ActionResult DisplayTicket(Guid ticketId)
        {
            var user = User.ApplicationUser();
            var ticket = _orderService.GetTicket(user.Id, ticketId);
            if (ticket == null)
            {
                ViewBag.IsValid = false;
            }
            else
            {
                ViewBag.IsValid = true;
                ViewBag.TicketAccessCode = ticket.AccessCode;
                ViewBag.EventName = ticket.ParentEvent.Name;
                ViewBag.UserEmail = user.Email;
            }
            return View();
        }

        [Authorize]
        public ActionResult ConfirmTicket(Guid ticketId)
        {
            var user = User.ApplicationUser();
            var ticket = _orderService.GetTicket(user.Id, ticketId);

            // TODO: Add the possibility of redirecting to payment if the ticket is not free
            if (ticket.TotalPrice == 0.0)
            {

                var confirmed = _orderService.ConfirmTicket(ticketId);
                ViewBag.IsConfirmed = confirmed;
                if (confirmed)
                {
                    return RedirectToAction("DisplayTicket", new { ticketId = ticketId });
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return RedirectToAction("DoTicketPayment", new { ticketId = ticketId });
            }
        }

        public ActionResult DoTicketPayment(Guid ticketId)
        {
           
            var ticket = _orderService.GetTicket(_identiService.GetUserId(), ticketId);

            ViewBag.TicketPrice = ticket.TotalPrice;
            ViewBag.TicketId = ticketId;
            ViewBag.TicketDescription = "Ticket for " + ticket.ParentEvent.Name;
            ViewBag.CallBackUrl = ConfigurationManager.AppSettings["CallBackUri"];
            ViewBag.PaymentUrl = ConfigurationManager.AppSettings["EndPoint"];
            ViewBag.Merchant = "Ticketer";

            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ConfirmTicketPayment([Bind(Include = "result,transactionKey")] PaymentCallbackInfo paymentInfo)
        {
            var ticketId = paymentInfo.TransactionKey;
       

            var ticket = _orderService.GetTicket(_identiService.GetUserId(), ticketId);

            if (paymentInfo.Result == "success")
            {
                var confirmed = _orderService.ConfirmTicket(ticketId);
                ViewBag.IsConfirmed = confirmed;
                return RedirectToAction("DisplayTicket", new { ticketId = ticketId });
            }

            return View();
        }
    

        [HttpGet]
        public ActionResult ConfirmTicketPayment(Guid transactionKey, string result)
        {
            var ticket = _ticketsRepository.GetTicket(transactionKey);

            if (result == "success")
            {
                var confirmed = _orderService.ConfirmTicket(transactionKey);
                ViewBag.IsConfirmed = confirmed;
                return RedirectToAction("DisplayTicket", new { ticketId = transactionKey });
            }

            return View();
        }

        [Authorize]
        public ActionResult DeleteEvent(Guid eventId)
        {
            var result = _eventManagementService.DeleteEvent(eventId);
            return RedirectToAction("MyEvents");
        }

        [Authorize]
        public ActionResult DeleteTicket(Guid ticketId)
        {
            var result = _orderService.DeleteTicket(ticketId);
            return RedirectToAction("MyTickets");
        }

        [Authorize]
        public ActionResult MakeEventLive(Guid eventId)
        {
            var result = _eventManagementService.MakeEventLive(eventId);
            return RedirectToAction("MyEvents");
        }
    }
}