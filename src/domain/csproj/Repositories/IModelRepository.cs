using Fuxion.Domain.Models;
using Fuxion.Domain.Events;
using Fuxion.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fuxion.Domain.Repositories
{
    public interface IModelRepository<TModel> where TModel : IModel
    {
        IQueryable<TModel> Query();
        void Add(TModel entity);
        TModel Find(object id);
        void Remove(TModel entity);
        Task SaveChangesAsync();
    }
}
