using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using HMI.Model.Module;
using HMI.Model.Module.UI;
using HMI.Model.Module.Services;
using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.Presentation.Twr.Constants;
using HMI.Presentation.Twr.Properties;
using Utilities;
using NLog;
using Microsoft.Practices.CompositeUI;

namespace HMI.Presentation.Twr.UI
{


    public partial class ListaDeParticipantes : UserControl
    {
        private IModelCmdManagerService _CmdManager;
        private StateManagerService _StateManager;
        public void setup([ServiceDependency] IModelCmdManagerService cmdManager, [ServiceDependency] StateManagerService stateManager)
        {
            _CmdManager = cmdManager;
            _StateManager = stateManager;
        }
        public ListaDeParticipantes()
        {
            InitializeComponent();
            initvarhw();
        }

        List<string> GetParticipantes(string sala)
        {
            return _CmdManager.GetListaParticipantesEstado(sala);
        }
        List<string> GetParticipantesConf(string sala)
        {
            return _CmdManager.RefrescaListaParticipantesConf(sala);
        }
        List<string> GetSalasConf(string sala)
        {
            return _CmdManager.RefrescaListaParticipantesConf(sala);
        }

        private void SetIdConferencia(string id)
        {
            if (IdConferencia.Text != id)
            {
                IdConferencia.Text = id;
                listBox1.Items.Clear();
            }
        }
        private void initvarhw()
        {
            Sala = "--";
            count = 0;
            maxtabla = 6;   // elementos que caben en la lista
            corte = 15;     // solo para pruebas internas,
            seg = 0;        // segundos transcurridos
            segesp = 30;      // Segundos de presentación
            oculto = true;  // permite informar a la clase para que se oculte.
        }
        private void initvarsw()
        {
            Sala = "--";
            count = 0;
            maxtabla = 6;   // elementos que caben en la lista
            corte = 15;     // solo para pruebas internas,
            seg = 0;        // segundos transcurridos
            segesp = 30;      // Segundos de presentación
        }

        string Sala = "--";
        int count = 0;
        int maxtabla = 6;   // elementos que caben en la lista
        int corte = 15;     // solo para pruebas internas,
        int seg = 0;        // segundos transcurridos
        int segesp = 30;      // Segundos de presentación
        public bool oculto = true;  // permite informar a la clase para que se oculte.
        private void uiTimer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Visible)
            {
                if (Todos.Checked)
                {
                    int ant = listBox1.Items.Count;
                    if (ant == 0)
                    {
                        listBox1.Items.Add("presenta configurados");
                        List<string> par = GetParticipantesConf(Sala);//participantes configurados
                        listBox1.Items.Clear();
                        if (par != null) foreach (string name in par)
                        {
                            if (listBox1.Items.IndexOf(name) < 0)
                            {
                                count++;
                                listBox1.Items.Add(name);
                            }
                        }
                    }
                }
                else
                {
                    if (IdConferencia.TextLength > 0)
                    {
                        int ant = listBox1.Items.Count;
                        List<string> par = GetParticipantes(Sala);
                        if (ant == par?.Count())
                        {
                            // si se puede ver toda la tabla.
                            // espero x segundos para ocultarla.
                            if (par.Count < maxtabla)
                            {
                                count++;
                                if (count >= segesp)
                                {
                                    //_CmdManager.CancelTlfClick();
                                    _CmdManager.ShowAdButtons(9);
                                    oculto = true;
                                    count = 0;
                                }
                            }
                        }
                        else
                            count = 0;
                        listBox1.Items.Clear();
                        if (par != null) foreach (string name in par)
                            {
                                if (listBox1.Items.IndexOf(name) < 0)
                                {
                                    count++;
                                    listBox1.Items.Add(name);
                                }
                            }
                    }
                }
            }
            if (listBox1.Items.Count > maxtabla)
            {
                listBox1.Items.RemoveAt(0);
            }
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
            if (oculto)
                this.Hide();
            else
                this.Show();

        }

        public void SetSala(string sala)
        {
            if (Sala != sala)
            {
                initvarsw();
                Sala = sala;
                listBox1.Items.Clear();
                SetIdConferencia(Sala);
                listBox1.Visible = true;//hace falta?
                oculto = false;//hace falta;
            }
        }
        public void SetConfiguracionTodos(bool estado=false)
        {
            Todos.Checked = estado;
        }

        private void Todos_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Todos_MouseUp(object sender, MouseEventArgs e)
        {
            listBox1.Items.Clear();
        }
    }
}
