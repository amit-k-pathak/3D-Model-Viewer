using _3DTools;
using HelixToolkit.Wpf;
using ModelViewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Xml;
using WPF3DFileViewer.WPF3DFileViewer;

namespace WPF3DFileViewer
{
    public partial class MainViewModel : BaseViewModel
    {
        private Point3D initialPoint, finalPoint;
        private Point initial2DPoint, final2DPoint;
        private MeshGeometry3D initialHittedGeo, finalHittedGeo;
        private Vector3D initialNorm, finalNorm;
        private static int flag;
        private int count;
        private Rect3D modelBbox;
        private Drawing dr;
        private MarkUpType export3d;
        private bool createT;
        private string prevModel;
        private string text;
        private MarkUpType selectedMarkupType;
        private bool ttAdded;

        #region Commands

        public ICommand FileOpenCommand { get; set; }
        public ICommand FileExitCommand { get; set; }
        public ICommand ViewZoomExtentsCommand { get; set; }
        public ICommand SaveSnapshotCommand { get; set; }
        public ICommand DrawArrowCommand { get; set; }
        public ICommand DrawRectangleCommand { get; set; }
        public ICommand Draw3DCommentCommand { get; set; }
        public ICommand DrawTextCommand { get; set; }
        public ICommand DrawFreeHandMarkupCommand { get; set; }
        public ICommand DrawSphereCommand { get; set; }
        public ICommand SetRenderModeCommand { get; set; }
        public ICommand SetAnnotationModeCommand { get; set; }
        public ICommand DrawCubeCommand { get; set; }
        public ICommand ExportMarkupCommand { get; set; }
        public ICommand ImportMarkupCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand DrawNewObjCommand { get; set; }
        public ICommand SpinStartCommand { get; set; }
        public ICommand SpinStopCommand { get; set; }
       
        
        #endregion

        #region Command Methods

        private void Bind()
        {
            this.FileOpenCommand = new DelegateCommand(this.FileOpen);
            this.FileExitCommand = new DelegateCommand(FileExit);
            this.ViewZoomExtentsCommand = new DelegateCommand(this.ViewZoomExtents);
            this.SaveSnapshotCommand = new DelegateCommand(this.SaveSnapshot);
            this.DrawArrowCommand = new DelegateCommand(DrawArrow);
            this.DrawRectangleCommand = new DelegateCommand(DrawRectangle);
            this.DrawTextCommand = new DelegateCommand(DrawText);
            this.DrawFreeHandMarkupCommand = new DelegateCommand(DrawFreeHandMarkup);
            this.Draw3DCommentCommand = new DelegateCommand(Draw3DComment);
            this.DrawSphereCommand = new DelegateCommand(DrawSphere);
            this.SetRenderModeCommand = new DelegateCommand(SetModelRenderMode);
            this.SetAnnotationModeCommand = new DelegateCommand(SetAnnotMode);
            this.DrawCubeCommand = new DelegateCommand(DrawCube);
            this.ExportMarkupCommand = new DelegateCommand(ExportMarkup);
            this.ImportMarkupCommand = new DelegateCommand(ImportMarkup);
            this.RefreshCommand = new DelegateCommand(clearSurface);
            this.DrawNewObjCommand = new DelegateCommand(DrawNewObj);
            this.SpinStartCommand = new DelegateCommand(SpinStart);
            this.SpinStopCommand = new DelegateCommand(SpinStop);
            export3d = MarkUpType.None;
        }

        private void SpinStart()
        {
            if (this.viewport != null && this.group != null)
            {
                this.viewport.CameraController.InfiniteSpin = true;
                //double x = this.modelBbox.Location
                //Point3D center = new Point3D(this.modelBbox.Location
                Point p = new Point(this.viewport.Viewport.ActualWidth / 2, this.viewport.Viewport.ActualHeight / 2);
                //p.X = p.Y=0;
                this.viewport.CameraController.StartSpin(new Vector(50, 50), p, this.viewport.CameraController.CameraTarget);
                //this.viewport.CameraController.cam
                //this.viewport.CameraController.
                //this.viewport.Camera.Changed += new EventHandler(HandleCamera);
                //Camera c = new OrthographicCamera()
                //{
                //    NearPlaneDistance = 1,
                //    FarPlaneDistance = Int32.MaxValue,
                //    Width = 1000,
                //    Position = this.viewport.Camera.Position,
                //    LookDirection = this.viewport.Camera.LookDirection,
                //    UpDirection = this.viewport.Camera.UpDirection,
                //};
                
                
            }
        }

