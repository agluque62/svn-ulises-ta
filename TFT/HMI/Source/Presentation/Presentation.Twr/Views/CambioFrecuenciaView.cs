using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Services;
using Microsoft.Practices.CompositeUI;
using Microsoft.Practices.CompositeUI.SmartParts;
using HMI.Model.Module.UI;
using NLog;

namespace HMI.Presentation.Twr.Views
{
    [SmartPart]
    public partial class CambioFrecuenciaView : UserControl
    {
        private IModelCmdManagerService _CmdManager = null;
        private StateManagerService _StateManager = null;
        private bool _IsCurrentView = false;
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        public CambioFrecuenciaView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();

            _CmdManager = cmdManager;
            _StateManager = stateManager;
            rellenalista();
            _IsCurrentView = true;
        }

        private void rellenalista()
        {
            listboxFr.Items.Clear();
            foreach (RdDst r in _StateManager.Radio.Destinations)
            {
                listboxFr.Items.Add(r.Frecuency);
            }
            listboxFr.Items.Clear(); 
            foreach (String r in _StateManager.Radio.GetListaFrecuencias())
            {
                if (r.Length > 0)
                    listboxFr.Items.Add(r);

            }
            int c = listboxFr.Items.Count;
            for (int i=c+1;i<=16;i++)
                listboxFr.Items.Add("frecuencia xxx"+i.ToString());
            if (this._HMIButtons.Count==0)
            {
 
                _HMIButtons.Add(hmiButton1);
                _HMIButtons.Add(hmiButton2);
                _HMIButtons.Add(hmiButton3);
                _HMIButtons.Add(hmiButton4);
                _HMIButtons.Add(hmiButton8);
                _HMIButtons.Add(hmiButton7);
                _HMIButtons.Add(hmiButton6);
                _HMIButtons.Add(hmiButton5);
                _HMIButtons.Add(hmiButton12);
                _HMIButtons.Add(hmiButton11);
                _HMIButtons.Add(hmiButton10);
                _HMIButtons.Add(hmiButton9);
                _HMIButtons.Add(hmiButton16);
                _HMIButtons.Add(hmiButton15);
                _HMIButtons.Add(hmiButton14);
                _HMIButtons.Add(hmiButton13);

            }
            int j = 0;
     
            foreach (var r in this._HMIButtons)
            {
                r.MouseUp += new System.Windows.Forms.MouseEventHandler(this.hmiButton_MouseUp);
                r.MouseDown += new System.Windows.Forms.MouseEventHandler(this.hmiButton_MouseDown);
                String n =_StateManager.Radio.GetListaFrecuencias()[j];
                r.Reset(n, false, Color.Gray);
                j = j + 1;
                if (n == "")
                    r.Hide();
            }
            //rdButton1.Name = "HOLA";
            //rdButton1.Id = 0;
            //rdButton1.Reset("1234", "5678", false, true,0, null, null, null, Color.White,Color.Red, Color.AliceBlue, 
            //    Color.AntiqueWhite, Color.Aqua, Color.Azure, FrequencyState.Available, 0);
        }

        // Devuelve el id asociado al nombre de una frecuencia.
        private int GetId(string fr)
        {
            foreach (RdDst r in _StateManager.Radio.Destinations)
            {
                if (r.Frecuency == fr)
                    return r.Id;
            }
            return -1;
        }

        private void _TitleLB_Click(object sender, EventArgs e)
        {
            rellenalista();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int indice = listboxFr.SelectedIndex;
            string fr = listboxFr.SelectedItem.ToString();
            //rellenalista();
            listboxFr.SelectedIndex = indice;
        }

        private void Aceptar_Click(object sender, EventArgs e)
        {
            try
            {
                if (listboxFr.SelectedIndex >= 0)
                {
                    string fr = listboxFr.SelectedItem.ToString();
                    int id = GetId(fr);
                    if (id >= 0)
                    {
                        try
                        {
                             int idsel = _StateManager.Radio.Idsel;
                            _CmdManager.SetNewFrecuency(idsel, fr);
                            _CmdManager.SwitchRadView("RdView", idsel, fr);
                            _IsCurrentView = false;
                        }
                        catch (Exception ex)
                        {
                            //_Logger.Error("ERROR cerrando la vista Cambnio de frecuencia", ex);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string msg = string.Format("ERROR Aceptar_Click");
                _Logger.Error(msg, ex);
            }
        }

        private void Cancelar_Click(object sender, EventArgs e)
        {
            try
            {
                _CmdManager.SwitchRadView("RdView",0,"");
                _IsCurrentView = false;
            }
            catch (Exception ex)
            {
                //_Logger.Error("ERROR cerrando la vista dependencia", ex);
            }
        }

        private void listboxFr_VisibleChanged(object sender, EventArgs e)
        {
            rellenalista();
        }

        private void hmiButton_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (var r in this._HMIButtons)
            {
                int indice = listboxFr.SelectedIndex;
                if ((r == (HMIButton)sender) && indice>=0)
                    r.Reset(r.Text, false, Color.Yellow);
                else if (r.Text.Length > 0)
                    r.Reset(r.Text, false, Color.Gray);
            }
        }

        private void hmiButton_MouseDown(object sender, MouseEventArgs e)
        {
            foreach (var r in this._HMIButtons)
            {
                int indice = -1;
                if (r.Text.Length>0) indice = listboxFr.FindString(r.Text);
                if (r == (HMIButton)sender)
                    if (indice >= 0)
                        listboxFr.SelectedIndex = indice;
            }
        }
    }
}
