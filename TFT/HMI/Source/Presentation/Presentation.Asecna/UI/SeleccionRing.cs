using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using HMI.Model.Module.Services;
using Microsoft.Practices.CompositeUI;

namespace HMI.Presentation.Asecna.UI
{
    public partial class SeleccionRing : UserControl
    {
        public class Datos
        {
            public string Usuario { get; set; }
            public string Tipo_llamada { get; set; }
            public string Tono { get; set; }
            public string TonoPrio { get; set; }
        }
        public class TìpollamadaTono
        {
            public string Tipo_llamada { get; set; }
            public string Tono { get; set; }

        }


        private StateManagerService _StateManager = null;
        private IModelCmdManagerService _CmdManager = null;
        private IEngineCmdManagerService _CmdManagerEngine = null;
        public void Setup([ServiceDependency] StateManagerService stateManager, [ServiceDependency] IModelCmdManagerService engineCmdManager)
        {
            _StateManager = stateManager;
            _CmdManager = engineCmdManager;
         
        }

        public SeleccionRing()
        {
            InitializeComponent();
            //LoadTones();
            //CargaCombos();
            //TipoLlamadaComboBox.SelectedIndex = 0;
        }

        List<Datos> listaDatos = new List<Datos>();
        List<TìpollamadaTono> listallamadaTono = new List<TìpollamadaTono>();
        string Filename = "tonos.json";
        string usuario = "--";
        //private Dictionary<string, string> tonosPorLlamada = new Dictionary<string, string>();
        static string RingValue = "Ring";
        //static string RingValueDefault = "RingDefault";
        static string RingPrioValue = "RingPrio";
        //static string RingPrioValueDefault = "RingPrioDefault";
        static string No_IntrusivaValue = "RingNoIntrusivo";
        public bool cambiotipo = false;
        public bool salvado = true;
        public bool cambiotono = false;
        public bool cambiotonoprio = false;
        private int indiceAnteriorTipoLlamada=-1;

        private List<string> Tipo_de_llamada = new List<string>()
        {
            //PrioritariaValue,No_IntrusivaValue,
            "PAP","ATS",
            "04 PRF4","05 PRF5","06 PRF6","07 PRF7","08 PRF8","09 PRF9",
            "10 PRF10","11 PRF11","12 PRF12","13 PRF13","14 PRF14","15 PRF15",
            "16 PRF16","17 PRF17","18 PRF18","19 PRF19",
        };
        private List<string> Tonos_posibles = new List<string>()
        {   RingValue,No_IntrusivaValue,RingPrioValue,"Ring01","Ring02","Ring03",
            "Ring04","Ring05","Ring06","Ring07","Ring08","Ring09","Ring10",
        };
        private void CargaComboLlamadas()
        {
            TipoLlamadaComboBox.Items.Clear();
            foreach (string item in Tipo_de_llamada)
            {
                TipoLlamadaComboBox.Items.Add(item);
            }
        }
        private void CargaComboTonos()
        {
            tonoComboBox.Items.Clear();
            tonoprioComboBox.Items.Clear();
            foreach (string item in Tonos_posibles)
            {
                tonoComboBox.Items.Add(item);
                tonoprioComboBox.Items.Add(item);
            }
        }
        private void CargaCombos()
        {
            CargaComboLlamadas();
            CargaComboTonos();
        }
        private void TonosPorDefecto()
        {
            if (_StateManager == null)
                return;
            foreach (string tipoLlamada in Tipo_de_llamada)
                _StateManager.SetTonosPorLlamada(tipoLlamada, RingValue, RingPrioValue);
            // relleno el modelo y motor
            foreach (string tipoLlamada in Tipo_de_llamada)
                _CmdManager?.SetToneporllamadaModel(tipoLlamada, "Ring", "RingPrio");
        }
        private void LoadTonesPorLlamada(string usuario)
        {
            ReadJson(usuario);

        }
        private void LoadTones()
        {
            // Esto sobra si tonos por defecto es cargado en loadtones por llamada
            if (!File.Exists(Filename))
            {
                // si no existe cargo los de defecto, y si si existe? cargo los de defecto y luego actualizo ?
                TonosPorDefecto();
            }

            LoadTonesPorLlamada(usuario);
            if (_StateManager == null)
                return;

#if false

            _StateManager.tonosPorLlamada = tonosPorLlamada;
#else
            //nada
#endif
        }

