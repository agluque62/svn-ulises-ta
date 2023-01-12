using HMI.Infrastructure.Interface.Constants;
using HMI.Model.Module.BusinessEntities;
using HMI.Presentation.Twr.Properties;
using Microsoft.Practices.CompositeUI.EventBroker;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Utilities;



namespace HMI.Presentation.Twr.UI
{

    public partial class ControlRecord : UserControl
    {
        public event EventHandler LevelUp;
        public event EventHandler LevelUpReproduce;

        enum estados
        {
            Deshabilitado,
            Reposo,
            Reproduciendo,
            Error
        };
        private bool _jacks = false;
        private bool _filegrabado = false;
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        private estados _estado=estados.Reposo;
        private estados estado
        {
            get
            {
                return _estado;
            }

            set
            {
                
                if (_estado != value)
                {
                    if (value == estados.Reposo)
                    {
                        if (Jacks)
                        {
                            StopRep();
                            hmiButtonPlay.Enabled = true;
                        }
                    }
                    else if (value == estados.Reproduciendo)
                    {
                        StartRep();
                        hmiButtonPlay.Enabled = false;
                    }
                    else if (value == estados.Deshabilitado)
                    {
                        StopRep();
                        hmiButtonPlay.Enabled = false;
                    }
                    else if (value == estados.Error)
                    {
                        StopRep();
                        hmiButtonPlay.Enabled = false;
                    }
                    if (Jacks)
                        _estado = (estados)value;
                    else
                        _estado = estados.Deshabilitado;
                }
            }
        }

        private int _TiempoMax=30;

        public  int Estado { get => (int)estado; set => estado = (estados)value; }
        public int TiempoMax
        {
            get => (int)_TiempoMax;
            set
            {
                _TiempoMax = value;
                ProgressBar.Maximum = _TiempoMax;
                this.hmiButtonStop.Text = TiempoMax.ToString();
                if (_TiempoMax == 0 && estado == estados.Reposo)
                {
                    estado = estados.Deshabilitado;
                    Timer2();// Puede que no haga falta, se llama refresh, por seguridad lo dejamos.
                }
                if (_TiempoMax > 0 && estado==estados.Deshabilitado)
                    estado = estados.Reposo;
            }
        }
        
        public bool Jacks 
        { 
            get => _jacks;
            set { _jacks = value;
#if _JACKS_
                _jacks = true;
#endif
            }
                
        }
        public bool FileGrabado 
        { 
            get => _filegrabado;
            set
            {
                _filegrabado = value;
            }
        }
        
        public ControlRecord()
        {
            InitializeComponent();
            estado = estados.Deshabilitado;
            string dirName = Settings.Default.DirectorioGLPRxRadio;
            if (!System.IO.Directory.Exists(dirName))
            {
                try { 
                    System.IO.Directory.CreateDirectory(dirName);
                }
                catch (System.IO.IOException /*e*/)
                {
                    _Logger.Warn("Directorio de grabación no está vacío o no existe.");
                }
            }
        }

        private void ProgressBar_Click(object sender, EventArgs e)
        {
            hmiButtonPlay.Show();
            hmiButtonStop.Hide();
            ProgressBar.Hide();
        }

        private void uiTimer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (estado == estados.Reproduciendo)
            {
                this.hmiButtonStop.Text = (ProgressBar.Maximum - ProgressBar.Value).ToString();
                if (ProgressBar.Value < ProgressBar.Maximum)
                    ProgressBar.Value += 1;
                else
                {
                    ProgressBar.Value = 0;
                    estado = estados.Reposo;
                }
            }
            else
                ProgressBar.Value = 0;
        }

        //private void uiTimer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        public void Timer2()
        { 
            //Comprobar si hay ficheros grabados.
            if (estado == estados.Reposo || estado==estados.Deshabilitado)
            {
                try
                {
                    string dirName = Settings.Default.DirectorioGLPRxRadio;
                    if (Directory.Exists(Settings.Default.DirectorioGLPRxRadio))
                    {
                        DirectoryInfo di = new DirectoryInfo(dirName);
                        FileInfo[] fi = di.GetFiles("RxRadio_*.wav", SearchOption.TopDirectoryOnly);
                        if (fi.Length == 0)
                            CambiaFileGrabado(false);
                        if (fi.Length > 0)
                            CambiaFileGrabado(true);
                        if (FileGrabado && Jacks)
                            estado = estados.Reposo;
                        else
                            estado = estados.Deshabilitado;
                    }
                }
                catch (Exception exc)
                {
                    _Logger.Error("Error en Timer 2 Creando fichero.Excepcion " + exc.Message);
                }
            }
        }

