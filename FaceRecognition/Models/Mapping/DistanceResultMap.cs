using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FaceRecognition.Models.Mapping
{
    public class DistanceResultMap : EntityTypeConfiguration<DistanceResult>
    {
        public DistanceResultMap()
        {
            // Primary Key
            this.HasKey(t => t.distanceResultId);

            // Properties
            // Table & Column Mappings
            this.ToTable("DistanceResults");
            this.Property(t => t.distanceResultId).HasColumnName("distanceResultId");
            this.Property(t => t.employeeId).HasColumnName("employeeId");
            //this.Property(t => t.label).HasColumnName("label");
            this.Property(t => t.photoName).HasColumnName("photoPath");

            // Relationships
            this.HasRequired(t => t.Employee)
                .WithMany(t => t.DistanceResults)
                .HasForeignKey(d => d.employeeId);

        }
    }
}
