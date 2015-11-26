namespace FaceRecognition.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DistanceResults",
                c => new
                    {
                        distanceResultId = c.Int(nullable: false, identity: true),
                        employeeId = c.Int(nullable: false),
                        label = c.String(),
                        photoPath = c.String(),
                    })
                .PrimaryKey(t => t.distanceResultId)
                .ForeignKey("dbo.Employees", t => t.employeeId, cascadeDelete: true)
                .Index(t => t.employeeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DistanceResults", "employeeId", "dbo.Employees");
            DropIndex("dbo.DistanceResults", new[] { "employeeId" });
            DropTable("dbo.DistanceResults");
        }
    }
}
