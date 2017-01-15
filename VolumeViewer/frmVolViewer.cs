using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VolumeLib;

namespace VolumeViewer
{
    public partial class frmVolViewer : Form
    {
        public frmVolViewer()
        {
            InitializeComponent();
        }

        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            RefreshVolume();
        }
        private void RefreshVolume()
        {
            var AllAppInfo = VolumeUtilities.EnumerateApplications().ToList();
            lvwVolViewer.Items.Clear();

            if(lvwVolViewer.Columns.Count==0)
            {
                lvwVolViewer.Columns.Add("Session Name");
                lvwVolViewer.Columns.Add("Process Name");
                lvwVolViewer.Columns.Add("Process ID");
                lvwVolViewer.Columns.Add("Volume Setting");
            }

            
            foreach(var iterateApp in AllAppInfo)
            {
                Process AppProcess = null;
                try
                {
                    AppProcess = Process.GetProcessById((int)iterateApp.ProcessID);
                }
                catch(Exception exx)
                {
                    AppProcess = null;
                }
                String ProcessName = AppProcess == null ? "Unknown" : AppProcess.ProcessName;

                ListViewItem buildItem = new ListViewItem(new String[] { iterateApp.Name, ProcessName, iterateApp.ProcessID.ToString(), iterateApp.Volume.ToString() });
                lvwVolViewer.Items.Add(buildItem);

                //iterateApp.Name
                //    iterateApp.Volume
                //    iterateApp.ProcessID


            }


        }

        private void frmVolViewer_Load(object sender, EventArgs e)
        {
            RefreshVolume();
        }
    }
}
