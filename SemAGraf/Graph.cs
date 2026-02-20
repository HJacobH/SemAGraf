using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public class Graph<TKey, TData> where TKey : notnull
    {
        // POUŽÍVEJTE POUZE TENTO SLOVNÍK (odstraňte _vertices)
        public Dictionary<TKey, Vertex<TKey, TData>> Vertices { get; set; } = new();

        public void AddVertex(TKey id, TData data) => Vertices[id] = new Vertex<TKey, TData>(id, data);

        public List<TKey>? ComputePath(TKey start, TKey end, HashSet<(TKey, TKey)> ignoredEdges)
        {
            // Kontrola existence klíčů v naplněném slovníku Vertices
            if (!Vertices.ContainsKey(start) || !Vertices.ContainsKey(end)) return null;

            var distances = new Dictionary<TKey, double>();
            var previous = new Dictionary<TKey, TKey?>();
            var priorityQueue = new PriorityQueue<TKey, double>();

            foreach (var key in Vertices.Keys)
            {
                distances[key] = double.PositiveInfinity;
                previous[key] = default;
            }

            distances[start] = 0;
            priorityQueue.Enqueue(start, 0);

            while (priorityQueue.Count > 0)
            {
                var current = priorityQueue.Dequeue();
                if (current.Equals(end)) break;

                // Zde byla chyba - nyní přistupujeme k Vertices[current]
                foreach (var edge in Vertices[current].Neighbors)
                {
                    if (edge.IsProblematic || ignoredEdges.Contains((current, edge.Target)))
                        continue;

                    double alt = distances[current] + edge.Weight;
                    if (alt < distances[edge.Target])
                    {
                        distances[edge.Target] = alt;
                        previous[edge.Target] = current;
                        priorityQueue.Enqueue(edge.Target, alt);
                    }
                }
            }

            if (!distances.ContainsKey(end) || distances[end] == double.PositiveInfinity) return null;

            var path = new List<TKey>();
            TKey? curr = end;
            while (curr != null)
            {
                path.Insert(0, curr);
                if (curr.Equals(start)) break;
                curr = previous[curr];
            }
            return path;
        }
    }
}
