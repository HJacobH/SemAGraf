using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public class SuccessorVector<TKey>
    {
        private Dictionary<TKey, List<TKey>> _successors = new();

        public void AddSuccessor(TKey current, TKey next)
        {
            if (!_successors.ContainsKey(current)) _successors[current] = new List<TKey>();
            if (!_successors[current].Contains(next)) _successors[current].Add(next);
        }
    }
}