        private void HandleCamera(object sender, EventArgs e)
        {
            this.viewport.Camera.NearPlaneDistance = 1;
            this.viewport.Camera.FarPlaneDistance = 400000;
        }

        private void SpinStop()
        {
            this.viewport.CameraController.InfiniteSpin = false;
        }

        private void DrawNewObj()
        {
            this.currentShape = Shapes3D.FreeHandMarkup;
            flag = 0;
        }

        private void ImportMarkup()
        {
            if (this.fileDialogService == null)
                this.fileDialogService = new FileDialogService();

            string fileName = this.fileDialogService.OpenFileDialog(@"C:\Users\amitp\Desktop\markup export", "", "Xml File|*.xml", ".xml");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                clearSurface();
                MarkupImporter importer = MarkupImporter.GetOrCreateMarkupImporter();
                MarkUpType t = importer.LoadViewFromFile(fileName, this.viewport);

                if (t != MarkUpType.None)
                {
                    CameraInfo info = importer.GetCameraInfo();
                    List<MarkupInfoFromFile> markups = importer.GetMarkups();

                    if (info != null)
                        this.viewport.Camera.AnimateTo(info.pos, info.lookDir, info.upDir, 700);

                    if (markups.Count > 0)
                    {
                        clearSurface();
                        //Drawing3D.ClearMarkupList();
                        IBase parent= null;
                        if (t == MarkUpType.Shape3D)
                            parent = new Drawing3D(this.viewport, this.modelBbox);
                        else
                            parent = new Drawing2D(this.surface);

                        parent.ImportMarkups(markups);
                    }
                }
            }
            flag = 0;
        }

