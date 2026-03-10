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

                    bool isProblematic = _graph.ProblematicEdges.Contains((vertex.Id, edge.Target));
                    Line line = new Line
                    {
                        X1 = vertex.Data.X * 5,
                        Y1 = vertex.Data.Y * 5,
                        X2 = target.Data.X * 5,
                        Y2 = target.Data.Y * 5,
                        Stroke = isProblematic ? Brushes.Orange : Brushes.Gray,
                        StrokeThickness = isProblematic ? 3 : 1
                    };
                    GraphCanvas.Children.Add(line);

                    double midX = (line.X1 + line.X2) / 2;
                    double midY = (line.Y1 + line.Y2) / 2;

                    TextBlock weightLabel = new TextBlock
                    {
                        Text = $"{edge.Weight}",
                        FontSize = 10,
                        Foreground = Brushes.DarkSlateGray,
                        FontWeight = FontWeights.Bold,
                        Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                        Padding = new Thickness(2, 0, 2, 0)
                    };

                    Canvas.SetLeft(weightLabel, midX - 5);
                    Canvas.SetTop(weightLabel, midY - 5);

                    GraphCanvas.Children.Add(weightLabel);
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

            var allPaths = _graph.GetKShortestPaths(start, cil, 5);

            if (allPaths.Count == 0)
            {
                MessageBox.Show("Cesta neexistuje.");
                return;
            }

            for (int i = 0; i < allPaths.Count; i++)
            {
                double delka = VypocitejDelku(allPaths[i]);
                _foundPaths.Add(new PathUIViewModel
                {
                    Label = i == 0 ? $"Základní ({delka:F1} min)" : $"Alternativa {i} ({delka:F1} min)",
                    Nodes = allPaths[i],
                    Color = i == 0 ? Brushes.Red : Brushes.Blue,
                    Length = delka,
                    IsVisible = true
                });

                for (int j = 0; j < allPaths[i].Count - 1; j++)
                {
                    vektorNasledniku.AddSuccessor(allPaths[i][j], allPaths[i][j + 1]);
                }
            }

            LbPaths.ItemsSource = null;
            LbPaths.ItemsSource = _foundPaths.OrderBy(p => p.Length).ToList();

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
            if (_graph != null)
            {
                if (_graph.ProblematicEdges == null)
                    _graph.ProblematicEdges = new HashSet<(string, string)>();

                CbFrom.ItemsSource = _graph.Vertices.Values;
                CbTo.ItemsSource = _graph.Vertices.Values;

               CbFrom.SelectedIndex = 0;
                CbTo.SelectedIndex = 1;

                DrawGraph();
                TxtStatus.Text = "Mapa byla načtena a seznamy obcí byly aktualizovány.";
            }
        }
        private void BtnSetProblematic_Click(object sender, RoutedEventArgs e)
        {
            var uzelA = (CbFrom.SelectedItem as Vertex<string, Coordinates>)?.Id;
            var uzelB = (CbTo.SelectedItem as Vertex<string, Coordinates>)?.Id;

            if (uzelA != null && uzelB != null)
            {
                _graph.SetEdgeProblematic(uzelA, uzelB, true);

                DrawGraph();
            }
            else
            {
                MessageBox.Show("Vyberte prosím oba uzly (obce) pro úpravu komunikace.");
            }
        }

        private void BtnClearProblematic_Click(object sender, RoutedEventArgs e)
        {
            if (_graph == null) return;

            _graph.ProblematicEdges.Clear();

            DrawGraph();
        }
        private void BtnRemoveEdge_Click(object sender, RoutedEventArgs e)
        {
            var uzelA = (CbFrom.SelectedItem as Vertex<string, Coordinates>)?.Id;
            var uzelB = (CbTo.SelectedItem as Vertex<string, Coordinates>)?.Id;

            if (uzelA != null && uzelB != null)
            {
                _graph.RemoveEdge(uzelA, uzelB);

                TxtStatus.Text = $"Komunikace mezi {uzelA} a {uzelB} byla trvale odstraněna z modelu.";
                DrawGraph();
            }
        }
        private void BtnSearchNode_Click(object sender, RoutedEventArgs e)
        {
            string id = TxtNodeId.Text;
            var uzel = _graph.FindVertex(id);

            if (uzel != null)
            {
                CbFrom.SelectedItem = uzel;
                TxtStatus.Text = $"Uzel '{id}' nalezen na souřadnicích [{uzel.Data.X}, {uzel.Data.Y}].";
            }
            else MessageBox.Show("Uzel nebyl nalezen.");
        }
        private void BtnAddNode_Click(object sender, RoutedEventArgs e)
        {
            string id = TxtNodeId.Text;
            if (!string.IsNullOrEmpty(id) && double.TryParse(TxtNodeX.Text, out double x) && double.TryParse(TxtNodeY.Text, out double y))
            {
                _graph.AddVertex(id, new Coordinates { X = x, Y = y });
                RefreshUI();
                TxtStatus.Text = $"Uzel '{id}' byl vložen do sítě.";
            }
        }

        private void BtnAddEdge_Click(object sender, RoutedEventArgs e)
        {
            var uzelA = (CbFrom.SelectedItem as Vertex<string, Coordinates>)?.Id;
            var uzelB = (CbTo.SelectedItem as Vertex<string, Coordinates>)?.Id;

            if (uzelA == null || uzelB == null)
            {
                MessageBox.Show("Vyberte prosím v seznamech 'Z města' a 'Do města' obce, které chcete propojit.");
                return;
            }

            if (uzelA == uzelB)
            {
                MessageBox.Show("Nelze propojit obec samu se sebou.");
                return;
            }

            if (double.TryParse(TxtNewEdgeWeight.Text, out double vaha))
            {
                _graph.AddEdge(uzelA, uzelB, vaha);

                DrawGraph();
                TxtStatus.Text = $"Nová obousměrná komunikace mezi {uzelA} a {uzelB} (čas: {vaha} min) byla vložena.";
            }
            else
            {
                MessageBox.Show("Zadejte platnou číselnou hodnotu pro čas průjezdu.");
            }
        }

        private void RefreshUI()
        {
            CbFrom.ItemsSource = null;
            CbFrom.ItemsSource = _graph.Vertices.Values;
            CbTo.ItemsSource = null;
            CbTo.ItemsSource = _graph.Vertices.Values;
            DrawGraph();
        }

        private void BtnDeleteEdge_Click(object sender, RoutedEventArgs e)
        {
            if (_graph == null) return;

            var uzelA = (CbFrom.SelectedItem as Vertex<string, Coordinates>)?.Id;
            var uzelB = (CbTo.SelectedItem as Vertex<string, Coordinates>)?.Id;

            if (string.IsNullOrWhiteSpace(uzelA) || string.IsNullOrWhiteSpace(uzelB) || uzelA == uzelB)
            {
                MessageBox.Show("Vyberte dvě různé obce v sekci 'Správa komunikací' pro smazání jejich propojení.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _graph.RemoveEdge(uzelA, uzelB);

            TxtStatus.Text = $"Komunikace mezi {uzelA} a {uzelB} byla smazána.";
            DrawGraph();
        }

        private void PathVisibility_Changed(object sender, RoutedEventArgs e)
        {
            UpdateVisualization();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_graph == null)
            {
                MessageBox.Show("Není načten žádný graf k uložení.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON soubory (*.json)|*.json|Všechny soubory (*.*)|*.*";
            saveFileDialog.FileName = "upravena_mapa.json";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNameCaseInsensitive = true,
                        IncludeFields = true
                    };

                    string jsonString = JsonSerializer.Serialize(_graph, options);

                    File.WriteAllText(saveFileDialog.FileName, jsonString);

                    TxtStatus.Text = $"Model úspěšně uložen do: {System.IO.Path.GetFileName(saveFileDialog.FileName)}";
                    MessageBox.Show("Data byla úspěšně exportována.", "Export dokončen", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Chyba při ukládání souboru: " + ex.Message, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}