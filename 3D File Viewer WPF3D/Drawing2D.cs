using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WPF3DFileViewer
{
    public class Drawing2D : Drawing, IBase
    {
        private Canvas drawSurface;
        private static int zIndex;
        static List<MarkupInfoFromFile> list;
        private static List<MarkupDetails> mainViewList;
        private static CameraInfo cam;
        private static bool getNewList;
        private static int count;
        private static int tmpCount;
        private static int activeMarkupListIndex;
        private Path arc_path;
        private PathGeometry pathGeometry;
        public Drawing2D(Canvas can)
        {
            this.drawSurface = can;
            zIndex = 2;
            getNewList = false;
            this.arc_path = null;
            this.pathGeometry = null;
        }

        #region static members
        public static void Init()
        {
            if(list == null)
                list = new List<MarkupInfoFromFile>();

            if (cam == null)
                cam = new CameraInfo();

            if (mainViewList == null)
                mainViewList = new List<MarkupDetails>();
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
            activeMarkupListIndex = -1;
        }

        public static void SetCount(int ct)
        {
            tmpCount = ct;
        }

        public static MarkupDetails GetMarkupsByIndex(int index)
        {
            if (mainViewList != null)
            {
                if (index >= 0 && index < mainViewList.Count)
                {
                    //activeMarkupListIndex = index;
                    return mainViewList[index];
                }
            }
            //activeMarkupListIndex = -1;
            return null;
        }

        public static void SetactiveMarkupListIndex(int val)
        {
            activeMarkupListIndex = val;
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
                    if (activeMarkupListIndex >= 0 && mainViewList.Count > 0)
                    {
                        mainViewList[activeMarkupListIndex].Merge(list);
                        activeMarkupListIndex = -1;
                    }
                    else
                    {
                        mainViewList.Add(new MarkupDetails("Markup View " + tmpCount++, cam, list, MarkUpType.Shape2D));
                        count++;
                    }
                }
                cam = ca;
                list = new List<MarkupInfoFromFile>();
            }
        }

        public static int GetCounter()
        {
            return count;
        }

        public static MarkupDetails GetMainMarkupList()
        {
            if (mainViewList.Count > 0)
                return mainViewList[count-1];
            return null;
        }

        private static void AddToList(Point p0, Point p1, double w, double h, int strW, ShapeType t, string content)
        {
            MarkupInfoFromFile m = new MarkupInfoFromFile();
            m.p0 = p0;
            m.p1 = p1;
            m.type = t;
            m.height = h;
            m.width = w;
            m.strokeWidth = strW;
            m.content = content;
            list.Add(m);
        }     

        #endregion

        #region Markup Draw

        public override void DrawFreeHandMarkup(PointCollection arr)
        {
            //System.Windows.Shapes.Path freehandPath = new System.Windows.Shapes.Path();
            //freehandPath.Stroke = Brushes.Red;
            //freehandPath.StrokeThickness = 3;
            //PathFigure pathFigure = new PathFigure();
            //BezierSegment curve = new BezierSegment(this.PathPoints[count - 3], this.PathPoints[count - 2], this.PathPoints[count - 1], true);
            //pathFigure.Segments.Add(curve);
            //PathGeometry geo = new PathGeometry();

            //geo.AddGeometry(new Geometry());

            //freehandPath.Data = pathFigure;          
        }

        public override void DrawSphere(Point initial, Point final)
        {
            Ellipse e = new Ellipse();
            double d = Math.Max(Math.Abs(initial.X - final.X), Math.Abs(initial.Y - final.Y));
            e.Height = e.Width = d;
            e.Fill = Brushes.Black;
            e.StrokeThickness = 3;
            SetRect(initial, final, e);
            AddToList(initial, final, e.Height, e.Width, (int)e.StrokeThickness, ShapeType.Circle, "");
            //ArcSegment s = new ArcSegment(initial, new Size(10, 10), 180, false, SweepDirection.Clockwise, true);
            //Canvas c = new Canvas();
            //Canvas.SetZIndex(s, zIndex++);
            //Canvas.SetLeft(s, initial.X);
            //Canvas.SetTop(s, initial.Y);
        }

        public override void DrawRectangle(Point initial2DPoint, Point final2DPoint)
        {
            Rectangle rec = new Rectangle();
            Point pt = new Point();
            pt.X = Math.Min(initial2DPoint.X, final2DPoint.X);
            pt.Y = Math.Min(final2DPoint.Y, initial2DPoint.Y);
            rec.Height = Math.Abs(initial2DPoint.Y - final2DPoint.Y);
            rec.Width = Math.Abs(initial2DPoint.X - final2DPoint.X);
            rec.StrokeThickness = 4;
            rec.Stroke = Brushes.Red;
            SetRect(initial2DPoint, final2DPoint, rec);
            AddToList(initial2DPoint, final2DPoint, rec.Width, rec.Height, (int)rec.StrokeThickness, ShapeType.Rect, "");

            //Path p = new Path();
            //ArcSegment arc = new ArcSegment(initial2DPoint, new Size(20, 20), 180, false, SweepDirection.Clockwise, true);
            //List<PathSegment> segments = new List<PathSegment>();
            //segments.Add(arc);
            //PathFigure fig = new PathFigure(initial2DPoint, segments, false);
            //fig.Segments.Add(arc);
            //List<PathFigure> figsList = new List<PathFigure>();
            //figsList.Add(fig);
            //PathGeometry geom = new PathGeometry();
            //geom.Figures.Add(fig);
            //p.Data = geom;
            //p.Stroke = Brushes.Red;
            //p.StrokeThickness = 4;




            //SetRect(initial2DPoint, final2DPoint, p);
            //AddToList(initial2DPoint, final2DPoint, p.Width, p.Height, (int)p.StrokeThickness, ShapeType.Rect, "");
            //DrawArc(20, 0, 180, drawSurface, initial2DPoint, final2DPoint,1,1);
        }

        public void DrawArc(double radius, double start_angle, double end_angle, Canvas canvas, Point p0, Point p1, int xFlag, int yFlag)
        {
            Vector center = new Vector((p0.X + p1.X) / 2, (p0.Y + p1.Y) / 2);

            if (this.arc_path == null)
            {
                this.arc_path = new Path();
                arc_path.Stroke = Brushes.Red;
                arc_path.StrokeThickness = 3;
            }
            if (this.pathGeometry == null)
                this.pathGeometry = new PathGeometry();

            //if(pathFigure == null)
            //    pathFigure = new PathFigure();
            //Path arc_path = new Path();

            //Canvas c = new Canvas();
            //start_angle = ((start_angle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
            //end_angle = ((end_angle % (Math.PI * 2)) + Math.PI * 2) % (Math.PI * 2);
            start_angle = (Math.PI / 180) * start_angle;
            end_angle = (Math.PI / 180) * end_angle;
            if (end_angle < start_angle)
            {
                double temp_angle = end_angle;
                end_angle = start_angle;
                start_angle = temp_angle;
            }
            double angle_diff = end_angle - start_angle;
            //PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            ArcSegment arcSegment = new ArcSegment();
            arcSegment.IsLargeArc = angle_diff >= Math.PI;
            //Set start of arc
            
            //pathFigure.StartPoint = new Point(center.X + radius * Math.Cos(start_angle), center.Y + radius * Math.Sin(start_angle));
            //pathFigure.StartPoint = new Point(center.X + radius * xFlag, center.Y + radius * yFlag);
            pathFigure.StartPoint = p0;

            //pathFigure.StartPoint = pathFigure.StartPoint + newDir;
            //set end point of arc.
            //arcSegment.Point = new Point(center.X + radius * xFlag, center.Y + radius * -yFlag);
            arcSegment.Point = p1;

            radius *= 0.5;
            arcSegment.Size = new Size(radius, radius);
            arcSegment.SweepDirection = SweepDirection.Clockwise;
            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            //arc_path.Data = pathGeometry;
            //c.Children.Add(arc_path);
            //Canvas.SetZIndex(c, zIndex++);
            //canvas.Children.Add(c);
        }

        public override void DrawText(Point pos, string text, int height)
        {
            Label l = new Label();
            l.Content = text;
            l.Margin = new Thickness(pos.X, pos.Y, pos.X + 20, pos.Y + 10);
            Canvas.SetZIndex(l, zIndex++);
            this.drawSurface.Children.Add(l);
            AddToList(pos, pos, l.Width, l.Height, 0, ShapeType.Text, text);
        }

        public override void DrawArrow(Point p0, Point p1)
        {
            PointCollection col = CreateLineWithArrowPointCollection(p0, p1, 5);
            Polygon polygon = new Polygon();
            polygon.Points = col;
            polygon.Fill = Brushes.Red;
            polygon.StrokeThickness = 5;
            //polygon.Width = 5;
            Canvas c = new Canvas();
            //Canvas.SetLeft(c, pt.X);
            //Canvas.SetTop(c, pt.Y);
            c.Children.Add(polygon);
            Canvas.SetZIndex(c, zIndex++);
            this.drawSurface.Children.Add(c);
            AddToList(p0, p1, 0, 0, (int)polygon.StrokeThickness, ShapeType.Arrow, "");
        }

        public static Polygon DrawCallout(Point p0, Point p1, int flag, int rW, int rH, int lL)
        {
            Polygon polygon = new Polygon();
            polygon.Points = CreateCalloutGeometry(p0, p1, 3, rW, rH, lL, flag);
            polygon.Stroke = Brushes.Red;
            polygon.StrokeThickness = 1;
            Canvas.SetZIndex(polygon, zIndex++);
            if (flag == 1)
                AddToList(p0, p1, rW, rH, (int)polygon.StrokeThickness, ShapeType.Callout, "");
            return polygon;
        }

        public static Canvas DrawCalloutRectangle(Point initial2DPoint, Point final2DPoint)
        {
            Rectangle rect = new Rectangle();
            Point pt = new Point();
            pt.X = Math.Min(initial2DPoint.X, final2DPoint.X);
            pt.Y = Math.Min(final2DPoint.Y, initial2DPoint.Y);
            rect.Height = Math.Abs(initial2DPoint.Y - final2DPoint.Y);
            rect.Width = Math.Abs(initial2DPoint.X - final2DPoint.X);
            rect.StrokeThickness = 4;
            rect.Stroke = Brushes.Red;
            Canvas c = new Canvas();
            Canvas.SetLeft(c, pt.X);
            Canvas.SetTop(c, pt.Y);
            c.Children.Add(rect);
            Canvas.SetZIndex(c, zIndex++);
            return c;
        }

        public Path DrawCloud(Point initial2DPoint, Point final2DPoint, int flag)
        {
            Point topLeft = new Point(Math.Min(initial2DPoint.X, final2DPoint.X), Math.Min(initial2DPoint.Y, final2DPoint.Y));
            Point bottomRight = new Point(Math.Max(initial2DPoint.X, final2DPoint.X), Math.Max(initial2DPoint.Y, final2DPoint.Y));
            double height = Math.Abs(topLeft.Y - bottomRight.Y);
            double width = Math.Abs(topLeft.X - bottomRight.X);
            clearData();
            bool success = CreateRectCloud(topLeft, bottomRight, (int)width, (int)height);

            if (success)
            {
                Canvas.SetZIndex(arc_path, zIndex++);
                if (flag == 1)
                    AddToList(topLeft, bottomRight, width, height, 3, ShapeType.Cloud, "");
                return arc_path;
            }
            return null;
        }

        #endregion

        #region Draw Markups from xml/list

        public void DrawTextFromFile(MarkupInfoFromFile mList)
        {
            Label l = new Label();
            l.Content = mList.content;
            l.Margin = new Thickness(mList.p0.X, mList.p0.Y, mList.p0.X + 20, mList.p0.Y + 10);
            Canvas.SetZIndex(l, zIndex++);
            this.drawSurface.Children.Add(l);
        }

        public void DrawRectangleFromFile(MarkupInfoFromFile m)
        {
            Rectangle rec = new Rectangle();
            Point pt = new Point();
            pt.X = Math.Min(m.p0.X, m.p1.X);
            pt.Y = Math.Min(m.p0.Y, m.p1.Y);
            rec.Height = m.height;
            rec.Width = m.width;
            rec.StrokeThickness = m.strokeWidth;
            rec.Stroke = Brushes.Red;
            SetRect(m.p0, m.p1, rec);
        }

        public void DrawSphereFromFile(MarkupInfoFromFile m)
        {
            Ellipse e = new Ellipse();
            e.Height = m.height;
            e.Width = m.width;
            e.Fill = Brushes.Black;
            e.StrokeThickness = m.strokeWidth;
            SetRect(m.p0, m.p1, e);
        }

        public void DrawCalloutFromFile(MarkupInfoFromFile m)
        {
            PointCollection col = CreateCalloutGeometry(m.p0, m.p1, 3, (int)m.width, (int)m.height, 50, 1);
            //PointCollection col1 = CreateCalloutWithCloud(m.p0, m.p1, 3, (int)m.width, (int)m.height, 50);
            Polygon polygon = new Polygon();
            polygon.Points = col;
            //polygon.Fill = Brushes.Red;
            polygon.Stroke = Brushes.Red;
            polygon.StrokeThickness = m.strokeWidth;
            Canvas c = new Canvas();
            c.Children.Add(polygon);
            Canvas.SetZIndex(c, zIndex++);
            this.drawSurface.Children.Add(c);
        }

        public void DrawArrowFromFile(MarkupInfoFromFile m)
        {
            PointCollection col = CreateLineWithArrowPointCollection(m.p0, m.p1, m.strokeWidth);
            Polygon polygon = new Polygon();
            polygon.Points = col;
            polygon.Fill = Brushes.Red;
            polygon.StrokeThickness = m.strokeWidth;
            Canvas c = new Canvas();
            c.Children.Add(polygon);
            Canvas.SetZIndex(c, zIndex++);
            this.drawSurface.Children.Add(c);
        }

        public void ImportAnnotsFromList(List<MarkupInfoFromFile> mList)
        {
            foreach (MarkupInfoFromFile m in mList)
            {
                switch (m.type)
                {
                    case ShapeType.Arrow:
                        DrawArrowFromFile(m);
                        break;

                    case ShapeType.Rect:
                        DrawRectangleFromFile(m);
                        break;

                    case ShapeType.Circle:
                        DrawSphereFromFile(m);
                        break;

                    case ShapeType.Text:
                        DrawTextFromFile(m);
                        break;

                    case ShapeType.Callout:
                        DrawCalloutFromFile(m);
                        break;

                    case ShapeType.Cloud:
                        DrawCloudFromFile(m);
                        break;
                }
            }
        }

        private void DrawCloudFromFile(MarkupInfoFromFile m)
        {
            clearData();
            bool success = CreateRectCloud(m.p0, m.p1, (int)m.width, (int)m.height);

            if (success)
            {
                Canvas c = new Canvas();
                c.Children.Add(arc_path);
                Canvas.SetZIndex(c, zIndex++);
                drawSurface.Children.Add(c);
            }
        }

        #endregion

        #region Other Methods

        private const double _maxArrowLengthPercent = 0.3; // factor that determines how the arrow is shortened for very short lines
        private const double _lineArrowLengthFactor = 3.73205081; // 15 degrees arrow:  = 1 / Math.Tan(15 * Math.PI / 180); 
        public static PointCollection CreateLineWithArrowPointCollection(Point startPoint, Point endPoint, double lineWidth)
        {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * lineWidth * 0.5;
            double lineLength = direction.Length;

            double defaultArrowLength = lineWidth * _lineArrowLengthFactor;

            // Prepare usedArrowLength
            // if the length is bigger than 1/3 (_maxArrowLengthPercent) of the line length adjust the arrow length to 1/3 of line length

            double usedArrowLength;
            if (lineLength * _maxArrowLengthPercent < defaultArrowLength)
                usedArrowLength = lineLength * _maxArrowLengthPercent;
            else
                usedArrowLength = defaultArrowLength;

            // Adjust arrow thickness for very thick lines
            double arrowWidthFactor;
            if (lineWidth <= 1.5)
                arrowWidthFactor = 3;
            else if (lineWidth <= 2.66)
                arrowWidthFactor = 4;
            else
                arrowWidthFactor = 1.5 * lineWidth;

            Vector arrowWidthVector = normalizedlineWidenVector * arrowWidthFactor;


            // Now we have all the vectors so we can create the arrow shape positions
            var pointCollection = new PointCollection(7);

            Point endArrowCenterPosition = endPoint - (normalizedDirection * usedArrowLength);

            pointCollection.Add(endPoint); // Start with tip of the arrow
            pointCollection.Add(endArrowCenterPosition + arrowWidthVector);
            pointCollection.Add(endArrowCenterPosition + lineWidenVector);
            pointCollection.Add(startPoint + lineWidenVector);
            pointCollection.Add(startPoint - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - arrowWidthVector);
            return pointCollection;
        }

        private static PointCollection CreateCalloutGeometry(Point startPoint, Point endPoint, double lineWidth, int recWidth, int recHgt, int calloutLineLen, int flag)
        {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * lineWidth * 0.5;
            double lineLength = direction.Length;

            double defaultArrowLength = lineWidth * _lineArrowLengthFactor;

            // Prepare usedArrowLength
            // if the length is bigger than 1/3 (_maxArrowLengthPercent) of the line length adjust the arrow length to 1/3 of line length

            double usedArrowLength;
            if (lineLength * _maxArrowLengthPercent < defaultArrowLength)
                usedArrowLength = lineLength * _maxArrowLengthPercent;
            else
                usedArrowLength = defaultArrowLength;

            // Adjust arrow thickness for very thick lines
            double arrowWidthFactor;
            if (lineWidth <= 1.5)
                arrowWidthFactor = 3;
            else if (lineWidth <= 2.66)
                arrowWidthFactor = 4;
            else
                arrowWidthFactor = 1.5 * lineWidth;

            Vector arrowWidthVector = normalizedlineWidenVector * arrowWidthFactor;


            // Now we have all the vectors so we can create the arrow shape positions
            var pointCollection = new PointCollection(15);
            endPoint.X += 10;
            Point endArrowCenterPosition = endPoint - (normalizedDirection * usedArrowLength);
            
            Point pt0 = new Point();
            Point pt1 = new Point();
            Point newPt = startPoint + lineWidenVector;
            pt1.X = newPt.X + calloutLineLen;
            pt1.Y = newPt.Y;
            pt0.X = pt1.X;
            newPt = startPoint - lineWidenVector;
            pt0.Y = newPt.Y;
            Point pNext = new Point();
            pNext = pt1;
            pNext.Y -= recHgt * 0.5;

            pointCollection.Add(endPoint); // Start with tip of the arrow
            pointCollection.Add(endArrowCenterPosition + arrowWidthVector);
            pointCollection.Add(endArrowCenterPosition + lineWidenVector);
            pointCollection.Add(startPoint + lineWidenVector);
            pointCollection.Add(pt1);

            if (flag == 0)
                pointCollection.Add(pt0);
            else if (flag == 1)
            {
                pointCollection.Add(pNext);
                pNext.X += recWidth;
                pointCollection.Add(pNext);
                pNext.Y += recHgt ;
                pointCollection.Add(pNext);
                pNext.X = pt1.X;
                pointCollection.Add(pNext);
                pointCollection.Add(pt0);
            }
            pointCollection.Add(startPoint - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - arrowWidthVector);
            return pointCollection;
        }

        private void assignFlags(ref int flag1, ref int flag2, Point p0, Point p1)
        {
            if (p0.X <= p1.X && p0.Y == p1.Y)
            {
                flag1 = 1;
                flag2 = -flag2;
            }
            else if (p0.X >= p1.X && p0.Y == p1.Y)
            {
                flag1 = -1;
                flag2 = -flag2;
            }
            else if (p0.Y <= p1.Y && p0.X == p1.X)
            {
                flag1 = -flag1;
                flag2 = 1;
            }
            else if (p0.Y >= p1.Y && p0.X == p1.X)
            {
                flag1 = -flag1;
                flag2 = -1;
            }
        } //obsolete

        private void assignFlagValues(ref int flag1, ref int flag2, Point p0, Point p1)
        {
            if (p0.X <= p1.X && p0.Y == p1.Y)
            {
                flag1 = 1;
                flag2 = 0;
            }
            else if (p0.X >= p1.X && p0.Y == p1.Y)
            {
                flag1 = -1;
                flag2 = 0;
            }
            else if (p0.Y <= p1.Y && p0.X == p1.X)
            {
                flag1 = 0;
                flag2 = 1;
            }
            else if (p0.Y >= p1.Y && p0.X == p1.X)
            {
                flag1 = 0;
                flag2 = -1;
            }
        }

        private void updateEndPoint(ref Point pEnd, int iteration, int recHgt, int recWidth, Point pt0, Point pt1)
        {
            switch (iteration)
            {
                case 0:
                    pEnd.X = pt1.X + recWidth + 5;
                    break;

                case 1:
                    pEnd.Y += recHgt;
                    break;

                case 2:
                    pEnd.X = pt1.X;
                    break;

                case 3:
                    pEnd = pt0;
                    break;
            }
        }

        private  PointCollection CreateCalloutWithCloud(Point startPoint, Point endPoint, double lineWidth, int recWidth, int recHgt, int calloutLineLen)
        {
            Vector direction = endPoint - startPoint;

            Vector normalizedDirection = direction;
            normalizedDirection.Normalize();

            Vector normalizedlineWidenVector = new Vector(-normalizedDirection.Y, normalizedDirection.X); // Rotate by 90 degrees
            Vector lineWidenVector = normalizedlineWidenVector * lineWidth * 0.5;
            double lineLength = direction.Length;

            double defaultArrowLength = lineWidth * _lineArrowLengthFactor;

            double usedArrowLength;
            if (lineLength * _maxArrowLengthPercent < defaultArrowLength)
                usedArrowLength = lineLength * _maxArrowLengthPercent;
            else
                usedArrowLength = defaultArrowLength;

            double arrowWidthFactor;
            if (lineWidth <= 1.5)
                arrowWidthFactor = 3;
            else if (lineWidth <= 2.66)
                arrowWidthFactor = 4;
            else
                arrowWidthFactor = 1.5 * lineWidth;

            Vector arrowWidthVector = normalizedlineWidenVector * arrowWidthFactor;

            var pointCollection = new PointCollection(15);

            Point endArrowCenterPosition = endPoint - (normalizedDirection * usedArrowLength);

            Point pt0 = new Point();
            Point pt1 = new Point();
            Point newPt = startPoint + lineWidenVector;
            pt1.X = newPt.X + calloutLineLen;
            pt1.Y = newPt.Y;
            pt0.X = pt1.X;
            newPt = startPoint - lineWidenVector;
            pt0.Y = newPt.Y;
            Point pNext = new Point();
            pNext = pt1;
            pNext.Y -= recHgt * 0.5 - 5;

            pointCollection.Add(endPoint); 
            pointCollection.Add(endArrowCenterPosition + arrowWidthVector);
            pointCollection.Add(endArrowCenterPosition + lineWidenVector);
            pointCollection.Add(startPoint + lineWidenVector);
            pointCollection.Add(pt1);

            int max = 5;
           
            int yFlag = 1;
            int xFlag = 1;
            double segLenFactor = 20;
            double multiplier = segLenFactor * 0.5;
            Point pStart = pt1;
            //Point pStart = pt1 + new Vector(multiplier, 0);
            Point pEnd = pNext;
            //pEnd.X = pStart.X;
            Point p0 = pStart;
            //Point p1 = pt0 + new Vector(multiplier, 0);
            Point p1 = pEnd;
            for (int iteration = 0; iteration < max; ++iteration)
            {
                double len = Math.Max(Math.Abs(pEnd.X - pStart.X), Math.Abs(pEnd.Y - pStart.Y));
                //double segLen = Math.Round(val, MidpointRounding.AwayFromZero);
                //Vector dir = pEnd - pStart;
                //dir.Normalize();
                //dir *= 0.1 * segLen;
                int pattCount = 0;
                Point ps = pStart;
                Point pe = new Point();
                xFlag = yFlag = 1;
                //  pointCollection.Add(pStart);
                if (len > 0)
                {
                    double v = Math.Round(len/segLenFactor);
                    pattCount = (int)v;
                    
                    assignFlagValues(ref xFlag, ref yFlag, pStart, pEnd);
                    for (int i = 0; i < 2*pattCount; ++i)
                    {
                        //assignFlags(ref xFlag, ref yFlag, pStart, pEnd);

                        pe.X = ps.X + xFlag * multiplier;
                        pe.Y = ps.Y + yFlag * multiplier;

                        //DrawAnyOneOf(ps, pEnd, pointCollection);
                        //Check(ref pEnd, pe,iteration);
                        DrawArc(multiplier, 0, 180, this.drawSurface, ps, pe,xFlag,yFlag);
                        //pointCollection.Add(pe);
                        ps = pe;
                        //if (pEnd.Equals(pe))
                        //    break;
                    }
                    DrawArc(multiplier * 0.2, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                    //double remainingLen = Math.Max(Math.Abs(ps.X - pEnd.X), Math.Abs(ps.Y - pEnd.Y)); 
                    //if(remainingLen < multiplier*0.5)
                    //    DrawArc(multiplier*0.5, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                    //else
                    //    DrawArc(multiplier, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                }
                pStart = pEnd;
                //pointCollection.Add(pEnd);
                updateEndPoint(ref pEnd, iteration, recHgt, recWidth, pt0, pt1);
            }

            pointCollection.Add(pt0);
            pointCollection.Add(startPoint - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - lineWidenVector);
            pointCollection.Add(endArrowCenterPosition - arrowWidthVector);
            return pointCollection;
        } //obsolete

        private void GetNewValue(ref Point end, int iteration, double delta)
        {
            switch (iteration)
            {
                case 1:
                    end.X += delta;
                break;

                case 2:
                    end.Y += delta;
                break;

                case 3:
                    end.X -= delta;
                break;

                case 4:
                    end.Y -= delta;
                break;
            }
        }

        private bool CreateRectCloud(Point startPoint, Point endPoint, int recWidth, int recHgt)
        {
            var pointCollection = new PointCollection(15);
            Point pt0 = startPoint;
            Point pt1 = startPoint;
            Point pNext = startPoint;
            pNext.X += recWidth + 5;
            int max = 5;
            int yFlag = 1;
            int xFlag = 1;
            int segLenFactor = 30;
            double multiplier = segLenFactor * 0.5;
            Point pStart = startPoint;
            Point pEnd = pNext;
           
            //int drawRoundCorner = 0;

            for (int iteration = 1; iteration < max; ++iteration)
            {
                double len = Math.Max(Math.Abs(pEnd.X - pStart.X), Math.Abs(pEnd.Y - pStart.Y));

                if (len % segLenFactor != 0)
                {
                    double leftFraction = segLenFactor - len % segLenFactor;
                    GetNewValue(ref pEnd, iteration, leftFraction);
                    len += leftFraction;
                }

                int pattCount = 0;
                Point ps = pStart;
                Point pe = new Point();
                xFlag = yFlag = 1;

                if (len > 0)
                {
                    //double v = Math.Round(len / segLenFactor);
                    double v = Math.Truncate(len / segLenFactor);
                   
                    //GetNewValue(ref pEnd, iteration, leftFraction);
                    pattCount = (int)v;
                    assignFlagValues(ref xFlag, ref yFlag, pStart, pEnd);
                    for (int i = 0; i < 2 * pattCount; ++i)
                    {
                        pe.X = ps.X + xFlag * multiplier;
                        pe.Y = ps.Y + yFlag * multiplier;
                        DrawArc(multiplier, 0, 180, this.drawSurface, ps, pe, xFlag, yFlag);
                        ps = pe;
                        //if (pEnd.Equals(pe))
                        //    break;

                    }
                    //DrawArc(multiplier * 0.1, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                    double remainingLen = Math.Max(Math.Abs(ps.X - pEnd.X), Math.Abs(ps.Y - pEnd.Y));
                    //DrawArc(Math.Abs(multiplier - remainingLen), 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                    //GetNewValue(ref pEnd, iteration, segLenFactor - len % segLenFactor);
                    //DrawArc(multiplier, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                    // drawRoundCorner = 0;
                    if (remainingLen < multiplier * 0.5)
                    {
                        //double factor = remainingLen / multiplier;
                        //DrawArc(multiplier * factor, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                        //drawRoundCorner = 1;
                    }
                    else
                    {
                        //DrawArc(multiplier, 0, 180, this.drawSurface, ps, pEnd, xFlag, yFlag);
                    }
                }
                //if (drawRoundCorner == 0)
                pStart = pEnd;
                //else
                {
                    //pStart = pe;
                }
                updateEndPoint(ref pEnd, iteration, recHgt, recWidth, pt0, pt1);
            }

            bool result = this.arc_path != null && this.pathGeometry != null;
            if (result)
            {
                this.arc_path.Data = this.pathGeometry;
                return true;
            }
            return false;
        }

        private void Clear()
        {
            zIndex = 2;
        }

        private void clearData()
        {
            if (this.arc_path != null)
                this.arc_path = null;

            if (this.pathGeometry != null)
                this.pathGeometry = null;
        }

        private void SetRect(Point initial, Point final, Shape s)
        {
            Canvas c = new Canvas();
            Point pt = new Point();
            pt.X = Math.Min(initial.X, final.X);
            pt.Y = Math.Min(final.Y, initial.Y);
            Canvas.SetLeft(c, pt.X);
            Canvas.SetTop(c, pt.Y);
            pt.X = Math.Max(initial.X, final.X);
            pt.Y = Math.Max(final.Y, initial.Y);
            Canvas.SetRight(c, pt.X);
            Canvas.SetBottom(c, pt.Y);
            c.Children.Add(s);
            Canvas.SetZIndex(c, zIndex++);
            this.drawSurface.Children.Add(c);
        }

        #endregion

        public void ImportMarkups(List<MarkupInfoFromFile> imList)
        {
            foreach (MarkupInfoFromFile m in imList)
            {
                switch (m.type)
                {
                    case ShapeType.Arrow:
                        DrawArrowFromFile(m);
                        break;

                    case ShapeType.Rect:
                        DrawRectangleFromFile(m);
                        break;

                    case ShapeType.Circle:
                        DrawSphereFromFile(m);
                        break;

                    case ShapeType.Text:
                        DrawTextFromFile(m);
                        break;

                    case ShapeType.Callout:
                        DrawCalloutFromFile(m);
                        break;

                    case ShapeType.Cloud:
                        DrawCloudFromFile(m);
                        break;
                }
            }
        }
    }
}