        private void uiTimer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer2();
        }

        void BorraFilesGrabados()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(Settings.Default.DirectorioGLPRxRadio);
                FileInfo[] fi = di.GetFiles("RxRadio_*.*", SearchOption.AllDirectories);
                foreach (System.IO.FileInfo f in fi)
                {
                    //_Logger.Info("Purge file", f.Name);
                    try
                    {
                        File.Delete(f.Directory + "/" + f.Name);
                    }
                    catch (System.IO.IOException /*e*/)
                    {
                        _Logger.Warn("Error al intentar borrar el fichero "+f.Name);
                    }
                }
            }
            catch (Exception exc)
            {
                _Logger.Error("Error en BorraFilesGrabados: " + exc.Message);
            }
        }

        private void StartRep()
        {

            hmiButtonPlay.Hide();
            hmiButtonStop.Show();
            ProgressBar.Value = 0;
            ProgressBar.Show();
        }

        public void Reproduciendo()
        {
            if (TiempoMax>0)
                estado = estados.Reproduciendo;
        }

        public void StopReproducion()
        {
            if (this.estado == estados.Reproduciendo)
            {
                estado = estados.Reposo;
            }
        }

        private void StopRep()
        {
            if (estado == estados.Reproduciendo)
            {
                hmiButtonPlay.Show();
                hmiButtonStop.Hide();
                ProgressBar.Hide();
            }
        }

        private void hmiButtonStop_MouseUp(object sender, MouseEventArgs e)
        {
            //StopRep();
            estado = estados.Reposo;
            General.SafeLaunchEvent(LevelUp, this);// Envia comando parar reproduccion
        }

        
        private void hmiButtonPlay_MouseUp(object sender, MouseEventArgs e)
        {
            // este comando llega tambien al desabilitar el boton.
            //StartRep();
            //General.SafeLaunchEvent(LevelDown, this);
            if (estado==estados.Reposo)
            {
                estado = estados.Reproduciendo;
                General.SafeLaunchEvent(LevelUpReproduce, this);
            }
        }

        // Fin de grabacion
        private void ControlRecord_Leave(object sender, EventArgs e)
        {
            General.SafeLaunchEvent(LevelUp, this);
        }

        private void hmiButtonStop_Leave(object sender, EventArgs e)
        {

        }

        private void hmiButtonStop_Click(object sender, EventArgs e)
        {

        }
        
        public void CambiaJacks(bool on)
        {
            if (on && this.FileGrabado && !this.Jacks)
                this.estado = estados.Reposo;
            if (!on && this.FileGrabado && this.Jacks)
            {
                if (estado!=estados.Reposo)
                {
                    if (estado == estados.Reproduciendo)
                        hmiButtonStop_Click(this, null);
                }
                estado = estados.Reposo;
                estado = estados.Deshabilitado;
            }
            //221125
            else if (!on)
            {
                if (estado != estados.Reposo)
                {
                    if (estado == estados.Reproduciendo)
                        hmiButtonStop_Click(this, null);
                }
                estado = estados.Reposo;
                estado = estados.Deshabilitado;
            }
            Jacks = on;
            BorraFilesGrabados();
        }

        public void CambiaFileGrabado(bool on)
        {
            if (on && !FileGrabado && Jacks)
            {
                if (estado == estados.Deshabilitado)
                    estado = estados.Reposo;
            }
            if (!on && FileGrabado && Jacks)
            {
                if (estado == estados.Reposo)
                    estado = estados.Deshabilitado;
                else
                {
                    estado = estados.Reposo;
                    estado = estados.Deshabilitado;
                }
            }
            FileGrabado = on;
        }
        
        public void Habilitado(bool valor)
        {
            if (valor)
            {
                if (estado == estados.Deshabilitado)
                    estado = estados.Reposo;
                if (estado == estados.Error)
                    estado = estados.Reposo;
            }
            else
            {
                if (estado == estados.Reproduciendo)
                    estado = estados.Reposo;
                if (estado == estados.Reposo)
                    estado = estado = estados.Deshabilitado;
                if (estado == estados.Error)
                    estado = estado = estados.Deshabilitado;
            }
        }
    }
}
