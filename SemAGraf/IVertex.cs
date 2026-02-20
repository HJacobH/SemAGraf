using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public interface IVertex<TKey, TData> : IIdentifiable<TKey> where TKey : notnull
    {
        TData Data { get; set; }
    }
}
