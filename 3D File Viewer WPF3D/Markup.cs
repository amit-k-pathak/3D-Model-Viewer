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
    public class MarkupRetrievableInfo
    {
        public string mViewName { get; set; }
        public int index { get; set; }
        public MarkUpType type { get; set; }
    }

    public class CameraInfo
    {
        public Point3D pos { get; set; }
        public Vector3D lookDir { get; set; }
        public Vector3D upDir { get; set; }

        public override bool Equals(object obj)
        {
            CameraInfo newCam = obj as CameraInfo;

            if (newCam != null)
            {
                bool posFlag = (pos.X == newCam.pos.X) && (pos.Y == newCam.pos.Y) && (pos.Z == newCam.pos.Z);
                bool lkFlag = (lookDir.X == newCam.lookDir.X) && (lookDir.Y == newCam.lookDir.Y) && (lookDir.Z == newCam.lookDir.Z);
                bool upFlag = (upDir.X == newCam.upDir.X) && (upDir.Y == newCam.upDir.Y) && (upDir.Z == newCam.upDir.Z); 
                return posFlag && lkFlag && upFlag;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class MarkupDetails
    {
        private CameraInfo camInf;
        private List<MarkupInfoFromFile> mlist;
        private string name;
        private MarkUpType type;
        public MarkupDetails(string name, CameraInfo cam, List<MarkupInfoFromFile> lst, MarkUpType tp)
        {
            this.name = name;
            this.camInf = cam;
            this.mlist = lst;
            this.type = tp;
        }

        public void Merge(List<MarkupInfoFromFile> newList)
        {
            if (this.mlist != null)
            {
                this.mlist.AddRange(newList);
            }
        }

        public void SetMarkupType(MarkUpType t)
        {
            type = t;
        }

        public MarkUpType GetMarkupType()
        {
            return type;
        }

        public CameraInfo GetCamera()
        {
            return this.camInf;
        }

        public List<MarkupInfoFromFile> GetMarkupList()
        {
            return this.mlist;
        }

        public void SetMarkupList(List<MarkupInfoFromFile> l)
        {
            this.mlist = l;
        }

        public string GetName()
        {
            return this.name;
        }
    }

    public class MarkupView
    {
        public MarkupView()
        {
            this.Views = new ObservableCollection<MarkupDetails>();
        }

        public string Name { get; set; }

        public ObservableCollection<MarkupDetails> Views { get; set; }
    }

   
}
