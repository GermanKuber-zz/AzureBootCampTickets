using System.Data.Entity;
using AzureBootCampTickets.Entities.Entities;

namespace AzureBootCampTickets.Data.Context.AzureBootCampTickets
{
    public class AzureBootCampTicketsContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Event> Events { get; set; }
        public AzureBootCampTicketsContext()
            : base("DefaultConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new AzureBootCampTicketsContextInitializer());

   
            modelBuilder.Entity<Event>()
                .HasMany<Ticket>(e => e.Tickets)
                .WithRequired(t => t.ParentEvent);
        }
    }

}