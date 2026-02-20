using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SemAGraf
{
    public interface IIdentifiable<TKey> where TKey : notnull
    {
        TKey Id { get; }
    }
}
