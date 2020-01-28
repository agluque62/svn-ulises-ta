using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;

using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Snmp;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;

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
        private bool _LocalRecordingEnabled = false;
        private object _Sync = new object();

        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private Timer _SupervisorTimer = new Timer (5*60*1000);
        private Timer _BriefingSessionTimer = new Timer(Settings.Default.BriefingSessionTimer*60*1000);
        private Timer _SupervisorLengthRecording = new Timer(15000);

        public event GenericEventHandler<StateMsg<bool>> BriefingChanged;
        public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;

        public RecorderManager(bool enable)
        {
            // TODO: Complete member initialization
            this._LocalRecordingEnabled = enable;

            _Logger.Info("Grabacion habilitada: " + enable.ToString());
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
            }
            catch (System.IO.IOException /*e*/)
            {
                _Logger.Warn("Directorio de grabación no está vacío o no existe.");
            }
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

            if (LstDispositivos == null)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCall"></param>
        /// <param name="fuente"></param>
        /// <param name="iniciar"></param>
        internal void SessionGlp(int idCall, FuentesGlp fuente, bool iniciar)
        {
            if (!this._LocalRecordingEnabled)
                return;

            lock (_Sync)
            {
                try
                {
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
                else if (iniciar && (fuente == FuentesGlp.RxRadio || fuente == FuentesGlp.Telefonia) && _SessionsFile[(int)fuente] != -1)
                {
                    if (!_GlpCallId[(int)fuente].Contains(idCall))
                    {
                        Top.Mixer.Link(idCall, _SessionsFile[(int)fuente], MixerDir.Send, fuente);
                        _GlpCallId[(int)fuente].Add(idCall);
                    }
                }
                else if (!iniciar && _GlpSessionsStarted[(int)fuente])
                {
                    if (_GlpCallId[(int)fuente].Contains(idCall))
                        _GlpCallId[(int)fuente].Remove(idCall);

                    if (_GlpCallId[(int)fuente].Count == 0)
                    {
                        Top.Mixer.Unlink(_SessionsFile[(int)fuente]);
                        if (_SessionsFile[(int)fuente] >= 0)
                        {
                            SipAgent.DestroyWavRecorder(_SessionsFile[(int)fuente]);
                            _SessionsFile[(int)fuente] = -1;
                            _GlpSessionsStarted[(int)fuente] = false;

                            try
                            {
                                File.Move(_SessionsFileName[(int)fuente], _SessionsFileName[(int)fuente].Replace("@", ""));
                            }
                            catch (System.IO.IOException /*e*/)
                            {
                                File.Delete(_SessionsFileName[(int)fuente]);
                                _Logger.Warn("GLP.SesionGlp. El fichero ya existe.");
                            }

                            _SessionsFileName[(int)fuente] = string.Empty;
                        }
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
        /// 
        /// </summary>
        /// <param name="fuente"></param>
        /// <param name="iniciar"></param>
        internal void SessionGlp(FuentesGlp fuente, bool iniciar)
        {
            if (!this._LocalRecordingEnabled)
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

                            try
                            {
                                File.Move(_SessionsFileName[(int)fuente], _SessionsFileName[(int)fuente].Replace("@", ""));
                            }
                            catch (System.IO.IOException /*e*/)
                            {
                                File.Delete(_SessionsFileName[(int)fuente]);
                                _Logger.Warn("GLP.SesionGlp. El fichero ya existe.");
                            }

                            _SessionsFileName[(int)fuente] = string.Empty;
                        }

                        if (fuente == FuentesGlp.Briefing && Settings.Default.SNMPEnabled == 1)
                        {
                            Top.WorkingThread.Enqueue("SetSnmp", delegate()
                            {
                                string evento = Top.Cfg.PositionId + "_0";
                                General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.StartingBriefingSessionOid, evento));
                            });
                        }
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

                try
                {
                    File.Move(_SessionsFileName[(int)fuente], _SessionsFileName[(int)fuente].Replace("@", ""));
                }
                catch (System.IO.IOException /*e*/)
                {
                    File.Delete(_SessionsFileName[(int)fuente]);
                    _Logger.Warn("GLP.SesionGlp. El fichero ya existe.");
                }

                _SessionsFileName[(int)fuente] = string.Empty;

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
            _SupervisorLengthRecording.Enabled = false;

            for (FuentesGlp i = FuentesGlp.RxRadio; i <= FuentesGlp.TxLc; i++)
            {
                if (_GlpSessionsStarted[(int)i] && _SessionsFileName[(int)i] != string.Empty)
                {
                    FileInfo fi = new FileInfo(_SessionsFileName[(int)i]);
                    _Logger.Info("Supervisor GLP: GLP session started. Recording file: " + fi.FullName);
                    int interval = (i == FuentesGlp.RxRadio || i == FuentesGlp.TxRadio) ? 1 : 30;
                    if (fi != null && fi.Exists && fi.Length > interval * 60 * 16000 /*interval minutos, aprox. */)
                        ResetRecording(i);
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
            if (!this._LocalRecordingEnabled)
                return;

            try
            {
                /* AGL.REC Directorio de Grabacion Local Configurable
                Directory.Delete("Recording", true);
                 * */
                if (Directory.Exists (Settings.Default.DirectorioGLP))
                    Directory.Delete(Settings.Default.DirectorioGLP, true);
                /* Fin Modificacion */
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
                    if (_ReplayTone == -1)
                    {
                        _ReplayTone = SipAgent.CreateWavPlayer(fileName, true);
                        Top.Mixer.LinkReplay(_ReplayTone, via);
                        _Replaying = true;

                        _StopPlaying = new Timer(1000 * fileLength / 16000);
                        _StopPlaying.Elapsed += new ElapsedEventHandler(StopPlayingElapsed);
                        _StopPlaying.AutoReset = false;
                        _StopPlaying.Enabled = true;

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

        private Timer _StopPlaying;
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
