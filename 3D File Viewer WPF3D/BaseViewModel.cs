using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelViewer;
using HelixToolkit.Wpf;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Media3D;
using _3DTools;

namespace WPF3DFileViewer
{
    public enum Mode
    {
        None,
        ModelRenderMode,
        AnnotationDrawingMode
    }

    public class BaseViewModel : Observable
    {
        #region protected variables

        protected const string OpenFileFilter = "3D model files (*.3ds;*.obj;*.lwo;*.stl)|*.3ds;*.obj;*.objz;*.lwo;*.stl";

        protected const string TitleFormatString = "3D model viewer - {0}";

        protected string currentModelPath;

        protected string applicationTitle;

        protected Model3D currentModel;

        protected Model3DGroup group;

        protected bool reload;

        protected Shapes3D currentShape;

       
        #endregion

        public string CurrentModelPath
        {
            get
            {
                return this.currentModelPath;
            }

            set
            {
                this.currentModelPath = value;
                this.RaisePropertyChanged("CurrentModelPath");
            }
        }

        public string ApplicationTitle
        {
            get
            {
                return this.applicationTitle;
            }

            set
            {
                this.applicationTitle = value;
                this.RaisePropertyChanged("ApplicationTitle");
            }
        }

        public Model3D CurrentModel
        {
            get
            {
                return this.currentModel;
            }

            set
            {
                this.currentModel = value;
                this.RaisePropertyChanged("CurrentModel");
            }
        }
    }
}