using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public class Vertex<TKey, TData> where TKey : notnull
    {
        public TKey Id { get; set; } = default!;
        public TData Data { get; set; } = default!;
        public List<Edge<TKey>> Neighbors { get; set; } = new List<Edge<TKey>>();

        public Vertex() { }
        public Vertex(TKey id, TData data) { Id = id; Data = data; }
    }
}
