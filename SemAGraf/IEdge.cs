using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public interface IEdge<TKey, TWeight> where TKey : notnull
    {
        TKey From { get; }
        TKey To { get; }
        TWeight Weight { get; set; }
        bool IsProblematic { get; set; }
    }
}
