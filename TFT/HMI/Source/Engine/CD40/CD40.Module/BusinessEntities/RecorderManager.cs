using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;
using System.Threading.Tasks;

using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Snmp;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;
using System.Threading;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
    internal class FileComparer : Object, IComparer<FileInfo>
    {
        public FileComparer() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(FileInfo x, FileInfo y)
        {
            return x.CreationTime.CompareTo(y.CreationTime);
        }

    }

    /// <summary>
    /// Fuentes de Audio para la Grabacion Local del Puesto.
    /// </summary>
    public enum FuentesGlp { RxRadio, TxRadio, Telefonia, RxLc, TxLc, Briefing };

    /// <summary>
    /// 
    /// </summary>
#if DEBUG
    public class RecorderManager
#else
	class RecorderManager
#endif
	{
		const int NUM_DEVICE = 20;              // AGL. 20. Para la grabacion HF... Habia 10...
#if _AUDIOGENERIC_
        /** AGL.CMEDIA */
        public Dictionary<CORESIP_SndDevType, ISndDevIO> LstDispositivos;
#else
        private Dictionary<CORESIP_SndDevType, SndDev> LstDispositivos;
#endif
        private int[] NumRecordingByDevice = new int[NUM_DEVICE];
        private int[] _SessionsFile = { -1, -1, -1, -1, -1, -1 };
        private int[] _SessionsId = { -1, -1, -1, -1, -1, -1 };
        private string[] _SessionsFileName = { "", "", "", "", "", "" };
        private bool[] _GlpSessionsStarted = new bool[6];
        private List<int>[] _GlpCallId = new List<int>[6];
        private bool _LocalRecordingEnabled = true;//RQF35
        private bool _LocalRecordingOnlyRadio = true;//RQF35
        private long _TempMinRecorderRadio = 1000;//RQF35
        private long _TiempoGrabacionRecorderRadio = 30;//#7214 segundos, 30 segundos
        private long _TiempoAlmacenamRecorderRadio = 1800;//#7214 segundos, 30 minutos
        private object _Sync = new object();

        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private System.Timers.Timer _SupervisorTimer = new System.Timers.Timer (5*60*1000);
        private System.Timers.Timer _BriefingSessionTimer = new System.Timers.Timer(Settings.Default.BriefingSessionTimer*60*1000);
        private System.Timers.Timer _SupervisorLengthRecording = new System.Timers.Timer(15000);

        public event GenericEventHandler<StateMsg<bool>> BriefingChanged;
        public event GenericEventHandler<StateMsg<bool>> FileRecordedChanged;
        public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
        static Semaphore semaforo_move;
        public RecorderManager(bool enable, bool onlyradio = false)
        {
            // TODO: Complete member initialization
            this._LocalRecordingEnabled = enable;
            this._LocalRecordingOnlyRadio = onlyradio;
            this._TempMinRecorderRadio = Settings.Default.TempMinRecorderRadio;
            this._TiempoGrabacionRecorderRadio = Settings.Default.TiempoGrabacionRecorderRadio;//#7214
            this._TiempoAlmacenamRecorderRadio = Settings.Default.TiempoAlmacenamRecorderRadio;//#7214
            _Logger.Info("Grabacion habilitada: " + enable.ToString()+" Radio" + onlyradio.ToString());
            if (onlyradio)
                PurgeFilesRadio(true);

        }

        public bool LocalRecordingOnlyRadio
        {
            set 
            { 
                if (_LocalRecordingOnlyRadio != value)
                {
                    _LocalRecordingOnlyRadio = value;
                   
                }
            }
            get { return _LocalRecordingOnlyRadio; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Briefing
        {
            get { return this._LocalRecordingEnabled && _GlpSessionsStarted[(int)FuentesGlp.Briefing]; }
        }

        /// <summary>
        /// 
        /// </summary>
		public void Init()
		{
            Top.Cfg.ConfigChanged += OnConfigChanged;

            _SupervisorTimer.AutoReset = true;
            _SupervisorTimer.Elapsed += OnSupervisorTimerElapsed;
            _SupervisorTimer.Enabled = true && this._LocalRecordingEnabled;    // ((Top.Cfg.Permissions & Permissions.Replay) == Permissions.Replay);

            _SupervisorLengthRecording.AutoReset = true;
            _SupervisorLengthRecording.Elapsed += OnSupervisorLengthRecordingTimerElapsed;
            _SupervisorLengthRecording.Enabled = true && this._LocalRecordingEnabled;

			// Tarea que supervisa los ficheros de audio grabados.
            Task.Run(() =>
            {
                while(true)
                {
                    Task.Delay(1000).Wait();
                    PurgeFilesRadio();

                }
            });

            _BriefingSessionTimer.AutoReset = false;
            _BriefingSessionTimer.Elapsed += new ElapsedEventHandler(_SupervisorTimer_Elapsed);
            _BriefingSessionTimer.Enabled = false;

            for (int i = 0; i < 6;i++)
            {
                _GlpCallId[i] = new List<int>();
            }

            try
            {
                /* AGL.REC Directorio de Grabacion Local Configurable
                System.IO.Directory.Delete("Recording", true);
                 * */
                if (Directory.Exists(Settings.Default.DirectorioGLP)) 
                    System.IO.Directory.Delete(Settings.Default.DirectorioGLP, true);
                /* Fin Modificacion */
                if (Directory.Exists(Settings.Default.DirectorioGLPRxRadio))
                {
                    // da problemas al borrar y crear
                    //Directory.Delete(Settings.Default.DirectorioGLPRxRadio, true);
                }
            }
            catch (System.IO.IOException /*e*/)
            {
                _Logger.Warn("Directorio de grabación no está vacío o no existe.");
            }

            semaforo_move = new Semaphore(1, 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _SupervisorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SessionGlp(FuentesGlp.Briefing, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listaDispositivos"></param>
#if _AUDIOGENERIC_
        /// AGL.CMEDIA
        public void Start()
#else
		public void Start(Dictionary<CORESIP_SndDevType, ISndDevIO> listaDispositivos)
#endif
		{
#if _AUDIOGENERIC_

            foreach (ISndDevIO sndDevice in LstDispositivos.Values)
#else
            LstDispositivos = listaDispositivos;
			foreach (SndDev sndDevice in LstDispositivos.Values)            
#endif
            {
				// Subir señal de grabación ???
#if _AUDIOGENERIC_
                sndDevice.SenGrabacion(false);
#else
                sndDevice.SetGpio(HwManager.OUT_GRABACION, HwManager.OFF);            
#endif
            }
		}

        /// <summary>
        /// 
        /// </summary>
		public void End()
		{
			if (LstDispositivos == null)
				return;
#if _AUDIOGENERIC_
            /** AGL.CMEDIA */
            foreach (ISndDevIO sndDevice in LstDispositivos.Values)
#else
            foreach (SndDev sndDevice in LstDispositivos.Values)
#endif
            {		// Bajar señal de grabación				
#if _AUDIOGENERIC_
                sndDevice.SenGrabacion(false);
#else
                sndDevice.SetGpio(HwManager.OUT_GRABACION, HwManager.OUT);
#endif
            }

            for (int i = 0; i < 6; i++)
            {
                if (_SessionsFile[i] >= 0 && _GlpSessionsStarted[i])
                {
                    SipAgent.DestroyWavRecorder(_SessionsFile[i]);
                    _SessionsFile[i] = -1;
                    _SessionsFileName[i] = string.Empty;
                }
            }
		}

        /// <summary>
        /// Sube o Baja la señal de grabación ??? según el tipo de llamada y el estado del puesto....
        /// </summary>
        /// <param name="callType"></param>
        /// <param name="rec"></param>
		public void Rec(CORESIP_CallType callType, bool rec)
		{
#if _AUDIOGENERIC_
            /** AGL.CMEDIA */
            ISndDevIO snd;
#else
            SndDev snd;
#endif

			switch (callType)
			{
				case CORESIP_CallType.CORESIP_CALL_IA:
					if (Top.Mixer.SplitMode == SplitMode.Off)
					{
						if (LstDispositivos.TryGetValue(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, out snd))
#if _AUDIOGENERIC_
                            snd.SenGrabacion(rec);
#else
                            snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
					}
					if (LstDispositivos.TryGetValue(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, out snd))
#if _AUDIOGENERIC_
                            snd.SenGrabacion(rec);
#else
                        	snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
					break;

				case CORESIP_CallType.CORESIP_CALL_DIA:
					if (Top.Mixer.SplitMode == SplitMode.Off || Top.Mixer.SplitMode == SplitMode.RdLc)
					{
						if (LstDispositivos.TryGetValue(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, out snd))
#if _AUDIOGENERIC_
                            snd.SenGrabacion(rec);
#else
                            snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
					}
					if (Top.Mixer.SplitMode == SplitMode.Off || Top.Mixer.SplitMode == SplitMode.LcTf)
					{
						if (LstDispositivos.TryGetValue(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, out snd))
#if _AUDIOGENERIC_
                            snd.SenGrabacion(rec);
#else
                            snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
					}
					break;

				case CORESIP_CallType.CORESIP_CALL_RD:
					if (Top.Mixer.SplitMode == SplitMode.Off || Top.Mixer.SplitMode == SplitMode.RdLc)
					{
						if (LstDispositivos.TryGetValue(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, out snd))
#if _AUDIOGENERIC_
                            snd.SenGrabacion(rec);
#else
                            snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
					}
					if (Top.Mixer.SplitMode == SplitMode.Off || Top.Mixer.SplitMode == SplitMode.LcTf)
					{
						if (LstDispositivos.TryGetValue(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, out snd))
#if _AUDIOGENERIC_
                            snd.SenGrabacion(rec);
#else
                            snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
					}
					break;
			}
		}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="enable"></param>
            public void EnableSupervisor(bool enable)
        {
            _SupervisorTimer.Enabled = enable && this._LocalRecordingEnabled && ((Top.Cfg.Permissions & Permissions.Replay) == Permissions.Replay);
        }

        /// <summary>
        /// AGL-REC.... (2)
        /// </summary>
        /// <param name="who"></param>
        /// <param name="rec"></param>
        internal void Rec(CORESIP_SndDevType who, bool rec)
		{
#if _AUDIOGENERIC_
            /** AGL.CMEDIA */
            ISndDevIO snd;
#else
            SndDev snd;
#endif

            if (LstDispositivos == null ||LstDispositivos.Count==0)
                return;

            // Poner la Salida de Grabación.
			if (LstDispositivos.TryGetValue(who, out snd))
#if _AUDIOGENERIC_
                snd.SenGrabacion(rec);
#else
                snd.SetGpio(HwManager.OUT_GRABACION, rec ? HwManager.ON : HwManager.OFF);
#endif
            // Gestión para añadir o quitar el dispositivo a la mezcla de grabacion.
            if (rec && NumRecordingByDevice[(int)who] == 0)
            {
                NumRecordingByDevice[(int)who] = 1;
                Top.Mixer.LinkRecord(who, rec);
            }
            else if (!rec && NumRecordingByDevice[(int)who] == 1)
            {
                NumRecordingByDevice[(int)who] = 0;
                Top.Mixer.LinkRecord(who, rec);
            }
		}

        class item
        {
            public string Name;
            public DateTime Datetime;
            public long Lenght;
            public int Seg;
            public String Text;
            item(string name, DateTime fecha, long len, int seg,String path)
            {
                Name = name;
                Datetime = fecha;
                Lenght = len;
                Seg = seg;
                Text = path;
            }
            item() 
            {
            }
        }

        private void PurgeFilesRadio(bool allrecords = false)
        {
            try
            {
                string dirName = Settings.Default.DirectorioGLPRxRadio;
                if (!System.IO.Directory.Exists(dirName))
                    System.IO.Directory.CreateDirectory(dirName);
                DirectoryInfo di = new DirectoryInfo(Settings.Default.DirectorioGLPRxRadio);
                FileInfo[] fi;
                fi = di.GetFiles("RxRadio_*.*", SearchOption.AllDirectories);
                if (fi.Length == 0)
                {
                    General.SafeLaunchEvent(FileRecordedChanged, this, new StateMsg<bool>(false));// Deshabilito reproduccion
                    return;
                }
                Array.Sort(fi, new FileComparer());
                FileInfo lastInfo = fi[fi.Length - 1];
                long ms = lastInfo.Length * 1000L / 16000L;
                long secmax = (long)(DateTime.Now - lastInfo.CreationTime).TotalSeconds;

                //_Logger.Info($"Compruebo secmax:{secmax} _TiempoAlmacenamRecorderRadio: {_TiempoAlmacenamRecorderRadio}");

                if (ms < (long)_TempMinRecorderRadio)
                {
                    _Logger.Info("Purge file:"+ lastInfo.Name);
                    try
                    {
                        File.Delete(lastInfo.Directory + "/" + lastInfo.Name);
                    }
                    catch (System.IO.IOException /*e*/)
                    {
                        _Logger.Warn("Error al intentar borrar el fichero " + lastInfo.Name);
                    }

                    fi[fi.Length - 1].Delete();
                    General.SafeLaunchEvent(FileRecordedChanged, this, new StateMsg<bool>(false));
                }
                else if (secmax > _TiempoAlmacenamRecorderRadio)//#7214 221121
                {
                    _Logger.Info("Purge file: " + lastInfo.Name);
                    try
                    {
                        //File.Move(lastInfo.Directory + "/" + lastInfo.Name, lastInfo.Directory + "/" + lastInfo.Name.Replace("@", ""));
                        File.Delete(lastInfo.Directory + "/" + lastInfo.Name);
                    }
                    catch (System.IO.IOException /*e*/)
                    {
                        _Logger.Warn("Error al intentar borrar el fichero " + lastInfo.Name);
                    }
                    fi[fi.Length - 1].Delete();
                    General.SafeLaunchEvent(FileRecordedChanged, this, new StateMsg<bool>(false));
                }
                else if (secmax > _TiempoGrabacionRecorderRadio*20)
                {
                    try
                    {
                        File.Delete(lastInfo.Directory + "/" + lastInfo.Name);
                    }
                    catch (System.IO.IOException /*e*/)
                    {
                        _Logger.Warn("Error al intentar borrar el fichero " + lastInfo.Name);
                    }
                    fi[fi.Length - 1].Delete();
                    General.SafeLaunchEvent(FileRecordedChanged, this, new StateMsg<bool>(false));
                }
                else
                {
                    //221201
                    //General.SafeLaunchEvent(FileRecordedChanged, this, new StateMsg<bool>(true));


                    foreach (System.IO.FileInfo f in fi)
                    {
                        if (!f.Name.Contains("@") &&
                            (allrecords || f.Name != lastInfo.Name))
                        {
                            {
                                _Logger.Info("Purge file: "+ f.Name);
                                try
                                {
                                    File.Delete(f.Directory + "/" + f.Name);
                                }
                                catch (System.IO.IOException /*e*/)
                                {
                                    _Logger.Warn("Error al intentar borrar el fichero " + f.Name);
                                }
                            }
                        }
                    }

                    General.SafeLaunchEvent(FileRecordedChanged, this, new StateMsg<bool>(true));// 221127 lo envio dos veces.
                }
            }
            catch (System.IO.IOException /*e*/)
            {
                _Logger.Warn("Cannot purge files.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCall"></param>
        /// <param name="fuente"></param>
        /// <param name="iniciar"></param>
        internal void SessionGlp(int idCall, FuentesGlp fuente, bool iniciar,bool finalizar=true)
        {
            if (!this._LocalRecordingEnabled)
                return;
            //lalm 220110
            if (this._LocalRecordingOnlyRadio && !(fuente == FuentesGlp.RxRadio || fuente == FuentesGlp.RxRadio))
            {
                
                return;
            }
            lock (_Sync)
            {
                try
                {
                // Si no está la grabación sobre radio arrancada.
                if (iniciar && !_GlpSessionsStarted[(int)fuente])
                {
                    _GlpSessionsStarted[(int)fuente] = !_GlpSessionsStarted[(int)fuente];

                    if (_GlpSessionsStarted[(int)fuente] && _SessionsFile[(int)fuente] == -1)
                    {
                        /* AGL.REC Directorio de Grabacion Local Configurable
                        string dirName = "Recording/" + fuente.ToString();
                         * */
                        string dirName = Settings.Default.DirectorioGLP + fuente.ToString();
                            /* Fin Modificacion */
                        if (_LocalRecordingOnlyRadio)
                            dirName = Settings.Default.DirectorioGLPRxRadio;
                        if (!System.IO.Directory.Exists(dirName))
                            System.IO.Directory.CreateDirectory(dirName);

                        string fileName = dirName + "/" + "@" + fuente.ToString() + "_" + Top.Cfg.MainId + "_" +
                            DateTime.Now.TimeOfDay.Hours + "_" +
                            DateTime.Now.TimeOfDay.Minutes + "_" +
                            DateTime.Now.TimeOfDay.Seconds + ".wav";
                        _SessionsFile[(int)fuente] = SipAgent.CreateWavRecorder(fileName);
                        _SessionsFileName[(int)fuente] = fileName;
                        _Logger.Info("Recording GLP. Filename: " + _SessionsFileName[(int)fuente]);

                    }

                    Top.Mixer.Link(idCall, _SessionsFile[(int)fuente], MixerDir.Send, fuente);
                    _GlpCallId[(int)fuente].Add(idCall);

                    if (fuente == FuentesGlp.Telefonia)
                        Top.Mixer.LinkGlpTfl(_SessionsFile[(int)fuente]);

                }
                // Si la grabacion ya esta esta arrancada.
                else if (iniciar && (fuente == FuentesGlp.RxRadio || fuente == FuentesGlp.Telefonia) && _SessionsFile[(int)fuente] != -1)
                {
                    // Comprueba si idcall no está y si es así lo incluye.
                    if (!_GlpCallId[(int)fuente].Contains(idCall))
                    {
                        Top.Mixer.Link(idCall, _SessionsFile[(int)fuente], MixerDir.Send, fuente);
                        _GlpCallId[(int)fuente].Add(idCall);
                    }
                }
                // si se envia quitar y la grabacion esta arrancada
                else if (!iniciar && _GlpSessionsStarted[(int)fuente])
                {
                    // Comprueba si idcall esta en la llamada, y lo quita.
                    if (_GlpCallId[(int)fuente].Contains(idCall))
                        _GlpCallId[(int)fuente].Remove(idCall);

                    // si no quedan idcalls en la grabación Finaliza la grabación.
                    // nuevo parametro finalizar=true para que finalice la llamada.
                    if ((_GlpCallId[(int)fuente].Count == 0) && finalizar)
                    {
                        Top.Mixer.Unlink(_SessionsFile[(int)fuente]);
                        if (_SessionsFile[(int)fuente] >= 0)
                        {
                            SipAgent.DestroyWavRecorder(_SessionsFile[(int)fuente]);
                            _SessionsFile[(int)fuente] = -1;
                            _GlpSessionsStarted[(int)fuente] = false;
                            semaforo_move.WaitOne();
                                try
                           {
                                File.Move(_SessionsFileName[(int)fuente], _SessionsFileName[(int)fuente].Replace("@", ""));
                                AjustaTiempoFichero(_SessionsFileName[(int)fuente].Replace("@", ""), (int)_TiempoGrabacionRecorderRadio);
                            }
                            catch (System.IO.IOException /*e*/)
                            {
                                File.Delete(_SessionsFileName[(int)fuente]);
                                _Logger.Warn("GLP.SesionGlp. El fichero ya existe.");
                            }
                           semaforo_move.Release();

                                _SessionsFileName[(int)fuente] = string.Empty;
                            }
                            PurgeFilesRadio();
                        }
                        //_GlpCallId[(int)fuente] = 0;
                    }
                }
                catch (Exception exc)
                {
                    _Logger.Error("GLP.SesionGlp Excepcion "+ exc.Message);
                }
            }
        }
        /// <summary>
        /// AjustaTiempoFichero
        /// Cambia el tamaño de un fichero de audio al tiempo maximo permitido, dejanod los ultimos segundos
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="tiempo"></param>
        void AjustaTiempoFichero(string Name,int tiempo)
        {
            WaveIO waveio = new WaveIO();
            string temp = Name+".tmp";
            File.Move(Name, temp);
            waveio.Recorta(temp,Name, 0, _TiempoGrabacionRecorderRadio*1000, true);
            File.Delete(temp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fuente"></param>
        /// <param name="iniciar"></param>
        internal void SessionGlp(FuentesGlp fuente, bool iniciar)
        {
            if (!this._LocalRecordingEnabled)
                return;
            //lalm 220110
            // Solo permito Iniciar grabacion si estando habilitado onlyradio la fuente es RxRadio.
            if (iniciar && this._LocalRecordingOnlyRadio && !(fuente == FuentesGlp.RxRadio))
                return;

            lock (_Sync)
            {
                try { 
                    if (iniciar != _GlpSessionsStarted[(int)fuente])
                    {
                        _GlpSessionsStarted[(int)fuente] = !_GlpSessionsStarted[(int)fuente];

                        if (fuente == FuentesGlp.Briefing)
                        {
                            General.SafeLaunchEvent(BriefingChanged, this, new StateMsg<bool>(Briefing));
                            _BriefingSessionTimer.Enabled = true;

                            if (Settings.Default.SNMPEnabled == 1 && iniciar)
                            {
                                Top.WorkingThread.Enqueue("SetSnmp", delegate()
                                {
                                    string evento = Top.Cfg.PositionId + "_1";
                                    General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.StartingBriefingSessionOid, evento));
                                });
                            }
                        }

                        if (_GlpSessionsStarted[(int)fuente] && _SessionsFile[(int)fuente] == -1)
                        {
                            /* AGL.REC Directorio de Grabacion Local Configurable
                            string dirName = "Recording/" + fuente.ToString();
                             * */
                            string dirName = Settings.Default.DirectorioGLP + fuente.ToString();
                            /* Fin Modificacion */
                            if (_LocalRecordingOnlyRadio)
                                dirName = Settings.Default.DirectorioGLPRxRadio;

                            if (!System.IO.Directory.Exists(dirName))
                                System.IO.Directory.CreateDirectory(dirName);

                            string fileName = dirName + "/" + "@" + fuente.ToString() + "_" + Top.Cfg.MainId + "_" +
                                DateTime.Now.TimeOfDay.Hours + "_" +
                                DateTime.Now.TimeOfDay.Minutes + "_" +
                                DateTime.Now.TimeOfDay.Seconds + ".wav";
                            _SessionsFile[(int)fuente] = SipAgent.CreateWavRecorder(fileName);
                            _SessionsFileName[(int)fuente] = fileName;
                            _Logger.Info("Recording GLP. Filename: " + _SessionsFileName[(int)fuente]);

                            switch (fuente)
                            {
                                case FuentesGlp.Briefing:
                                    Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpRd, fuente);
                                    Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpTlf, fuente);
                                    break;
                                case FuentesGlp.TxRadio:
                                    Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpRd, fuente);
                                    break;
                                case FuentesGlp.TxLc:
                                    Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpLc, fuente);
                                    break;
                                case FuentesGlp.RxRadio:
                                    foreach (int idCall in _GlpCallId[(int)fuente])
                                    {
                                        try
                                        {
                                            //Top.Mixer.Unlink(idCall);
                                            Top.Mixer.Link(idCall, _SessionsFile[(int)fuente], MixerDir.Send, fuente);
                                        }
                                        catch (Exception e)
                                        {
                                            _Logger.Error("SessionGLP-Rx: ", e);
                                        }
                                    }

                                    //if (_SessionsId[(int)FuentesGlp.RxRadio] != -1)
                                    //    Top.Mixer.Link(_SessionsId[(int)FuentesGlp.RxRadio], _SessionsFile[(int)fuente], MixerDir.Send);
                                    break;
                                default:
                                    break;

                            }
                        }
                        else if (!_GlpSessionsStarted[(int)fuente] && (_SessionsFile[(int)fuente] >= 0))
                        {
                            _GlpCallId[(int)fuente].Clear();

                            Top.Mixer.Unlink(_SessionsFile[(int)fuente]);
                            if (_SessionsFile[(int)fuente] >= 0)
                            {
                                SipAgent.DestroyWavRecorder(_SessionsFile[(int)fuente]);
                                _SessionsFile[(int)fuente] = -1;
                                //_SessionsId[(int)fuente] = -1;
                                _GlpSessionsStarted[(int)fuente] = false;
                                semaforo_move.WaitOne();
                                try
                                {
                                    File.Move(_SessionsFileName[(int)fuente], _SessionsFileName[(int)fuente].Replace("@", ""));
                                    AjustaTiempoFichero(_SessionsFileName[(int)fuente].Replace("@", ""), (int)_TiempoGrabacionRecorderRadio);
                                }
                                catch (System.IO.IOException /*e*/)
                                {
                                    File.Delete(_SessionsFileName[(int)fuente]);
                                    _Logger.Warn("GLP.SesionGlp. El fichero ya existe.");
                                }

                                _SessionsFileName[(int)fuente] = string.Empty;
                                semaforo_move.Release();
                            }

                            if (fuente == FuentesGlp.Briefing && Settings.Default.SNMPEnabled == 1)
                            {
                                Top.WorkingThread.Enqueue("SetSnmp", delegate()
                                {
                                    string evento = Top.Cfg.PositionId + "_0";
                                    General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.StartingBriefingSessionOid, evento));
                                });
                            }
                            //_GlpCallId[(int)fuente] = 0;
                            PurgeFilesRadio();
                        }
                    }
                }
                catch (Exception exc)
                {
                    _Logger.Error("GLP.SesionGlp Excepcion " + exc.Message);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="fuente"></param>
        /// LALM __SessionsId no parece que se use para nada...
        /// SetIdSession no se utiliza.
        public void SetIdSession(int id, FuentesGlp fuente)
        {
            _SessionsId[(int)fuente] = id;
            //if (!_GlpCallId[(int)fuente].Contains(id))
              //  _GlpCallId[(int)fuente].Add(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        private void ResetRecording(FuentesGlp fuente)
        {
            if (!this._LocalRecordingEnabled)
                return;

            _Logger.Info("ResetRecording source: " + fuente.ToString());

            //SessionGlp(_GlpCallId[(int)fuente], fuente, false);
            ResetSessionGlp(fuente);
            //System.Threading.Thread.Sleep(200);
            //SessionGlp(fuente, true);
        }

        private void ResetSessionGlp(FuentesGlp fuente)
        {
            if (!this._LocalRecordingEnabled)
                return;

            // Recording off
            if (_GlpSessionsStarted[(int)fuente] && (_SessionsFile[(int)fuente] >= 0))
            {
                Top.Mixer.Unlink(_SessionsFile[(int)fuente]);
                SipAgent.DestroyWavRecorder(_SessionsFile[(int)fuente]);

                _SessionsFile[(int)fuente] = -1;
                _GlpSessionsStarted[(int)fuente] = false;
                semaforo_move.WaitOne();
                try
                {
                    File.Move(_SessionsFileName[(int)fuente], _SessionsFileName[(int)fuente].Replace("@", ""));
                    AjustaTiempoFichero(_SessionsFileName[(int)fuente].Replace("@", ""), (int)_TiempoGrabacionRecorderRadio);
                }
                catch (System.IO.IOException /*e*/)
                {
                    File.Delete(_SessionsFileName[(int)fuente]);
                    _Logger.Warn("GLP.SesionGlp. El fichero ya existe.");
                }

                _SessionsFileName[(int)fuente] = string.Empty;
                semaforo_move.Release();
                if (fuente == FuentesGlp.Briefing && Settings.Default.SNMPEnabled == 1)
                {
                    Top.WorkingThread.Enqueue("SetSnmp", delegate()
                    {
                        string evento = Top.Cfg.PositionId + "_0";
                        General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.StartingBriefingSessionOid, evento));
                    });
                }
            }

            // Recording on
            _GlpSessionsStarted[(int)fuente] = true;

            if (_SessionsFile[(int)fuente] == -1)
            {
                /* AGL.REC Directorio de Grabacion Local Configurable
                string dirName = "Recording/" + fuente.ToString();
                 * */
                string dirName = Settings.Default.DirectorioGLP + fuente.ToString();
                /* Fin Modificacion */
                if (_LocalRecordingOnlyRadio)
                    dirName = Settings.Default.DirectorioGLPRxRadio;

                if (!System.IO.Directory.Exists(dirName))
                    System.IO.Directory.CreateDirectory(dirName);

                string fileName = dirName + "/" + "@" + fuente.ToString() + "_" + Top.Cfg.MainId + "_" +
                    DateTime.Now.TimeOfDay.Hours + "_" +
                    DateTime.Now.TimeOfDay.Minutes + "_" +
                    DateTime.Now.TimeOfDay.Seconds + ".wav";
                _SessionsFile[(int)fuente] = SipAgent.CreateWavRecorder(fileName);
                _SessionsFileName[(int)fuente] = fileName;
                _Logger.Info("Recording GLP. Filename: " + _SessionsFileName[(int)fuente]);

                switch (fuente)
                {
                    case FuentesGlp.Briefing:
                        Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpRd, fuente);
                        Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpTlf, fuente);
                        break;
                    case FuentesGlp.TxRadio:
                        Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpRd, fuente);
                        break;
                    case FuentesGlp.TxLc:
                        Top.Mixer.LinkGlp(_SessionsFile[(int)fuente], MixerDev.MhpLc, fuente);
                        break;
                    case FuentesGlp.RxRadio:
                        lock (_GlpCallId)
                        {
                            foreach (int idCall in _GlpCallId[(int)fuente])
                            {
                                Top.Mixer.Link(idCall, _SessionsFile[(int)fuente], MixerDir.Send, fuente);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSupervisorLengthRecordingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_LocalRecordingOnlyRadio)
            {
                PurgeFilesRadio(false);
                if (_GlpSessionsStarted[(int)FuentesGlp.RxRadio] && _SessionsFileName[(int)FuentesGlp.RxRadio] != string.Empty)
                {
                    FileInfo fi = new FileInfo(_SessionsFileName[(int)FuentesGlp.RxRadio]);
                    _Logger.Info("Supervisor GLP: GLP session started. Recording file: " + fi.FullName);
                    int interval =  60 ;//en segundos
                    if ((_TiempoGrabacionRecorderRadio > _TempMinRecorderRadio/1000) && _TiempoGrabacionRecorderRadio < _TiempoAlmacenamRecorderRadio)
                        interval = (int)_TiempoGrabacionRecorderRadio;
                    if (fi != null && fi.Exists && fi.Length > interval * 16000 /*interval en segundos, aprox. */)
                        ResetRecording(FuentesGlp.RxRadio);
                    if (!Top.Rd.AnySquelch)
                        ResetRecording(FuentesGlp.RxRadio);
                }
            }
            else
            {
                for (FuentesGlp i = FuentesGlp.RxRadio; i <= FuentesGlp.TxLc; i++)
                {
                    if (_GlpSessionsStarted[(int)i] && _SessionsFileName[(int)i] != string.Empty)
                    {
                        FileInfo fi = new FileInfo(_SessionsFileName[(int)i]);
                        _Logger.Info("Supervisor GLP: GLP session started. Recording file: " + fi.FullName);
                        int interval = (i == FuentesGlp.RxRadio || i == FuentesGlp.TxRadio) ? (int)_TiempoGrabacionRecorderRadio : 1800;
                        if (fi != null && fi.Exists && fi.Length > interval * 16000 /*interval segundos, aprox. */)
                            ResetRecording(i);
                    }
                }
            }

            _SupervisorLengthRecording.Enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSupervisorTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                /* AGL.REC Directorio de Grabacion Local Configurable
                DirectoryInfo di = new System.IO.DirectoryInfo("Recording");
                 * */
                DirectoryInfo di = new System.IO.DirectoryInfo(Settings.Default.DirectorioGLP);
                /* Fin Modificacion */
                if (_LocalRecordingOnlyRadio)
                    di = new System.IO.DirectoryInfo(Settings.Default.DirectorioGLPRxRadio);

                FileInfo[] fi = di.GetFiles("*.*", SearchOption.AllDirectories);

                if (fi.Length == 0)
                    return;

                _Logger.Info("Supervisor GLP: Removing older files");
                Array.Sort(fi, new FileComparer());

                FileInfo lastInfo = fi[fi.Length - 1];

                foreach (System.IO.FileInfo f in fi)
                {
                    if (!f.Name.Contains("@"))
                    {
                        if ((lastInfo.LastWriteTime.Ticks - f.LastWriteTime.Ticks) * 1e-7 >= Settings.Default.RecordingDeep * 60)
                        {
                            File.Delete(f.Directory + "/" + f.Name);
                        }
                    }
                }
            }
            catch (DirectoryNotFoundException /*ex*/)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        private void OnConfigChanged(object sender)
        {

            // Aqui se trata la variable que viene de configuración para informar si esta la grabacion habilitada.
            if ( (Top.Cfg.Permissions & Permissions.ReplayOnlyRadio) == Permissions.ReplayOnlyRadio)
            {
                this._LocalRecordingEnabled = true;
                this._LocalRecordingOnlyRadio = true;
            }
            else
            {
                this._LocalRecordingEnabled = false;
                this._LocalRecordingOnlyRadio = false;
            }


           try
            {
                /* AGL.REC Directorio de Grabacion Local Configurable
                Directory.Delete("Recording", true);
                 * */
                if (Directory.Exists (Settings.Default.DirectorioGLP))
                    Directory.Delete(Settings.Default.DirectorioGLP, true);
                /* Fin Modificacion */
                if (Directory.Exists(Settings.Default.DirectorioGLPRxRadio))
                {
                    // Da problemas al borrar y crear
                    //Directory.Delete(Settings.Default.DirectorioGLPRxRadio, true);
                    //220116 no borro el directorio al cambiar de configuracion
                    //Directory.Delete(Settings.Default.DirectorioGLPRxRadio, true);
                }
            }
            catch (System.IO.IOException /*e*/)
            {
                _Logger.Warn("Directorio de grabación no está vacío o no existe.");
            }
            
        }
    }

    /// <summary>
    /// 
    /// </summary>
#if DEBUG
    public class ReplayManager
#else
    class ReplayManager
#endif
    {
        public event GenericEventHandler<StateMsg<bool>> PlayingChanged;
        public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
        public event GenericEventHandler<StateMsg<bool>> FileRecordedChanged1;

        /// <summary>
        /// 
        /// </summary>
        public bool Replaying
        {
            get { return _Replaying; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcion"></param>
        /// <param name="via"></param>
        /// <param name="fileName"></param>
        /// <param name="fileLength"></param>
        public void DoFunction(FunctionReplay funcion, ViaReplay via, string fileName, long fileLength)
        {
       
            switch (funcion)
            {
                case FunctionReplay.Stop:
                    if (_ReplayTone != -1)
                    {
                        Top.Mixer.Unlink(_ReplayTone);
                        SipAgent.DestroyWavPlayer(_ReplayTone);
                        _ReplayTone = -1;
                        _Replaying = false;
                        _StopPlaying.Enabled = false;

                        General.SafeLaunchEvent(PlayingChanged, this, new StateMsg<bool>(false));
                        if (Settings.Default.SNMPEnabled == 1)
                        {
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string evento = Top.Cfg.PositionId + "_0";
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.StartingReplayOid, evento));
                            });
                        }
                    }
                    break;
                case FunctionReplay.Play:
                case FunctionReplay.PlayNoLoop:
                    if (_ReplayTone == -1)
                    {
                        if (funcion == FunctionReplay.Play)
                            _ReplayTone = SipAgent.CreateWavPlayer(fileName, true);
                        if (funcion == FunctionReplay.PlayNoLoop)
                            _ReplayTone = SipAgent.CreateWavPlayer(fileName, false );
                        Top.Mixer.LinkReplay(_ReplayTone, via);
                        _Replaying = true;

                        _StopPlaying = new System.Timers.Timer(1000 * fileLength / 16000);
                        _StopPlaying.Elapsed += new ElapsedEventHandler(StopPlayingElapsed);
                        _StopPlaying.AutoReset = false;
                        _StopPlaying.Enabled = true;
                        
                        // 230118 Enciendo led de altavozLC
                        Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.ON);

                        General.SafeLaunchEvent(PlayingChanged, this, new StateMsg<bool>(true));
                        if (Settings.Default.SNMPEnabled == 1)
                        {
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string evento = Top.Cfg.PositionId + "_1";
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.StartingReplayOid, evento));
                            });
                        }
                    }
					break;
                case FunctionReplay.DisableSupervisor:
                    
                case FunctionReplay.Pause:
                    /* De momento no se va a implementar */
                    break;
            }
        }

        private System.Timers.Timer _StopPlaying;
        private int _ReplayTone = -1;
        private bool _Replaying;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopPlayingElapsed(object sender, ElapsedEventArgs e)
        {
            DoFunction(FunctionReplay.Stop, ViaReplay.None, null, 0);
        }
    }
}
