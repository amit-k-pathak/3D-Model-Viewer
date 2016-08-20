using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Xml;

namespace WPF3DFileViewer
{
    public class MarkupImporter
    {
        string[] values;
        private CameraInfo cam;
        private List<MarkupInfoFromFile> markUpList;
        private ImporterType importer;
        public static MarkupImporter mImporter;

        private MarkupImporter()
        {

        }

        public static MarkupImporter GetOrCreateMarkupImporter()
        {
            if (mImporter == null)
                mImporter = new MarkupImporter();
            return mImporter;
        }

        private void InitReader()
        {
            this.values = new string[6] { "<X>", "</X>", "<Y>", "</Y>", "<Z>", "</Z>" };
        }

        public MarkUpType LoadViewFromFile(string lastfileName, IHelixViewport3D v)
        {
            if (File.Exists(lastfileName))
                return ReadXml(lastfileName);
            return MarkUpType.None;
        }

        private MarkUpType ReadXml(string file)
        {
            MarkUpType tp = MarkUpType.None;
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            if (doc.HasChildNodes)
            {
                InitReader();
                tp = ParseMarkupFileType(doc);
                this.importer = (tp == MarkUpType.Shape2D) ? ImporterType.Imp2D : ImporterType.Imp3D;
                ParseCameraInfoFromXml(doc);
                ParseMarkupFromXml(doc);
            }
            return tp;
        }

        private MarkUpType ParseMarkupFileType(XmlDocument doc)
        {
            MarkUpType type = MarkUpType.None;

            if (doc.HasChildNodes)
            {
                type = doc.InnerXml.Contains("Annotaions2D") ? MarkUpType.Shape2D : MarkUpType.Shape3D;
            }

            return type;
        }

        private void ParseMarkupFromXml(XmlDocument doc)
        {
            this.markUpList = CreateMarkupInfo(doc);
        }

        public List<MarkupInfoFromFile> GetMarkups()
        {
            return this.markUpList;
        }

        private List<MarkupInfoFromFile> CreateMarkupInfo(XmlDocument doc)
        {
            List<MarkupInfoFromFile> inf = new List<MarkupInfoFromFile>();
            GetElementValues(doc, inf, ShapeType.Rect);
            GetElementValues(doc, inf, ShapeType.Circle);
            GetElementValues(doc, inf, ShapeType.Arrow);
            GetElementValues(doc, inf, ShapeType.Cube);
            GetElementValues(doc, inf, ShapeType.Text);
            GetElementValues(doc, inf, ShapeType.Text3D);
            GetElementValues(doc, inf, ShapeType.Callout);
            GetElementValues(doc, inf, ShapeType.Cloud);
            return inf;
        }

        private string GetMarkupExpr(ShapeType type)
        {
            string expr = string.Empty;
            string prefix = "";

            if (this.importer == ImporterType.Imp2D)
                prefix = "/Annotaions2D/";
            else if (this.importer == ImporterType.Imp3D)
                prefix = "/Annotaions3D/";
            else
                return "";

            switch (type)
            {
                case ShapeType.Rect:
                    expr = prefix + "Rectangle";
                    break;

                case ShapeType.Circle:
                    expr = prefix + "Circle";
                    break;

                case ShapeType.Arrow:
                    expr = prefix + "Arrow";
                    break;

                case ShapeType.Cube:
                    expr = prefix + "Cube";
                    break;

                case ShapeType.Text:
                    expr = prefix + "Text";
                    break;

                case ShapeType.Text3D:
                    expr = prefix + "Text3D";
                    break;

                case ShapeType.Callout:
                    expr = prefix + "Callout";
                    break;

                case ShapeType.Cloud:
                    expr = prefix + "Cloud";
                    break;
            }

            return expr;
        }

