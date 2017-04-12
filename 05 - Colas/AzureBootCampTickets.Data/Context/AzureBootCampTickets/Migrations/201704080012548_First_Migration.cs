using System.Data.Entity.Migrations;

namespace AzureBootCampTickets.Data.Context.AzureBootCampTickets.Migrations
{
    public partial class First_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Description = c.String(),
                        StatusId = c.Int(nullable: false),
                        TotalSeats = c.Int(nullable: false),
                        TicketPrice = c.Double(nullable: false),
                        AvailableSeats = c.Int(nullable: false),
                        Organizer = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tickets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Attendee = c.String(),
                        TotalPrice = c.Double(nullable: false),
                        TicketStatusId = c.Int(nullable: false),
                        AccessCode = c.String(),
                        ParentEvent_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.ParentEvent_Id, cascadeDelete: true)
                .Index(t => t.ParentEvent_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tickets", "ParentEvent_Id", "dbo.Events");
            DropIndex("dbo.Tickets", new[] { "ParentEvent_Id" });
            DropTable("dbo.Tickets");
            DropTable("dbo.Events");
        }
    }
}
