using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace WPF3DFileViewer
{
    public class MarkupViewHandler : IDisposable
    {
        private System.Windows.Controls.TreeView modelTree;
        private static MarkupViewHandler vHandler;
        private List<MarkupRetrievableInfo> retrievableViewList;
        private IHelixViewport3D vPort;
        private Canvas drawingSurface;
        private Rect3D modelBBox;
        private Model3DGroup group;
        private int activeMarkupIndex;
        private int viewCounter;
        private Mode appMode;
        private bool cameraChanged;
        private CameraInfo cInf;

        private MarkupViewHandler(System.Windows.Controls.TreeView tv, IHelixViewport3D view, Canvas s)
        {
            this.modelTree = tv;
            this.viewCounter = 0;
            this.activeMarkupIndex = -1;
            this.vPort = view;
            this.drawingSurface = s;
            this.appMode = Mode.None;
            this.cameraChanged = false;
        }

        public static MarkupViewHandler GetOrCreateMarkupViewHandler(System.Windows.Controls.TreeView tv, IHelixViewport3D v, Canvas sFace)
        {
            if (vHandler == null)
                vHandler = new MarkupViewHandler(tv, v, sFace);
            return vHandler;
        }

        public void SetModelBBox(Rect3D box)
        {
            modelBBox = box;
        }

        public void SetModelGroup(Model3DGroup g)
        {
            group = g;
        }

        public void SetApplicationMode(Mode m)
        {
            appMode = m;
        }

        public int GetViewCounter()
        {
            return this.viewCounter;
        }

        public int GetActiveMarkupIndex()
        {
            return this.activeMarkupIndex;
        }

        public void SetActiveMarkupIndex(int ct)
        {
            this.activeMarkupIndex = ct;
        }

        public bool IsCameraChanged()
        {
            return this.cameraChanged;
        }

        public void ClearFlags()
        {
            this.cameraChanged = false;
        }

        public MarkupRetrievableInfo GetMarkupInfo(int index)
        {
            if (index >= 0 && index < this.retrievableViewList.Count)
                return this.retrievableViewList[index];
            return null;
        }

        public void AddView(MarkupDetails details)
        {
            if (this.retrievableViewList == null)
                this.retrievableViewList = new List<MarkupRetrievableInfo>();

            if (!IsViewPresent(details.GetName()))
            {
                MarkUpType t = details.GetMarkupType();
                int id = -1;
                if (t == MarkUpType.Shape2D)
                    id = Drawing2D.GetCounter();
                else if (t == MarkUpType.Shape3D)
                    id = Drawing3D.GetCounter();

                this.retrievableViewList.Add(new MarkupRetrievableInfo() { index = id - 1, mViewName = details.GetName(), type = details.GetMarkupType() });

                viewCounter++;

                if (this.modelTree != null)
                {
                    MarkupView v = new MarkupView() { Name = details.GetName() };
                    v.Views.Add(details);
                    this.modelTree.Items.Add(v);
                }
            }
        }

        private bool IsViewPresent(string v)
        {
            bool flag = false;

            for (int i = 0; i < this.modelTree.Items.Count; ++i)
            {
                MarkupView item = this.modelTree.Items[i] as MarkupView;

                if (item != null && item.Name.Equals(v))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        private void SetCameraInfo()
        {
            CameraInfo cInfo = new CameraInfo();
            cInfo.pos = this.vPort.Camera.Position;
            cInfo.lookDir = this.vPort.Camera.LookDirection;
            cInfo.upDir = this.vPort.Camera.UpDirection;
            if (this.appMode == Mode.AnnotationDrawingMode)
            {
                Drawing2D.Init();
                Drawing2D.SetCameraInfo(cInfo);
            }
            else if (this.appMode == Mode.ModelRenderMode)
            {
                Drawing3D.Init();
                Drawing3D.SetCameraInfo(cInfo);
            }
        }

        internal MarkUpType LoadView(string p, ObservableCollection<MarkupDetails> details)
        {
            MarkUpType mType = MarkUpType.None;
            this.cameraChanged = false;

            if (details != null && details.Count > 0 && !string.IsNullOrWhiteSpace(p))
            {
                MarkupRetrievableInfo mReInfo = GetViewByName(p);
                MarkupDetails mDetails = null;

                if (mReInfo != null)
                {
                    mType = mReInfo.type;
                    if (mReInfo.type == MarkUpType.Shape2D)
                       mDetails = Drawing2D.GetMarkupsByIndex(mReInfo.index);
                    else if (mReInfo.type == MarkUpType.Shape3D)
                       mDetails = Drawing3D.GetMarkupsByIndex(mReInfo.index);

                    if (mDetails != null)
                    {
                        while (this.vPort.Camera.HasAnimatedProperties);

                        CameraInfo cam = mDetails.GetCamera();
                        cInf = cam;
                        this.vPort.Camera.AnimateTo(cam.pos, cam.lookDir, cam.upDir, 800);
                        
                        //while (!IsCameraRestored(cam)) ;
                        
                        SetCameraInfo();
                        List<MarkupInfoFromFile> l = mDetails.GetMarkupList();

                        //while (this.vPort.Camera.HasAnimatedProperties) ;

                            if (mDetails.GetMarkupType() == MarkUpType.Shape2D)
                            {
                                Drawing2D d = new Drawing2D(this.drawingSurface);
                                d.ImportAnnotsFromList(l);
                                Drawing2D.SetactiveMarkupListIndex(mReInfo.index);
                                this.SetActiveMarkupIndex(mReInfo.index);
                            }
                            else if (mDetails.GetMarkupType() == MarkUpType.Shape3D)
                            {
                                Drawing3D d3 = new Drawing3D(this.vPort);
                                d3.SetBBox(this.modelBBox);
                                d3.ImportAnnotsFromFile(l);
                                Drawing3D.SetactiveMarkupListIndex(mReInfo.index);
                                this.SetActiveMarkupIndex(mReInfo.index);
                            }
                     
                    }
                }
            }
            return mType;
        }

        private bool IsCameraRestored(CameraInfo cam)
        {
            cameraChanged = false;
            if (this.vPort.Camera.Position.DistanceTo(cam.pos) == 0.0)
            {
                cameraChanged = true;
                return true;
            }

            return false;
        }

        private MarkupRetrievableInfo GetViewByName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                for (int i = 0; i < this.retrievableViewList.Count; ++i)
                {
                    if (this.retrievableViewList[i].mViewName.Equals(name))
                    {
                        activeMarkupIndex = i;
                        return this.retrievableViewList[i];
                    }
                }
            }
            activeMarkupIndex = -1;
            return null;
        }

        public void Dispose()
        {
            this.activeMarkupIndex = -1;
            if (this.retrievableViewList != null)
            {
                this.retrievableViewList.Clear();
                this.retrievableViewList = null;
            }
            this.viewCounter = 0;
        }

        internal CameraInfo GetMarkupCamera()
        {
            return this.cInf;
        }
    }
}
