// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WPF3DFileViewer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Media3D;
    using System.Windows.Threading;
    using HelixToolkit.Wpf;
    using System.Windows.Media;
    using _3DTools;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using System.IO;
    using WPF3DFileViewer;
    using ModelViewer;

    public partial class MainViewModel : BaseViewModel
    {
        private IFileDialogService fileDialogService;
        private IHelixViewport3D viewport;
        private TreeView modelTree;
        private Dispatcher dispatcher;
        private Canvas surface;
        private TextBox tt;
        MenuItem mainMenu;

        private Mode applicationMode;
        private int lastAdded;
        private int drawS;
        private int interval, factor;
        private Point lastPoint, creationPoint;
        private List<Point> calloutPts;
        public ObservableCollection<MenuItem> menuItems2D;
        public ObservableCollection<MenuItem> menuItems3D;
       
        MarkupViewHandler vHandler;

        public MainViewModel(IFileDialogService fds, HelixViewport3D viewport, TreeView modelDetails, Canvas parent, MenuItem item)
        {
            if (viewport == null)
            {
                throw new ArgumentNullException("viewport"); 
            }
            viewport.ShowFrameRate = true;
            mainMenu = item;
            this.modelTree = modelDetails;
            this.group = null;
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.fileDialogService = fds;
            this.viewport = viewport;
            this.reload = true;
            this.ApplicationTitle = "3D File Viewer";
            this.surface = parent;
            this.applicationMode = Mode.ModelRenderMode;
            this.currentShape = Shapes3D.None;
            Bind();
            this.dr = new Drawing3D(this.viewport);
            drawS = -1;
            this.lastAdded = -1;
            this.interval = 2;
            this.factor = 3;
            tt = null;
            lastPoint = new Point();
            this.calloutPts = null;
            SetAnnot3DMenu();
            selectedMarkupType = MarkUpType.None;
            this.ttAdded = false;
            //new
            this.viewport.Viewport.Camera.Changed += Camera_Changed;
        }

        void Camera_Changed(object sender, EventArgs e)
        {
            if (this.currentShape != Shapes3D.None)
            {
                SetCameraInfo();

                if (this.selectedMarkupType == MarkUpType.None)
                    HandleViewChanged();
                else
                {
                    if (!this.viewport.Camera.HasAnimatedProperties)
                    {
                        ClearLoadedMarkupView(this.selectedMarkupType);
                        this.selectedMarkupType = MarkUpType.None;
                    }
                }
            }
        }

        private void RemoveTempDrawingInfo()
        {
            if (lastAdded > 0)
            {
                if (this.surface.Children.Count > lastAdded)
                {
                    this.surface.Children.RemoveAt(lastAdded);
                    this.lastAdded = -1;
                }
            }

            if (this.calloutPts != null)
            {
                this.calloutPts.Clear();
                this.calloutPts = null;
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (drawS == 1)
            {
                DrawCallOutWithCloud(e.GetPosition(this.surface));
            }
            else if (drawS == 2)
            {
                double height = final2DPoint.Y-initial2DPoint.Y; 
                DrawCallOutNew(e.GetPosition(this.surface), new Point(Math.Max(final2DPoint.X, initial2DPoint.X), final2DPoint.Y - height*0.5));
            }
        }

        private void DrawCallOutNew(Point mousePosition, Point start)
        {
            if (this.interval % factor == 0)
            {
                RemoveTempDrawingInfo();

                if (this.calloutPts == null)
                    this.calloutPts = new List<Point>();

                Point pt1 = mousePosition;
                this.calloutPts.Add(mousePosition);
                this.calloutPts.Add(start);
                pt1.X = mousePosition.X + 50;
                lastPoint = pt1;
                this.surface.Children.Add(Drawing2D.DrawCallout(mousePosition, start, 0, 0, 0, 50));

                this.lastAdded = this.surface.Children.Count - 1;
            }

            this.interval++;
        }

        private void DrawCallOutWithCloud(Point mousePosition)
        {
            if (this.interval % factor == 0)
            {
                RemoveTempDrawingInfo();
                this.surface.Children.Add(Drawing2D.DrawCalloutRectangle(initial2DPoint, mousePosition));
                this.lastAdded = this.surface.Children.Count - 1;
            }

            this.interval++;
        }

        public void MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            doThisNew();
        }

        private void SetAnnot2DMenu()
        {
            if (this.menuItems2D != null)
                mainMenu.ItemsSource = this.menuItems2D;
            else
            {
                this.menuItems2D = new ObservableCollection<MenuItem>();
                this.menuItems2D.Add(new MenuItem() { Header = "Arrow", Command = DrawArrowCommand });
                this.menuItems2D.Add(new MenuItem() { Header = "Circle", Command = DrawSphereCommand });
                this.menuItems2D.Add(new MenuItem() { Header = "Rectangle", Command = DrawRectangleCommand });
                this.menuItems2D.Add(new MenuItem() { Header = "Callout", Command = DrawTextCommand });
                mainMenu.ItemsSource = this.menuItems2D;
            }
        }

        private void SetAnnot3DMenu()
        {
            if (this.menuItems3D != null)
                mainMenu.ItemsSource = this.menuItems3D;
            else
            {
                this.menuItems3D = new ObservableCollection<MenuItem>();
                this.menuItems3D.Add(new MenuItem() { Header = "Arrow", Command = DrawArrowCommand });
                this.menuItems3D.Add(new MenuItem() { Header = "Sphere", Command = DrawSphereCommand });
                this.menuItems3D.Add(new MenuItem() { Header = "Rectangle", Command = DrawRectangleCommand });
                this.menuItems3D.Add(new MenuItem() { Header = "Cube", Command = DrawCubeCommand });
                this.menuItems3D.Add(new MenuItem() { Header = "3D Comment", Command = Draw3DCommentCommand });
                mainMenu.ItemsSource = this.menuItems3D;
            }
        }

        private void doThisNew()
        {
            if (this.currentShape == Shapes3D.Text2D)
            {
                //if (createT == false)
                {
                    if(this.tt != null)
                        text = string.IsNullOrWhiteSpace(tt.Text) ? "Sample 2D Text" : tt.Text;

                    int width = (int)this.tt.ActualWidth;
                    int height = (int)this.tt.ActualHeight;
                    Point newPos = new Point(creationPoint.X, creationPoint.Y - height*0.5-3);
                    this.tt.Visibility = Visibility.Hidden;

                    this.dr.DrawText(newPos, text, 30);
                    if (this.applicationMode == Mode.AnnotationDrawingMode)
                    {
                        this.surface.Children.Add(Drawing2D.DrawCallout(this.calloutPts[0], this.calloutPts[1], 1, width, height, 50));
                    }

                    RemoveTempDrawingInfo();
                    this.createT = false;
                    //flag = -1;
                    //this.createT = true;
                }
            }
            else if (this.currentShape == Shapes3D.Text3D)
            {
                //if (createT == false)
                {
                    this.tt.Visibility = Visibility.Hidden;
                    this.createT = true;
                    flag = 0;
                }
            }
        }

        private TextBox AddTextt(Point pt)
        {
            TextBox x = new TextBox();
            x.KeyDown += new KeyEventHandler(HandleKeyDown);
            x.TextWrapping = TextWrapping.Wrap;
            x.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            x.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            x.AcceptsReturn = true;
            x.Margin = new Thickness(pt.X-10, pt.Y-10, pt.X + 100, pt.Y + 100);
            x.Background = Brushes.Gray;
            Canvas.SetZIndex(x, 200);
            this.surface.Children.Add(x);
            x.Focus();
            return x;
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                
            }
        }

        private void ChangeLocation(Point pt)
        {
            tt.Margin = new Thickness(pt.X, pt.Y - 10, pt.X + 100, pt.Y + 100);
            tt.Focus();
        }

        private void HandleViewChanged()
        {
            if (this.applicationMode == Mode.AnnotationDrawingMode)
            {
                if (Drawing2D.ViewChanged())
                {
                    Clear2DAnnots();
                    int selectedIndex = this.vHandler.GetActiveMarkupIndex();
                     
                    if (selectedIndex == -1)
                    {
                        //Clear2DAnnots();
                        MarkupDetails item = Drawing2D.GetMainMarkupList();
                        if (item != null)
                        {
                            this.vHandler.SetApplicationMode(this.applicationMode);
                            this.vHandler.AddView(item);
                        }
                    }
                    else
                    {
                        //selectedIndex = -1;
                        this.vHandler.SetActiveMarkupIndex(-1);
                    }
                }
            }
            else if (this.applicationMode == Mode.ModelRenderMode)
            {
                if (Drawing3D.ViewChanged())
                {
                    ClearAnnots3d();
                    int selectedIndex = this.vHandler.GetActiveMarkupIndex();
                    if (selectedIndex == -1)
                    {
                        //ClearAnnots3d();
                        MarkupDetails item = Drawing3D.GetMainMarkupList();

                        if (item != null)
                        {
                            this.vHandler.SetApplicationMode(this.applicationMode);
                            this.vHandler.AddView(item);
                        }
                    }
                    else
                    {
                        //selectedIndex = -1;
                        this.vHandler.SetActiveMarkupIndex(-1);
                    }
                }
            }      
         }

        private void ClearLoadedMarkupView(MarkUpType m)
        {
            if (m == MarkUpType.Shape2D)
                    Clear2DAnnots();   
            else if (m == MarkUpType.Shape3D)
                    ClearAnnots3d();
        }
    }
}

