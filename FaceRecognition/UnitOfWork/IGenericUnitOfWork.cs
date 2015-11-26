using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FaceRecognition.GenericRepository;

namespace FaceRecognition.UnitOfWork
{
    interface IGenericUnitOfWork
    {
        GenericRepository<TEntityType> GetRepoInstance<TEntityType>() where TEntityType : class;
        void SaveChanges();
    }
}
