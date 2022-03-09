using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Timers;

using HMI.Model.Module.BusinessEntities;
using HMI.Model.Module.Messages;
using HMI.CD40.Module.Properties;
using U5ki.Infrastructure;
using Utilities;

using NLog;


namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// Identificadores para la gestión de la Mezcla en puesto.
    /// </summary>
#if DEBUG
    public enum MixerDev
#else
	enum MixerDev
#endif
    { MhpTlf,                     // Microcasco + Mic en Telefonía
                    MhpLc,                      // Microcasco + Mic en LC
                    MhpRd,                      // Microcasco + Mic en Radio.
                    SpkLc,                      // Altavoz de LC
                    SpkRd,                      // Altavoz Radio.
                    SpkHf,                      // Altavoz HF
                    Ring,                       // Señal de RING.
                    Invalid }
    /// <summary>
    /// Identificadores de los tipos de enlaces programados en el mezclador.
    /// </summary>
#if DEBUG
    public enum MixerDir
#else
	enum MixerDir
#endif
    { Send,                       // IN --> OUT Altavoz rx
                    Recv,                       // IN <-- OUT micro tx
                    SendRecv                    // IN <-> OUT
                  }

    public enum TlfRxAudioVia
    {
        NoAudio,
        HeadPhones,
        Speaker,
    }

    /// <summary>
    /// 
    /// </summary>
#if DEBUG
    public 	class MixerManager
#else
	class MixerManager
#endif
    {
        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler<SplitMode> SplitModeChanged;

        /// <summary>
        /// 
        /// </summary>
		public int RdSpeakerDev
		{
			get { return _RdSpeakerDev; }
		}

        /// <summary>
        /// 
        /// </summary>
		public int LcSpeakerDev
		{
			get { return _LcSpeakerDev; }
		}

        /// <summary>
        /// 
        /// </summary>
        public int HfSpeakerDev
        {
            get { return _HfSpeakerDev; }
        }

        /// <summary>
        /// 
        /// </summary>
		public int InstructorDev
		{
			get { return _InstructorDev; }
		}

        /// <summary>
        /// 
        /// </summary>
		public int AlumnDev
		{
			get { return _AlumnDev; }
		}

        ///// <summary>
        ///// 
        ///// </summary>
        //public int AlumnRecorderDev
        //{
        //    get { return _AlumnRecorderDev; }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public int InstructorRecorderDev
        //{
        //    get { return _InstructorRecorderDev; }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public int RadioRecorderDev
        //{
        //    get { return _RadioRecorderDev; }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public int LcRecorderDev
        //{
        //    get { return _LcRecorderDev; }
        //}

        /// <summary>
        /// 
        /// </summary>
		public SplitMode SplitMode
		{
			get { return _SplitMode; }
		}

        /// <summary>
        /// 
        /// </summary>
		public bool BuzzerEnabled
		{
			get { return _BuzzerEnabled; }
		}

        public TlfRxAudioVia RxTlfAudioVia
        {
            get { return _RxTlfAudioVia; }
        }

        //Funcion llamada desde el modelo que pone el modoSoloAltavoces según valor configurado
        public bool ModoSoloAltavoces
        {
            get { return _ModoSoloAltavoces; }
            set { _ModoSoloAltavoces = true; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listaDispositivos"></param>
        /** AGL.START */
#if _AUDIOGENERIC_
        public void Init()
#else
		public void Init(List <CORESIP_SndDevType> listaDispositivos)
#endif
		{
#if _AUDIOGENERIC_
            Top.Hw.JacksChangedHw -= OnJacksChanged;
            Top.Tlf.ActivityChanged -= OnTlfActivityChanged;
            _UnlinkGlpRadioTimer.Elapsed -= OnUnlinkGlpRadioTimerElapsed;
#endif

            Top.Hw.JacksChangedHw += OnJacksChanged;
			Top.Tlf.ActivityChanged += OnTlfActivityChanged;
            Top.Hw.SpeakerExtChangedHw += OnHwChanged;
            Top.Hw.SpeakerChangedHw += OnHwChanged;
            Top.Tlf.Listen.ListenChanged += OnListenChanged;
            Top.Tlf.HangToneChanged += OnTlfToneChanged;
            _UnlinkGlpRadioTimer.AutoReset = false;
            _UnlinkGlpRadioTimer.Elapsed += OnUnlinkGlpRadioTimerElapsed;

            /** AGL */
#if _AUDIOGENERIC_
            eAudioDeviceTypes tipoAudio = HwManager.AudioDeviceType;
            if (tipoAudio==eAudioDeviceTypes.GENERIC || tipoAudio==eAudioDeviceTypes.GENERIC_PTT)     // Microcascos y altavoces USB comerciales...
            {
                HidGenericHwManager.LoadChannels();

                _AlumnDev = HidGenericHwManager.AddDevice(Settings.Default.AlumnMHP, CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP);
                _LcSpeakerDev = HidGenericHwManager.AddDevice(Settings.Default.LcSpeaker, CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);
                _RdSpeakerDev = HidGenericHwManager.AddDevice(Settings.Default.RdSpeaker, CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);

                _InstructorDev = _HfSpeakerDev = -1;
                _InstructorRecorderDevIn = _AlumnRecorderDevIn = _LcRecorderDevIn = _RadioRecorderDevIn = _RadioHfRecorderIn = -1;
                _InstructorRecorderDevOut = _AlumnRecorderDevOut = _IntRecorderDevOut = -1;
            }
            else if (tipoAudio==eAudioDeviceTypes.CMEDIA)    // IAU-CMEDIA
            {
                HidCMediaHwManager.LoadChannels();

                /** Dispositivos */
                _InstructorDev = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, CMediaDevMode.Bidirectional);
                _AlumnDev = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, CMediaDevMode.Bidirectional);
                _LcSpeakerDev = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, CMediaDevMode.Output);
                _RdSpeakerDev = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, CMediaDevMode.Output);
                if (Settings.Default.HfSpeaker)
                    _HfSpeakerDev = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, CMediaDevMode.Output);
                else 
                    _HfSpeakerDev = -1;

                if (Settings.Default.RecordMode == 1)              // Pointe Noire.
                {
                    /** Retornos de Grabacion */
                    _InstructorRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, CMediaDevMode.Input);
                    _AlumnRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, CMediaDevMode.Input);
                    _RadioRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, CMediaDevMode.Input);
                    _LcRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, CMediaDevMode.Input);
                    _RadioHfRecorderIn = -1;

                    /** Salidas de Grabacion. */
                    _IntRecorderDevOut = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, CMediaDevMode.Output);
                    _AlumnRecorderDevOut = _InstructorRecorderDevOut = _IntRecorderDevOut;
                }
                else if (Settings.Default.RecordMode == 2)         // Nouakchott.
                {
                    /** Retornos de Grabacion */
                    _InstructorRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, CMediaDevMode.Input);
                    _AlumnRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, CMediaDevMode.Input);
                    _RadioRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, CMediaDevMode.Input);
                    _LcRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, CMediaDevMode.Input);
                    _RadioHfRecorderIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, CMediaDevMode.Input); ;

                    /** Salidas de Grabacion. */
                    _IntRecorderDevOut = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, CMediaDevMode.Output);
                    _AlumnRecorderDevOut = _InstructorRecorderDevOut = _IntRecorderDevOut;
                }
                else /* if (Settings.Default.RecordMode == 0) */   // Por defecto será ENAIRE...
                {
                    /** Retornos de Grabacion */
                    _InstructorRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, CMediaDevMode.Input);
                    _AlumnRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, CMediaDevMode.Input);
                    _RadioRecorderDevIn = -1;
                    _LcRecorderDevIn = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, CMediaDevMode.Input);
                    _RadioHfRecorderIn = -1;

                    /** Salidas de Grabacion. */
                    _IntRecorderDevOut = -1;                    
                    _AlumnRecorderDevOut = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, CMediaDevMode.Output);
                    if (Settings.Default.CMediaBkpVersion == "B41A")
                    {
                        // BKP_VERSION_B41A
                        _InstructorRecorderDevOut = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, CMediaDevMode.Output);
                        //No es compatible la grabación analogica de telefona por altavoz con este hw
                    }
                    else
                    {
                        // BKP_VERSION_B43A
                        _InstructorRecorderDevOut = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, CMediaDevMode.Output);
                        _IntRecorderDevOut = HidCMediaHwManager.AddDevice(true, CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, CMediaDevMode.Output);
                    }
                }
            }
            else if (tipoAudio == eAudioDeviceTypes.MICRONAS)    // IAU-MICRONAS. 
            {
#endif
                if (Settings.Default.InstructorMHP)
                {
                    _InstructorDev = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, Settings.Default.InstructorInChannel, Settings.Default.InstructorOutChannel);
                    if (_InstructorDev >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", Settings.Default.InstructorInChannel, Settings.Default.InstructorOutChannel, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP);
                }
                if (Settings.Default.AlumnMHP)
                {
                    _AlumnDev = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, Settings.Default.AlumnInChannel, Settings.Default.AlumnOutChannel);
                    if (_AlumnDev >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", Settings.Default.AlumnInChannel, Settings.Default.AlumnOutChannel, CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP);
                }
                if (Settings.Default.LcSpeaker)
                {
                    _LcSpeakerDev = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, -1, Settings.Default.LcSpeakerOutChannel);
                    if (_LcSpeakerDev >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", -1, Settings.Default.LcSpeakerOutChannel, CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);
                }
                if (Settings.Default.RdSpeaker)
                {
                    _RdSpeakerDev = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, -1, Settings.Default.RdSpeakerOutChannel);
                    if (_RdSpeakerDev >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", -1, Settings.Default.RdSpeakerOutChannel, CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);
                }
                if (Settings.Default.HfSpeaker)
                {
                    _HfSpeakerDev = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, -1, Settings.Default.HfSpeakerChannel);
                    if (_HfSpeakerDev >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2} (HF)", -1, Settings.Default.HfSpeakerChannel, CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER);
                }

                // Canales de grabación.
                if (Settings.Default.InstructorRecorder)
                {
                    _InstructorRecorderDevIn = _InstructorRecorderDevIn = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, Settings.Default.InstructorInHpChannel, Settings.Default.InstructorOutRecorderChannel);
                    if (_InstructorRecorderDevIn >= 0)
                    {
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", Settings.Default.InstructorInHpChannel, Settings.Default.InstructorOutRecorderChannel, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER);
                    }
                }
                if (Settings.Default.AlumnRecorder)
                {
                    _AlumnRecorderDevIn = _AlumnRecorderDevOut = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, Settings.Default.AlumnInHpChannel, Settings.Default.AlumnOutRecorderChannel);
                    if (_AlumnRecorderDevIn >= 0)
                    {
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", Settings.Default.AlumnInHpChannel, Settings.Default.AlumnOutRecorderChannel, CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER);
                    }
                }
                if (Settings.Default.LCRecorder)
                {
                    _LcRecorderDevIn = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, Settings.Default.LcInSpeakerChannel, Settings.Default.LcOutRecorderChannel);
                    if (_LcRecorderDevIn >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", Settings.Default.LcInSpeakerChannel, Settings.Default.LcOutRecorderChannel, CORESIP_SndDevType.CORESIP_SND_LC_RECORDER);
                }
                if (Settings.Default.RadioRecorder)
                {
                    _RadioRecorderDevIn = SipAgent.AddSndDevice(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, Settings.Default.RadioInSpeakerChannel, Settings.Default.RadioOutRecorderChannel);
                    if (_RadioRecorderDevIn >= 0)
                        _Logger.Info("Canal In {0}. Canal Out {1} asignados al dispositivo de sonido tipo {2}", Settings.Default.RadioInSpeakerChannel, Settings.Default.RadioOutRecorderChannel, CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER);
                }
