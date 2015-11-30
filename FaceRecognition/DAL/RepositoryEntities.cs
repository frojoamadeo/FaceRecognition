using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using FaceRecognition.Models;

namespace FaceRecognition.DAL
{
    public class RepositoryEntities : DbContext
    {
        public DbSet _dbSet { get; set; }

        //public RepositoryEntities()
        //    : base("ConnectionStringFaceRDB")
        //{
        //    _dbSet = Employee;
        //    _dbSet = DistanceResult;
        //}

        public RepositoryEntities()
            : base("ConnectionStringFRDB") //Nombre ConnectionString
        {
            //Database.SetInitializer<RepositoryEntities>(new DropCreateDatabaseIfModelChanges<RepositoryEntities>());
        }

        public DbSet<Employee> Employee { get; set; }
        public DbSet<DistanceResult> DistanceResult { get; set; }

        //Sobreescribo OnModelCreating para que autogenere el GUID de Employee
        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Employee>().Property(p => p.employeeId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        //    base.OnModelCreating(modelBuilder);
        //}
    }
}
