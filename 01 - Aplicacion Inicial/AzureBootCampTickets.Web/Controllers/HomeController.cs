using System;
using System.Configuration;
using System.Web.Mvc;
using AzureBootCampTickets.Contracts.Repositories;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Entities.Entities;


namespace AzureBootCampTickets.Web.Controllers
{
    public class HomeController : Controller
    {

        private readonly IIdentiService _identiService;

        private readonly IEventsRepository _eventsRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IOrderService _orderService;
        private readonly IEventManagementService _eventManagementService;


        public HomeController(IIdentiService identiService,IEventsRepository eventsRepository,ITicketsRepository ticketsRepository,
            IOrderService orderService,IEventManagementService eventManagementService) : base()
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

            ViewBag.MyEvents = _eventsRepository.MyEvents();
            return View();
        }

        public ActionResult Events()
        {
            ViewBag.Events = _eventsRepository.EventsLive();
            return View();
        }

        public ActionResult CreateEvent()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateEventConfirmed(Event newEvent)
        {
            var isCreated = _eventManagementService.CreateNewEvent(newEvent.Name, newEvent.Description, newEvent.TotalSeats, newEvent.TicketPrice, _identiService.GetUserId());
            if (isCreated)
                return RedirectToAction("MyEvents");
            else
                return View();

        }

        [Authorize]
        public ActionResult MyTickets()
        {

            ViewBag.MyTickets = _ticketsRepository.MyTickets();
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

            var ticket = _ticketsRepository.GetTicket(ticketId);
            if (ticket == null)
            {
                ViewBag.IsValid = false;
            }
            else
            {
                ViewBag.IsValid = true;
                ViewBag.TicketAccessCode = ticket.AccessCode;
                ViewBag.EventName = ticket.ParentEvent.Name;
                ViewBag.UserEmail = _identiService.GetUserEmail();
            }
            return View();
        }

        [Authorize]

        public ActionResult ConfirmTicket(Guid ticketId)
        {

            var ticket = _ticketsRepository.GetTicket(ticketId);

            if (ticket.TotalPrice == 0.0)
            {
                //Sie evento es gratis
                var confirmed = _orderService.ConfirmTicket(ticketId);
                ViewBag.IsConfirmed = confirmed;
                if (confirmed)
                    return RedirectToAction("DisplayTicket", new { ticketId = ticketId });
                else
                    return View();

            }
            else
                return RedirectToAction("DoTicketPayment", new { ticketId = ticketId });

        }

        public ActionResult DoTicketPayment(Guid ticketId)
        {
            //Si el evento no es gratis redirijo a el proovedor de pago
            var ticket = _ticketsRepository.GetTicket(ticketId);

            ViewBag.TicketPrice = ticket.TotalPrice;
            ViewBag.TicketId = ticketId;
            ViewBag.TicketDescription = "Ticket for " + ticket.ParentEvent.Name;
            ViewBag.CallBackUrl = ConfigurationManager.AppSettings["CallBackUri"];
            ViewBag.PaymentUrl = ConfigurationManager.AppSettings["EndPoint"];

            return View();
        }

   
        [HttpGet]
    public ActionResult ConfirmTicketPayment(Guid transactionKey, string result)
        {
            //Luego de pagar en el proovedor, este me envia a este método
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