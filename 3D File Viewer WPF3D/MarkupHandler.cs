using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml;

namespace WPF3DFileViewer
{
    public enum ShapeType
    {
        None = -1,
        Rect,
        Circle,
        Arrow,
        Cube,
        Text,
        Text3D,
        Callout,
        Cloud
    }

    public enum Shapes3D
    {
        None,
        Arrow,
        Text2D,
        Text3D,
        Rectangle,
        Cube,
        Sphere,
        FreeHandMarkup
    }

    public enum MarkUpType
    {
        None,
        Shape2D,
        Shape3D
    }

    public enum ExporterType
    {
        None,
        Ex2D,
        Ex3D
    }

    public enum ImporterType
    {
        None,
        Imp2D,
        Imp3D
    }

    public class MarkupInfoFromFile
    {
        public ShapeType type { get; set; }
        public Point p0 { get; set; }
        public Point p1 { get; set; }
        public double width { get; set; }
        public double height { get; set; }
        public int strokeWidth { get; set; }
        public Vector3D inNorm { get; set; }
        public Vector3D finNorm { get; set; }
        public Point3D center { get; set; }
        public string content { get; set; }
    }


}
