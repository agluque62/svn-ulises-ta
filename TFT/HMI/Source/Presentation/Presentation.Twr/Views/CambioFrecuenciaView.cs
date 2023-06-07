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
using Microsoft.Practices.CompositeUI.EventBroker;
using HMI.Infrastructure.Interface.Constants;
using HMI.Infrastructure.Interface;

namespace HMI.Presentation.Twr.Views
{
    [SmartPart]
    public partial class CambioFrecuenciaView : UserControl
    {

        //[EventPublication(EventTopicNames.RdListaFrecuencias, PublicationScope.Global)]
        //public event EventHandler<int> RdListaFrecuencias;

        private IModelCmdManagerService _CmdManager = null;
        private StateManagerService _StateManager = null;
        private bool _IsCurrentView = false;
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private int _tiempo;
        private int TiemMax;

        List<string> listafrecuencias = null;
        public CambioFrecuenciaView([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            InitializeComponent();

            _CmdManager = cmdManager;
            _StateManager = stateManager;
            rellenalista();
            _IsCurrentView = true;
            TiemMax = 3;
        }

        [EventSubscription(EventTopicNames.ActiveViewChanging, ThreadOption.Publisher)]
        public void OnActiveViewChanging(object sender, EventArgs<string> e)
        {
            _tiempo = TiemMax;
            rellenalista();
        }

        private void rellenalista()
        {
            int id = _StateManager.Radio.Idsel;
            listafrecuencias = _StateManager.Radio[id].Frecuencia_Sel;
            listboxFr.Items.Clear();
            listboxFr.Hide();
            foreach (string s in listafrecuencias)
            {
                listboxFr.Items.Add(s);
            }
            listboxFr.Items.Clear(); 
            foreach (String s in listafrecuencias)
            {
                if (s.Length > 0)
                    listboxFr.Items.Add(s);

            }
            int c = listboxFr.Items.Count;
            //for (int i=c+1;i<=16;i++)
            //    listboxFr.Items.Add("frecuencia xxx"+i.ToString());
            if (this._HMIButtons.Count==0)
            {
 
                _HMIButtons.Add(hmiButton1);
                _HMIButtons.Add(hmiButton2);
                _HMIButtons.Add(hmiButton3);
                _HMIButtons.Add(hmiButton4);
                _HMIButtons.Add(hmiButton5);
                _HMIButtons.Add(hmiButton6);
                _HMIButtons.Add(hmiButton7);
                _HMIButtons.Add(hmiButton8);
                _HMIButtons.Add(hmiButton9);
                _HMIButtons.Add(hmiButton10);
                _HMIButtons.Add(hmiButton11);
                _HMIButtons.Add(hmiButton12);
                _HMIButtons.Add(hmiButton13);
                _HMIButtons.Add(hmiButton14);
                _HMIButtons.Add(hmiButton15);
                _HMIButtons.Add(hmiButton16);

            }
            int j = 0;
     
            foreach (var r in this._HMIButtons)
            {
                r.MouseUp += new System.Windows.Forms.MouseEventHandler(this.hmiButton_MouseUp);
                r.MouseDown += new System.Windows.Forms.MouseEventHandler(this.hmiButton_MouseDown);
                String n =_StateManager.Radio.GetListaFrecuencias()[j];
                r.Reset(n, false, Color.Gray);
                if (j >= listboxFr.Items.Count)
                    r.Hide();
                else
                {
                    r.Text = listboxFr.Items[j].ToString();
                    r.Show();
                }
                j = j + 1;
            }
        }

        // Devuelve el id asociado al nombre de una frecuencia.
        private int GetId(string fr)
        {
            int indice = 0;
            int id = _StateManager.Radio.Idsel;
            List<string> listafrecuencias = _StateManager.Radio[id].Frecuencia_Sel;
            foreach (string r in listafrecuencias)
            {
                if (r == fr)
                    return indice;
                indice++;
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
            try
            {
                listboxFr.SelectedIndex = indice;
            }
            catch
            {

            }
            color(sender);
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
                _Logger.Error("ERROR cerrando la vista Cambio de frecuencia", ex);
            }
        }

        private void listboxFr_VisibleChanged(object sender, EventArgs e)
        {
            rellenalista();
            color(sender);
            int indice = listboxFr.SelectedIndex;
            if (indice < 0)
                listboxFr.SelectedItem = 0;
        }

        private void color(object sender)
        {
            foreach (var r in this._HMIButtons)
            {
                int indice = listboxFr.SelectedIndex;
                //_HMIButtons[indice].Reset(r.Text, false, Color.Yellow);
                if (indice>=0 && (r == _HMIButtons[indice]))
                    r.Reset(r.Text, false, Color.Yellow);
                else if (r.Text.Length > 0)
                    r.Reset(r.Text, false, Color.Gray);
            }
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
                    {
                        listboxFr.SelectedIndex = indice;
                        _tiempo = TiemMax;
                    }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (_tiempo>0)
                _tiempo--;
            else if (_tiempo==0)
            {
                Cancelar_Click(sender, e);
                _tiempo--;
            }
        }

        private void CambioFrecuenciaView_Load(object sender, EventArgs e)
        {

        }

        private void hmiButton14_Click(object sender, EventArgs e)
        {

        }

        private void hmiButton10_Click(object sender, EventArgs e)
        {

        }

        //[EventSubscription(EventTopicNames.RdListaFrecuencias, ThreadOption.Publisher)]
        //public void OnRdListaFrecuencias(object sender, EventArgs e)
        //{
        //    rellenalista();
        //}
    }
}
