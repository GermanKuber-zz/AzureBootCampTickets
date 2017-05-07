﻿using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBootCampTickets.Entities.TableStorage
{
    //TODO : 06 - Creamos entidad para leer
    public class EventRead: TableEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status {get; set;}
        public DateTime EventDate { get; set; }

        public int TotalSeats { get; set; }
        public double TicketPrice { get; set; }
        public int AvailableSeats { get; set; }

        public string Organizer { get; set; }
    }
}