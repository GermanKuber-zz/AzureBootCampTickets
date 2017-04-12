using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AzureBootCampTickets.Cache;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
using AzureBootCampTickets.Data.Context.CloudContext;
using AzureBootCampTickets.Entities.Entities;
using Microsoft.WindowsAzure.ServiceRuntime;

//TODO : 01 - Creo worker roll
namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent _runCompleteEvent = new ManualResetEvent(false);

        private AzureBootCampTicketsContext _ticketCtx;
        private AzureBootCampTicketsCloudContext _azureCtx;
        private ICacheService _cache = new CacheService();

        public override void Run()
        {
            Trace.TraceInformation("TicketerProcessor is running");

            try
            {
                this.RunAsync(this._cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this._runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("TicketerProcessor has been started");

            _ticketCtx = new AzureBootCampTicketsContext();
            _cache = new CacheService();
            _azureCtx = new AzureBootCampTicketsCloudContext(_cache);

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("TicketerProcessor is stopping");

            this._cancellationTokenSource.Cancel();
            this._runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("TicketerProcessor has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Processing order requests");

                var message = await _azureCtx.GetPendingOrderFromQueue();
                if (message != null)
                {
                    Trace.TraceInformation("New message from the queue: Ticket {0}", message.TicketId);
                    ProcessPlacedOrder(Guid.Parse(message.EventId), message.UserId, message.TicketId);
                    await _azureCtx.DeletePendingOrderFromQueue(message.MessageId, message.PopReceipt);
                }

                await Task.Delay(500);
            }
        }

        public void ProcessPlacedOrder(Guid eventId, string userId, string ticketId)
        {
            var ticketGuid = Guid.Parse(ticketId);
            var parentEvent = _ticketCtx.Events.SingleOrDefault(x => x.Id == eventId);
            var existingTicket = _ticketCtx.Tickets.SingleOrDefault(t => t.Id == ticketGuid);

            if (parentEvent != null && existingTicket == null)
            {
                Trace.TraceInformation("Adding the ticket to the database");
                var ticket = new Ticket()
                {
                    AccessCode = Ticket.GenerateRandomAccessCode(),
                    Attendee = userId,
                    TotalPrice = parentEvent.TicketPrice,
                    Status = TicketStatus.Pending,
                    Id = Guid.Parse(ticketId),
                    ParentEvent = parentEvent
                };

                _ticketCtx.Tickets.Add(ticket);
                _ticketCtx.SaveChanges();

                // Update read model
                _azureCtx.AddTicket(ticket);
            }
            else
            {
                if (existingTicket != null)
                {
                    Trace.TraceInformation("The ticket {0} already exists.", existingTicket.Id);
                }
                if (parentEvent == null)
                {
                    Trace.TraceInformation("The event {0} for the ticket doesn't exist.", eventId);
                }
            }
        }
    }
}
