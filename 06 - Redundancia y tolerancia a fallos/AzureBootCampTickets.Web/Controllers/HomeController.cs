using System;
using System.Configuration;
using System.Threading.Tasks;
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
        public async Task<ActionResult> MyEvents()
        {
            ViewBag.MyEvents = await _eventManagementService.GetMyEventsAsync(this._identiService.GetUserId());
            return View();
        }

        public async Task<ActionResult> Events()
        {
            ViewBag.Events = await _eventManagementService.GetLiveEventsAsync(DateTime.Now);
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
        public async Task<ActionResult> MyTickets()
        {

            ViewBag.MyTickets = await _orderService.GetMyTicketsAsync(_identiService.GetUserId());
            return View();
        }

        [Authorize]
        public async Task<ActionResult> OrderTicket(Guid eventId)
        {
            var user = User.ApplicationUser();
            var ticketId =await _orderService.PlaceOrder(eventId, _identiService.GetUserId());

            ViewBag.TicketId = ticketId;

            return View();
        }
        [HttpGet]
        public async Task<bool> DoesTicketExist(Guid ticketId)
        {
            var result = false;
            var user = User.ApplicationUser();

            var ticketSummary = await _orderService.GetTicketSummary(ticketId, user.Id);
            if (ticketSummary != null && ticketSummary.IsPending)
            {
                result = true;
            }
            return result;
        }
        [Authorize]
        public async Task<ActionResult> DisplayTicket(Guid ticketId)
        {
            var user = User.ApplicationUser();
            var ticket = await _orderService.GetTicketAsync(user.Id, ticketId);
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
        public async Task<ActionResult> ConfirmTicket(Guid ticketId)
        {
            var user = User.ApplicationUser();
            var ticket = await _orderService.GetTicketAsync(user.Id, ticketId);

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

        public async Task<ActionResult> DoTicketPayment(Guid ticketId)
        {

            var ticket = await _orderService.GetTicketAsync(_identiService.GetUserId(), ticketId);

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
        public async Task<ActionResult> ConfirmTicketPayment([Bind(Include = "result,transactionKey")] PaymentCallbackInfo paymentInfo)
        {
            var ticketId = paymentInfo.TransactionKey;


            var ticket = await _orderService.GetTicketAsync(_identiService.GetUserId(), ticketId);

            if (paymentInfo.Result == "success")
            {
                var confirmed = _orderService.ConfirmTicket(ticketId);
                ViewBag.IsConfirmed = confirmed;
                return RedirectToAction("DisplayTicket", new { ticketId = ticketId });
            }

            return View();
        }


        [HttpGet]
        public async Task<ActionResult> ConfirmTicketPayment(Guid transactionKey, string result)
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
        public async Task<ActionResult> DeleteEvent(Guid eventId)
        {
            var result = _eventManagementService.DeleteEvent(eventId);
            return RedirectToAction("MyEvents");
        }

        [Authorize]
        public async Task<ActionResult> DeleteTicket(Guid ticketId)
        {
            var result = _orderService.DeleteTicket(ticketId);
            return RedirectToAction("MyTickets");
        }

        [Authorize]
        public async Task<ActionResult> MakeEventLive(Guid eventId)
        {
            var result = _eventManagementService.MakeEventLive(eventId);
            return RedirectToAction("MyEvents");
        }
    }
}