using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemAGraf
{
    public class Graph<TKey, TData> where TKey : notnull
    {
        public Dictionary<TKey, Vertex<TKey, TData>> Vertices { get; set; } = new();

        public void AddVertex(TKey id, TData data) => Vertices[id] = new Vertex<TKey, TData>(id, data);

        public List<TKey>? ComputePath(TKey start, TKey end, HashSet<(TKey, TKey)> ignoredEdges)
        {
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
        public void SetEdgeProblematic(TKey from, TKey to, bool isProblematic)
{
            if (!Vertices.ContainsKey(from) || !Vertices.ContainsKey(to)) return;

            var edge1 = Vertices[from].Neighbors.FirstOrDefault(e => e.Target.Equals(to));
            var edge2 = Vertices[to].Neighbors.FirstOrDefault(e => e.Target.Equals(from));

            if (edge1 != null) edge1.IsProblematic = isProblematic;
            if (edge2 != null) edge2.IsProblematic = isProblematic;
        }
        public void AddEdge(TKey from, TKey to, double weight, bool isProblematic = false)
        {
            if (!Vertices.ContainsKey(from) || !Vertices.ContainsKey(to)) return;
 
            Vertices[from].Neighbors.Add(new Edge<TKey>(to, weight, isProblematic));
            Vertices[to].Neighbors.Add(new Edge<TKey>(from, weight, isProblematic));
        }

        public void RemoveEdge(TKey from, TKey to)
        {
            if (!Vertices.ContainsKey(from) || !Vertices.ContainsKey(to)) return;

            Vertices[from].Neighbors.RemoveAll(e => e.Target.Equals(to));
            Vertices[to].Neighbors.RemoveAll(e => e.Target.Equals(from));
        }
        public Vertex<TKey, TData>? FindVertex(TKey id)
        {
            if (Vertices.TryGetValue(id, out var vertex)) return vertex;
            return null;
        }
    }
}
