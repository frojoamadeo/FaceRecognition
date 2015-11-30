using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FaceRecognition.Models.Mapping
{
    public class EmployeeMap : EntityTypeConfiguration<Employee>
    {
        public EmployeeMap()
        {
            // Primary Key
            this.HasKey(t => t.employeeId);

            // Properties
            // Table & Column Mappings
            this.ToTable("Employees");
            this.Property(t => t.employeeId).HasColumnName("employeeId");
            this.Property(t => t.name).HasColumnName("name");
            this.Property(t => t.middleName).HasColumnName("middleName");
            this.Property(t => t.lastName).HasColumnName("lastName");
        }
    }
}
