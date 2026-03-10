using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SemAGraf
{
    public class Graph<TKey, TData> where TKey : notnull
    {
        public Dictionary<TKey, Vertex<TKey, TData>> Vertices { get; set; } = new();

        public void AddVertex(TKey id, TData data) => Vertices[id] = new Vertex<TKey, TData>(id, data);

        public HashSet<(TKey, TKey)> ProblematicEdges { get; set; } = new();

        public void SetEdgeProblematic(TKey from, TKey to, bool isProblematic)
        {
            if (isProblematic)
            {
                ProblematicEdges.Add((from, to));
                ProblematicEdges.Add((to, from));
            }
            else
            {
                ProblematicEdges.Remove((from, to));
                ProblematicEdges.Remove((to, from));
            }
        }

        public List<TKey>? ComputePath(TKey start, TKey end, HashSet<(TKey, TKey)> ignoredEdges, HashSet<TKey> ignoredVertices)
        {
            if (!Vertices.ContainsKey(start) || !Vertices.ContainsKey(end)) return null;
            if (ignoredVertices.Contains(start) || ignoredVertices.Contains(end)) return null;

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
                    if (ProblematicEdges.Contains((current, edge.Target)) ||
                        ignoredEdges.Contains((current, edge.Target)) ||
                        ignoredVertices.Contains(edge.Target))
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

        public List<List<TKey>> GetKShortestPaths(TKey start, TKey end, int k)
        {
            List<List<TKey>> shortestPaths = new();
            List<List<TKey>> potentialPaths = new();

            var firstPath = ComputePath(start, end, new(), new());
            if (firstPath == null) return shortestPaths;
            shortestPaths.Add(firstPath);

            for (int i = 1; i < k; i++)
            {
                var previousPath = shortestPaths[i - 1];

                for (int j = 0; j < previousPath.Count - 1; j++)
                {
                    TKey spurNode = previousPath[j];
                    List<TKey> rootPath = previousPath.GetRange(0, j + 1);

                    HashSet<(TKey, TKey)> ignoredEdges = new();
                    HashSet<TKey> ignoredVertices = new();

                    foreach (var p in shortestPaths)
                    {
                        if (p.Count > j && rootPath.SequenceEqual(p.GetRange(0, j + 1)))
                        {
                            ignoredEdges.Add((p[j], p[j + 1]));
                            ignoredEdges.Add((p[j + 1], p[j]));
                        }
                    }

                    for (int n = 0; n < rootPath.Count - 1; n++)
                    {
                        ignoredVertices.Add(rootPath[n]);
                    }

                    var spurPath = ComputePath(spurNode, end, ignoredEdges, ignoredVertices);

                    if (spurPath != null)
                    {
                        List<TKey> totalPath = new List<TKey>(rootPath);
                        totalPath.AddRange(spurPath.GetRange(1, spurPath.Count - 1));

                        if (!potentialPaths.Any(p => p.SequenceEqual(totalPath)))
                        {
                            potentialPaths.Add(totalPath);
                        }
                    }
                }

                if (potentialPaths.Count == 0) break;

                potentialPaths = potentialPaths.OrderBy(p => VypocitejDelkuCesty(p)).ToList();
                shortestPaths.Add(potentialPaths[0]);
                potentialPaths.RemoveAt(0);
            }

            return shortestPaths;
        }

        private double VypocitejDelkuCesty(List<TKey> path)
        {
            double length = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                var vertex = Vertices[path[i]];
                var edge = vertex.Neighbors.First(e => e.Target.Equals(path[i + 1]));
                length += edge.Weight;
            }
            return length;
        }

        public void AddEdge(TKey from, TKey to, double weight, bool isProblematic = false)
        {
            if (!Vertices.ContainsKey(from) || !Vertices.ContainsKey(to)) return;
 
            Vertices[from].Neighbors.Add(new Edge<TKey>(to, weight));
            Vertices[to].Neighbors.Add(new Edge<TKey>(from, weight));
        }

        public void RemoveEdge(TKey from, TKey to)
        {
            if (Vertices.ContainsKey(from) && Vertices.ContainsKey(to))
            {
                Vertices[from].Neighbors.RemoveAll(e => e.Target.Equals(to));

                Vertices[to].Neighbors.RemoveAll(e => e.Target.Equals(from));

                ProblematicEdges.Remove((from, to));
                ProblematicEdges.Remove((to, from));
            }
        }

        public Vertex<TKey, TData>? FindVertex(TKey id)
        {
            if (Vertices.TryGetValue(id, out var vertex)) return vertex;
            return null;
        }
    }
}
