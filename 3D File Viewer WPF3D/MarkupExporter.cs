using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF3DFileViewer
{
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
        public class MarkupExporter
        {
            private ExporterType exporter;
            private static MarkupExporter mExporter;
            private MarkupExporter()
            {
                
            }

            public static MarkupExporter GetOrCreateMarkupExporter()
            {
                if (mExporter == null)
                    mExporter = new MarkupExporter();
                return mExporter;
            }

            public void SetExporter(ExporterType t)
            {
                this.exporter = t;
            }

            public bool ExportMarkup(string fileName, List<MarkupDetails> mList, IHelixViewport3D vPort)
            {
                if (this.exporter == ExporterType.Ex2D)
                {
                    return Export2D(fileName, mList, vPort);
                }
                else if(this.exporter == ExporterType.Ex3D)
                {
                    return Export3D(fileName, mList, vPort);
                }
                else
                   return false;
            }

            private bool Export3D(string fileName, List<MarkupDetails> mrkpList, IHelixViewport3D vPort)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement(string.Empty, "Annotaions3D", string.Empty);
                root.AppendChild(ExportCamera(doc, vPort));

                try
                {
                    foreach (MarkupDetails d in mrkpList)
                    {
                        List<MarkupInfoFromFile> mList = d.GetMarkupList();

                        foreach (MarkupInfoFromFile m in mList)
                        {
                            switch (m.type)
                            {
                                case ShapeType.Rect:
                                    XmlElement rect = doc.CreateElement("Rectangle");
                                    GetElementVals(doc, m, rect);
                                    root.AppendChild(rect);
                                    break;

                                case ShapeType.Circle:
                                    XmlElement circle = doc.CreateElement("Circle");
                                    GetElementVals(doc, m, circle);
                                    root.AppendChild(circle);
                                    break;

                                case ShapeType.Arrow:
                                    XmlElement arr = doc.CreateElement("Arrow");
                                    GetElementVals(doc, m, arr);
                                    root.AppendChild(arr);
                                    break;

                                case ShapeType.Cube:
                                    XmlElement cube = doc.CreateElement("Cube");
                                    GetElementVals(doc, m, cube);
                                    root.AppendChild(cube);
                                    break;

                                case ShapeType.Text3D:
                                    XmlElement text3d = doc.CreateElement("Text3D");
                                    GetElementVals(doc, m, text3d);
                                    SetContent(doc, text3d, m.content);
                                    root.AppendChild(text3d);
                                    break;
                            }
                        }

                        doc.AppendChild(root);
                        doc.Save(fileName);
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    if (doc != null)
                        doc = null;
                }
                return File.Exists(fileName);
            }

            private bool Export2D(string fileName, List<MarkupDetails> dls, IHelixViewport3D vport)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement(string.Empty, "Annotaions2D", string.Empty);
                root.AppendChild(ExportCamera(doc, vport));

                try
                {
                    foreach (MarkupDetails d in dls)
                    {
                        List<MarkupInfoFromFile> mInfos = d.GetMarkupList();

                        for (int i = 0; i < mInfos.Count; ++i)
                        {
                            MarkupInfoFromFile m = mInfos[i];

                            switch (m.type)
                            {
                                case ShapeType.Rect:
                                    XmlElement eRect = doc.CreateElement(string.Empty, "Rectangle", string.Empty);
                                    GetHeightAndWidth(doc, m.width, m.height, eRect);
                                    XmlElement ele = GetRectInfo(doc, m.p0, m.p1);
                                    eRect.AppendChild(ele);
                                    XmlElement s = GetStrokeInfo(doc, m.strokeWidth);
                                    eRect.AppendChild(s);
                                    root.AppendChild(eRect);
                                    break;

                                case ShapeType.Circle:
                                    XmlElement eCircle = doc.CreateElement(string.Empty, "Circle", string.Empty);
                                    GetHeightAndWidth(doc, m.width, m.height, eCircle);

                                    XmlElement e2 = GetRectInfo(doc, m.p0, m.p1);
                                    eCircle.AppendChild(e2);

                                    XmlElement strk = GetStrokeInfo(doc, m.strokeWidth);
                                    eCircle.AppendChild(strk);
                                    root.AppendChild(eCircle);
                                    break;

                                case ShapeType.Arrow:
                                    XmlElement eArrow = doc.CreateElement(string.Empty, "Arrow", string.Empty);
                                    List<Point> pList = new List<Point>();
                                    pList.Add(m.p0);
                                    pList.Add(m.p1);
                                    GetArrowPoints(doc, eArrow, pList);
                                    XmlElement str = GetStrokeInfo(doc, m.strokeWidth);
                                    eArrow.AppendChild(str);
                                    root.AppendChild(eArrow);
                                    break;

                                case ShapeType.Text:
                                    XmlElement eText = doc.CreateElement(string.Empty, "Text", string.Empty);
                                    XmlElement recInfo = GetRectInfo(doc, m.p0, m.p1);
                                    eText.AppendChild(recInfo);
                                    XmlElement strInfo = GetStrokeInfo(doc, m.strokeWidth);
                                    eText.AppendChild(strInfo);
                                    SetContent(doc, eText, m.content);
                                    root.AppendChild(eText);
                                    break;

                                case ShapeType.Callout:
                                    XmlElement eCallout = doc.CreateElement(string.Empty, "Callout", string.Empty);
                                    GetHeightAndWidth(doc, m.width, m.height, eCallout);
                                    XmlElement recInf = GetRectInfo(doc, m.p0, m.p1);
                                    eCallout.AppendChild(recInf);
                                    XmlElement strkInfo = GetStrokeInfo(doc, m.strokeWidth);
                                    eCallout.AppendChild(strkInfo);
                                    SetContent(doc, eCallout, m.content);
                                    root.AppendChild(eCallout);
                                    break;

                                case ShapeType.Cloud:
                                    XmlElement eCloud = doc.CreateElement(string.Empty, "Cloud", string.Empty);
                                    GetHeightAndWidth(doc, m.width, m.height, eCloud);
                                    XmlElement eCloudRecInf = GetRectInfo(doc, m.p0, m.p1);
                                    eCloud.AppendChild(eCloudRecInf);
                                    XmlElement eCloudStrkInfo = GetStrokeInfo(doc, m.strokeWidth);
                                    eCloud.AppendChild(eCloudStrkInfo);
                                    root.AppendChild(eCloud);
                                    break;
                            }
                        }
                    }

                    if (root != null)
                    {
                        doc.AppendChild(root);
                        doc.Save(fileName);
                    }
                }
                catch
                {
                    return false;
                }
                finally
                {
                    if (doc != null)
                        doc = null;
                }
                return File.Exists(fileName);
            }

            private void SetContent(XmlDocument doc, XmlElement e, string content)
            {
                XmlElement name = doc.CreateElement("Content");
                XmlText t = doc.CreateTextNode(content);
                name.AppendChild(t);
                e.AppendChild(name);
            }

            private bool Export3D(string fileName, List<MarkupInfoFromFile> mrkpList, IHelixViewport3D vport)
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement(string.Empty, "Annotaions3D", string.Empty);
                root.AppendChild(ExportCamera(doc, vport));

                try
                {
                    foreach (MarkupInfoFromFile m in mrkpList)
                    {
                        switch (m.type)
                        {
                            case ShapeType.Rect:
                                XmlElement rect = doc.CreateElement("Rectangle");
                                GetElementVals(doc, m, rect);
                                root.AppendChild(rect);
                                break;

                            case ShapeType.Circle:
                                XmlElement circle = doc.CreateElement("Circle");
                                GetElementVals(doc, m, circle);
                                root.AppendChild(circle);
                                break;

                            case ShapeType.Arrow:
                                XmlElement arr = doc.CreateElement("Arrow");
                                GetElementVals(doc, m, arr);
                                root.AppendChild(arr);
                                break;

                            case ShapeType.Cube:
                                XmlElement cube = doc.CreateElement("Cube");
                                GetElementVals(doc, m, cube);
                                root.AppendChild(cube);
                                break;
                        }
                    }

                    doc.AppendChild(root);
                    doc.Save(fileName);
                }
                catch
                {
                    return false;
                }
                finally
                {
                    if (doc != null)
                        doc = null;
                }
                return File.Exists(fileName);
            }

            private void GetElementVals(XmlDocument doc, MarkupInfoFromFile m, XmlElement root)
            {
                GetHeightAndWidth(doc, m.width, m.height, root);
                XmlElement rect = GetRectInfo(doc, m.p0, m.p1);
                root.AppendChild(rect);
                XmlElement stroke = GetStrokeInfo(doc, m.strokeWidth);
                root.AppendChild(stroke);
                GetNormal(doc, m, root, "InitialNormal", m.inNorm);
                GetNormal(doc, m, root, "FinalNormal", m.finNorm);
            }

            private void GetNormal(XmlDocument doc, MarkupInfoFromFile m, XmlElement root, string name, Vector3D normal)
            {
                XmlElement n = doc.CreateElement(name);
                XmlElement x = doc.CreateElement("X");
                XmlElement y = doc.CreateElement("Y");
                XmlElement z = doc.CreateElement("Z");
                n.AppendChild(x);
                n.AppendChild(y);
                n.AppendChild(z);
                XmlText t1 = doc.CreateTextNode(normal.X.ToString());
                XmlText t2 = doc.CreateTextNode(normal.Y.ToString());
                XmlText t3 = doc.CreateTextNode(normal.Z.ToString());
                x.AppendChild(t1);
                y.AppendChild(t2);
                z.AppendChild(t3);
                root.AppendChild(n);
            }

            private void GetArrowPoints(XmlDocument doc, XmlElement root, List<Point> pts)
            {
                XmlElement e0 = doc.CreateElement("Start");
                XmlElement e1 = doc.CreateElement("End");
                XmlElement l = doc.CreateElement("X0");
                XmlElement t = doc.CreateElement("Y0");
                XmlElement r = doc.CreateElement("X1");
                XmlElement b = doc.CreateElement("Y1");
                XmlText s0 = doc.CreateTextNode(pts[0].X.ToString());
                l.AppendChild(s0);
                XmlText s1 = doc.CreateTextNode(pts[0].Y.ToString());
                t.AppendChild(s1);
                XmlText end0 = doc.CreateTextNode(pts[1].X.ToString());
                r.AppendChild(end0);
                XmlText end1 = doc.CreateTextNode(pts[1].Y.ToString());
                b.AppendChild(end1);
                root.AppendChild(e0);
                e0.AppendChild(l);
                e0.AppendChild(t);
                root.AppendChild(e1);
                e1.AppendChild(r);
                e1.AppendChild(b);
            }

            private XmlElement GetRectInfo(XmlDocument doc, Point p0, Point p1)
            {
                XmlElement corners = doc.CreateElement("Corners");
                XmlElement l = doc.CreateElement("X0");
                XmlElement t = doc.CreateElement("Y0");
                XmlElement r = doc.CreateElement("X1");
                XmlElement b = doc.CreateElement("Y1");
                corners.AppendChild(l);
                corners.AppendChild(t);
                corners.AppendChild(r);
                corners.AppendChild(b);
                XmlText left = doc.CreateTextNode(p0.X.ToString());
                XmlText top = doc.CreateTextNode(p0.Y.ToString());
                XmlText right = doc.CreateTextNode(p1.X.ToString());
                XmlText bottom = doc.CreateTextNode(p1.Y.ToString());
                l.AppendChild(left);
                t.AppendChild(top);
                r.AppendChild(right);
                b.AppendChild(bottom);
                return corners;
            }

            private void GetHeightAndWidth(XmlDocument doc, double wth, double ht, XmlElement root)
            {
                XmlElement w = doc.CreateElement("Width");
                XmlElement h = doc.CreateElement("Height");
                XmlText width = doc.CreateTextNode(wth.ToString());
                XmlText height = doc.CreateTextNode(ht.ToString());
                w.AppendChild(width);
                h.AppendChild(height);
                root.AppendChild(w);
                root.AppendChild(h);
            }

            private XmlElement ExportCamera(XmlDocument doc, IHelixViewport3D v)
            {
                XmlElement cam = doc.CreateElement(string.Empty, "Camera", string.Empty);
                XmlElement camPos = GetCameraElement(v.Camera, doc, "Position");
                cam.AppendChild(camPos);

                XmlElement camLookDir = GetCameraElement(v.Camera, doc, v.Camera.LookDirection, "LookDirection");
                cam.AppendChild(camLookDir);

                XmlElement camUpDir = GetCameraElement(v.Camera, doc, v.Camera.UpDirection, "UpDirection");
                cam.AppendChild(camUpDir);
                return cam;
            }

            private XmlElement GetCameraElement(ProjectionCamera cam, XmlDocument doc, string name)
            {
                XmlElement element = doc.CreateElement(string.Empty, name, string.Empty);
                XmlElement camX = doc.CreateElement(string.Empty, "X", string.Empty);
                XmlText camPosX = doc.CreateTextNode(cam.Position.X.ToString());
                camX.AppendChild(camPosX);
                XmlElement camY = doc.CreateElement(string.Empty, "Y", string.Empty);
                XmlText camPosY = doc.CreateTextNode(cam.Position.Y.ToString());
                camY.AppendChild(camPosY);
                XmlElement camZ = doc.CreateElement(string.Empty, "Z", string.Empty);
                XmlText camPosZ = doc.CreateTextNode(cam.Position.Z.ToString());
                camZ.AppendChild(camPosZ);
                element.AppendChild(camX);
                element.AppendChild(camY);
                element.AppendChild(camZ);
                return element;
            }

            private XmlElement GetCameraElement(ProjectionCamera cam, XmlDocument doc, Vector3D val, string name)
            {
                XmlElement element = doc.CreateElement(string.Empty, name, string.Empty);
                XmlElement camX = doc.CreateElement(string.Empty, "X", string.Empty);
                XmlText camPosX = doc.CreateTextNode(val.X.ToString());
                camX.AppendChild(camPosX);
                XmlElement camY = doc.CreateElement(string.Empty, "Y", string.Empty);
                XmlText camPosY = doc.CreateTextNode(val.Y.ToString());
                camY.AppendChild(camPosY);
                XmlElement camZ = doc.CreateElement(string.Empty, "Z", string.Empty);
                XmlText camPosZ = doc.CreateTextNode(val.Z.ToString());
                camZ.AppendChild(camPosZ);
                element.AppendChild(camX);
                element.AppendChild(camY);
                element.AppendChild(camZ);
                return element;
            }

            private XmlElement GetStrokeInfo(XmlDocument doc, int thickness)
            {
                XmlElement stroke = doc.CreateElement("Stroke");
                XmlElement strW = doc.CreateElement("Width");
                XmlElement strC = doc.CreateElement("Color");
                stroke.AppendChild(strW);
                XmlText stext = doc.CreateTextNode(thickness.ToString());
                strW.AppendChild(stext);
                return stroke;
            }
        }
    }
}
