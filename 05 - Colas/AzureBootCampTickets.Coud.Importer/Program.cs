//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using AzureBootCampTickets.Data.Context.ApplicationDb;
//using AzureBootCampTickets.Data.Context.AzureBootCampTickets;
//using AzureBootCampTickets.Entities.Entities;
//using AzureBootCampTickets.Entities.TableStorage;
//using Microsoft.WindowsAzure.Storage;
//using Microsoft.WindowsAzure.Storage.Table;
//using Microsoft.WindowsAzure.Storage.Table.Protocol;

//namespace AzureBootCampTickets.Coud.Importer
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager
//               .ConnectionStrings["StorageConnectionString"].ConnectionString);
//            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

//            // Open Azure Table                      
//            CloudTable tableTickets = tableClient.GetTableReference("TicketsRead");
//            CloudTable tableEvents = tableClient.GetTableReference("EventsRead");
//            CloudTable tableMyEvents = tableClient.GetTableReference("MyEventsRead");

//            // Delete and Recreate Azure Tables
//            tableTickets.DeleteIfExists();
//            tableEvents.DeleteIfExists();
//            tableMyEvents.DeleteIfExists();

//            try
//            {
//                tableTickets.CreateIfNotExists();
//                tableEvents.CreateIfNotExists();
//                tableMyEvents.CreateIfNotExists();
//            }
//            catch (StorageException e)
//            {
//                if ((e.RequestInformation.HttpStatusCode == 409) && (e.RequestInformation.ExtendedErrorInformation.ErrorCode.Equals(TableErrorCodeStrings.TableBeingDeleted)))
//                {
//                    // The table is currently being deleted. Try again until it works.
//                    Thread.Sleep(1000);
//                }
//                else
//                {
//                    throw;
//                }
//            }

//            // Open SQL Database
//            var context = new AzureBootCampTicketsContext();
//            var userContext = ApplicationDbContext.Create();

//            foreach (var ticket in context.Tickets.Include("ParentEvent"))
//            {
//                var azureTicket = new TicketRead();
//                azureTicket.PartitionKey = ticket.Attendee;
//                azureTicket.RowKey = ticket.Id.ToString();
//                azureTicket.AccessCode = ticket.AccessCode;
//                azureTicket.AttendeeName = userContext.Users.First(u => u.Id == ticket.Attendee).Email;
//                azureTicket.ParentEventName = ticket.ParentEvent.Name;
//                azureTicket.ParentEventDescription = ticket.ParentEvent.Description;
//                azureTicket.ParentEventDate = ticket.ParentEvent.EventDate;
//                azureTicket.TicketStatus = ticket.Status.ToString();
//                azureTicket.TotalPrice = ticket.TotalPrice;

//                // Insert into Azure Table storage                
//                TableOperation insertOperation = TableOperation.InsertOrReplace(azureTicket);
//                tableTickets.Execute(insertOperation);
//            }

//            foreach (var myEvent in context.Events.Where(e => e.StatusId == (int)EventStatus.Live))
//            {
//                var azureEvent = new EventRead();
//                azureEvent.PartitionKey = myEvent.EventDate.Year.ToString();
//                azureEvent.RowKey = myEvent.Id.ToString();
//                azureEvent.AvailableSeats = myEvent.AvailableSeats;
//                azureEvent.Description = myEvent.Description;
//                azureEvent.Organizer = userContext.Users.Where(u => u.Id == myEvent.Organizer).First().Email;
//                azureEvent.Name = myEvent.Name;
//                azureEvent.Status = "Live";
//                azureEvent.EventDate = myEvent.EventDate;
//                azureEvent.TicketPrice = myEvent.TicketPrice;
//                azureEvent.TotalSeats = myEvent.TotalSeats;

//                // Insert into Azure Table storage                
//                TableOperation insertOperation = TableOperation.InsertOrReplace(azureEvent);
//                tableEvents.Execute(insertOperation);
//            }

//            foreach (var myEvent in context.Events)
//            {
//                var organizer = userContext.Users.Where(u => u.Id == myEvent.Organizer).First();
//                var azureEvent = new EventRead();
//                azureEvent.PartitionKey = organizer.Id.ToString();
//                azureEvent.RowKey = myEvent.Id.ToString();
//                azureEvent.AvailableSeats = myEvent.AvailableSeats;
//                azureEvent.Description = myEvent.Description;
//                azureEvent.Organizer = organizer.Email;
//                azureEvent.Name = myEvent.Name;
//                azureEvent.EventDate = myEvent.EventDate;
//                azureEvent.TicketPrice = myEvent.TicketPrice;
//                azureEvent.TotalSeats = myEvent.TotalSeats;
//                azureEvent.Status = myEvent.Status.ToString();

//                // Insert into Azure Table storage                
//                TableOperation insertOperation = TableOperation.InsertOrReplace(azureEvent);
//                tableMyEvents.Execute(insertOperation);
//            }
//        }
//    }
//}
