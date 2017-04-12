using System.Data.Entity;

namespace AzureBootCampTickets.Data.Context.AzureBootCampTickets
{
    public class AzureBootCampTicketsContextInitializer : DropCreateDatabaseIfModelChanges<AzureBootCampTicketsContext>
    {
        protected override void Seed(AzureBootCampTicketsContext dbContext)
        {
            // seed data
            base.Seed(dbContext);
        }
    }

}