        private void ExportMarkup()
        {
            if (this.fileDialogService == null)
                this.fileDialogService = new FileDialogService();

            string fileName = this.fileDialogService.SaveFileDialog(@"C:\Users\amitp\Desktop\markup export", "", "Xml File|*.xml", ".xml");

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                MarkupExporter exporter = MarkupExporter.GetOrCreateMarkupExporter();
                List<MarkupDetails> tmpList = new List<MarkupDetails>();
                int selectedIndex = this.vHandler.GetActiveMarkupIndex();

                if (selectedIndex >= 0)
                {
                    MarkupRetrievableInfo mReInfo = this.vHandler.GetMarkupInfo(selectedIndex);
                    //export3d = this.retrievableViewList[selectedIndex].type;
                    export3d = mReInfo.type;
                    if (export3d == MarkUpType.Shape2D)
                    {
                        tmpList.Add(Drawing2D.GetMarkupsByIndex(mReInfo.index));
                        exporter.SetExporter(ExporterType.Ex2D);
                        exporter.ExportMarkup(fileName, tmpList, this.viewport);
                    }
                    else if (export3d == MarkUpType.Shape3D)
                    {
                        MarkupDetails d = Drawing3D.GetMarkupsByIndex(mReInfo.index);
                        if(d!=null)
                        {
                            tmpList.Add(d);
                            exporter.SetExporter(ExporterType.Ex3D);
                            exporter.ExportMarkup(fileName, tmpList, this.viewport); 
                        }
                    }
                    if (File.Exists(fileName))
                        MessageBox.Show("Export success...");
                    else
                        MessageBox.Show("Export failed...");
                }
                else
                    MessageBox.Show("Select a markup first....");

            }
            flag = 0;
        }

        private void DrawCube()
        {
            this.currentShape = Shapes3D.Cube;
            flag = 0;
        }

        private void SetAnnotMode()
        {
            this.applicationMode = Mode.AnnotationDrawingMode;
            flag = 0;
            this.dr = new Drawing2D(this.surface);
            //Drawing2D.ClearViewInfo();
            Drawing2D.SetCount(this.vHandler.GetViewCounter());
            //Drawing2D.SetCount(mIndex);
            clearSurface();
            SetAnnot2DMenu();
        }

        private void SetModelRenderMode()
        {
            this.applicationMode = Mode.ModelRenderMode;
            flag = 0;
            //Drawing3D.ClearViewInfo();
            //Drawing3D.SetCount(mIndex);
            Drawing3D.SetCount(this.vHandler.GetViewCounter());
            //this.dr = new Drawing3D(this.viewport);
            clearSurface();
            SetAnnot3DMenu();
        }

        public void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && this.currentShape != Shapes3D.None)
            {
                Point p = e.GetPosition(this.viewport.Viewport);
                var pt = Viewport3DHelper.FindNearestPoint(this.viewport.Viewport, p);
                HitTestResult hitResult = VisualTreeHelper.HitTest(viewport.Viewport, p);
                RayMeshGeometry3DHitTestResult result = hitResult as RayMeshGeometry3DHitTestResult;
                
                //Matrix3D mat = viewport.Viewport.GetViewMatrix();
                //Plane3D p = 

                if (flag == 0)
                {
                    if (pt.HasValue)
                        initialPoint = (Point3D)pt;
                    else
                        return;
                    if (p != null)
                        initial2DPoint = p;

                    if (result != null)
                    {
                            initialHittedGeo = result.MeshHit;
                            //if(initialHittedGeo.Normals.Count != 0)
                            //    initialNorm = initialHittedGeo.Normals[result.VertexIndex1];
                            //else
                            //{
                            Vector3D p0 = initialHittedGeo.Positions[result.VertexIndex2] - initialHittedGeo.Positions[result.VertexIndex1];
                            Vector3D p1 = initialHittedGeo.Positions[result.VertexIndex3] - initialHittedGeo.Positions[result.VertexIndex1];
                            initialNorm =  Vector3D.CrossProduct(p0, p1);
                            initialNorm.Normalize();

                            double sign = Vector3D.DotProduct(initialNorm, viewport.Camera.LookDirection);
                            if (sign > 0)
                                initialNorm = -initialNorm;
                        //}                   
                            //double d = GetDist(initialPoint); 
                    }
                }
                else if (flag == 1)
                {
                    if (pt.HasValue)
                        finalPoint = (Point3D)pt;
                    else
                        return;
                    if (p != null)
                        final2DPoint = p;

                    if (result != null)
                    {
                            finalHittedGeo = result.MeshHit;
                            //if (finalHittedGeo.Normals.Count != 0)
                            //    finalNorm = finalHittedGeo.Normals[result.VertexIndex1];
                            //else
                            //{
                            Vector3D p0 = finalHittedGeo.Positions[result.VertexIndex2] - finalHittedGeo.Positions[result.VertexIndex1];
                            Vector3D p1 = finalHittedGeo.Positions[result.VertexIndex3] - finalHittedGeo.Positions[result.VertexIndex1];
                            finalNorm = Vector3D.CrossProduct(p0,p1);
                            finalNorm.Normalize();
                            double sign = Vector3D.DotProduct(finalNorm, viewport.Camera.LookDirection);
                            if (sign > 0)
                                finalNorm = -finalNorm;
                        //}
                    }
                }

                if (this.applicationMode == Mode.ModelRenderMode)
                {
                    Drawing3D d3d = new Drawing3D(this.viewport);
                    d3d.SetStartVec(initialNorm);
                    d3d.SetEndVec(finalNorm);
                    d3d.SetBBox(this.modelBbox);
                    SetCameraInfo();
                    this.dr = d3d;
                }
                else if (this.applicationMode == Mode.AnnotationDrawingMode)
                {
                    Drawing2D d2d = new Drawing2D(this.surface);
                    SetCameraInfo();
                    this.dr = d2d;
                }

                if(this.applicationMode != Mode.None)
                    DrawShapes(this.currentShape, initial2DPoint, final2DPoint);
            }
        }

        private void DrawSphere()
        {
            this.currentShape = Shapes3D.Sphere;
            flag = 0;
        }

        private void Draw3DComment()
        {
            this.currentShape = Shapes3D.Text3D;
            flag = 0;
        }

        private void DrawFreeHandMarkup()
        {
            this.currentShape = Shapes3D.FreeHandMarkup;
            flag = 0;
        }

        private void DrawText()
        {
            this.currentShape = Shapes3D.Text2D;
            flag = 0;
        }

        private void DrawRectangle()
        {
            this.currentShape = Shapes3D.Rectangle;
            flag = 0;
        }

        private void DrawArrow()
        {
            this.currentShape = Shapes3D.Arrow;
            flag = 0;
        }

        private void SetCameraInfo()
        {
            CameraInfo cInfo = new CameraInfo();
            cInfo.pos = this.viewport.Camera.Position;
            cInfo.lookDir = this.viewport.Camera.LookDirection;
            cInfo.upDir = this.viewport.Camera.UpDirection;
            if (this.applicationMode == Mode.AnnotationDrawingMode)
            {
                Drawing2D.Init();
                Drawing2D.SetCameraInfo(cInfo);
            }
            else if(this.applicationMode == Mode.ModelRenderMode)
            {
                Drawing3D.Init();
                Drawing3D.SetCameraInfo(cInfo);
            }
        }

        private void CheckAndClear()
        {
            if (this.applicationMode == Mode.AnnotationDrawingMode)
            {
                if (this.selectedMarkupType == MarkUpType.Shape3D)
                {
                    ClearAnnots3d();
                }
            }
            else if (this.applicationMode == Mode.ModelRenderMode)
            {
                if (this.selectedMarkupType == MarkUpType.Shape2D)
                {
                    Clear2DAnnots();
                }
            }
        }

        private void DrawShapes(Shapes3D shape, Point p0, Point p1)
        {
            CheckAndClear();
            switch (shape)
            {
                case Shapes3D.Arrow:
                    if (flag == 1)
                    {
                        this.dr.DrawArrow(p0, p1);
                        flag = -1;
                    }
                    break;
                case Shapes3D.Cube:
                    if (flag == 0)
                    {
                        this.dr.DrawCube(p0, p1);
                        flag = -1;
                    }
                    break;
                case Shapes3D.Rectangle:
                    if (flag == 1)
                    {
                        this.dr.DrawRectangle(p0, p1);
                        flag = -1;
                    }
                    break;
                case Shapes3D.Sphere:
                    if (flag == 1)
                    {
                        this.dr.DrawSphere(p0, p1);
                        flag = -1;
                    }
                    break;
                case Shapes3D.Text2D:

                    bool aMode = this.applicationMode == Mode.AnnotationDrawingMode;
                    if (flag == 0 && aMode)
                        this.drawS = 1;
                    else if (flag == 1)
                    {
                        RemoveTempDrawingInfo();
                        this.drawS = 2;
                        Drawing2D d = new Drawing2D(this.surface);
                        System.Windows.Shapes.Path arcPath = d.DrawCloud(initial2DPoint, final2DPoint, 1);
                        if(arcPath != null)
                            this.surface.Children.Add(arcPath);
                    }
                    else if (flag == 2)
                    {
                        this.drawS = -1;
                        text = "Sample 2D Text";
                       
                        if(tt!=null)
                            text = string.IsNullOrWhiteSpace(tt.Text) ? "Sample 2D Text" : tt.Text;
                        creationPoint = this.lastPoint;
                        if (!createT)
                        {
                            if (aMode)
                                EnableTxttInput(creationPoint);
                        }
                        flag = -2;
                    }

                    break;

                case Shapes3D.Text3D:
                    if (flag == 0)
                    {
                        text = "Sample 3D Text";
                        
                        if(tt!=null)
                            text = string.IsNullOrWhiteSpace(tt.Text) ? "Sample 3D Text" : tt.Text;

                        if (!createT)
                            EnableTxttInput(p0);
                        else
                        {
                            this.dr.Draw3DComment(p0, text, 25);
                            createT = false;
                        }
                        flag = -1;
                    }
                    break;
                case Shapes3D.FreeHandMarkup:
                    if (flag == 0)
                        this.drawS = 1;
                    else if (flag == 1)
                    {
                        this.drawS = -1;
                        flag = -1;
                    }
                    break;
            }
            ++flag;
        }

        private void EnableTxttInput(Point pt)
        {
            if (this.tt == null)
            {
                this.tt = AddTextt(pt);
                ttAdded = true;
                return;
            }

            if (this.tt.Visibility == Visibility.Hidden)
            {
                //this.tt = AddTextt(pt);
                tt.Clear();
                tt.Visibility = Visibility.Visible;
                if (!ttAdded)
                {
                    this.surface.Children.Add(tt);
                    ttAdded = true;
                }
            }
           
            ChangeLocation(pt);
        }

        private void Clear2DAnnots()
        {
            if (this.surface != null)
            {
                var iterator = this.surface.Children.GetEnumerator();

                while (iterator.MoveNext())
                {
                    UIElement c = iterator.Current as UIElement;
                    
                    if (c != null)
                    {
                        Type tp = c.GetType();
                        if (tp == typeof(Canvas) || tp == typeof(Label) || tp == typeof(System.Windows.Shapes.Polygon) || tp == typeof(TextBox) || tp == typeof(System.Windows.Shapes.Path))
                        {
                            this.surface.Children.Remove(c);
                            iterator = this.surface.Children.GetEnumerator();
                        }

                        if (tp == typeof(TextBox))
                            ttAdded = false;
                    }
                }
                Drawing2D.SetactiveMarkupListIndex(-1);
             
                if (this.tt != null)
                    this.tt.Visibility = Visibility.Hidden;
            }
        }

        private void clearSurface()
        {
            Clear2DAnnots();
            ClearAnnots3d();
            ResetTextInputs();
            RemoveTempDrawingInfo();
            ResetView();
        } 

        private void ResetView()
        {
            //this.selectedIndex = -1;
            if(this.vHandler != null)
                this.vHandler.SetActiveMarkupIndex(-1);
            Drawing2D.SetactiveMarkupListIndex(-1);
            Drawing3D.SetactiveMarkupListIndex(-1);
        }

        private void ResetTextInputs()
        {
            this.createT = false;
            this.currentShape = Shapes3D.None;
            flag = -1;
        }

        private void SaveSnapshot()
        {
            if (this.fileDialogService == null)
                this.fileDialogService = new FileDialogService();

            string  savePath = this.fileDialogService.SaveFileDialog(null, null, Exporters.Filter, ".png");

            if (!string.IsNullOrWhiteSpace(savePath))
            {
                try
                {
                    Viewport3DHelper.SetCanvas(this.surface);
                    this.viewport.Export(savePath);

                    if (File.Exists(savePath))
                        MessageBox.Show("Snapshot saved.", "Save Snapshot", MessageBoxButton.OK);
                    else
                        MessageBox.Show("Snapshot save error", "Save Snapshot", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Snapshot save failed " + ex.Message + "\n" + ex.StackTrace, "Save Snapshot", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private static void FileExit()
        {
            Application.Current.Shutdown();
        }

        private void ViewZoomExtents()
        {
            this.viewport.ZoomExtents(500);
        }

        private void Init3dViewer(Model3DGroup grp)
        {
            if (this.viewport != null)
            {
                this.viewport.Viewport.Children.Add(new DefaultLights());
                ModelVisual3D mdl = new ModelVisual3D();
                mdl.Content = grp;
                this.viewport.Viewport.Children.Add(mdl);
            }
        }

        private async void FileOpen()
        {
            if (this.fileDialogService == null)
                this.fileDialogService = new FileDialogService();

            ClearModelGroup();
            clearSurface();
            this.CurrentModelPath = this.fileDialogService.OpenFileDialog(@"C:\Users\amitp\Desktop\obj", null, OpenFileFilter, ".obj");
           
            if (!string.IsNullOrWhiteSpace(this.CurrentModelPath))
            {
                if (!string.IsNullOrWhiteSpace(this.prevModel))
                {
                    if (this.prevModel.Equals(this.currentModelPath))
                        return;
                }
                group = await this.LoadAsync(this.CurrentModelPath, false);
                ClearTreeView();
                ClearMarkupViews();
                Init3dViewer(group);
                this.ApplicationTitle = string.Format(TitleFormatString, this.CurrentModelPath);
                this.viewport.ZoomExtents(0);
                this.modelBbox = Visual3DHelper.FindBounds(this.viewport.Viewport.Children);
                count = this.viewport.Viewport.Children.Count;
                this.prevModel = this.CurrentModelPath;

                if (this.vHandler != null)
                {
                    this.vHandler.Dispose();
                    this.vHandler = null;
                }
                this.vHandler = MarkupViewHandler.GetOrCreateMarkupViewHandler(this.modelTree, this.viewport, this.surface);
                this.vHandler.SetModelGroup(group);
                this.vHandler.SetModelBBox(this.modelBbox);
            }
        }

        private void ClearMarkupViews()
        {
            Drawing2D.ClearViewInfo();
            Drawing3D.ClearViewInfo();
        }

        private void ClearTreeView()
        {
            if (this.modelTree.Items.Count > 0)
                this.modelTree.Items.Clear();
        }

        private void ClearAnnots3d()
        {
            if (this.count < this.viewport.Viewport.Children.Count)
            {
                this.viewport.Viewport.Children.Clear();
                if(this.group != null)
                    this.Init3dViewer(this.group);
            }
            Drawing3D.SetactiveMarkupListIndex(-1);
            //if (tt != null)
            //    tt.Visibility = Visibility.Hidden;

            if (this.tt != null)
                this.tt.Visibility = Visibility.Hidden;
        }

        private void ClearModelGroup()
        {
            if (this.group != null)
            {
                this.viewport.Viewport.Children.Clear();
                this.group = null;
                this.CurrentModel = null;
            }
        }

        #region Loader

        private async Task<Model3DGroup> LoadAsync(string model3DPath, bool freeze)
        {
            return await Task.Factory.StartNew(() =>
            {
                var mi = new ModelImporter();

                if (freeze)
                {
                    // Alt 1. - freeze the model 
                    return mi.Load(model3DPath, null, true);
                }

                // Alt. 2 - create the model on the UI dispatcher
                return mi.Load(model3DPath, this.dispatcher);
            });
        }

        public void LoadMarkupView(string mViewName, ObservableCollection<MarkupDetails> details)
        {
            Clear2DAnnots();
            ClearAnnots3d();
            this.createT = false;
            selectedMarkupType = this.vHandler.LoadView(mViewName, details);
        }

        #endregion

        #endregion
    }
}

