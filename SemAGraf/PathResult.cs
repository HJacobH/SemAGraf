using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SemAGraf
{
    public class PathResult<TKey>
    {
        public List<TKey> Nodes { get; set; } = new List<TKey>();
        public double TotalWeight { get; set; }
    }
}
