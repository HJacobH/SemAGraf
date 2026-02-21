using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SemAGraf
{
    public class PathUIViewModel
    {
        public string Label { get; set; }
        public List<string> Nodes { get; set; }
        public bool IsVisible { get; set; } = true;
        public Brush Color { get; set; }
        public double Length { get; set; }
    }
}