        public void ReadJson(string usuario)
        {
            //podria llamarse tonospordefecto
            if (_StateManager == null)
                return;

            TonosPorDefecto();
            if (File.Exists(Filename))
            { 

                string json = File.ReadAllText(Filename);
                listaDatos = JsonConvert.DeserializeObject<List<Datos>>(json);
                List<Datos> datosUsuario = listaDatos.Where(d => d.Usuario == usuario).ToList();
                if (datosUsuario?.Count > 0)
                {
                    foreach (Datos t in datosUsuario)
                    {
                        _StateManager.tonosPorLlamada[t.Tipo_llamada] = new string[]{ t.Tono,t.TonoPrio};

                        _CmdManager?.SetToneporllamadaModel(t.Tipo_llamada, t.Tono,t.TonoPrio);
                    }
                }
            }
        }
        public void SalvarJson(string usuario, string tipollamada, string tono)
        {
            ReadJson(usuario);

            var tipollamadatono1 = new TìpollamadaTono
            {
                Tipo_llamada = tipollamada,
                Tono = tono
            };
            var datosNuevo = new Datos
            {
                Usuario = usuario,
                Tipo_llamada = tipollamada,
                Tono = tono,
            };

            bool datosExistentes = listaDatos.Any(d => d.Usuario == usuario && d.Tipo_llamada == tipollamada && d.Tono == tono);
            if (!datosExistentes)
            {
                int indice = listaDatos.FindIndex(d => d.Usuario == usuario && d.Tipo_llamada == tipollamada);
                if (indice != -1)
                {
                    // Eliminar el elemento de la lista
                    listaDatos.RemoveAt(indice);
                }

                listaDatos.Add(datosNuevo);

                // Serializa la lista de nuevo a JSON
                string nuevoJson = JsonConvert.SerializeObject(listaDatos, Formatting.Indented);

                // Escribe el JSON actualizado en el archivo
                File.WriteAllText(Filename, nuevoJson);
            }


    }

    private void SaveSelectionJson(string usuario, string tipollamada, string tono,string tonoprio)
        {
            ReadJson(usuario);
            var datosNuevo = new Datos
            {
                Usuario = usuario,
                Tipo_llamada = tipollamada,
                Tono = tono,
                TonoPrio = tonoprio,
            };

            //bool datosExistentes = listaDatos.Any(d => d.Usuario == usuario && d.Tipo_llamada == tipollamada);
            //if (!datosExistentes)
            {
                int indice = listaDatos.FindIndex(d => d.Usuario == usuario && d.Tipo_llamada == tipollamada);
                if (indice != -1)
                {
                    // Eliminar el elemento de la lista
                    listaDatos.RemoveAt(indice);
                }

                listaDatos.Add(datosNuevo);

            }
            // Serializa la lista de nuevo a JSON
            string nuevoJson = JsonConvert.SerializeObject(listaDatos, Formatting.Indented);

            // Escribe el JSON actualizado en el archivo
            File.WriteAllText(Filename, nuevoJson);
            cambiotipo = false;
            salvado = true;
            cambiotono = false;
            cambiotonoprio = false;
        }
        private void aceptarButton_Click(object sender, EventArgs e)
        {
            string llamada = TipoLlamadaComboBox?.SelectedItem?.ToString();
            string tono = tonoComboBox?.SelectedItem?.ToString();
            string tonoprio = tonoprioComboBox?.SelectedItem?.ToString();
            if (usuario != _StateManager?.Title.Id)
            {
                usuario = _StateManager?.Title.Id;
                // TODO.volver a cargar todo
                LoadTones();
                CargaCombos();
                TipoLlamadaComboBox.SelectedIndex = 0;
            }
            SaveSelectionJson(usuario, llamada, tono, tonoprio);
            _CmdManager.SetToneporllamadaModel(llamada, tono, tonoprio);

        }

