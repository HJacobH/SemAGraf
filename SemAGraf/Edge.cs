using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public class Edge<TKey>
    {
        public TKey Target { get; set; } = default!;
        public double Weight { get; set; }

        public Edge() { }
        public Edge(TKey target, double weight)
        {
            Target = target;
            Weight = weight;
        }
    }
}
