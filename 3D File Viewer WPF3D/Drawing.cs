using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WPF3DFileViewer
{
    public interface IBase
    {
        void ImportMarkups(List<MarkupInfoFromFile> imList);
    }

    public class Drawing
    {
        public virtual void DrawArrow(Point initial, Point final) { }
        public virtual void DrawSphere(Point initial, Point final) { }
        public virtual void DrawRectangle(Point p0, Point p1) { }
        public virtual void Draw3DComment(Point pos, string text, int height) { }
        public virtual void DrawText(Point pos, string text, int height) { }
        public virtual void DrawCube(Point p0, Point p1) { }
        public virtual void DrawFreeHandMarkup(PointCollection pts) { }
    }
}
