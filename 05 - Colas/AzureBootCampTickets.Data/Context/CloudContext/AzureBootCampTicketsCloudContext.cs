using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AzureBootCampTickets.Contracts;
using AzureBootCampTickets.Contracts.Services;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.TableStorage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBootCampTickets.Data.Context.CloudContext
{
    public class AzureBootCampTicketsCloudContext : IAzureBootCampTicketsCloudContext
    {
        private readonly ICacheService _cacheService;
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _tableClient;
        private readonly CloudTable _tableTickets;
        private readonly CloudTable _tableEvents;
        private readonly CloudTable _tableMyEvents;
        private CloudQueueClient _queueClient;
        private readonly CloudQueue _ordersQueue;
        public AzureBootCampTicketsCloudContext(ICacheService cacheService)
        {
            _cacheService = cacheService;
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            _tableClient = _storageAccount.CreateCloudTableClient();
            _tableTickets = _tableClient.GetTableReference("TicketsRead");
            _tableEvents = _tableClient.GetTableReference("EventsRead");
            _tableMyEvents = _tableClient.GetTableReference("MyEventsRead");
            _queueClient = _storageAccount.CreateCloudQueueClient();
            _ordersQueue = _queueClient.GetQueueReference("tickets");
            try
            {
                _tableTickets.CreateIfNotExists();
                _tableEvents.CreateIfNotExists();
                _tableMyEvents.CreateIfNotExists();
            }
            catch (StorageException e)
            {
                throw;
            }
        }
     
        public async Task<Ticket> GetTicketAsync(string userId, Guid ticketId)
        {
            return await Task.Run(() =>
            {
                Ticket ticket = null;
                string partitionKey = userId.ToString();
                string rowKey = ticketId.ToString();

                TableOperation readOperation = TableOperation.Retrieve<TicketRead>(partitionKey, rowKey);

                var result = _tableTickets.Execute(readOperation);
                if (result.Result != null)
                {
                    var nosqlTicket = (TicketRead)result.Result;
                    ticket = nosqlTicket.ToTicket();
                }
                return ticket;
            });
        }
        public async Task<Guid> PlaceOrderInQueue(Guid eventId, string userId)
        {
            try
            {
                var ticketId = Guid.NewGuid();
                var messageContent = String.Format("Order;{0};{1};{2}", eventId, userId, ticketId);
                var orderMessage = new CloudQueueMessage(messageContent);
                await _ordersQueue.AddMessageAsync(orderMessage);
                return ticketId;
            }
            catch (Exception ex)
            {
                // Log the exception somewhere
                return Guid.Empty;
            }
        }
        //TODO : 03 - Creo funciones manejadoras de la queue
        public async Task DeletePendingOrderFromQueue(string messageId, string popReceipt)
        {
            await _ordersQueue.DeleteMessageAsync(messageId, popReceipt);
        }
        public async Task<OrderDetails> GetPendingOrderFromQueue()
        {
            OrderDetails returnMessage = null;
            try
            {
                var message = await _ordersQueue.GetMessageAsync();
                if (message != null)
                {
                    if (message.DequeueCount > 5)
                    {
                        // Poisoned message, delete it
                        await _ordersQueue.DeleteMessageAsync(message);
                        return null;
                    }
                    var messageContent = message.AsString;
                    string[] segments = messageContent.Split(';');
                    if (segments[0] == "Order")
                    {
                        var eventId = segments[1];
                        var userId = segments[2];
                        var ticketId = segments[3];

                        returnMessage = new OrderDetails()
                        {
                            EventId = eventId,
                            UserId = userId,
                            TicketId = ticketId,
                            MessageId = message.Id,
                            PopReceipt = message.PopReceipt
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception somewhere
            }

            return returnMessage;
        }
        public async Task<List<Ticket>> GetMyTicketsAsync(string userId)
        {
            List<Ticket> tickets = new List<Ticket>();
            var key = GenerateMyTicketsKey(userId);

            return await _cacheService.GetFromCacheAsync<List<Ticket>>(key, async () =>
                       {
                           
                           TableQuery<TicketRead> query =
                               new TableQuery<TicketRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                                   QueryComparisons.Equal, userId));
                           TableQuerySegment<TicketRead> currentSegment = null;
                           currentSegment = await _tableTickets.ExecuteQuerySegmentedAsync(query, currentSegment != null ? currentSegment.ContinuationToken : null);
                           foreach (TicketRead nosqlTicket in currentSegment.Results)
                           {
                               var ticket = nosqlTicket.ToTicket();
                               tickets.Add(ticket);
                           }
                           return tickets;
                       });
        }

        public async Task<List<Event>> GetMyEventsAsync(string userId)
        {
            List<Event> events = new List<Event>();
            var key = GenerateMyEventsKey(userId);
            return await _cacheService.GetFromCacheAsync<List<Event>>(key, async () =>
            {
                TableQuery<EventRead> query =
                    new TableQuery<EventRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal, userId));
                TableQuerySegment<EventRead> currentSegment = null;
                currentSegment = await _tableMyEvents.ExecuteQuerySegmentedAsync(query, currentSegment != null ? currentSegment.ContinuationToken : null);
                foreach (EventRead nosqlEvent in currentSegment.Results)
                {
                    var eventObj = nosqlEvent.ToEvent();
                    events.Add(eventObj);
                }
                return events;
            });
        }

        public async Task<List<Event>> GetLiveEventsAsync(DateTime currentDate)
        {
            string year = currentDate.Year.ToString();
            var key = GenerateLiveEventsKey(year);
            var yearEvents = await _cacheService.GetFromCacheAsync<List<Event>>(key, async () =>
            {

                List<Event> events = new List<Event>();

                string partitionKey = year;

                TableQuery<EventRead> query = new TableQuery<EventRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                TableQuerySegment<EventRead> currentSegment = null;
                currentSegment = await _tableEvents.ExecuteQuerySegmentedAsync(query, currentSegment != null ? currentSegment.ContinuationToken : null);
                foreach (EventRead nosqlEvent in currentSegment.Results)
                {
                    if (nosqlEvent.EventDate >= currentDate)
                    {
                        var eventObj = nosqlEvent.ToEvent();
                        events.Add(eventObj);
                    }
                }
                return events;
            });
            return yearEvents.Where(e => e.EventDate >= currentDate).ToList();
        }

        public void ConfirmTicket(Ticket ticket)
        {
            string partitionKey = ticket.Attendee;
            string rowKey = ticket.Id.ToString();
            var ticketToUpdate = new DynamicTableEntity() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };
            Dictionary<string, EntityProperty> newProperties = new Dictionary<string, EntityProperty>
            {
                {"TicketStatus", new EntityProperty("Paid")}
            };
            ticketToUpdate.Properties = newProperties;
            TableOperation updateOperation = TableOperation.Merge(ticketToUpdate);
            _tableTickets.Execute(updateOperation);

            _cacheService.InvalidateCache(GenerateMyTicketsKey(ticket.Attendee));
        }

        public void MakeEventLive(Event eventObj)
        {
            string partitionKey = eventObj.Organizer;
            string rowKey = eventObj.Id.ToString();
            var eventToUpdate = new DynamicTableEntity() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };
            Dictionary<string, EntityProperty> newProperties = new Dictionary<string, EntityProperty>
            {
                {"Status", new EntityProperty("Live")}
            };
            eventToUpdate.Properties = newProperties;
            TableOperation updateOperation = TableOperation.Merge(eventToUpdate);
            _tableMyEvents.Execute(updateOperation);

            // Add the new live event to All Events table
            var eventToAdd = eventObj.ToEventRead();
            eventToAdd.Status = "Live";
            TableOperation addOperation = TableOperation.InsertOrReplace(eventToAdd);
            _tableEvents.Execute(addOperation);

            _cacheService.InvalidateCache(GenerateLiveEventsKey(eventToAdd.PartitionKey));
            _cacheService.InvalidateCache(GenerateMyEventsKey(eventToAdd.Organizer));

        }

        public void UpdateEventSeats(Event eventObj)
        {
            string partitionKey = eventObj.EventDate.Year.ToString();
            string rowKey = eventObj.Id.ToString();
            var eventToUpdate = new DynamicTableEntity() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };
            Dictionary<string, EntityProperty> newProperties = new Dictionary<string, EntityProperty>
            {
                {"AvailableSeats", new EntityProperty(eventObj.AvailableSeats)}
            };
            eventToUpdate.Properties = newProperties;
            TableOperation updateOperation = TableOperation.Merge(eventToUpdate);
            _tableEvents.Execute(updateOperation);


            _cacheService.InvalidateCache(GenerateLiveEventsKey(partitionKey));
        }

        public void DeleteTicket(Ticket ticket)
        {
            string partitionKey = ticket.Attendee;
            string rowKey = ticket.Id.ToString();
            var ticketToDelete = new TicketRead() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };

            TableOperation deleteOperation = TableOperation.Delete(ticketToDelete);
            _tableTickets.Execute(deleteOperation);


            _cacheService.InvalidateCache(GenerateMyTicketsKey(partitionKey));
        }

        public void DeleteEvent(Event eventObj)
        {
            string partitionKey = eventObj.Organizer;
            string rowKey = eventObj.Id.ToString();
            var eventToDelete = new TicketRead() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };

            TableOperation deleteOperation = TableOperation.Delete(eventToDelete);
            _tableMyEvents.Execute(deleteOperation);


            _cacheService.InvalidateCache(GenerateMyEventsKey(partitionKey));
        }
        //TODO : 06 -Retorno mis ticekts
        public async Task<Ticket> GetTicket(string userId, Guid ticketId)
        {
            var myTickets = await GetMyTickets(userId);
            return myTickets.SingleOrDefault(t => t.Id == ticketId);
        }
        //TODO : 05 - Consulto mis ticekts
        public async Task<List<Ticket>> GetMyTickets(string userId)
        {
            var key = GenerateMyTicketsKey(userId);
            return await _cacheService.GetFromCacheAsync<List<Ticket>>(key, async () =>
            {
                List<Ticket> tickets = new List<Ticket>();
                string partitionKey = userId.ToString();

                TableQuery<TicketRead> query = new TableQuery<TicketRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
                TableQuerySegment<TicketRead> currentSegment = null;
                while (currentSegment == null || currentSegment.ContinuationToken != null)
                {
                    currentSegment = await _tableTickets.ExecuteQuerySegmentedAsync(query, currentSegment != null ? currentSegment.ContinuationToken : null);
                    var result = _tableTickets.ExecuteQuery(query);
                    foreach (TicketRead nosqlTicket in currentSegment.Results)
                    {
                        var ticket = nosqlTicket.ToTicket();
                        tickets.Add(ticket);
                    }
                }
                return tickets;
            });
        }
        public void AddTicket(Ticket ticket)
        {
            var ticketToAdd = ticket.ToTicketRead();
            TableOperation addOperation = TableOperation.InsertOrReplace(ticketToAdd);
            _tableTickets.Execute(addOperation);

            _cacheService.InvalidateCache(GenerateMyTicketsKey(ticketToAdd.PartitionKey));
        }

        public void AddEvent(Event eventObj)
        {
            try
            {
                var eventToAdd = eventObj.ToEventRead();
                eventToAdd.PartitionKey = eventToAdd.Organizer;
                TableOperation addOperation = TableOperation.InsertOrReplace(eventToAdd);
                _tableMyEvents.Execute(addOperation);

                _cacheService.InvalidateCache(GenerateMyEventsKey(eventToAdd.PartitionKey));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        private static string GenerateLiveEventsKey(string year)
        {
            var key = $"LiveEvents-{year}";
            return key;
        }

        private static string GenerateMyEventsKey(string userId)
        {
            var key = $"MyEvents-{userId}";
            return key;
        }

        private static string GenerateMyTicketsKey(string userId)
        {
            var key = $"MyTickets-{userId}";
            return key;
        }
    }

}