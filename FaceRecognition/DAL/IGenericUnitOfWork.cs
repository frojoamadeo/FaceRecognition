using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognition.DAL
{
    interface IGenericUnitOfWork
    {
        GenericRepository<TEntityType> GetRepoInstance<TEntityType>() where TEntityType : class;
        void SaveChanges();
    }
}
