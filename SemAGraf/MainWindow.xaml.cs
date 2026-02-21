using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SemAGraf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Graph<string, Coordinates> _graph;

        private List<PathUIViewModel> _foundPaths = new List<PathUIViewModel>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawGraph()
        {
            GraphCanvas.Children.Clear();
            if (_graph == null) return;

            foreach (var vertex in _graph.Vertices.Values)
            {
                foreach (var edge in vertex.Neighbors)
                {
                    var target = _graph.Vertices[edge.Target];
                    Line line = new Line
                    {
                        X1 = vertex.Data.X * 5,
                        Y1 = vertex.Data.Y * 5,
                        X2 = target.Data.X * 5,
                        Y2 = target.Data.Y * 5,
                        Stroke = edge.IsProblematic ? Brushes.Orange : Brushes.Gray,
                        StrokeThickness = edge.IsProblematic ? 3 : 1,
                        StrokeDashArray = edge.IsProblematic ? new DoubleCollection { 2 } : null
                    };
                    GraphCanvas.Children.Add(line);
                }
            }

            foreach (var vertex in _graph.Vertices.Values)
            {
                Ellipse circle = new Ellipse { Width = 10, Height = 10, Fill = Brushes.Black };
                Canvas.SetLeft(circle, (vertex.Data.X * 5) - 5);
                Canvas.SetTop(circle, (vertex.Data.Y * 5) - 5);

                TextBlock label = new TextBlock { Text = vertex.Id, FontSize = 10 };
                Canvas.SetLeft(label, vertex.Data.X * 5 + 7);
                Canvas.SetTop(label, vertex.Data.Y * 5 - 15);

                GraphCanvas.Children.Add(circle);
                GraphCanvas.Children.Add(label);
            }
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            _foundPaths.Clear();
            var vektorNasledniku = new SuccessorTable<string>();

            string start = TxtStart.Text;
            string cil = TxtEnd.Text;

            var zakladniNodes = _graph.ComputePath(start, cil, new HashSet<(string, string)>());

            if (zakladniNodes == null)
            {
                MessageBox.Show("Cesta mezi zadanými uzly neexistuje.");
                return;
            }

            double delkaZakladni = VypocitejDelku(zakladniNodes);
            _foundPaths.Add(new PathUIViewModel
            {
                Label = $"Základní ({delkaZakladni:F1} min)",
                Nodes = zakladniNodes,
                Color = Brushes.Red,
                Length = delkaZakladni,
                IsVisible = true
            });

            for (int i = 0; i < zakladniNodes.Count - 1; i++)
            {
                var uzelA = zakladniNodes[i];
                var uzelB = zakladniNodes[i + 1];
                var ignore = new HashSet<(string, string)> { (uzelA, uzelB), (uzelB, uzelA) };

                var altNodes = _graph.ComputePath(start, cil, ignore);

                if (altNodes != null && !_foundPaths.Any(p => p.Nodes.SequenceEqual(altNodes)))
                {
                    double delkaAlt = VypocitejDelku(altNodes);
                    _foundPaths.Add(new PathUIViewModel
                    {
                        Label = $"Alternativa ({delkaAlt:F1} min)",
                        Nodes = altNodes,
                        Color = Brushes.Blue,
                        Length = delkaAlt,
                        IsVisible = true
                    });
                }
            }

            _foundPaths = _foundPaths.OrderBy(p => p.Length).ToList();

            foreach (var path in _foundPaths)
            {
                for (int i = 0; i < path.Nodes.Count - 1; i++)
                {
                    vektorNasledniku.AddSuccessor(path.Nodes[i], path.Nodes[i + 1]);
                }
            }

            LbPaths.ItemsSource = null;
            LbPaths.ItemsSource = _foundPaths;

            DgSuccessors.ItemsSource = vektorNasledniku.Table.Select(kvp => new {
                Uzel = kvp.Key,
                Naslednici = string.Join(", ", kvp.Value)
            }).ToList();

            UpdateVisualization();
        }

        private void UpdateVisualization()
        {
            GraphCanvas.Children.Clear();
            DrawGraph();

            double offset = 0;
            foreach (var path in _foundPaths)
            {
                if (path.IsVisible)
                {
                    HighlightPath(path.Nodes, path.Color, offset);
                    offset += 4;
                }
            }
        }

        private void PathCheckBox_Click(object sender, RoutedEventArgs e)
        {
            UpdateVisualization();
        }

        private double VypocitejDelku(List<string> nodes)
        {
            double delka = 0;
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var uzel = _graph.Vertices[nodes[i]];
                var hrana = uzel.Neighbors.FirstOrDefault(n => n.Target == nodes[i + 1]);
                if (hrana != null) delka += hrana.Weight;
            }
            return delka;
        }

        private void HighlightPath(List<string> path, Brush color, double offset)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var v1 = _graph.Vertices[path[i]];
                var v2 = _graph.Vertices[path[i + 1]];

                double dx = v2.Data.X - v1.Data.X;
                double dy = v2.Data.Y - v1.Data.Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                double ux = -dy / length;
                double uy = dx / length;

                Line highlight = new Line
                {
                    X1 = (v1.Data.X * 5) + (ux * offset),
                    Y1 = (v1.Data.Y * 5) + (uy * offset),
                    X2 = (v2.Data.X * 5) + (ux * offset),
                    Y2 = (v2.Data.Y * 5) + (uy * offset),
                    Stroke = color,
                    StrokeThickness = 3,
                    Opacity = 0.8,
                    ToolTip = $"Trasa: {string.Join("->", path)}"
                };
                GraphCanvas.Children.Add(highlight);
            }
        }

        public class SuccessorRow
        {
            public string Uzel { get; set; }
            public string Naslednici { get; set; }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string jsonString = File.ReadAllText(openFileDialog.FileName);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true
                };

                _graph = JsonSerializer.Deserialize<Graph<string, Coordinates>>(jsonString, options);

                if (_graph != null)
                {
                    int nodeCount = _graph.Vertices.Count;
                    int edgeCount = 0;
                    foreach (var v in _graph.Vertices.Values) edgeCount += v.Neighbors.Count;

                    TxtStatus.Text = $"Načteno: Uzly {nodeCount}, Hrany {edgeCount / 2}";
                    DrawGraph();
                }
            }
        }
    }
}