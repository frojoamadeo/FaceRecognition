using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace FaceRecognition.DAL
{
    public class GenericUnitOfWork : IGenericUnitOfWork, System.IDisposable
    {
        private RepositoryEntities _dbContext = new RepositoryEntities();
        private bool disposed = false;

        //private EmployeeRepository _employeeRepository;
        public Type TheType { get; set; }

        public GenericRepository<TEntityType> GetRepoInstance<TEntityType>() where TEntityType : class
        {
            return new GenericRepository<TEntityType>(_dbContext);
        }

        //public EmployeeRepository EmployeeRepository
        //{
        //    get { return _employeeRepository ?? (_employeeRepository = new EmployeeRepository(_dbContext)); }
        //}

        public void SaveChanges()
        {
            _dbContext.SaveChanges();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }
    }
}