        private void CambiarTipoDeLlamada(object sender, EventArgs e)
        {
            if (TipoLlamadaComboBox.SelectedIndex < 0 || _StateManager == null)
                return;
            string llamada = TipoLlamadaComboBox.SelectedItem.ToString();
            if (_StateManager.tonosPorLlamada.ContainsKey(llamada))
            {
                string[] tono = _StateManager.tonosPorLlamada[llamada];
                int index = tonoComboBox.FindStringExact(tono.First());
                if (index != -1)
                {
                    tonoComboBox.SelectedIndex = index;
                }
                int index1 = tonoComboBox.FindStringExact(tono[1]);
                if (index1 != -1)
                {
                    tonoprioComboBox.SelectedIndex = index1;
                }
            }
            //cambiotipo = true;
            salvado = true;
            cambiotono = true;
            cambiotonoprio = true;

        }
        private bool HayCambiosNoGuardados()
        {
            return ((!salvado) && (cambiotono || cambiotonoprio));
        }

        private void GuardarCambios(object sender, EventArgs e)
        {
            // Aquí deberías implementar la lógica para guardar los cambios antes de cambiar el tipo de llamada.
            // Puedes guardar los datos en una base de datos, archivo o realizar cualquier otra acción necesaria.
            // En este ejemplo, simplemente mostramos un cuadro de mensaje para simular el proceso de guardar.
            MessageBox.Show("Guardando cambios antes de cambiar el tipo de llamada...",
                            "Guardando Cambios",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

            CambiarTipoDeLlamada(sender,e);
        }

        private void TipoLlamadaComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (HayCambiosNoGuardados())
            {
                if (TipoLlamadaComboBox.SelectedIndex != indiceAnteriorTipoLlamada)
                MessageBox.Show("¡ADVERTENCIA! Cambios no guardados.\n" +
                                "Guarde los cambios antes de cambiar la selección.",
                                "Cambios no guardados",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);

                TipoLlamadaComboBox.SelectedIndex = indiceAnteriorTipoLlamada;
            }
            else
            {
                // No hay cambios no guardados, podemos cambiar el tipo de llamada directamente.
                CambiarTipoDeLlamada(sender, e);
            }
        }

        private void tonoComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tipollamada = TipoLlamadaComboBox.Text;
            int indice = listaDatos.FindIndex(d => d.Usuario == usuario && d.Tipo_llamada == tipollamada);
            if (indice > 0)
            {
                string tono = listaDatos[indice].Tono;
                string tonoprio = listaDatos[indice].TonoPrio;

            }
            //ReproducirSonidoSeleccionado();
            //cambiotipo = false;
            salvado = false;
            cambiotono = true;
            //cambiotonoprio = false;
        }
        private void tonoprioComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            tonoComboBox_SelectedIndexChanged(sender,e);
        }

        private void escuchar_Click(object sender, EventArgs e)
        {
            ReproducirSonidoSeleccionado();

        }

        private void ReproducirSonidoSeleccionado()
        {
            string tono = tonoComboBox?.SelectedItem?.ToString();
            if (tono != null)
            {
                string pathSonido = $"Resources\\Tones\\{tono}.wav"; // Reemplaza ".wav" con la extensión de archivo correcta
                if (!File.Exists(pathSonido))
                    MessageBox.Show("No se ha encotnrado el fichero {pathSonido}.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                try
                {
                    using (System.Media.SoundPlayer player = new System.Media.SoundPlayer(pathSonido))
                    {
                        player.Play();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al reproducir el sonido: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No se ha seleccionado un tono para esta llamada.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void SeleccionRing_VisibleChanged(object sender, EventArgs e)
        {
            if (usuario != _StateManager?.Title.Id)
            {
                usuario = _StateManager?.Title.Id;
                // TODO.volver a cargar todo
                LoadTones();
                CargaCombos();
                TipoLlamadaComboBox.SelectedIndex = 0;
            }
            else
            {
                // cuando se presenta volver a leer el fichero siempre.
                usuario = _StateManager?.Title.Id;
                // TODO.volver a cargar todo
                LoadTones();
                CargaCombos();
                TipoLlamadaComboBox.SelectedIndex = 0;

            }



        }

    }
}
