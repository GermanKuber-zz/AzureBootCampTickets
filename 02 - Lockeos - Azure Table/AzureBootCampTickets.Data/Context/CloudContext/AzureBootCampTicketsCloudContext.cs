using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using AzureBootCampTickets.Contracts;
using AzureBootCampTickets.Entities.Entities;
using AzureBootCampTickets.Entities.TableStorage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace AzureBootCampTickets.Data.Context.CloudContext
{
    public class AzureBootCampTicketsCloudContext : IAzureBootCampTicketsCloudContext
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _tableClient;
        private readonly CloudTable _tableTickets;
        private readonly CloudTable _tableEvents;
        private readonly CloudTable _tableMyEvents;

        public AzureBootCampTicketsCloudContext()
        {
            _storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
            _tableClient = _storageAccount.CreateCloudTableClient();
            _tableTickets = _tableClient.GetTableReference("TicketsRead");
            _tableEvents = _tableClient.GetTableReference("EventsRead");
            _tableMyEvents = _tableClient.GetTableReference("MyEventsRead");
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

        public Ticket GetTicket(string userId, Guid ticketId)
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
        }


        public List<Ticket> GetMyTickets(string userId)
        {
            List<Ticket> tickets = new List<Ticket>();

            string partitionKey = userId.ToString();

            TableQuery<TicketRead> query = new TableQuery<TicketRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

            var result = _tableTickets.ExecuteQuery(query);
            foreach (TicketRead nosqlTicket in result)
            {
                var ticket = nosqlTicket.ToTicket();
                tickets.Add(ticket);
            }
            return tickets;
        }

        public List<Event> GetMyEvents(string userId)
        {
            List<Event> events = new List<Event>();

            TableQuery<EventRead> query = new TableQuery<EventRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));

            var result = _tableMyEvents.ExecuteQuery(query);
            foreach (EventRead nosqlEvent in result)
            {
                var eventObj = nosqlEvent.ToEvent();
                events.Add(eventObj);
            }
            return events;
        }

        public List<Event> GetLiveEvents(DateTime currentDate)
        {
            string year = currentDate.Year.ToString();

            List<Event> events = new List<Event>();

            string partitionKey = year;

            TableQuery<EventRead> query = new TableQuery<EventRead>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            var result = _tableEvents.ExecuteQuery(query);
            foreach (EventRead nosqlEvent in result)
            {
                if (nosqlEvent.EventDate >= currentDate)
                {
                    var eventObj = nosqlEvent.ToEvent();
                    events.Add(eventObj);
                }
            }
            return events;
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
        }

        public void DeleteTicket(Ticket ticket)
        {
            string partitionKey = ticket.Attendee;
            string rowKey = ticket.Id.ToString();
            var ticketToDelete = new TicketRead() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };

            TableOperation deleteOperation = TableOperation.Delete(ticketToDelete);
            _tableTickets.Execute(deleteOperation);
        }

        public void DeleteEvent(Event eventObj)
        {
            string partitionKey = eventObj.Organizer;
            string rowKey = eventObj.Id.ToString();
            var eventToDelete = new TicketRead() { PartitionKey = partitionKey, RowKey = rowKey, ETag = "*" };

            TableOperation deleteOperation = TableOperation.Delete(eventToDelete);
            _tableMyEvents.Execute(deleteOperation);
        }

        public void AddTicket(Ticket ticket)
        {
            var ticketToAdd = ticket.ToTicketRead();
            TableOperation addOperation = TableOperation.InsertOrReplace(ticketToAdd);
            _tableTickets.Execute(addOperation);
        }

        public void AddEvent(Event eventObj)
        {
            try
            {
                var eventToAdd = eventObj.ToEventRead();
                eventToAdd.PartitionKey = eventToAdd.Organizer;
                TableOperation addOperation = TableOperation.InsertOrReplace(eventToAdd);
                _tableMyEvents.Execute(addOperation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}