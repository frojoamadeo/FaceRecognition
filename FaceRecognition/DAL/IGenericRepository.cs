using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRecognition.DAL
{
    interface IGenericRepository<TEntity>
    {
        IEnumerable<TEntity> GetAllRecords();
        void Add(TEntity entity);
        void Update(TEntity entity);
        TEntity GetFirstOrDefault(Int64 recordId);
        void Delete(TEntity entity);
    }
}