        private void GetElementValues(XmlDocument doc, List<MarkupInfoFromFile> inf, ShapeType type)
        {
            string expr = GetMarkupExpr(type);

            if (!string.IsNullOrWhiteSpace(expr))
            {
                XmlNodeList nodeList;
                XmlNode root = doc.DocumentElement;
                string v = "";
                nodeList = root.SelectNodes(expr);

                for (int i = 0; i < nodeList.Count; i++)
                {
                    MarkupInfoFromFile mInfo = new MarkupInfoFromFile();
                    mInfo.type = type;
                    XmlNode node = nodeList[i];

                    for (int j = 0; j < node.ChildNodes.Count; ++j)
                    {
                        v = node.ChildNodes[j].InnerText;

                        switch (node.ChildNodes[j].Name)
                        {
                            case "Width":
                                if (!string.IsNullOrWhiteSpace(v))
                                {
                                    mInfo.width = Convert.ToDouble(v);
                                }
                                break;
                            case "Height":
                                if (!string.IsNullOrWhiteSpace(v))
                                {
                                    mInfo.height = Convert.ToDouble(v);
                                }
                                break;
                            case "Stroke":
                                if (!string.IsNullOrWhiteSpace(v))
                                {
                                    mInfo.strokeWidth = Convert.ToInt32(v);
                                }
                                break;
                            case "Corners":
                                XmlNode l = node.ChildNodes[j];
                                mInfo.p0 = new Point(Convert.ToDouble(l.ChildNodes[0].InnerText), Convert.ToDouble(l.ChildNodes[1].InnerText));
                                mInfo.p1 = new Point(Convert.ToDouble(l.ChildNodes[2].InnerText), Convert.ToDouble(l.ChildNodes[3].InnerText));
                                break;

                            case "Start":
                                mInfo.p0 = GetPoint(node.ChildNodes[j]);
                                break;
                            case "End":
                                mInfo.p1 = GetPoint(node.ChildNodes[j]);
                                break;
                            case "InitialNormal":
                                mInfo.inNorm = GetNormal(node.ChildNodes[j]);
                                break;
                            case "FinalNormal":
                                mInfo.finNorm = GetNormal(node.ChildNodes[j]);
                                break;
                            case "Content":
                                mInfo.content = v;
                                break;
                        }
                    }

                    inf.Add(mInfo);
                }
            }
        }

        private Vector3D GetNormal(XmlNode n)
        {
            Vector3D vec = new Vector3D();

            if (n != null)
            {
                vec = new Vector3D(Convert.ToDouble(n.ChildNodes[0].InnerText), Convert.ToDouble(n.ChildNodes[1].InnerText), Convert.ToDouble(n.ChildNodes[2].InnerText));
            }

            return vec;
        }

        private Point GetPoint(XmlNode n)
        {
            Point p = new Point();
            if (p != null)
            {
                p = new Point(Convert.ToDouble(n.ChildNodes[0].InnerText), Convert.ToDouble(n.ChildNodes[1].InnerText));
            }
            return p;
        }

        private void ParseCameraInfoFromXml(XmlDocument doc)
        {
            Point3D pos = new Point3D();
            Vector3D lookDir = new Vector3D();
            Vector3D upDir = new Vector3D();
            double[] vals = GetVal("descendant::Camera[Position]", doc, 0);
            pos.X = vals[0]; pos.Y = vals[1]; ; pos.Z = vals[2];
            vals = GetVal("descendant::Camera[LookDirection]", doc, 1);
            lookDir.X = vals[0]; lookDir.Y = vals[1]; lookDir.Z = vals[2];
            vals = GetVal("descendant::Camera[UpDirection]", doc, 2);
            upDir.X = vals[0]; upDir.Y = vals[1]; upDir.Z = vals[2];
            SetCameraInfo(pos, lookDir, upDir);
        }

        public CameraInfo GetCameraInfo()
        {
            return this.cam;
        }

        public void SetCameraInfo(Point3D p, Vector3D lookDir, Vector3D upDir)
        {
            this.cam = new CameraInfo();
            cam.pos = p;
            cam.lookDir = lookDir;
            cam.upDir = upDir;
        }

        private double[] GetVal(string expr, XmlDocument doc, int id)
        {
            double[] points = new double[3];
            if (!string.IsNullOrWhiteSpace(expr))
            {
                XmlNodeList nodeList;
                XmlNode root = doc.DocumentElement;
                string val = "";

                nodeList = root.SelectNodes(expr);

                for (int i = 0; i < nodeList.Count; i++)
                {
                    XmlNode node = nodeList[i];
                    string v = node.ChildNodes[id].InnerXml;

                    for (int j = 0; j < node.ChildNodes.Count; j++)
                    {
                        int sIndex = v.IndexOf(this.values[2 * j]) + 3;

                        if (sIndex > 0)
                        {
                            val = v.Substring(sIndex, v.IndexOf(this.values[2 * j + 1]) - sIndex);
                            points[j] = Convert.ToDouble(val);
                        }
                    }
                }
            }
            return points;
        }
    }
}
