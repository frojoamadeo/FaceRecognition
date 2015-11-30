using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using FaceRecognition.Models.Mapping;

namespace FaceRecognition.Models
{
    public partial class FaceRecognitionDBContext : DbContext
    {
        static FaceRecognitionDBContext()
        {
            Database.SetInitializer<FaceRecognitionDBContext>(null);
        }

        public FaceRecognitionDBContext()
            : base("Name=FaceRecognitionDBContext")
        {
        }

        public DbSet<DistanceResult> DistanceResults { get; set; }
        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new DistanceResultMap());
            modelBuilder.Configurations.Add(new EmployeeMap());
        }
    }
}
