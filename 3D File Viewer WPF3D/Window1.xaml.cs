// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Window1.xaml.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Interaction logic for Window1.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace WPF3DFileViewer
{
    /// <summary>
    /// Interaction logic for Window1.
    /// </summary>
    public partial class Window1
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Window1"/> class.
        /// </summary>
        MainViewModel model;

        public Window1()
        {
            this.InitializeComponent();
            //AddHandler(HelixToolkit.Wpf.HelixViewport3D.MouseDownEvent, method);
            model = new MainViewModel(new FileDialogService(), view1, modeldetails, surf, annotMenu);
            //model = new MainViewModel(new FileDialogService(), view1, new TreeView(), surf, annotMenu);
            this.DataContext = model;
        }

        private void view1_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //System.Windows.MessageBox.Show("");
            //model.OnMouseDown(sender, e);
        }

        private void view1_MouseDoubleClick_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //PopupTest.Placement = System.Windows.Controls.Primitives.PlacementMode.Mouse;
            //PopupTest.StaysOpen = false;
            //PopupTest.Height = 1000;
            //PopupTest.Width = 500;
            //PopupTest.IsOpen = true;

            //System.Windows.Controls.TextBox txtBox = new System.Windows.Controls.TextBox();
            //txtBox.Text = "New Text Here....";
            //this.AddChild(txtBox);

            model.MouseDoubleClick(sender, e);
        }

        private void surf_MouseDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //model.OnMouseDown(sender, e);
        }

        private void surf_MouseMove_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            model.MouseMove(sender, e);
        }

        private void surf_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //model.MouseDoubleClick(sender, e);
            }
            else
            {
                model.OnMouseDown(sender, e);
            }
        }

        private void modeldetails_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.model != null)
            {
                MarkupView itm = e.NewValue as MarkupView;
                
                TreeView tv = e.Source as TreeView;

                if (tv != null)
                {
                    if (itm != null)
                        this.model.LoadMarkupView(itm.Name, itm.Views);
                }
            }
        }
    }
}