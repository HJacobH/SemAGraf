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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DrawGraph()
        {
            GraphCanvas.Children.Clear();
            if (_graph == null) return;

            // 1. Vykreslení hran (komunikací)
            foreach (var vertex in _graph.Vertices.Values)
            {
                foreach (var edge in vertex.Neighbors)
                {
                    var target = _graph.Vertices[edge.Target];
                    Line line = new Line
                    {
                        X1 = vertex.Data.X * 5, // Měřítko pro Canvas
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

            // 2. Vykreslení uzlů (obcí)
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
            GraphCanvas.Children.Clear();
            DrawGraph(); // Překreslíme základní síť

            string start = TxtStart.Text;
            string cíl = TxtEnd.Text;

            // 1. Výpočet ZÁKLADNÍ nejkratší trasy
            var nejkratsíTrasa = _graph.ComputePath(start, cíl, new HashSet<(string, string)>());

            if (nejkratsíTrasa == null)
            {
                TxtStatus.Text = "Cesta neexistuje.";
                return;
            }

            HighlightPath(nejkratsíTrasa, Brushes.Red); // Základní trasa (červená) [cite: 51]

            var vektorNasledniků = new SuccessorTable<string>();

                        // Projdeme hrany na nejkratší trase a zkusíme je jednu po druhé "zakázat"
            for (int i = 0; i < nejkratsíTrasa.Count - 1; i++)
            {
                var uzelA = nejkratsíTrasa[i];
                var uzelB = nejkratsíTrasa[i + 1];

                var ignore = new HashSet<(string, string)> { (uzelA, uzelB), (uzelB, uzelA) };

                var alternativa = _graph.ComputePath(start, cíl, ignore);

                if (alternativa != null)
                {
                    HighlightPath(alternativa, Brushes.Blue); // Alternativní trasy [cite: 51]

                    for (int j = 0; j < alternativa.Count - 1; j++)
                    {
                        vektorNasledniků.AddSuccessor(alternativa[j], alternativa[j + 1]);
                    }
                }
            }

            TxtStatus.Text = $"Vypočteny alternativy pro trasu {start}->{cíl}. Data uložena ve vektoru následníků.";
        }

        private void HighlightPath(List<string> path, Brush color)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var v1 = _graph.Vertices[path[i]];
                var v2 = _graph.Vertices[path[i + 1]];
                Line highlight = new Line
                {
                    X1 = v1.Data.X * 5,
                    Y1 = v1.Data.Y * 5,
                    X2 = v2.Data.X * 5,
                    Y2 = v2.Data.Y * 5,
                    Stroke = color,
                    StrokeThickness = 4,
                    Opacity = 0.6
                };
                GraphCanvas.Children.Add(highlight);
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string jsonString = File.ReadAllText(openFileDialog.FileName);

                // Nastavení pro necitlivost na velikost písmen
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    IncludeFields = true // Pro jistotu, pokud byste měli pole místo vlastností
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