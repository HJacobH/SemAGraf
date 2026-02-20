using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public class SuccessorTable<TKey> where TKey : notnull
    {
        public Dictionary<TKey, HashSet<TKey>> Table { get; } = new();

        public void AddSuccessor(TKey current, TKey next)
        {
            if (!Table.ContainsKey(current))
                Table[current] = new HashSet<TKey>();

            Table[current].Add(next);
        }
    }
}
