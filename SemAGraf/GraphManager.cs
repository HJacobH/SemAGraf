using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SemAGraf
{
    internal class GraphManager<TKey, TData> where TKey : notnull
    {
        public void SaveToFile(Graph<TKey, TData> graph, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(graph, options);
            File.WriteAllText(filePath, json);
        }

        public Graph<TKey, TData>? LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Graph<TKey, TData>>(json);
        }
    }
}
