namespace AzureBootCampTickets.Data.Context.AzureBootCampTickets.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Tickets : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "EventDate", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Events", "EventDate");
        }
    }
}
