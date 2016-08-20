using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace WPF3DFileViewer
{
    public class Drawing3D : Drawing, IBase
    {
        IHelixViewport3D viewport;
        Vector3D inNorm;
        Vector3D finNorm;
        Rect3D rec;
        static List<MarkupInfoFromFile> list;
        private static int count;
        private static List<MarkupDetails> mainViewList;
        private static CameraInfo cam;
        private static bool getNewList;
        private static int tmpCount;
        private static int activeMarkupIndex = -1;

        public Drawing3D(IHelixViewport3D vp)
        {
            this.viewport = vp;
            getNewList = false;
        }

        public Drawing3D(IHelixViewport3D vp, Rect3D bbox)
        {
            this.viewport = vp;
            getNewList = false;
            this.rec = bbox;
        }

        public static void Init()
        {
            if(list == null)
                list = new List<MarkupInfoFromFile>();

            if (cam == null)
                cam = new CameraInfo();

            if (mainViewList == null)
                mainViewList = new List<MarkupDetails>();
        }

        public void SetStartVec(Vector3D v)
        {
            this.inNorm = v;
        }

        public static MarkupDetails GetMarkupsByIndex(int index)
        {
            if (mainViewList != null)
            {
                if (index >= 0 && index < mainViewList.Count)
                    return mainViewList[index];
            }
            return null;
        }

        public static int GetCounter()
        {
            return count;
        }

        public void SetEndVec(Vector3D v)
        {
            this.finNorm = v;
        }

        public void SetBBox(Rect3D r)
        {
            this.rec = r;
        }

        public static void ClearViewInfo()
        {
            if (list != null)
            {
                list.Clear();
                list = null;
            }

            if (mainViewList != null)
            {
                mainViewList.Clear();
                mainViewList = null;
            }
            cam = null;
            tmpCount = count = 0;
            activeMarkupIndex = -1;
        }

        public static bool ViewChanged()
        {
            return getNewList;
        }

        public static void SetCameraInfo(CameraInfo ca)
        {
            getNewList = !cam.Equals(ca);
            if (getNewList)
            {
                if (list.Count > 0)
                {
                    if (activeMarkupIndex >= 0 && mainViewList.Count > 0)
                    {
                        mainViewList[activeMarkupIndex].Merge(list);
                        activeMarkupIndex = -1;
                    }
                    else
                    {
                        mainViewList.Add(new MarkupDetails("Markup View " + tmpCount++, cam, list, MarkUpType.Shape3D));
                        count++;
                    }
                }
                cam = ca;
                list = new List<MarkupInfoFromFile>();
            }
        }

        public static void SetactiveMarkupListIndex(int val)
        {
            activeMarkupIndex = val;
        }

        public static void SetCount(int ct)
        {
            tmpCount = ct;
        }

        public static MarkupDetails GetMainMarkupList()
        {
            if (mainViewList.Count > 0)
                return mainViewList[count - 1];
            return null;
        }

        public static List<MarkupInfoFromFile> getMarkupList()
        {
            return list;
        }

        private void AddToMarkupList(MarkupInfoFromFile info)
        {
            Init();
            list.Add(info);
        }

        public static void ClearMarkupList()
        {
            if (list != null && list.Count > 0)
            {
                list.Clear();
                list = null;
            }
        }
       
        #region Markup Draw/Import

        public override void DrawText(Point pos, string text, int height)
        {
            var pt0 = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, pos);
            Point3D point1 = new Point3D();
            BillboardTextGroupVisual3D txtGroup = new BillboardTextGroupVisual3D();
            txtGroup.Background = System.Windows.Media.Brushes.White;
            txtGroup.Foreground = System.Windows.Media.Brushes.Black;
            txtGroup.BorderThickness = new Thickness(1);
            txtGroup.FontSize = 15;
            txtGroup.Padding = new Thickness(2);
            txtGroup.Offset = new Vector(50, 50);
            txtGroup.PinBrush = System.Windows.Media.Brushes.Gray;
            List<BillboardTextItem> txt = new List<BillboardTextItem>();
            BillboardTextItem itm = new BillboardTextItem();

            if (pt0.HasValue)
            {
                point1 = (Point3D)pt0;
                Point3D newpt = this.rec.Location;

                Rect3D r = Visual3DHelper.FindBounds(this.viewport.Viewport.Children);
                Ray3D ray = new Ray3D(this.viewport.Camera.Position, this.viewport.Camera.LookDirection);
                newpt = ray.GetNearest(point1);
                var intersectPt = ray.PlaneIntersection(r.Location, inNorm);
                Point3D newinPt = newpt;
                if (intersectPt.HasValue)
                    newinPt = (Point3D)intersectPt;

                //CameraHelper.ZoomToRectangle(this.viewport.Camera, viewport.Viewport, new Rect(new Size() { Height = this.viewport.Viewport.ActualHeight, Width = this.viewport.Viewport.ActualWidth }));
                double d = GetNearestFaceDistance(this.viewport.Camera.Position);
                double offset = this.viewport.Camera.LookDirection.Length - d;
                newinPt.Offset(offset * inNorm.X, offset * inNorm.Y, offset * inNorm.Z);
                point1.Offset(height * inNorm.X, height * inNorm.Y, height * inNorm.Z);
                itm.Position = point1;
                itm.Text = text;
                itm.DepthOffset = 1e-6;
                itm.WorldDepthOffset = 0.2;
                txt.Add(itm);
                txtGroup.Items = txt;
                this.viewport.Viewport.Children.Add(txtGroup);
                MarkupInfoFromFile m = new MarkupInfoFromFile();
                m.type = ShapeType.Text;
                m.height = height;
                m.width = 0;
                m.inNorm = inNorm;
                m.p0 = pos;
                m.p1 = pos;
                m.content = text;
                AddToMarkupList(m);
            }
        }

        public override void DrawRectangle(Point p0, Point p1)
        {
            RectangleVisual3D rec = new RectangleVisual3D();

            rec.Normal = inNorm;
            rec.LengthDirection = inNorm.FindAnyPerpendicular();
            double width = Math.Max(Math.Max(this.rec.SizeX, this.rec.SizeY), this.rec.SizeZ) * 0.05;
            //double len = 30;
            var pt0 = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, p0);
            Point3D point1 = new Point3D();

            var pt1 = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, p1);
            Point3D point2 = new Point3D();

            if (pt0.HasValue && pt1.HasValue)
            {
                point1 = (Point3D)pt0;
                point2 = (Point3D)pt1;
                Point3D center = new Point3D();
                center.X = (point1.X + point2.X) / 2;
                center.Y = (point1.Y + point2.Y) / 2;
                center.Z = (point1.Z + point2.Z) / 2;
                //point1.Offset(len * inNorm.X, len * inNorm.Y, len * inNorm.Z);
                center.Offset(width * inNorm.X, width * inNorm.Y, width * inNorm.Z);
                rec.Origin = center;
                rec.Width = width;
                rec.BackMaterial = rec.Material;
                rec.Normal = inNorm;
                rec.Length = width * 2;
                //rec.LengthDirection = n;
                this.viewport.Viewport.Children.Add(rec);
                MarkupInfoFromFile m = new MarkupInfoFromFile();
                m.type = ShapeType.Rect;
                m.height = rec.Length;
                m.width = rec.Width;
                m.inNorm = rec.Normal;
                m.p0 = p0;
                m.p1 = p1;
                AddToMarkupList(m);
            }
        }

        public override void DrawArrow(Point point1, Point point2)
        {
            ArrowVisual3D arr = new ArrowVisual3D();
            var first = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, point1);
            Point3D p0 = new Point3D();

            var second = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, point2);
            Point3D p1 = new Point3D();

            if (first.HasValue && second.HasValue)
            {
                p0 = (Point3D)first;
                p1 = (Point3D)second;

                //Matrix3D mat = this.viewport.Camera.GetTotalTransform(1.0);
                //p0 = mat.Transform(p0);

                //Transform3D t = this.viewport.Viewport.Camera.Transform;
                //t.TryTransform(p0, out p1);

                //this.viewport.Viewport.Camera.GetTotalTransform
                //Point3D pf = new Point3D();
                //Viewport3DHelper.Point2DtoPoint3D(viewport.Viewport, point1, out p0, out pf);
                //Viewport3DHelper.Point2DtoPoint3D(viewport.Viewport, point2, out p1, out pf);

                arr.Diameter = Math.Sqrt((p0.X - p1.X) * (p0.X - p1.X) + (p0.Y - p1.Y) * (p0.Y - p1.Y) + (p0.Z - p1.Z) * (p0.Z - p1.Z)) * 0.05;
                double dia = arr.Diameter;

                //if (p0.Z <= p1.Z)
                //{
                //    p0.Offset(-dia, -dia, -dia);
                //    p1.Offset(dia, dia, dia);
                //}

                p0.Offset(dia * inNorm.X, dia * inNorm.Y, dia * inNorm.Z);
                p1.Offset(dia * finNorm.X, dia * finNorm.Y, dia * finNorm.Z);

                arr.Point1 = p0;
                arr.Point2 = p1;

                //arr.Direction = p1.ToVector3D();
                this.viewport.Viewport.Children.Add(arr);

                MarkupInfoFromFile m = new MarkupInfoFromFile();

                m.type = ShapeType.Arrow;
                m.height = 0.0;
                m.width = 0.0;
                m.inNorm = inNorm;
                m.finNorm = finNorm;
                m.p0 = point1;
                m.p1 = point2;
                AddToMarkupList(m);
            }
        }

        public override void DrawCube(Point p0, Point p1)
        {
            CubeVisual3D cube = new CubeVisual3D();
            var pt = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, p0);
            Point3D point = new Point3D();

            if (pt.HasValue)
            {
                point = (Point3D)pt;
                double len = Math.Max(Math.Max(this.rec.SizeX, this.rec.SizeY), this.rec.SizeZ) * 0.03;
                cube.SideLength = len;
                double val = cube.SideLength * 0.5;
                point.Offset(val * inNorm.X, val * inNorm.Y, val * inNorm.Z);
                cube.Center = point;
                cube.BackMaterial = cube.Material;
                this.viewport.Viewport.Children.Add(cube);
                MarkupInfoFromFile m = new MarkupInfoFromFile();
                m.type = ShapeType.Cube;
                m.inNorm = inNorm;
                m.p0 = p0;
                m.p1 = p1;
                AddToMarkupList(m);
            }
        }

        public override void DrawSphere(Point p0, Point p1)
        {
            SphereVisual3D sp = new SphereVisual3D();
            var pt = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, p0);
            Point3D point = new Point3D();
            double rad = Math.Max(Math.Max(this.rec.SizeX, this.rec.SizeY), this.rec.SizeZ) * 0.03;

            if (pt.HasValue)
            {
                point = (Point3D)pt;
                point.Offset(rad * inNorm.X, rad * inNorm.Y, rad * inNorm.Z);
                sp.Center = point;
                sp.Radius = rad;
                sp.BackMaterial = sp.Material;
                this.viewport.Viewport.Children.Add(sp);
                MarkupInfoFromFile m = new MarkupInfoFromFile();
                m.type = ShapeType.Circle;
                m.height = 0.0;
                m.width = 0.0;
                m.inNorm = inNorm;
                m.p0 = p0;
                m.p1 = p1;
                AddToMarkupList(m);
            }
        }

        public override void Draw3DComment(Point pos, string text, int height)
        {
            TextVisual3D txt = new TextVisual3D();
            var pt0 = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, pos);
            Point3D point1 = new Point3D();
            double off = 5;
            height = (int)Math.Max(Math.Max(Math.Max(this.rec.SizeX, this.rec.SizeY), this.rec.SizeZ) * 0.2, 20);
            if (pt0.HasValue)
            {
                point1 = (Point3D)pt0;
                //point1.Offset(height * inNorm.X, height * inNorm.Y, height * inNorm.Z);
                point1.Offset(off * inNorm.X, off * inNorm.Y, off * inNorm.Z);
                txt.Position = point1;
                txt.Text = text;
                txt.Height = height;
                txt.TextDirection = inNorm.FindAnyPerpendicular();
                txt.UpDirection = inNorm;
                Vector3D v = Vector3D.CrossProduct(txt.TextDirection, txt.UpDirection);
                txt.UpDirection = -v;
                this.viewport.Viewport.Children.Add(txt);
                MarkupInfoFromFile m = new MarkupInfoFromFile();
                m.type = ShapeType.Text3D;
                m.height = txt.Height;
                m.width = 0;
                m.inNorm = inNorm;
                m.p0 = pos;
                m.p1 = pos;
                m.content = text;
                AddToMarkupList(m);
            }
        }

        public void ImportAnnotsFromFile(List<MarkupInfoFromFile> mInfos)
        {
            foreach (MarkupInfoFromFile m in mInfos)
            {
                SetStartVec(m.inNorm);
                SetEndVec(m.finNorm);

                switch (m.type)
                {
                    case ShapeType.Rect:
                        DrawRectangle(m.p0, m.p1);
                        break;

                    case ShapeType.Circle:
                        DrawSphere(m.p0, m.p1);
                        break;

                    case ShapeType.Arrow:
                        DrawArrow(m.p0, m.p1);
                        break;

                    case ShapeType.Cube:
                        DrawCube(m.p0, m.p1);
                        break;

                    case ShapeType.Text:
                        DrawText(m.p0, m.content, (int)m.height);
                        break;

                    case ShapeType.Text3D:
                        Draw3DComment(m.p0, m.content, (int)m.height);
                        break;
                }
            }
            ClearMarkupList();
        }

        #endregion

        private double GetNearestFaceDistance(Point3D fromPoint)
        {
            double dist = Int32.MinValue;
            Rect3D bounds = Visual3DHelper.FindBounds(this.viewport.Viewport.Children);
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X, bounds.Y, bounds.Z)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X, bounds.Y, bounds.Z + bounds.SizeZ)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X + bounds.SizeX, bounds.Y, bounds.Z + bounds.SizeZ)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X + bounds.SizeX, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ)));
            dist = Math.Min(dist, fromPoint.DistanceTo(new Point3D(bounds.X, bounds.Y + bounds.SizeY, bounds.Z + bounds.SizeZ)));
            return dist;
        }

        public void Dispose()
        {
            ClearViewInfo();
        }

        public void ImportMarkups(List<MarkupInfoFromFile> imList)
        {
            foreach (MarkupInfoFromFile m in imList)
            {
                SetStartVec(m.inNorm);
                SetEndVec(m.finNorm);

                switch (m.type)
                {
                    case ShapeType.Rect:
                        DrawRectangle(m.p0, m.p1);
                        break;

                    case ShapeType.Circle:
                        DrawSphere(m.p0, m.p1);
                        break;

                    case ShapeType.Arrow:
                        DrawArrow(m.p0, m.p1);
                        break;

                    case ShapeType.Cube:
                        DrawCube(m.p0, m.p1);
                        break;

                    case ShapeType.Text:
                        DrawText(m.p0, m.content, (int)m.height);
                        break;

                    case ShapeType.Text3D:
                        Draw3DComment(m.p0, m.content, (int)m.height);
                        break;
                }
            }
            ClearMarkupList();
        }
    }
}