#if _AUDIOGENERIC_
            }
            else if (tipoAudio == eAudioDeviceTypes.SIMUL)
            {
                /** En el tipo Simulado, no hay dispositivos de audio */

                //Para que puedan pasarse de modo normal a disgregado
                //_InstructorDev = 0;
                //_AlumnDev = 0;
                //_RdSpeakerDev = 0;
                //_LcSpeakerDev = 0;
            }
            else
            {
                throw new Exception("Dispositivos de audio desconocidos");
            }
#endif
			switch (Settings.Default.RingDevice)
			{
				case 0:
                    if ((_InstructorDev >= 0) || (_AlumnDev >= 0))
					{
						_RingDev = MixerDev.MhpTlf;
					}
					else if (_LcSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkLc;
					}
					else if (_RdSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkRd;
					}
					break;
				case 2:
                    if ((_InstructorDev >= 0) || (_AlumnDev >= 0))
					{
						_RingDev = MixerDev.MhpRd;
					}
					else if (_LcSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkLc;
					}
					else if (_RdSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkRd;
					}
					break;
				case 4:
                    if (_RdSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkRd;
					}
					else if (_LcSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkLc;
					}
					else if ((_InstructorDev >= 0) || (_AlumnDev >= 0))
					{
						_RingDev = MixerDev.MhpTlf;
					}
					break;
				default:
                    if (_LcSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkLc;
					}
					else if ((_InstructorDev >= 0) || (_AlumnDev >= 0))
					{
						_RingDev = MixerDev.MhpTlf;
					}
					else if (_RdSpeakerDev >= 0)
					{
						_RingDev = MixerDev.SpkRd;
					}
					break;
			}

			if (_RingDev != MixerDev.Invalid)
			{
				_BuzzerEnabled = true;
			}
		}

        /// <summary>
        /// AGL-REC....(1)
        /// </summary> 
		public void Start()
		{
            if (_InstructorRecorderDevIn >= 0 && _InstructorJack)
            {
                _Logger.Debug("REC-ON <= Intructor-HPH");
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, true);
            }
            if (_AlumnRecorderDevIn >= 0 && _AlumnJack)
            {
                _Logger.Debug("REC-ON <= Alumn-HPH");
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, true);
            }

            // AGL.REC Grabacion Unificada...
            if (Settings.Default.RecordMode == 1 || Settings.Default.RecordMode==2)
            {
                if (_LcRecorderDevIn >= 0)
                {
                    _Logger.Debug("REC-ON <= Altavoz LCE");
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, true);
                }
                if (_RadioRecorderDevIn >= 0)
                {
                    _Logger.Debug("REC-ON <= Altavoz RAD-VHF");
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, true);
                }
                if (_RadioHfRecorderIn >= 0)
                {
                    _Logger.Debug("REC-ON <= Altavoz RAD-HF");
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_HF_RECORDER, true);
                }
            }

        }

        /// <summary>
        /// AGL-REC.... 
        /// </summary>
		public void End()
		{
            if (_InstructorRecorderDevIn >= 0 && _InstructorJack) 
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, false);
            if (_AlumnRecorderDevIn >= 0 && _AlumnJack)
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, false);

            // AGL.REC Grabacion Unificada...
            if (Settings.Default.RecordMode == 1 || Settings.Default.RecordMode==2)
            {
                if (_LcRecorderDevIn >= 0)
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, false);
                if (_RadioRecorderDevIn >= 0)
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, false);
                if (_RadioHfRecorderIn >= 0)
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_HF_RECORDER, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
		public bool SetSplitMode(SplitMode mode)
		{
			if (_SplitMode != mode)
			{
				if ((mode != SplitMode.Off) && 
					(!_InstructorJack || !_AlumnJack || (_InstructorDev < 0) || (_AlumnDev < 0)))
				{
					return false;
				}

				switch (mode)
				{
					case SplitMode.Off:
						SetSplitOff();
						break;
					case SplitMode.LcTf:
					case SplitMode.RdLc:
						SetSplitOn(mode);
						break;
				}

				SplitMode oldMode = _SplitMode;
				_SplitMode = mode;

				General.SafeLaunchEvent(SplitModeChanged, this, oldMode);
			}

			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled"></param>
        /// <returns></returns>
		public bool SetBuzzerState(bool enabled)
		{
            if (Settings.Default.AudioCardSimul == true)
			{
                _BuzzerEnabled = enabled;
                return true;
            }
			else if (_RingDev != MixerDev.Invalid)
			{
				if (enabled != _BuzzerEnabled)
				{
					_BuzzerEnabled = enabled;

					foreach (LinkInfo link in _LinksList)
					{
						if (link._Dev == MixerDev.Ring)
						{
							if (_BuzzerEnabled)
							{
								LinkRing(link._CallId, link._Priority);
							}
							else
							{
                                _Mixer.Unlink(link._CallId);
							}
						}
					}
				}

				return true;
			}

			return false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
		public bool SetBuzzerLevel(int level)
		{
            if (Settings.Default.AudioCardSimul == true)
			{
				_RingVolume = CalculateVolume(level);
                return true;
            }
            // LALM 210429
            // Peticiones #4810
            // Configurar la restricción de presencia de altavoz LC
            else if (Settings.Default.LcSpeakerSimul)
            {
                _RingVolume = CalculateVolume(level);
                return true;
            }
            else if (_BuzzerEnabled)
			{
				_RingVolume = CalculateVolume(level);

				foreach (LinkInfo link in _LinksList)
				{
					if (link._Dev == MixerDev.Ring)
					{
						SipAgent.SetVolume(link._CallId, _RingVolume);
					}
				}

				return true;
			}

			return false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
		public bool SetTlfHeadPhonesLevel(int level)
		{
			_TlfHeadPhonesVolume = CalculateVolume(level);

			foreach (LinkInfo link in _LinksList)
			{
				if ((link._Dev == MixerDev.MhpTlf) && 
					((link._Dir == MixerDir.Send) || (link._Dir == MixerDir.SendRecv)))
				{
					SipAgent.SetVolume(link._CallId, _TlfHeadPhonesVolume);
				}
			}

			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
		public bool SetLcSpeakerLevel(int level)
		{
			_LcSpeakerVolume = CalculateVolume(level);

			foreach (LinkInfo link in _LinksList)
			{
                // Se cambia el volumen para las conversaciones activas de ese tipo
				if (link._Dev == MixerDev.SpkLc && link._TipoFuente == FuentesGlp.RxLc)
				{
					SipAgent.SetVolume(link._CallId, _LcSpeakerVolume);
				}
			}

			return true;
		}

        /// <summary>
        /// Utiliza el volumen del altavoz de telefonía en el altavoz de LC
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool SetTlfSpeakerLevel(int level)
        {
            _TlfSpeakerVolume = CalculateVolume(level);

            foreach (LinkInfo link in _LinksList)
            {
                // Se cambia el volumen para las conversaciones activas de ese tipo
                if (link._Dev == MixerDev.SpkLc && link._TipoFuente == FuentesGlp.Telefonia)
                {
                    SipAgent.SetVolume(link._CallId, _TlfSpeakerVolume);
                }
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
		public bool SetRdHeadPhonesLevel(int level)
		{
			_RdHeadPhonesVolume = CalculateVolume(level);

			foreach (LinkInfo link in _LinksList)
			{
				if ((link._CurrentDev == MixerDev.MhpRd) &&
					((link._Dir == MixerDir.Send) || (link._Dir == MixerDir.SendRecv)))
				{
						SipAgent.SetVolume(link._CallId, _RdHeadPhonesVolume);
				}
			}

			return true;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool SetRdSpeakerLevel(int level)
        {
             //TEMP Para medidas !!!
            //SipAgent.EchoCancellerLCMic(true);
            _RdSpeakerVolume = CalculateVolume(level);

            foreach (LinkInfo link in _LinksList)
            {
                if ((link._Dev == MixerDev.SpkRd) || (link._CurrentDev == MixerDev.SpkRd))
                {
                    SipAgent.SetVolume(link._CallId, _RdSpeakerVolume);
                }
            }

            return true;
        }

        /// <summary>
        /// Registra el nivel del volumen del altavoz radio HF
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool SetHfSpeakerLevel(int level)
        {
            //TEMP Para medidas !!!
            //SipAgent.EchoCancellerLCMic(false);
            _HfSpeakerVolume = CalculateVolume(level);

            foreach (LinkInfo links in _LinksList)
            {
                if ((links._Dev == MixerDev.SpkHf) || (links._CurrentDev == MixerDev.SpkHf))
                {
                    SipAgent.SetVolume(links._CallId, _HfSpeakerVolume);
                }
            }

            return true;
        }

        /// <summary>
        /// Hace un link para telefonía teniendo en cuenta la selección de altavoz
        /// </summary>
        /// <param name="id"> callId</param>
        /// <param name="dir">direction</param>
        /// <param name="priority"></param>
        public void LinkTlf(int id, MixerDir dir, int priority)
        {
            if (_RxTlfAudioVia == TlfRxAudioVia.HeadPhones)
            {
                Link(id, MixerDev.MhpTlf, dir, priority, FuentesGlp.Telefonia);
            }
            if (_RxTlfAudioVia == TlfRxAudioVia.Speaker)
            {
                if ((dir == MixerDir.Recv) || (dir == MixerDir.SendRecv))
                    Link(id, MixerDev.MhpTlf, MixerDir.Recv, priority, FuentesGlp.Telefonia);
                if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
                {
                    Link(id, MixerDev.SpkLc, MixerDir.Send, priority, FuentesGlp.Telefonia);                    
                }
            }
         }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dev"></param>
        /// <param name="dir"></param>
        /// <param name="priority"></param>
        public void Link(int id, MixerDev dev, MixerDir dir, int priority, FuentesGlp tipoFuente)
		{
            LinkInfo link = new LinkInfo(dev, dir, priority, tipoFuente, id);
            _LinksList.Add(link);
            switch (dev)
			{
				case MixerDev.MhpTlf:
					if ((_InstructorDev >= 0 && _InstructorJack) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
					{
						Link(id, _InstructorDev, dir, priority);
                        // LALM 210922
                        //Errores #3909 HMI: Telefonia en Altavoz -> Grabación Enaire
                        if ((dir == MixerDir.SendRecv) || (dir == MixerDir.Recv))
                        {
                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, true);
                            /* AGL.REC */
                            _InstructorMhpTlfRecInProgress = true;
                        }
					}
					if ((_AlumnDev >= 0 && _AlumnJack) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
					{
						Link(id, _AlumnDev, dir, priority);
                        // LALM 210922
                        //Errores #3909 HMI: Telefonia en Altavoz -> Grabación Enaire
                        if ((dir == MixerDir.SendRecv) ||(dir == MixerDir.Recv))
                        {
                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, true);
                            /* AGL.REC */
                            _AlumnMhpTlfRecInProgress = true;
                        }
					}

					if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
					{
						if ((_InstructorDev >= 0) || (_AlumnDev >= 0))
						{
							foreach (int listenCall in _TlfListens)
							{
								_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
							}
						}
						SipAgent.SetVolume(id, _TlfHeadPhonesVolume);
					}
					if ((dir == MixerDir.Recv) || (dir == MixerDir.SendRecv))
					{
						TlfLinkAdded();
					}
					break;

				case MixerDev.MhpLc:
                    ManageECHandsFreeByLC();
                    if ((_InstructorDev >= 0 && _InstructorJack) && _SplitMode == SplitMode.Off)
                    {
                        Link(id, _InstructorDev, dir, priority);
                        Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, true);
                    }
                    if (_AlumnDev >= 0 && _AlumnJack)
                    {
                        Link(id, _AlumnDev, dir, priority);
                        Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, true);
                    }

					if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
					{
						SipAgent.SetVolume(id, _TlfHeadPhonesVolume);
					}
					break;

				case MixerDev.MhpRd:
                    bool alreadySessionOpen = false;
                    if ((_SplitMode != SplitMode.Off) || AutChangeToRdSpeaker() == false)
					{
                        // Graba la recepción en cascos bien del instructor o del alumno
						if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
						{
							Link(id, _InstructorDev, dir, priority);
                            if (dir == MixerDir.Send)
                            {
                                alreadySessionOpen = true;
                                if (Top.Rd.AnySquelch)
                                {
                                    Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
                                    Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, true);
                                    _UnlinkGlpRadioTimer.Enabled = false;
                                }
                                else
                                    Top.Recorder.SetIdSession(id, FuentesGlp.RxRadio);
                            }
                        }
						if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
						{
							Link(id, _AlumnDev, dir, priority);
                            if ((dir == MixerDir.Send) && (alreadySessionOpen == false))
                            {
                                if (Top.Rd.AnySquelch)
                                {
                                    Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
                                    Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, true);
                                    _UnlinkGlpRadioTimer.Enabled = false;
                                }
                                else
                                    Top.Recorder.SetIdSession(id, FuentesGlp.RxRadio);
                            }
                        }

						if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
						{
							SipAgent.SetVolume(id, _RdHeadPhonesVolume);
						}
					}
                    // Por telefonía por cascos, se pasa automáticamente a altavoz temporalmente
                    else if (Top.Hw.RdSpeaker || Top.Rd.HFSpeakerAvailable())
					{
						if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
						{
                            ConectaAltavozDisponible(ref link);
                            if  (dir == MixerDir.Send)
                            {
                                if (Top.Rd.AnySquelch)
                                {
                                    Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
                                    Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, true);
                                    _UnlinkGlpRadioTimer.Enabled = false;
                                }
                                else
                                    Top.Recorder.SetIdSession(id, FuentesGlp.RxRadio);
                            }                                                        
						}
						if ((dir == MixerDir.Recv) || (dir == MixerDir.SendRecv))
						{
							if (_InstructorDev >= 0)
							{
								_Mixer.Link(_InstructorDev, priority, id, Mixer.UNASSIGNED_PRIORITY);
							}
							if (_AlumnDev >= 0)
							{
								_Mixer.Link(_AlumnDev, priority, id, Mixer.UNASSIGNED_PRIORITY);
							}
						}
					}
                    Top.Rd.UpdateRadioSpeakerLed();
					break;

				case MixerDev.SpkLc:
                    ManageECHandsFreeByLC();
					Debug.Assert(dir == MixerDir.Send);
					if (_LcSpeakerDev >= 0)
					{
						_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _LcSpeakerDev, priority);
                        //Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.ON);
                        //lalm 20201214 cambio el onoff por update.
                        //lalm 20211004 sobra esta linea
                        //Top.Lc.lc_activo = true;
                        Top.Lc.UpdateLcSpeakerLed();

                        /*- AGL.REC La grabacion del Altavoz-LC es Continua ...
                        BS si es grabacion unificada. 
                        * En el caso de la telefonía por altavoz no unificada, no es continua */
                        if (Settings.Default.RecordMode == 0)
                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, true);
                        
                        //Se llama desde arriba
                        //Top.Recorder.SessionGlp(id, tipoFuente, true);

                        if (tipoFuente == FuentesGlp.Telefonia)
                            SipAgent.SetVolume(id, _TlfSpeakerVolume);
                        else
                            SipAgent.SetVolume(id, _LcSpeakerVolume);
					}
                    else if (Top.Hw is SimCMediaHwManager)
                    {
                        //Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.ON);
                        //lalm 20201214 cambio el onoff por update.
                        Top.Lc.lc_activo = true;
                        Top.Lc.UpdateLcSpeakerLed();
                    }
                    else
                    {
                        goto case MixerDev.MhpLc;
                    }
					break;

                case MixerDev.SpkRd:
                    Debug.Assert(dir == MixerDir.Send);
                    if (_RdSpeakerDev >= 0)
                    {
                        _Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _RdSpeakerDev, priority);
                        Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_RD_SPEAKER, HwManager.ON);
                        /*- AGL.REC La Grabación del Altavoz Radio es Continua...
						Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, true);
                        * */

                        if (Top.Rd.AnySquelch)
                        {
                            Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
                            Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, true);
                            _UnlinkGlpRadioTimer.Enabled = false;
                        }
                        else
                            Top.Recorder.SetIdSession(id, FuentesGlp.RxRadio);

                        SipAgent.SetVolume(id, _RdSpeakerVolume);
                    }
                    else
                    {
                        goto case MixerDev.MhpRd;
                    }
                    break;

                case MixerDev.SpkHf:
                    Debug.Assert(dir == MixerDir.Send);
                    if (_HfSpeakerDev >= 0)
                    {
                        _Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _HfSpeakerDev, priority);
                        Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_HF_SPEAKER, HwManager.ON);
                        /*- AGL.REC La Grabación del Altavoz Radio es Continua...
						Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, true);
                        * */

                        if (Top.Rd.AnySquelch)
                        {
                            Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
                            Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, true);
                            _UnlinkGlpRadioTimer.Enabled = false;
                        }
                        else
                            Top.Recorder.SetIdSession(id, FuentesGlp.RxRadio);

                        SipAgent.SetVolume(id, _HfSpeakerVolume);
                    }
                    else
                    {
                        goto case MixerDev.MhpRd;
                    }
                    break;

                case MixerDev.Ring:
					Debug.Assert(dir == MixerDir.Send);
					if (_BuzzerEnabled)
					{
						LinkRing(id, priority);
					}
					SipAgent.SetVolume(id, _RingVolume);
					break;
			}

		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId1"></param>
        /// <param name="callId2"></param>
        /// <param name="dir"></param>
		public void Link(int callId1, int callId2, MixerDir dir, FuentesGlp fuente)
		{
			Link(callId1, callId2, dir, Mixer.UNASSIGNED_PRIORITY);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void Unlink(int id)
		{
            List<LinkInfo> listToRemove = _LinksList.FindAll (link => link._CallId == id);
            int removed = _LinksList.RemoveAll(link => link._CallId == id);

            foreach (LinkInfo info in listToRemove)
			{
                if ((info._Dev == MixerDev.MhpTlf) && ((info._Dir == MixerDir.Recv) || (info._Dir == MixerDir.SendRecv)))
				{
					TlfLinkRemoved();
				}

				switch (info._Dev)
				{
					case MixerDev.SpkRd:
                    case MixerDev.SpkHf:
                        /* AGL.REC La grabación del altavoz radio es continua...
						Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, false);
                        * */
                        //_UnlinkGlpRadioTimer.Enabled = true;
                        Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
                        Top.Rd.UpdateRadioSpeakerLed();
						break;

					case MixerDev.SpkLc:
                       ManageECHandsFreeByLC();
                       /*- AGL.REC La grabacion del Altavoz-LC es Continua ...
                        BS si es grabacion unificada. 
                        * En el caso de la telefonía por altavoz no unificada, no es continua */
                       if (Settings.Default.RecordMode == 0)
                        Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, false);

                        Top.Recorder.SessionGlp(info._TipoFuente, false);
                        Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.OFF);
						break;

					case MixerDev.MhpRd:
                        //_UnlinkGlpRadioTimer.Enabled = true;
                        Top.Recorder.SessionGlp(id, FuentesGlp.RxRadio, false);
						break;

					case MixerDev.MhpTlf:
                        if (_InstructorDev >= 0 && info._Dir == MixerDir.SendRecv && 
                            (Top.Rd.PttSource != PttSource.Instructor && Top.Rd.PttSource != PttSource.Hmi))
                        {
                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, false);
                            /* AGL.REC */
                            _InstructorMhpTlfRecInProgress = false;
                        }
                        if (_AlumnDev >= 0 && info._Dir == MixerDir.SendRecv &&
                            (Top.Rd.PttSource != PttSource.Alumn && Top.Rd.PttSource != PttSource.Hmi))
                        {
                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, false);
                            /* AGL.REC */
                            _AlumnMhpTlfRecInProgress = false;
                        }
                        Top.Recorder.SessionGlp(id, FuentesGlp.Telefonia, false);
						break;

					case MixerDev.MhpLc:
                        ManageECHandsFreeByLC();
						if (_InstructorDev >= 0 && _SplitMode == SplitMode.Off)
						{
                            /* AGL.REC. Desactivar la grabacion del microfono, solo si no hay PTT pulsado ni llamadas telefónicas en curso ... */
                            if (Top.Hw.HwPtt_Activated == false && _InstructorMhpTlfRecInProgress == false)
                                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, false);
                        }
						if (_AlumnDev >= 0)
						{
                            /* AGL.REC. Desactivar la grabacion del microfono, solo si no hay PTT pulsado ni llamadas telefónicas en curso ... */
                            if (Top.Hw.HwPtt_Activated == false && _AlumnMhpTlfRecInProgress == false)
                                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, false);
                        }
						break;
				}
			}

            if (removed > 0)
            {
                _Mixer.Unlink(id);
                Top.Rd.UpdateRadioSpeakerLed();
            }
			_TlfListens.Remove(id);

            /** */
            RingLedToOff(id);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tone"></param>
        public void LinkSelCal(bool on, int tone)
        {
            if (on)
                _Mixer.Link(tone, Top.HostId, Settings.Default.RdSrvListenIp, Settings.Default.RdSrvListenPort);
            else
                _Mixer.Unlink(tone, Mixer.RD_REMOTE_PORT_ID);
        }

        /// <summary>
        /// 
        /// </summary>
		public void LinkRdInstructorTx()
		{
			if (_InstructorDev >= 0 && _InstructorJack)
			{
				Debug.Assert((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf));

				_Mixer.Link(_InstructorDev, Top.HostId, Settings.Default.RdSrvListenIp, Settings.Default.RdSrvListenPort);

                if (Top.Tlf.Activity())
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, false);  // Le quito la grabación a la telefonía

                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, true);
            }
		}

        /// <summary>
        /// 
        /// </summary>
		public void UnlinkRdInstructorTx()
		{

			if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
			{
				_Mixer.Unlink(_InstructorDev, Mixer.RD_REMOTE_PORT_ID);
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, false);
			}

        }

        /// <summary>
        /// 
        /// </summary>
		public void LinkRdAlumnTx()
		{
			if (_AlumnDev >= 0 && _AlumnJack)
			{
				Debug.Assert((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc));
				_Mixer.Link(_AlumnDev, Top.HostId, Settings.Default.RdSrvListenIp, Settings.Default.RdSrvListenPort);

                if (Top.Tlf.Activity())
                    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, false);  // Le quito la grabación a la telefonía

                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, true);
            }
		}

        /// <summary>
        /// 
        /// </summary>
		public void UnlinkRdAlumnTx()
		{
			if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
			{
				_Mixer.Unlink(_AlumnDev, Mixer.RD_REMOTE_PORT_ID);
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, false);
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
		public void LinkTlf(int id)
		{
			_TlfListens.Add(id);

			if (_TlfRxLinks > 0)
			{
				if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
				{
					_Mixer.Link(_InstructorDev, Mixer.TLF_PRIORITY, id, Mixer.UNASSIGNED_PRIORITY);
				}
				if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
				{
					_Mixer.Link(_AlumnDev, Mixer.TLF_PRIORITY, id, Mixer.UNASSIGNED_PRIORITY);
				}
			}

			foreach (LinkInfo link in _LinksList)
			{
				if (link._Dev == MixerDev.MhpTlf)
				{
					if (((link._Dir == MixerDir.Send) || (link._Dir == MixerDir.SendRecv)) &&
						((_InstructorDev >= 0) || (_AlumnDev >= 0)))
					{
						_Mixer.Link(link._CallId, Mixer.UNASSIGNED_PRIORITY, id, Mixer.UNASSIGNED_PRIORITY);
					}
				}
				else if (link._Dev == MixerDev.Ring)
				{
					if (_BuzzerEnabled)
					{
						_Mixer.Link(link._CallId, Mixer.UNASSIGNED_PRIORITY, id, Mixer.UNASSIGNED_PRIORITY);
					}
				}
			}
		}

        /// <summary>
        /// AGL-REC.... (3). Esta rutina configura las salidas (mezclas) de grabacion....
        /// </summary>
        /// <param name="dev"></param>
        /// <param name="on"></param>
        public void LinkRecord(CORESIP_SndDevType dev, bool on)
		{
            Dictionary<CORESIP_SndDevType, int> Type2DevIn = new Dictionary<CORESIP_SndDevType, int>()
            {
                {CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, _InstructorRecorderDevIn},
                {CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, _InstructorDev},
                {CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, _AlumnRecorderDevIn},
                {CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, _AlumnDev},
                {CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, _RadioRecorderDevIn},
                {CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, _LcRecorderDevIn},
                {CORESIP_SndDevType.CORESIP_SND_HF_RECORDER, _RadioHfRecorderIn}
            };

            _Logger.Debug("LinkRecord {0} => {1}", dev, on);

            if (Type2DevIn.ContainsKey(dev) == false)
            {
                _Logger.Error("MixerManager.LinkRecord Error. Dispositivo de Entrada no Soportado: {0}", dev);
                return;
            }
            int RecorderSourceDev = Type2DevIn[dev];
            int RecorderDev = -1;
            if (Settings.Default.RecordMode == 0)
            {
                Dictionary<CORESIP_SndDevType, int> Type2DevOut = new Dictionary<CORESIP_SndDevType, int>()
                {
                    {CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, _InstructorRecorderDevOut},
                    {CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, _InstructorRecorderDevOut},
                    {CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, _AlumnRecorderDevOut},
                    {CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, _AlumnRecorderDevOut},
                    {CORESIP_SndDevType.CORESIP_SND_LC_RECORDER, _IntRecorderDevOut}
                };
                if (Type2DevOut.ContainsKey(dev) == true)
                {
                    RecorderDev = Type2DevOut[dev];
                }
                else
                {
                    _Logger.Error("MixerManager.LinkRecord Error. Dispositivo de Salida no Soportado para: {0}", dev);
                    return;
                }
            }
            else if (Settings.Default.RecordMode == 1 || Settings.Default.RecordMode == 2)       // Grabacion Integrada
            {
                RecorderDev = _IntRecorderDevOut;
            }

            if (RecorderDev>= 0 && RecorderSourceDev>=0)
            {
                if (on)
                {
                    _Logger.Debug("{0} REC-ON  {1:X} <= {2:X}", dev, RecorderDev, RecorderSourceDev);
                    _Mixer.Link(RecorderSourceDev, Mixer.UNASSIGNED_PRIORITY, RecorderDev, Mixer.UNASSIGNED_PRIORITY);
                }
                else
                {
                    _Logger.Debug("{0} REC-OFF {1:X} <= {2:X}", dev, RecorderDev, RecorderSourceDev);
                    _Mixer.Unlink(RecorderSourceDev, RecorderDev);
                }
            }
            else                                        
            {
                _Logger.Error("MixerManager.LinkRecord Error. Dispositivo de Entrada o Salida no Disponible: {0}", dev);
                return;
            }
		}

        /// <summary>
        ///  Metodo para telefonía, tiene en cuenta el dispositivo de salida configurado
        /// </summary>
        /// <param name="file"></param>
        /// <param name="device"></param>
        public void LinkGlpTfl(int file)
        {
            LinkTlf(file, MixerDir.Recv, Mixer.UNASSIGNED_PRIORITY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="device"></param>
        public void LinkGlp(int file, MixerDev device, FuentesGlp tipoFuente)
        {
            Link(file, device, MixerDir.Recv, Mixer.UNASSIGNED_PRIORITY, tipoFuente );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="via"></param>       
        public void LinkReplay(int file, ViaReplay via)
        {
            LinkInfo link = null;
            
            switch (via)
            {
                case ViaReplay.HeadphonesAlumn:
                    link = new LinkInfo(MixerDev.MhpTlf, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Briefing, file);
                    Link(file,_AlumnDev, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY);
                    break;
                case ViaReplay.HeadphonesInstructor:
                    link = new LinkInfo(MixerDev.MhpTlf, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Briefing, file);
                    Link(file,_InstructorDev, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY);
                    break;
                case ViaReplay.SpeakerRadio:
                    link = new LinkInfo(MixerDev.MhpTlf, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Briefing, file);
                    Link(file,_RdSpeakerDev, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY);
                    break;
                case ViaReplay.SpeakerLc:
                    link = new LinkInfo(MixerDev.MhpTlf, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Briefing, file);
                    Link(file,_LcSpeakerDev, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY);
                    break;
            }
            _LinksList.Add(link);
        }

#if _AUDIOGENERIC_
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_dev"></param>
        /// <param name="onoff"></param>
        public void DeviceAudioEvent(CORESIP_SndDevType _dev, bool onoff)
        {
        }

        /// <summary>
        /// Guarda el atributo de salida del audio para telefonía
        /// Y actualiza la salida del audio en curso de la telefonía afectada
        /// y de la radio si está afectada por la conmutación automática
        /// </summary>
        /// <param name="speaker"> true si está seleccionado el altavoz</param>
        public void SetTlfAudioVia(bool speaker)
        {
            _RxTlfAudioVia = speaker ? TlfRxAudioVia.Speaker : TlfRxAudioVia.HeadPhones;
            _ECHandsFreeManager.TlfSpeakerState = speaker;

            //Actualizar la salida del audio en curso que está afectada
            List <LinkInfo> tempLinks = new List<LinkInfo>(_LinksList);
            foreach (LinkInfo link in tempLinks)
            {
                if (link._TipoFuente == FuentesGlp.Telefonia)
                {
                    if ((speaker && (link._Dev == MixerDev.MhpTlf)) || 
                        ((!speaker) && (link._Dev == MixerDev.SpkLc)))
                    {
                        Unlink(link._CallId);
                        if (!speaker)
                            LinkTlf(link._CallId, MixerDir.SendRecv, link._Priority);
                        else
                            LinkTlf(link._CallId, link._Dir, link._Priority);
                    }
                 }
            }
            //Si la Tlf cambia a cascos (speaker false), la radio pasa de cascos a altavoz (mphToSpk true)
            //Si la Tlf cambia a altavoz (speaker true), la radio vuelve de altavoz a cascos (mphToSpk false)
            if ((Top.Tlf!= null) && Top.Tlf.Activity())
                TogleRxAudioRadio(!speaker);
            Top.Rd.UpdateRadioSpeakerLed();
        }

        public bool MatchActiveLink(MixerDev dev, int id)
        {
            bool found = false;
            foreach (LinkInfo link in _LinksList)
            {
                if ((link._CallId == id) && (link._CurrentDev == dev))
                {
                    found = true;
                    break;
                }
            }            
            return found;
        }
#endif

		#region Private Members

		class LinkInfo
		{
            //Dispositivo seleccionado al que conectarse
			public MixerDev _Dev;
			public MixerDir _Dir;
			public int _Priority;
            public FuentesGlp _TipoFuente;
            //Dispositivo real al que está conectado. En casos de conmutación automática por colision con tlf
            //se conecta a otro dispositivo temporalmente
            public MixerDev _CurrentDev;
            public int _CallId;

			public LinkInfo(MixerDev dev, MixerDir dir, int priority, FuentesGlp tipoFuente, int callId)
			{
				_Dev = dev;
				_Dir = dir;
				_Priority = priority;
                _TipoFuente = tipoFuente;
                _CurrentDev = dev;
                _CallId = callId;
			}
		}

		private int _RdSpeakerDev = -1;
		private int _LcSpeakerDev = -1;
        private int _HfSpeakerDev = -1;
		private int _InstructorDev = -1;
		private int _AlumnDev = -1;
		private MixerDev _RingDev = MixerDev.Invalid;
		private int _TlfHeadPhonesVolume = 50;
        private int _TlfSpeakerVolume = 50;
        private int _RdHeadPhonesVolume = 50;
		private int _LcSpeakerVolume = 50;
        private int _RdSpeakerVolume = 50;
        private int _HfSpeakerVolume = 50;
        private int _RingVolume = 50;
		private bool _InstructorJack = false;
		private bool _AlumnJack = false;
		private bool _BuzzerEnabled = false;
		private SplitMode _SplitMode = SplitMode.Off;
		private Mixer _Mixer = new Mixer();
        private List<LinkInfo> _LinksList = new List<LinkInfo>();
        private List<int> _TlfListens = new List<int>();
		private int _TlfRxLinks = 0;

        /** Dispositivos de Retorno de Grabacion */
		private int _AlumnRecorderDevIn = -1;
		private int _InstructorRecorderDevIn = -1;
		private int _RadioRecorderDevIn = -1;
		private int _LcRecorderDevIn = -1;
        private int _RadioHfRecorderIn = -1;

        /** Dispositivos de Salida de Grabacion */
        private int _AlumnRecorderDevOut = -1;
        private int _InstructorRecorderDevOut = -1;
        private int _IntRecorderDevOut = -1;
		
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

        private Timer _UnlinkGlpRadioTimer = new Timer(100);
        // Estado dinámico del la salida de audio de la telefonía
        private TlfRxAudioVia _RxTlfAudioVia = TlfRxAudioVia.HeadPhones;
        // modo solo altavoces defindo en por configuración. No cambia
        private bool _ModoSoloAltavoces = false;

        /// <summary>
        /// Gestiona el Cancelador de Echo para manos libres
        /// El valor se actualiza teniendo en cuenta la configuración del cancelador de manos libres
        /// y el estado de la telefonía por altavoz
        /// </summary>
        class ECHandsFreeType
        {
            private bool _TlfSpeakerState = false;
            public bool TlfSpeakerState
            {
                set {
                    if (Settings.Default.ECHandsFreeTlf)
                    {
                        SipAgent.EchoCancellerLCMic(value);
                        _TlfSpeakerState = value;
                    }
                }
            }
            public bool fullDuplexLC
            {
                set
                {
                    if (value)
                        SipAgent.EchoCancellerLCMic(Settings.Default.ECHandsFreeLC);
                    else
                        SipAgent.EchoCancellerLCMic(_TlfSpeakerState);
                }
            }
        };
        private ECHandsFreeType _ECHandsFreeManager = new ECHandsFreeType();

        /* AGL.REC. Flag de Llamada Telefónica en curso, para la gestión de la grabacion
         * del microfono, con llamadas anidadas... */
        private bool _InstructorMhpTlfRecInProgress = false;
        private bool _AlumnMhpTlfRecInProgress = false;
        /* Fin de la Modificacion */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnlinkGlpRadioTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _UnlinkGlpRadioTimer.Enabled = false;

            if (!Top.Rd.AnySquelch)
                Top.Recorder.SessionGlp(FuentesGlp.RxRadio, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        private int CalculateVolume(int level)
		{
			level = Math.Max(0, level);
			level = Math.Min(7, level);

			double step = (Settings.Default.MaxVolume - Settings.Default.MinVolume) / 8.0;
			return (Settings.Default.MinVolume + (int)((level + 1) * step));
		}

        /// <summary>
        /// Método Link que tiene en cuenta la fuente y el destino
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dev"></param>
        /// <param name="dir"></param>
        /// <param name="priority"></param>
		private void Link(int id, int dev, MixerDir dir, int priority)
		{
			if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
			{
				_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, dev, priority);
			}
			if ((dir == MixerDir.Recv) || (dir == MixerDir.SendRecv))
			{
				_Mixer.Link(dev, priority, id, Mixer.UNASSIGNED_PRIORITY);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dev"></param>
        /// <param name="dir"></param>
		private void Unlink(int id, int dev, MixerDir dir)
		{
			if ((dir == MixerDir.Send) || (dir == MixerDir.SendRecv))
			{
				_Mixer.Unlink(id, dev);
			}
			if ((dir == MixerDir.Recv) || (dir == MixerDir.SendRecv))
			{
				_Mixer.Unlink(dev, id);
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="priority"></param>
		private void LinkRing(int id, int priority)
		{
			Debug.Assert(_BuzzerEnabled);

			switch (_RingDev)
			{
				case MixerDev.MhpTlf:
					if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
					{
						_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _InstructorDev, priority);
					}
					if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
					{
						_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _AlumnDev, priority);
					}
					break;

				case MixerDev.MhpRd:
					if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
					{
						_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _InstructorDev, priority);
					}
					if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
					{
						_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _AlumnDev, priority);
					}
					break;

				case MixerDev.SpkLc:
                    Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.ON);
					_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _LcSpeakerDev, priority);
					break;

				case MixerDev.SpkRd:
					_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, _RdSpeakerDev, priority);
					break;

                case MixerDev.Invalid:
                    if (Top.Hw is SimCMediaHwManager)
                        //Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.ON);
                        //lalm 20201214 cambio el onoff por update.
                        Top.Lc.lc_activo = false;
                        Top.Lc.ring_activo = false;
                        Top.Lc.UpdateLcSpeakerLed();
                    break;
			}

			foreach (int listenCall in _TlfListens)
			{
				_Mixer.Link(id, Mixer.UNASSIGNED_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
			}
            id_ringing = id;
		}
        int id_ringing = -1;
        private void RingLedToOff(int id)
        {
            if (id == id_ringing)
            {
                switch (_RingDev)
                {
                    case MixerDev.MhpTlf:
                    case MixerDev.MhpRd:
                    case MixerDev.SpkRd:
                    case MixerDev.SpkLc:
                            Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.OFF);
                        break;
                    case MixerDev.Invalid:
                        if (Top.Hw is SimCMediaHwManager)
                            //Top.Hw.OnOffLed(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER, HwManager.OFF);
                            //lalm 20201214 cambio el onoff por update.
                            Top.Lc.lc_activo = false;
                            Top.Lc.ring_activo = false;
                            Top.Lc.UpdateLcSpeakerLed();
                        break;
                }
                id_ringing = -1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
		private void SetSplitOff()
		{
			int tlfDev = (_SplitMode == SplitMode.LcTf ? _InstructorDev : _AlumnDev);
			int rdDev = (_SplitMode == SplitMode.LcTf ? _AlumnDev : _InstructorDev);

			foreach (LinkInfo p in _LinksList)
			{
				switch (p._Dev)
				{
					case MixerDev.MhpTlf:
						if ((tlfDev == _AlumnDev && _AlumnJack) ||
							(tlfDev == _InstructorDev && _InstructorJack))
							Link(p._CallId, tlfDev, p._Dir, p._Priority);
						break;

					case MixerDev.MhpLc:
						Link(p._CallId, _InstructorDev, p._Dir, p._Priority);
						break;

                    case MixerDev.MhpRd:
                        if ((_RdSpeakerDev < 0) || !Top.Tlf.Activity())
                        {
                            Link(p._CallId, rdDev, p._Dir, p._Priority);
                        }
                        else
                        {
                            if ((p._Dir == MixerDir.Send) || (p._Dir == MixerDir.SendRecv))
                            {
                                _Mixer.Unlink(p._CallId, tlfDev);
                                _Mixer.Link(p._CallId, Mixer.UNASSIGNED_PRIORITY, _RdSpeakerDev, p._Priority);
                                SipAgent.SetVolume(p._CallId, _RdSpeakerVolume);
                            }
                            if ((p._Dir == MixerDir.Recv) || (p._Dir == MixerDir.SendRecv))
                            {
                                _Mixer.Link(rdDev, p._Priority, p._CallId, Mixer.UNASSIGNED_PRIORITY);
                            }
                        }
                        break;

                    case MixerDev.Ring:
						if (_RingDev == MixerDev.MhpTlf)
						{
							_Mixer.Link(p._CallId, Mixer.UNASSIGNED_PRIORITY, tlfDev, p._Priority);
						}
						else if (_RingDev == MixerDev.MhpRd)
						{
							_Mixer.Link(p._CallId, Mixer.UNASSIGNED_PRIORITY, rdDev, p._Priority);
						}
						break;
				}
			}

			if (_TlfRxLinks > 0)
			{
				foreach (int listenCall in _TlfListens)
				{
					if (_SplitMode == SplitMode.RdLc)
					{
						_Mixer.Link(_InstructorDev, Mixer.TLF_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
					}
					else if (_SplitMode == SplitMode.LcTf)
					{
						_Mixer.Link(_AlumnDev, Mixer.TLF_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode"></param>
		private void SetSplitOn(SplitMode mode)
		{
			int tlfDev = (mode == SplitMode.LcTf ? _AlumnDev : _InstructorDev);
			int rdDev = (mode == SplitMode.LcTf ? _InstructorDev : _AlumnDev);

            List<LinkInfo> enlaces = new List<LinkInfo>(_LinksList);
            
            foreach (LinkInfo p in enlaces)
			{
				switch (p._Dev)
				{
					case MixerDev.MhpTlf:
						Unlink(p._CallId, rdDev, p._Dir);
						Link(p._CallId, tlfDev, p._Dir, p._Priority);
                        //if (_InstructorDev >= 0 && _InstructorJack && mode != SplitMode.RdLc)
                        //{
                        //    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, true);
                        //}
                        //if (_AlumnDev >= 0 && _AlumnJack && mode != SplitMode.LcTf)
                        //{
                        //    Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, true);
                        //}
						break;

					case MixerDev.MhpRd:
						Unlink(p._CallId, tlfDev, p._Dir);
                        Link(p._CallId, rdDev, p._Dir, p._Priority);
						break;

					case MixerDev.Ring:
						if (_RingDev == MixerDev.MhpTlf)
						{
							_Mixer.Unlink(p._CallId, rdDev);
							_Mixer.Link(p._CallId, Mixer.UNASSIGNED_PRIORITY, tlfDev, p._Priority);
						}
						else if (_RingDev == MixerDev.MhpRd)
						{
							_Mixer.Unlink(p._CallId, tlfDev);
							_Mixer.Link(p._CallId, Mixer.UNASSIGNED_PRIORITY, rdDev, p._Priority);
						}
						break;
				}
			}

			if (_TlfRxLinks > 0)
			{
				foreach (int listenCall in _TlfListens)
				{
					_Mixer.Unlink(rdDev, listenCall);
					_Mixer.Link(tlfDev, Mixer.UNASSIGNED_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="st"></param>
		private void OnJacksChanged(object sender, JacksStateMsg st)
		{
            try
            {
			bool instructorJack = st.RightJack;
			bool alumnJack = st.LeftJack;

			if ((_SplitMode != SplitMode.Off) && (!instructorJack || !alumnJack))
			{
				_InstructorJack = st.RightJack;
				_AlumnJack = st.LeftJack;
				SetSplitMode(SplitMode.Off);
			}
			else
			{
                ManageLinksJacks(st);
                
                _InstructorJack = st.RightJack;
				_AlumnJack = st.LeftJack;
            }
		}
            catch (Exception e)
            {
                _Logger.Debug("Excepcion. Mensaje: {0}", e.Message);
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="st"></param>
		private void ManageLinksJacks(JacksStateMsg st)
		{
			bool instructorJack = st.RightJack;
			bool alumnJack = st.LeftJack;

			foreach (LinkInfo p in _LinksList)
			{
				if (p._Dev == MixerDev.MhpTlf)
				{
					// Tratamiento jacks instructor
					if (instructorJack && !_InstructorJack)	// Conexión jacks instructor
					{
						Link(p._CallId, _InstructorDev, p._Dir, p._Priority);
                        // LALM 210922
                        //Errores #3909 HMI: Telefonia en Altavoz -> Grabación Enaire
                        if ((p._Dir == MixerDir.SendRecv)|| (p._Dir == MixerDir.Send))

                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_MHP, true);
                    }
					else if (!instructorJack && _InstructorJack) // Desconexión jacks instructor
					{
						Unlink(p._CallId, _InstructorDev, p._Dir);
					}

					// Tratamiento jacks alumno
					if (alumnJack && !_AlumnJack)	// Conexión jacks alumno
					{
						Link(p._CallId, _AlumnDev, p._Dir, p._Priority);
                        // LALM 210922
                        //Errores #3909 HMI: Telefonia en Altavoz -> Grabación Enaire
                        if ((p._Dir == MixerDir.SendRecv) || (p._Dir == MixerDir.Send))
                            Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_MHP, true);
                    }
					else if (!alumnJack && _AlumnJack) // Desconexión jacks alumno
					{
						Unlink(p._CallId, _AlumnDev, p._Dir);
					}
				}
            }

            // Tratamiento jacks instructor
            if (instructorJack && !_InstructorJack)	// Conexión jacks instructor
            {
                _Logger.Debug("REC-ON <= Intructor-HPH");
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, true);
            }
            else if (!instructorJack && _InstructorJack) // Desconexión jacks instructor
            {
                _Logger.Debug("REC-OFF <= Intructor-HPH");
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, false);
            }
            // Tratamiento jacks alumno
            if (alumnJack && !_AlumnJack)	// Conexión jacks alumno
            {
                _Logger.Debug("REC-ON <= Alumn-HPH");
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, true);
            }
            else if (!alumnJack && _AlumnJack) // Desconexión jacks alumno
            {
                _Logger.Debug("REC-OFF <= Alumn-HPH");
                Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, false);
            }
		}

        /// <summary>
        /// Devuelve true si la salida de audio de radio debe pasar automaticamente de cascos a cualquier altavoz
        /// porque compite con la telefonía por cascos
       /// </summary>
        /// <param name="sender"></param>
        public bool AutChangeToRdSpeaker()
        {
            if (_RxTlfAudioVia == TlfRxAudioVia.HeadPhones &&
               (Top.Tlf.Listen.State == FunctionState.Executing || Top.Tlf.Activity() || Top.Tlf.ToneOn))
                return true;
            else 
                return false;
        }

        /// <summary>
        /// Evento recibido por cambio en la actividad de telefonía.
        /// sólo se tiene en cuenta si compite con la radio por los cascos
        /// </summary>
        /// <param name="sender"></param>
		private void OnTlfActivityChanged(object sender)
		{
            ManageTogleRxAudio();
		}

        /// <summary>
        /// Evento recibido por cambio en el tono de telefonía
        /// sólo se tiene en cuenta si compite con la radio por los cascos
        /// </summary>
        private void OnTlfToneChanged(object sender, bool toneOn)
        {
            ManageTogleRxAudio();
        }

        /// <summary>
        /// Evento recibido por cambio en la actividad de escucha.
        /// </summary>
        /// <param name="sender"></param>
        private void OnListenChanged(object sender, ListenPickUpMsg msg)
        {
            ManageTogleRxAudio();
        }

        /// <summary>
        /// Se tiene en cuenta si compite con la radio por los cascos
        /// </summary>
        private void ManageTogleRxAudio()
        {
            if (_RxTlfAudioVia == TlfRxAudioVia.Speaker)
                return;

            TogleRxAudioRadio(Top.Tlf.Listen.State == FunctionState.Executing || Top.Tlf.Activity() || Top.Tlf.ToneOn);
            Top.Rd.UpdateRadioSpeakerLed();
        }

        /// <summary>
        /// Pasa automaticamente la salida de audio de las radios seleccionadas por cascos, 
        /// al altavoz de radio cuando hay una llamada de telefonía por cascos.
        /// Si no está presente el altavoz radio principal se usa el segundo si está disponible
        /// Se mantiene en cascos si no hay altavoces
        /// </summary>
        /// <param name="mphToSpk">true si el cambio es de casco a altavoz, false en caso contrario </param>
        private void TogleRxAudioRadio(bool mphToSpk)
        {
            // AGL. Hace una copia de los enlaces establecidos en el 'Mezclador'.
            List<LinkInfo> copyLinks = new List<LinkInfo>(_LinksList);
            // AGL... ???
            if (_SplitMode == SplitMode.Off) 
            {
                foreach (LinkInfo p in copyLinks)
                {
                    //Examina los enlaces correspondientes al Microfono para RADIO
                    if (p._Dev == MixerDev.MhpRd)
                    {
                        //Cambia de cascos a altavoz directamente en _Mixer, sin alterar _Links
                        //si hay disponibilidad de algún altavoz
                        if (mphToSpk)
                        {
                            if (Top.Hw.RdSpeaker || Top.Rd.HFSpeakerAvailable())
                            {
                                if ((p._Dir == MixerDir.Send) || (p._Dir == MixerDir.SendRecv))
                                {
                                    LinkInfo linkReal = _LinksList.Find(elem => elem == p);
                                    ConectaAltavozDisponible(ref linkReal);
                                    //Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, true);
                                }
                            }
                            else
                                _Logger.Info("No hay altavoces disponibles para poder conmutar radio desde cascos");
                        }
                        else //Cambia de altavoz (el que estuviera guardado) a cascos, según valor de _Links
                        {
                            //Top.Recorder.Rec(CORESIP_SndDevType.CORESIP_SND_RADIO_RECORDER, false);
                            Unlink(p._CallId, p._CurrentDev, MixerDir.Send);
                            p._CurrentDev = MixerDev.MhpRd;

                            if (_InstructorDev >= 0 && _InstructorJack)
                            {
                                //Link teniendo en cuenta la dirección
                                Link(p._CallId, _InstructorDev, p._Dir, p._Priority);
                                //Top.Recorder.Rec(p.Key, CORESIP_SndDevType.CORESIP_SND_INSTRUCTOR_RECORDER, p.Value.Dir, p.Value.Priority, true);
                            }
                            if (_AlumnDev >= 0 && _AlumnJack)
                            {

                                //Link teniendo en cuenta la dirección
                                Link(p._CallId, _AlumnDev, p._Dir, p._Priority);

                                //Top.Recorder.Rec(p.Key, CORESIP_SndDevType.CORESIP_SND_ALUMN_RECORDER, p.Value.Dir, p.Value.Priority, true);
                            }
                            // Ajustar el volumen sólo para el dispositivo source
                            int dev = 0;
                            if ((p._Dir == MixerDir.Recv) || (p._Dir == MixerDir.SendRecv))
                            {
                                dev = _AlumnDev;
                            }
                            if ((p._Dir == MixerDir.Send) || (p._Dir == MixerDir.SendRecv))
                            {
                                dev = p._CallId;
                            }
                            if (dev != 0)
                                SipAgent.SetVolume(dev, _RdHeadPhonesVolume);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Conecta el elemento que se pasa de cascos a un altavoz disponible.
        /// Si está conectado a un altavoz y sigue disponible no se cambia de altavoz.
        /// Si no hay altavoces disponibles no se hace nada.
        /// </summary>
        /// <param name="mphToSpk">true si el cambio es de casco a altavoz, false en caso contrario </param>
        private void ConectaAltavozDisponible(ref LinkInfo link)
        {
            if (((link._CurrentDev == MixerDev.SpkRd) && Top.Hw.RdSpeaker) ||
               ((link._CurrentDev == MixerDev.SpkHf) && Top.Rd.HFSpeakerAvailable()))
                return;

            Unlink(link._CallId, link._CurrentDev, MixerDir.Send);

            if (Top.Hw.RdSpeaker)
            {
                _Mixer.Link(link._CallId, Mixer.UNASSIGNED_PRIORITY, _RdSpeakerDev, link._Priority);
                SipAgent.SetVolume(link._CallId , _RdSpeakerVolume);
                link._CurrentDev = MixerDev.SpkRd;
            }
            else if (Top.Rd.HFSpeakerAvailable())
            {
                _Mixer.Link(link._CallId , Mixer.UNASSIGNED_PRIORITY, _HfSpeakerDev, link._Priority);
                SipAgent.SetVolume(link._CallId, _HfSpeakerVolume);
                link._CurrentDev = MixerDev.SpkHf;
            }
        }

        /// <summary>
        /// 
        /// </summary>
		private void TlfLinkAdded()
		{
			if (_TlfRxLinks++ == 0)
			{
				foreach (int listenCall in _TlfListens)
				{
					if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
					{
						_Mixer.Link(_InstructorDev, Mixer.TLF_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
					}
					if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
					{
						_Mixer.Link(_AlumnDev, Mixer.TLF_PRIORITY, listenCall, Mixer.UNASSIGNED_PRIORITY);
					}
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		private void TlfLinkRemoved()
		{
			if (--_TlfRxLinks == 0)
			{
				foreach (int listenCall in _TlfListens)
				{
					if ((_InstructorDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.LcTf)))
					{
						_Mixer.Unlink(_InstructorDev, listenCall);
					}
					if ((_AlumnDev >= 0) && ((_SplitMode == SplitMode.Off) || (_SplitMode == SplitMode.RdLc)))
					{
						_Mixer.Unlink(_AlumnDev, listenCall);
					}
				}
			}
		}

        /// <summary>
        /// Se recibe cuando hay un cambio de estado de cualquiera de los altavoces radio
        /// Se utiliza para cambiar la salida radio que está seleccionada en cascos pero se ha cambiado
        /// temporalmente a altavoz. Se cambia al altavoz disponible.
        /// </summary>
        /// <param name="sender">no se usa</param>
        /// <param name="st">no se usa</param>
        private void OnHwChanged(object sender, JacksStateMsg st)
        {
            if (Top.Tlf.Activity())
            {
                TogleRxAudioRadio(Top.Tlf.Activity());
                Top.Rd.UpdateRadioSpeakerLed();
            }
        }

        /// <summary>
        /// Unlink con dev type. Se llama al unlink con el device
        /// </summary>
        /// <param name="id"> llamada que se quiere desconectar</param>
        /// <param name="devType">tipo de dispositivo que se quiere desconectar</param>
        /// <param name="dir"> dirección</param>
        private void Unlink(int id, MixerDev devType, MixerDir dir)
        {
            int devDest = -1, devDest2 = -1;
            int devDestRecorder = -1, devDestRecorder2 = -1;
            switch (devType)
            {
                case MixerDev.SpkRd:
                    devDest = _RdSpeakerDev;
                    break;
                case MixerDev.SpkHf:
                    devDest = _HfSpeakerDev;
                    break;
                case MixerDev.MhpRd:
                    if (_SplitMode == SplitMode.Off)
                    {
                        if (_InstructorDev >= 0 && _InstructorJack)
                        {
                            devDest = _InstructorDev;
                            devDestRecorder = _InstructorRecorderDevIn;
                        }
                        if (_AlumnDev >= 0 && _AlumnJack)
                        {
                            devDest2 = _AlumnDev;
                            devDestRecorder2 = _AlumnRecorderDevIn;
                        }
                    }
                    break;
                default:
                    _Logger.Info("devType not implemented in this procedure {0}", devType);
                    break;
            }

            if (devDest != -1) Unlink(id, devDest, dir);
            if (devDestRecorder != -1) Unlink(id, devDestRecorder, dir);
            if (devDest2 != -1) Unlink(id, devDest2, dir);
            if (devDestRecorder2 != -1) Unlink(id, devDestRecorder2, dir);
        }

        /// <summary>
        /// Activa o desactiva el cancelador de echo para manos libres si hay 
        /// recepción y transmision de tipo LC
        /// </summary>
        private void ManageECHandsFreeByLC()
        {
            bool sendLc = false;
            bool receiveLc = false;
            foreach (LinkInfo link in _LinksList)
            {
                if ((link._TipoFuente == FuentesGlp.RxLc) || (link._TipoFuente == FuentesGlp.TxLc))
                    if (link._Dir == MixerDir.Recv)
                        receiveLc = true;
                    else if (link._Dir == MixerDir.Send)
                        sendLc = true;
            }
            _ECHandsFreeManager.fullDuplexLC = sendLc && receiveLc;
        }
		#endregion

        //LALM 211029 
        //# Error 3629 Terminal de Audio -> Señalización de Actividad en LED ALTV Intercom cuando seleccionada TF en ALTV
        public bool AltavozRingCompartidoLC
        {
            get { return (_RingDev == MixerDev.SpkLc); }
        }
    }
}
