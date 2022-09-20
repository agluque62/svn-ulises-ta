using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Timers;

using HMI.Model.Module.Messages;
using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;
using System.Collections;

namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// 
    /// </summary>
#if DEBUG
	public class TlfPosition : IDisposable
#else
	class TlfPosition : IDisposable
#endif	
	{
        /// <summary>
        /// 
        /// </summary>
		public event GenericEventHandler TlfPosStateChanged;
		public event GenericEventHandler<SnmpStringMsg<string, string>> SetSnmpString;
        public event GenericEventHandler<string> ForwardedCallMsg;

        /// <summary>
        /// 
        /// </summary>
		public int Pos
		{
			get { return _Pos; }
		}

        /// <summary>
        /// 
        /// </summary>
		public string Literal
		{
			get { return _Literal; }
		}

        /// <summary>
        /// 
        /// </summary>
		public TlfState OldState
		{
			get { return _OldState; }
		}

        ///lalm 211015
        ///#3618 Señal de Llamada Entrante durante CONV en altavoz
        bool LineaCalienteEnUso()
        {
            // 220617 solo puedo saber si el speker de linea caliente esta en uso
            if (Top.Mixer.SpkLcInUse())
                return true;

            return false;
        }

        //LALM 211029 Cambio lc activo por activity y se implementa activitylc
        //# Error 3629 Terminal de Audio -> Señalización de Actividad en LED ALTV Intercom cuando seleccionada TF en ALTV
        public virtual int Tone
        {
            get { return _Tone; }
            set
            {
                _Tone = value;
                Top.Mixer.Link(_Tone, MixerDev.Ring, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Telefonia);
                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);//#5829
            }
        }

        bool LineaTlfEnUso()
        {

            if (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker)
            {
                if (Top.Tlf.Activity())
                    return true;
            }
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
		public virtual TlfState State
		{
			get { return _State; }
			set 
			{
                if (_State != value)
				{
					if (_Tone >= 0)
					{
						Top.Mixer.Unlink(_Tone);
						SipAgent.DestroyWavPlayer(_Tone);
						_Tone = -1;
					}

					if (((_SipCall != null) && !_SipCall.Monitoring)
                        || (_SipCall== null))
					{
						switch (value)
						{
							case TlfState.Congestion:
								_Tone = SipAgent.CreateWavPlayer("Resources/Tones/Congestion.wav", true);
								Top.Mixer.LinkTlf(_Tone, MixerDir.Send, Mixer.TLF_PRIORITY);
								break;
							case TlfState.OutOfService:
								_Tone = SipAgent.CreateWavPlayer("Resources/Tones/OutOfService.wav", true);
								Top.Mixer.LinkTlf(_Tone, MixerDir.Send, Mixer.TLF_PRIORITY);
								break;
							case TlfState.Busy:
								_Tone = SipAgent.CreateWavPlayer("Resources/Tones/Busy.wav", true);
								Top.Mixer.LinkTlf(_Tone, MixerDir.Send, Mixer.TLF_PRIORITY);
								break;

							case TlfState.Conf:
								break;

							case TlfState.Out:
								_Tone = SipAgent.CreateWavPlayer("Resources/Tones/Calling.wav", true);
								Top.Mixer.LinkTlf(_Tone, MixerDir.Send, Mixer.TLF_PRIORITY);
								break;
							case TlfState.RemoteHold:
								_Tone = SipAgent.CreateWavPlayer("Resources/Tones/Hold.wav", true);
								Top.Mixer.LinkTlf(_Tone, MixerDir.Send, Mixer.TLF_PRIORITY);
								break;
							case TlfState.In:
                                //LALM 211014
                                //#3618 Señal de Llamada Entrante durante CONV en altavoz
                                // Si La linea caliente esta ocupada, genero otro tono.
                                if (LineaCalienteEnUso() || LineaTlfEnUso())
                                {
                                    _Tone = SipAgent.CreateWavPlayer("Resources/Tones/RingNoIntrusivo.wav", true);
                                    Top.Mixer.Link(_Tone, MixerDev.Ring, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Telefonia);
                                    //Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);//#5829
                                }
                                else
                                {
                                    _Tone = SipAgent.CreateWavPlayer("Resources/Tones/Ring.wav", true);
                                    Top.Mixer.Link(_Tone, MixerDev.Ring, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Telefonia);
                                    Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);//#5829
                                }
                                break;
							case TlfState.InPrio:
                                //LALM 211014
                                //#3618 Señal de Llamada Entrante durante CONV en altavoz
                                // Si La linea caliente esta ocupada, genero otro tono.
                                if (LineaCalienteEnUso() || LineaTlfEnUso())
                                    _Tone = SipAgent.CreateWavPlayer("Resources/Tones/RingNoIntrusivo.wav", true);
                                else
                                    _Tone = SipAgent.CreateWavPlayer("Resources/Tones/RingPrio.wav", true);
                                Top.Mixer.Link(_Tone, MixerDev.Ring, MixerDir.Send, Mixer.UNASSIGNED_PRIORITY, FuentesGlp.Telefonia);
                                Top.Mixer.SetVolumeTones(CORESIP_SndDevType.CORESIP_SND_LC_SPEAKER);//#5829
                                break;
                            //#2855
                            case TlfState.offhook:
                                _Tone = SipAgent.CreateWavPlayer("Resources/Tones/Descolgado.wav", true);
                                Top.Mixer.LinkTlf(_Tone, MixerDir.Send, Mixer.TLF_PRIORITY);
                                break;
                        }
                    }

					if ((_EvSub != IntPtr.Zero) && (value != TlfState.Out))
					{
						int code = SipAgent.SIP_ERROR;

						switch (value)
						{
							case TlfState.Set:
							case TlfState.Conf:
							case TlfState.RemoteHold:
								code = SipAgent.SIP_OK;
								break;
							case TlfState.Congestion:
								code = SipAgent.SIP_CONGESTION;
								break;
							case TlfState.OutOfService:
								code = SipAgent.SIP_NOT_FOUND;
								break;
							case TlfState.Busy:
								code = SipAgent.SIP_BUSY;
								break;
                            //#2855
                            case TlfState.offhook:
                                break;
                        }

                        SipAgent.TransferNotify(_EvSub, code);
						_EvSub = IntPtr.Zero;
					}

					_OldState = _State;
					_State = value;

					switch (_State)
					{
						case TlfState.Out:
						case TlfState.In:
						case TlfState.InPrio:
						case TlfState.RemoteIn:
							_RemoteHangUp = false;
							break;
                        case TlfState.Idle:
                            _SipCall = null;
                            break;
                        //#2855
                        case TlfState.offhook:
                            _SipCall = null;
                            break;
                    }

                    DeleteInScreen();
                    General.SafeLaunchEvent(TlfPosStateChanged, this);
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public int CallId
		{
			get { return (_SipCall != null) ? _SipCall.Id : -1; }
		}

        /// <summary>
        /// 
        /// </summary>
		public CORESIP_Priority CallPriority
		{
			get
			{
				if ((_SipCall != null) && _SipCall.IsActive)
				{
					return _SipCall.Priority;
				}

				return CORESIP_Priority.CORESIP_PR_UNKNOWN;
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public CORESIP_Priority EfectivePriority
		{
			get 
			{
				if ((_SipCall != null) && _SipCall.IsActive)
				{
					return (_Conference != null ? _Conference.Priority : _SipCall.Priority);
				}

				return CORESIP_Priority.CORESIP_PR_UNKNOWN; 
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public TlfConference Conference
		{
			get { return _Conference; }
		}

        /// <summary>
        /// 
        /// </summary>
		public string Uri
		{
			get
			{
				if (_Channels.Count > 0)
				{
					return _Channels[0].Uri;
				}

				return null;
			}
		}
        public string UriPropia
        {
            get
            {
                if (_Channels.Count > 0)
                {
                    return _Channels[0].UriPropia;
                }

                return null;
            }
        }

        public bool ChAllowsPriority()
        {
            bool ret = true;
            if (_Channels.Count > 0) ret = _Channels[0].PriorityAllowed;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Domain
        {
            get
            {
                if (_Channels.Count > 0)
                {
                    return _Channels[0].Domain;
                }

                return null;
            }
        }

        /// <summary>
        /// Devuelve los numeros de abonado del primer canal y primer destino
        /// </summary>
        public IList<string> NumeroAbonado
        {
            get
            {
                if (_Channels.Count > 0)
                {                    
                    return _Channels[0].RemoteDestinations[0].Ids;
                }

                return null;
            }
            set //Only for unit testing
            {
            } 
        }
        /// <summary>
        /// Devuelve los numeros de abonado del primer canal 
        /// Es virtual para poder mockearla
        /// </summary>
        public virtual IList<string> NumerosAbonado
        {
            get
            {
                if (_Channels.Count > 0)
                {
                    List<string> ret = new List<string>();
                    foreach (SipRemote remote in _Channels[0].RemoteDestinations)
                        ret.Add(remote.Ids[0]);
                    return ret;
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
		public bool IsTop
		{
			get { return (_Channels.Count > 0) && (_Channels[0].Prefix == Cd40Cfg.INT_DST); }
		}

        public bool IsPP
        {
            get { return (_Channels.Count > 0) && (_Channels[0].IsPP);
            }
        }
        public bool IsAtsIP
        {
            get
            {  
                return (_Channels.Count > 0) && (_Channels[0].Prefix == Cd40Cfg.ATS_DST) && 
                       (_Channels[0].ListLines.Count > 0) && (_Channels[0].ListLines[0].centralIP) ;
            }
        }

        public bool ChAllowsForward
        {
            get { return IsPP || IsTop || IsAtsIP; }
        }

        /// <summary>
        /// 
        /// </summary>
		public CfgEnlaceInterno Cfg
		{
			get { return _Cfg; }
		}
        public List<SipChannel> Channels
        {
            get { return _Channels; }
        }

        /// <summary>
        /// 
        /// </summary>
		public bool RemoteHangUp
		{
			get { return _RemoteHangUp; }
		}

        /// <summary>
        /// 
        /// </summary>
        public bool IpDestinity
        {
            get { return _IpDestinity; }
            set { IpDestinity = value; }
        }

        /// <summary>
        /// Se pone a true cuando se quiere aparcar la llamada pero está en Out. Hay que esperar
        /// a que se establezca.
        /// </summary>
        public bool HoldOnEstablish
        {
            set { _HoldOnEstablish = value; }
        }

        public bool InOperation()
        {
            if (Cfg != null)
            {
                switch (State)
                {
                    case TlfState.Conf:
                    case TlfState.Hold:
                    case TlfState.In:
                    case TlfState.InPrio:
                    case TlfState.InProcess:
                    case TlfState.Out:
                    case TlfState.RemoteHold:
                    case TlfState.RemoteIn:
                    case TlfState.Set:
                        return true;
                    case TlfState.NotAllowed:
                    case TlfState.Inactive:
                    case TlfState.Mem:
                    case TlfState.OutOfService:
                    case TlfState.PaPBusy:
                    case TlfState.RemoteMem:
                    case TlfState.Unavailable:
                    case TlfState.UnChanged:
                    case TlfState.Busy:
                    case TlfState.Congestion:
                        return false;
                }
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
		public TlfPosition(int pos)
		{
			_Pos = pos;

			_CallTout.AutoReset = false;
			_CallTout.Elapsed += OnCallTimeout;
        }

		#region IDisposable Members

        /// <summary>
        /// 
        /// </summary>
		public void Dispose()
		{
			foreach (SipChannel ch in _Channels)
			{
				ch.RsChanged -= OnRsChanged;
			}
            _Channels.Clear();
            _CallTout.Dispose();
            if (_Conference != null)
            _Conference.Dispose();
            if (_SipCall != null)
                _SipCall = null;
        }

		#endregion

        /// <summary>
        /// 
        /// </summary>
		public void Reset()
		{
			MakeHangUp(0);

            State = TlfState.Unavailable; //para lanzar el evento antes de borrar los datos
			_Cfg = null;
			_Literal = "";
			_Channels.Clear();

			//State = TlfState.Unavailable;
            _HoldOnEstablish = false;
		}

        //LALM 211201
        //#2855
        public void DescuelgaPos(bool descolgar)
        {
            if (State == TlfState.Idle && descolgar)
                State = TlfState.offhook;
            else if (State == TlfState.offhook && !descolgar)
                State = TlfState.Idle;
        }
        //*2855


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"></param>
		public virtual void Reset(CfgEnlaceInterno cfg)
		{
            bool setToIdle = false;
			_Literal = cfg.Literal;
			_Priority = (CORESIP_Priority)cfg.Prioridad;
            //Si cambia el origen, lo reinicio sin colgar porque ya ha cambiado la cuenta.
            if ((_Cfg != null) && !cfg.OrigenR2.Equals(_Cfg.OrigenR2))
            {
                //A la espera de un cambio en el SOAP para que venga el OrigenR2 coherente
                //tengo que colgar porque puede no ser un cambio de OrigenR2 sino de tipo 
                //de destino
                MakeHangUp(0);
                setToIdle = true;
                State = TlfState.Idle;
                _Logger.Warn("Cuelgo llamada con {0} por cambio de identidad", _Literal);
            }
            _Cfg = cfg;
			_Channels.Clear();

			foreach (CfgRecursoEnlaceInterno dst in cfg.ListaRecursos)
			{
				switch (dst.Prefijo)
				{
					case Cd40Cfg.INT_DST:
						string hostId = Top.Cfg.GetUserHost(dst.NombreRecurso);
						
						if ((hostId != null) && (_Channels.Find(delegate(SipChannel channel) 
							{ return ((channel.Prefix == dst.Prefijo) && (channel.Id == hostId)); }) == null))
						{
							SipChannel ch = new IntChannel(cfg.OrigenR2, hostId, dst.NombreRecurso, dst.Prefijo);
							ch.RsChanged += OnRsChanged;

							_Channels.Insert(0, ch);
						}

						break;

					case Cd40Cfg.EyM_DEST:	// Destinos EyM 4W para deshabilitar el echoCanceller
					case Cd40Cfg.PP_DST:
                    case Cd40Cfg.UNKNOWN_DST:
                    case Cd40Cfg.IP_DST:
                        if (this is TlfIaPosition)
                        {
                            StrNumeroAbonado route = null;
                            TlfNet securityNet = Top.Cfg.GetNet(dst.Prefijo, dst.NumeroAbonado, ref route);

                            if (securityNet != null)
                            {
                                SipChannel ch = _Channels.Find(delegate(SipChannel channel) { return ((channel.Prefix == dst.Prefijo) && (channel.Id == securityNet.Id)); });

                                if (ch == null)
                                {
                                    if ((dst.Prefijo ==  Cd40Cfg.PP_DST) && (securityNet.Lines[0].RsLine != null))
                                        ch = new TlfPPChannel(cfg.OrigenR2, dst.NumeroAbonado, securityNet.Lines[0].RsLine.Id, dst.Prefijo, dst.Interface);
                                    else
                                        ch = new TlfNetChannel(securityNet, cfg.OrigenR2, dst.NumeroAbonado, null, dst.Prefijo, _UnknownResource);
                                    ch.RsChanged += OnRsChanged;

                                    _Channels.Add(ch);
                                }
                                else
                                {
                                    ch.AddRemoteDestination(dst.NumeroAbonado, null);
                                }
                            }
                        }
                        else
                        {
                            if (_Channels.Find(delegate(SipChannel channel)
                                { return ((channel.Prefix == dst.Prefijo) && (channel.Id == dst.NombreRecurso)); }) == null)
                            {
                                SipChannel ch = new TlfPPChannel(cfg.OrigenR2, dst.NumeroAbonado, dst.NombreRecurso, dst.Prefijo, dst.Interface);
                                ch.RsChanged += OnRsChanged;

                                _Channels.Add(ch);
                            }
                        }
						break;
                    /*
					case Cd40Cfg.IP_DST:
						{
							SipChannel ch = new IpChannel(cfg.OrigenR2, dst.NumeroAbonado, dst.Prefijo);
							_Channels.Add(ch);
						}
						break;
                    */
                    default:
						string[] userId = Top.Cfg.GetUserFromAddress(dst.Prefijo, dst.NumeroAbonado);
						if (userId != null)
						{
							dst.NombreRecurso = userId[1];
							dst.Prefijo = Cd40Cfg.INT_DST;
							goto case Cd40Cfg.INT_DST;
						}
                        //Se crea un canal por cada red
                        TlfNet net = Top.Cfg.GetIPNet(dst.Prefijo, dst.NumeroAbonado);
                        if (net != null)
                            AddChannel(net, cfg.OrigenR2, dst.NumeroAbonado, null, dst.Prefijo);
                        else
                            _Logger.Warn("No encuentro red IP para {0} con prefijo {1} ", dst.NumeroAbonado, dst.Prefijo);

						StrNumeroAbonado altNet = null;
						net = Top.Cfg.GetNet(dst.Prefijo, dst.NumeroAbonado, ref altNet);
                        if (net != null)
                            AddChannel(net, cfg.OrigenR2, dst.NumeroAbonado, null, dst.Prefijo);
                        else
                            _Logger.Warn("No encuentro red AVGN para {0} con prefijo {1} ", dst.NumeroAbonado, dst.Prefijo);

						if (altNet != null)
						{
							net = Top.Cfg.GetNet(altNet.Prefijo, altNet.NumeroAbonado, ref altNet);
                            if (net != null)
                                AddChannel(net, cfg.OrigenR2, altNet.NumeroAbonado, dst.NumeroAbonado, altNet.Prefijo);
                        else
                            _Logger.Warn("No encuentro red backup/alternativa para {0} con prefijo {1} ", dst.NumeroAbonado, dst.Prefijo);
						}

						break;
				}
			}

			if ((_SipCall != null) && (_SipCall.Ch != null))
            {
                if (!_SipCall.IsValid(_Channels))
                {
                    MakeHangUp(0);	// Cortamos llamadas y quitamos busy y congestion
                    State = TlfState.Idle;
                    _Logger.Warn("Cuelgo llamada con {0} por cambio de configuracion", _Literal);
                }
            }

			if (_SipCall == null)
			{
                State = GetReachableState(setToIdle);
			}
		}

        private void  AddChannel(TlfNet net, string OrigenAcc, string numeroAbonado, string subNumber, uint prefijo)
        {
                SipChannel ch = _Channels.Find(delegate(SipChannel channel) { return ((channel.Prefix == prefijo) && (channel.Id == net.Id)); });

                if (ch == null)
                {
                    ch = new TlfNetChannel(net, OrigenAcc, numeroAbonado, null, prefijo, _UnknownResource);
                    ch.RsChanged += OnRsChanged;

                    _Channels.Add(ch);
                }
                else
                {
                    ch.AddRemoteDestination(numeroAbonado, null);
                }
         }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conference"></param>
		public void AddToConference(TlfConference conference)
		{
			Debug.Assert((_State == TlfState.Set) || (_State == TlfState.Conf) ||
				(_State == TlfState.Hold) || (_State == TlfState.RemoteHold));
			Debug.Assert((_SipCall != null) && _SipCall.IsActive);
			Debug.Assert(_Conference == null);

			SipAgent.AddCallToConference(_SipCall.Id);
			_Conference = conference;
		}

        /// <summary>
        /// 
        /// </summary>
		public void RemoveFromConference()
		{
			Debug.Assert((_State == TlfState.Set) || (_State == TlfState.Conf) ||
				(_State == TlfState.Hold) || (_State == TlfState.RemoteHold));
			Debug.Assert((_SipCall != null) && _SipCall.IsActive);
			Debug.Assert(_Conference != null);

			_Conference = null;
			SipAgent.RemoveCallFromConference(_SipCall.Id);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prio"></param>
		public void Call(bool prio)
		{
			Debug.Assert((_State == TlfState.NotAllowed) || (_State == TlfState.Unavailable) || (_State == TlfState.PaPBusy) || (_State == TlfState.Idle));
			Debug.Assert(_SipCall == null);
				
            if (Top.Tlf.Forward.IsForwardedHead(NumeroAbonado))
            {                
                return;
            }

			CORESIP_Priority priority = prio ? CORESIP_Priority.CORESIP_PR_EMERGENCY : _Priority;
			_SipCall = SipCallInfo.NewTlfCall(_Channels, priority, null);

			State = TryCall();
            if (_State == TlfState.Out)
            {
                _CallTout.Enabled = true;
            }
			//if (State == TlfState.NotAllowed)
			//{
			//    MakeHangUp(0);
			//    State = TlfState.Idle;
			//}
		}
        public bool CallPickUp(TlfPickUp.DialogData dialog)
        {
            int sipCallId = -1;
            SipPath path = null;
            SipChannel channel = null;
            Debug.Assert(_State == TlfState.In);
            Debug.Assert(_SipCall == null);
            _SipCall = SipCallInfo.NewReplacesCall (_Channels, dialog);
            foreach (SipChannel ch in _Channels)
            {
                path = ch.FindPath(dialog.remoteId);
                if (path != null)
                {
                    channel = ch;
                    break;
                }
            }
            if ((channel == null) || (path == null))
                return false;

            CORESIP_CallFlags flags = CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;
            if (path.ModoSinProxy == false)
                flags |= CORESIP_CallFlags.CORESIP_CALL_EXTERNAL_IP;

            sipCallId = SipAgent.MakeTlfCallReplaces(channel.AccId, dialog.remoteId, _SipCall.Priority, flags, _SipCall.Dialog.callId,
                _SipCall.Dialog.toTag, _SipCall.Dialog.fromTag);
            _SipCall.Update(sipCallId, channel.AccId, channel.RemoteDestinations[0].Ids[0], channel, path.Remote, path.Line);

            _Logger.Info("PickUp call to: {0} {1:X}", dialog.remoteId, sipCallId);
            if (sipCallId != -1)
            {
                _State = TlfState.Out;
                _CallTout.Enabled = true;
            }
            return (sipCallId != -1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferInfo"></param>
		public void Call(CORESIP_CallTransferInfo transferInfo)
		{
            String dstUri = "";
			_EvSub = transferInfo.EvSub;

			switch (_State)
			{
				case TlfState.Unavailable:
				case TlfState.PaPBusy:
				case TlfState.Idle:
					Debug.Assert(_SipCall == null);
					break;
				case TlfState.In:
				case TlfState.InPrio:
				case TlfState.RemoteIn:
					Accept(null);
					break;
				case TlfState.Hold:
					Unhold();
					break;
				default:		// No deberíamos llegar aquí
					Debug.Assert("Estado inesperado ejecutando la peticion de transferencia" == null);
					return;
			}

			_SipCall = SipCallInfo.NewTlfCall(_Channels, _Priority, transferInfo.ReferBy);

			if (transferInfo.ReferTo.Contains("Replaces="))
			{
				foreach (SipChannel ch in _Channels)
				{
                    //220602 mensaje motivorechazo.
					SipPath path = ch.FindPath(transferInfo.DstId, transferInfo.DstIp, transferInfo.DstSubId, transferInfo.DstRs);
                    if (path != null)
                    {
                        CORESIP_CallFlags flags = Settings.Default.EnableEchoCanceller && (ch.Prefix == Cd40Cfg.PP_DST || (ch.Prefix >= Cd40Cfg.RTB_DST && ch.Prefix < Cd40Cfg.EyM_DEST)) ? 
                            CORESIP_CallFlags.CORESIP_CALL_EC : CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;

                       if (path.ModoSinProxy == false)
                            flags |= CORESIP_CallFlags.CORESIP_CALL_EXTERNAL_IP;


                       // //para que funcione con el plugin del proxy para lineas de redes externas 
                       //if ((ch.Prefix >= Cd40Cfg.RTB_DST) && (!transferInfo.ReferTo.Contains(String.Format("{0}@", path.Line.Id))))
                       //{
                       //    dstUri = transferInfo.ReferTo.Replace("<sip:", String.Format("<sip:{0:D2}", ch.Prefix));
                       //}
                       //else
                           dstUri = transferInfo.ReferTo;

                       int sipCallId = SipAgent.MakeTlfCall(ch.AccId, dstUri, _SipCall.ReferBy, _SipCall.Priority, flags);
                       try
                       {
                           Top.Tlf.SaveParamLastCall(sipCallId, ch.AccId, dstUri, _SipCall.ReferBy, _SipCall.Priority, flags);
                           _Logger.Debug("Guardo llamada " + sipCallId.ToString() + dstUri);
                       }
                       catch (Exception ex)
                       {
                           _Logger.Warn("Imposible salvar llamada" + ex);
                       }
                       _SipCall.Update(sipCallId, ch.AccId, transferInfo.DstId, ch, path.Remote, path.Line);

                    _Literal = TlfManager.GetDisplayName(dstUri);
                    if (String.IsNullOrEmpty(_Literal))
                        _Literal = transferInfo.DstId;
                    State = TlfState.Out;
                    _Logger.Info("Making call on transfer to: {0} {1:X} prefix ch {2} rs {3}", dstUri, sipCallId, ch.Prefix, transferInfo.DstRs);
                    return;
                    }
				}

				State = TlfState.Congestion;
			}
			else
			{
				State = TryCall();

				if (_State == TlfState.Out)
				{
					_CallTout.Enabled = true;
				}
			}
		}

        /// <summary>
        /// 
        /// </summary>
		public void RetryCall()
		{
            if (ChAllowsPriority())
            {
                Debug.Assert((_State == TlfState.Busy) || (_State == TlfState.Congestion));
                Debug.Assert((_SipCall != null) && !_SipCall.IsActive);

                _SipCall.UpdateOutgoingCall(_Channels, CORESIP_Priority.CORESIP_PR_EMERGENCY);

                State = TlfState.Out;
                State = TryCall();

                if (_State == TlfState.Out)
                {
                    _CallTout.Enabled = true;
                }
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
		public void HangUp(int reason)
		{
			if (_SipCall != null)
			{
				MakeHangUp(reason);
			}
			else
			{
				// Reflejamos de nuevo nuestro estado por si hay una disfuncion en el HMI
				General.SafeLaunchEvent(TlfPosStateChanged, this);
			}
            //Se cambia el estado siempre, aunque no haya llamada, 
            //para tener una forma de limpiar la tecla en caso de inconsistencia
            State = TlfState.Idle;
            State = GetReachableState();

            //State = (_State == TlfState.Set) || (_State == TlfState.Congestion) || (_State == TlfState.OutOfService) || (_State == TlfState.Busy) ? GetReachableState()/*TlfState.Idle*/ : (Top.Tlf.PriorityCall ? TlfState.PaPBusy : TlfState.Idle);

            Top.Tlf.PriorityCall = false;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="conference"></param>
		public void Accept(TlfConference conference)
		{
			Debug.Assert((_State == TlfState.In) || (_State == TlfState.InPrio) || (_State == TlfState.RemoteIn));
			Debug.Assert((_SipCall != null) && _SipCall.IsActive);

            CORESIP_CallFlags flags = Settings.Default.EnableEchoCanceller && (_SipCall.Ch.Prefix == Cd40Cfg.PP_DST || (_SipCall.Ch.Prefix >= Cd40Cfg.RTB_DST && _SipCall.Ch.Prefix < Cd40Cfg.EyM_DEST)) ? CORESIP_CallFlags.CORESIP_CALL_EC : CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;
			SipAgent.AnswerCall(_SipCall.Id, SipAgent.SIP_OK | ((int)flags << 16), conference != null);

			_Conference = conference;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
		public void Reject(int code)
		{
			Debug.Assert((_State == TlfState.In) || (_State == TlfState.InPrio) || (_State == TlfState.RemoteIn));
			Debug.Assert((_SipCall != null) && _SipCall.IsActive);

			SipAgent.AnswerCall(_SipCall.Id, code, false);
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="force"></param>
		public void Hold(bool force)
		{
            Debug.Assert((_State == TlfState.Set) || (_State == TlfState.Conf) || (_State == TlfState.RemoteHold));
			Debug.Assert((_SipCall != null) && _SipCall.IsActive);

            try
            {
                SipAgent.HoldCall(_SipCall.Id);

			    if (force)
			    {
				    Top.Mixer.Unlink(_SipCall.Id);
				    State = TlfState.Hold;
			    }

                Top.WorkingThread.Enqueue("SetSnmp", delegate()
                {
                    string snmpString = Top.Cfg.PositionId + "_" + "HOLD" + "_" + this.Literal;
                    General.SafeLaunchEvent(SetSnmpString, this, new SnmpStringMsg<string, string>(Settings.Default.TlfFacilityOid, snmpString));
                });
            }
            catch (Exception exc)
            {
                _Logger.Error("Exception Hold: ", exc.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
		public void Unhold()
		{
            Debug.Assert((_State == TlfState.Hold) || (_State == TlfState.Set) || (_State == TlfState.Conf) || (_State == TlfState.RemoteHold));
            Debug.Assert((_SipCall != null) && _SipCall.IsActive);
            try
            {
			    SipAgent.UnholdCall(_SipCall.Id);
                _HoldOnEstablish = false;
		    }
            catch (Exception exc)
            {
                _Logger.Error("Exception Unhold: ", exc.Message);
            }

		}

        /// <summary>
        /// 
        /// </summary>
		public void Listen()
		{
			Debug.Assert((_State == TlfState.Unavailable) || (_State == TlfState.PaPBusy) || (_State == TlfState.Idle));
			Debug.Assert(_SipCall == null);

			_SipCall = SipCallInfo.NewMonitoringCall(_Channels);

			State = TryCall();
			if (_State == TlfState.Out)
			{
				_CallTout.Enabled = true;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="dst"></param>
        /// <param name="number"></param>
        /// <returns></returns>
		public virtual bool CanHandleOutputCall(uint prefix, string dst, string number, string literal = null, bool notUsed = false)
		{
			foreach (SipChannel ch in _Channels)
			{
				if ((ch.Prefix == prefix) && ((ch.ContainsRemote(dst, null) != null) || ch.ContainsRemote(number.Substring(2,number.Length-2),null) != null))
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
		public virtual bool HandleChangeInCallState(int sipCallId, CORESIP_CallStateInfo stateInfo)
		{
			if ((_SipCall != null) && (_SipCall.Id == sipCallId))
			{
                _LastCode = stateInfo.LastCode;

				if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_EARLY)
				{
					Debug.Assert(stateInfo.Role == CORESIP_CallRole.CORESIP_CALL_ROLE_UAC);
					Debug.Assert(_State == TlfState.Out);

					switch (stateInfo.LastCode)
					{
						case SipAgent.SIP_RINGING:
						case SipAgent.SIP_QUEUED:
						case SipAgent.SIP_INTRUSION_IN_PROGRESS:
							_CallTout.Enabled = false;		// No más intentos de llamada
							_SipCall.InterruptionWarning = false;


                            // Registrar en histórico local de llamadas
                            HistoricalManager.AddCall("Salientes", Top.Cfg.PositionId, HistoricalManager.AccessType.AD, _Literal);
                            
                            if (_EvSub != IntPtr.Zero)
							{
								SipAgent.TransferNotify(_EvSub, stateInfo.LastCode);
							}

							break;

						case SipAgent.SIP_INTERRUPTION_IN_PROGRESS:
							Debug.Assert(_SipCall.Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY);
							if (!_SipCall.InterruptionWarning)
							{
								_SipCall.InterruptionWarning = true;

								// Comprobamos que no haya recursos libres (porque se hayan liberado entre
								// que se envio la llamada y ahora o porque hicimos la llamada pensando que
								// el recurso estaba libre y cuando la pasarela recibió la petición ya estaba
								// ocupado
                                TryCallWithoutInterruption();
                            }
							break;

						case SipAgent.SIP_INTERRUPTION_END:
							_SipCall.InterruptionWarning = false;
							break;
					}
				}
				else if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_CONFIRMED)
				{
                    if (_SipCall.Redirect)
                        General.SafeLaunchEvent(ForwardedCallMsg, this, null);

                    _CallTout.Enabled = false;
					Top.Mixer.Unlink(sipCallId);

                    switch (stateInfo.MediaStatus)
                    {
                        case CORESIP_MediaStatus.CORESIP_MEDIA_ACTIVE:
                            // Caso de conferencia para intrusion con telefonos IP de pabx donde no hay Focus
                            // resuelve #2946 y #3081
                            if ((_SipCall.Ch.Prefix == Cd40Cfg.PP_DST) &&
                                (OldState == TlfState.PaPBusy) &&
                                (EfectivePriority == CORESIP_Priority.CORESIP_PR_EMERGENCY))
                                State = TlfState.Conf;
                            else
                                State = (stateInfo.LocalFocus != 0) || (stateInfo.RemoteFocus != 0) ? TlfState.Conf : TlfState.Set;
                            if (_SipCall.Monitoring)
                            {
                                // Parte escuchada
                                Debug.Assert(stateInfo.MediaDir == CORESIP_MediaDir.CORESIP_DIR_SENDONLY);
                                Top.Mixer.LinkTlf(sipCallId);
                            }
                            else
                            {
                                Debug.Assert(stateInfo.MediaDir == CORESIP_MediaDir.CORESIP_DIR_SENDRECV);
                                Top.Mixer.LinkTlf(sipCallId, MixerDir.SendRecv, Mixer.TLF_PRIORITY);
                                Top.Recorder.SessionGlp(sipCallId, FuentesGlp.Telefonia, true);
                            }
                            if (_HoldOnEstablish)
                                Hold(false);
 
                            break;

                        case CORESIP_MediaStatus.CORESIP_MEDIA_LOCAL_HOLD:
                            // Tener en cuenta la conferencia sin pasar por la intrusión
                            if ((stateInfo.LocalFocus != 0 || stateInfo.RemoteFocus != 0) && _Conference != null && _Conference.ConferenceState != ConfState.Hold)
                            {
                                Unhold();
                                State = TlfState.Conf;
                            }
                            else
                            {
                                State = TlfState.Hold;
                            }
                            break;

                        case CORESIP_MediaStatus.CORESIP_MEDIA_NONE:
                        case CORESIP_MediaStatus.CORESIP_MEDIA_REMOTE_HOLD:
                            State = TlfState.RemoteHold;
                            if (_SipCall.Monitoring)
                            {
                                // Parte escuchando
                                Debug.Assert(stateInfo.MediaDir == CORESIP_MediaDir.CORESIP_DIR_RECVONLY);
                                Top.Mixer.LinkTlf(sipCallId, MixerDir.Send, Mixer.TLF_PRIORITY);
                            }
                            break;
                        }

                    if ((stateInfo.LocalFocus != 0) && (_Conference != null) && _Conference.ConferenceState != ConfState.Hold)
					{
                        //State = TlfState.Conf;
                        _Conference.Add(this);
					}
				}
				else if (stateInfo.State == CORESIP_CallState.CORESIP_CALL_STATE_DISCONNECTED)
				{
					if (!_SipCall.Redirect)
                        _SipCall.Id = -1;
                    else
                        General.SafeLaunchEvent(ForwardedCallMsg, this, null);

					_RemoteHangUp = stateInfo.LastCode != SipAgent.SIP_REQUEST_TIMEOUT &&
									stateInfo.LastCode != SipAgent.SIP_ERROR &&
                                    stateInfo.LastCode != SipAgent.SIP_CONGESTION &&
                                    stateInfo.LastCode != SipAgent.SIP_NOT_FOUND;
                    
					if (_State == TlfState.Out)
					{
						_SipCall.LastCallResult = stateInfo.LastCode;

                        if (stateInfo.LastCode == SipAgent.SIP_BUSY)
                        {
                            //No se reintenta la llamada por otra linea
                            _CallTout.Enabled = false;
                            State = TlfState.Busy;
                        }
                        else if (_CallTout.Enabled)
                        {
                                TlfState statAux = TryCall();
                                //En caso de que el falle el intento de llamada por falta de recurso, y el último error es NOT_FOUND
                                //pasamos a Fuera de servicio #3446 en lugar de Congestion
                                if ((statAux == TlfState.Congestion) && (stateInfo.LastCode == SipAgent.SIP_NOT_FOUND))
                                    State = TlfState.OutOfService;
                                else
                                    State = statAux;
                         }
                        else
                        {
                            if (_SipCall.Ch.Type != TipoInterface.TI_IP_PROXY &&
                                _SipCall.Ch.Type != TipoInterface.TI_ATS_N5 &&
                                _SipCall.Ch.Type != TipoInterface.TI_ATS_R2)
                            {
                                if (stateInfo.LastCode == SipAgent.SIP_NOT_FOUND)
                                {
                                    State = TlfState.OutOfService;
                                }
                                else
                                {
                                    State = TlfState.Congestion;
                                }
                            }
                            else
                            {
                                State = TryCall();
                            }
                        }   
					}
					else
					{
						if ((_State == TlfState.Set) || (_State == TlfState.Conf) || (_State == TlfState.RemoteHold))
						{
							Top.Mixer.Unlink(sipCallId);
                            Top.Recorder.SessionGlp(FuentesGlp.Telefonia, false);
                        }
                        else if (_State == TlfState.In || _State == TlfState.InPrio)
                        {
                            // Registrar en histórico local de llamadas
                            HistoricalManager.AddCall("NoAtendidas", Top.Cfg.PositionId, HistoricalManager.AccessType.AD, _Literal);
                        }

						if (_Conference != null)
						{
							_Conference.Remove(this);
							_Conference = null;
						}

                        if ((_State == TlfState.Conf) && (stateInfo.LastCode != SipAgent.SIP_OK))
                        {
                            //_SipCall.LastCallResult = stateInfo.LastCode;
                            _CallTout.Enabled = false;
                            switch (stateInfo.LastCode)
                            {
                                case SipAgent.SIP_BUSY:
                                    State = TlfState.Busy;
                                    break;
                                case SipAgent.SIP_NOT_FOUND:
                                    State = TlfState.OutOfService;
                                    break;
                                default:
                                    State = TlfState.Congestion;
                                    break;
                            }
                        }
                        else
                        {
                            State = TlfState.Idle;
                            _SipCall = null;
                        }
					}
				}
                if (State != TlfState.Out)
                    _HoldOnEstablish = false;

				return true;
			}

			return false;
		}

        public virtual bool IsForMe(CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
        {
            string reason = "";
            SipCallInfo inCall = SipCallInfo.NewIncommingCall(_Channels, -1, info, inInfo, this is TlfIaPosition, out reason);
            return (inCall != null);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="call2replace"></param>
        /// <param name="info"></param>
        /// <param name="inInfo"></param>
        /// <returns></returns>
        //lalm 220603
        public virtual int HandleIncomingCall(int sipCallId, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo, out string reason)
		{
            //En teclas de AD, sólo se admiten llamada con los datos que coincidan con los configurados
            //reason = null;
            SipCallInfo inCall = SipCallInfo.NewIncommingCall(_Channels, sipCallId, info, inInfo, this is TlfIaPosition, out reason);

            if (inCall != null && 
                (inCall.Ch.Type==TipoInterface.TI_EyM_MARC || 
                 inCall.Ch.Type==TipoInterface.TI_EyM_PP || 
                 PermisoRed((uint)(((SipChannel)inCall.Ch).Prefix),true)))
			{

                if ((Top.ScreenSaverEnabled) || (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker && !Top.Hw.LCSpeaker) ||
                // Errores #4928
                // 211028 si no jacks ni altavoz de LC rechazo la llamada.
                (!Top.Hw.InstructorJack && !Top.Hw.AlumnJack && !Top.Hw.LCSpeaker && Top.Hw.ListaDispositivos.Count == 0))
                {
                        return SipAgent.SIP_DECLINE;
                }

				if (_SipCall != null)
				{
					if ((call2replace >= 0) && (_SipCall.Id == call2replace))
					{
						MakeHangUp(SipAgent.SIP_GONE);
					}
					else if (!_SipCall.IsActive || ((int)inCall.Priority < (int)_SipCall.Priority) ||
						((inCall.Priority == _SipCall.Priority) && (string.Compare(inInfo.SrcIp, Top.SipIp) < 0)))
					{
						if (_State == TlfState.Out)
						{
							_CallTout.Enabled = false;
                            _Logger.Debug("HandleIncomingCall HangupCall: {0}", _SipCall.RemoteId); 
                            SipAgent.HangupCall(_SipCall.Id);

							_SipCall = inCall;
                            CORESIP_CallFlags flags = Settings.Default.EnableEchoCanceller && (_SipCall.Ch.Prefix == Cd40Cfg.PP_DST || (_SipCall.Ch.Prefix >= Cd40Cfg.RTB_DST && _SipCall.Ch.Prefix < Cd40Cfg.EyM_DEST)) ? CORESIP_CallFlags.CORESIP_CALL_EC : CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;

							return (SipAgent.SIP_OK | ((int)flags << 16));
						}
						else
						{
							MakeHangUp(0);
						}
					}
					else
					{
						return SipAgent.SIP_BUSY;
					}
				}

				_SipCall = inCall;
				State = (inCall.Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY) ? TlfState.InPrio : TlfState.In;

                // Registrar en histórico local de llamadas
                HistoricalManager.AddCall("Entrantes", Top.Cfg.PositionId, HistoricalManager.AccessType.AD, _Literal);

				return SipAgent.SIP_RINGING;
			}

			return SipAgent.SIP_DECLINE;
		}

        public virtual bool HandleCallMoved(int sipCallId, out CORESIP_Priority prio)
        {
            prio = CORESIP_Priority.CORESIP_PR_UNKNOWN;
            if ((_SipCall != null) && (_SipCall.Id == sipCallId))
            {
                prio = _SipCall.Priority;
                _CallTout.Enabled = false;
                //Top.Mixer.Unlink(_SipCall.Id);
                _SipCall = null;
                State = TlfState.Idle;
                return true;
            }
            return false;
        }

        public void RedirectCall(int sipCallId, string givenDstUri)
        {
            CORESIP_Priority priority = this.CallPriority;
            _SipCall = SipCallInfo.NewRedirectCall(_Channels, priority, sipCallId);
            State = TryCall();
            if (_State == TlfState.Out)
            {
                _CallTout.Enabled = true;
            }
            else if ((_State == TlfState.NotAllowed) || (_State == TlfState.Congestion))
            {
                SipAgent.CallProccessRedirect(sipCallId, givenDstUri, CORESIP_REDIRECT_OP.CORESIP_REDIRECT_REJECT);
            }
            General.SafeLaunchEvent(ForwardedCallMsg, this, _Literal);
        }

        //lalm 210930
        //Peticiones #3638, anulada, solo puede existir una lina de AI.
//#if PETICION_3638
        public void RefrescaPos()
        {
            General.SafeLaunchEvent(TlfPosStateChanged, this);
        }
//#endif
        #region Protected Members
        /// <summary>
        /// 
        /// </summary>
        protected static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		protected int _Pos;
		protected string _Literal = "";
		protected CfgEnlaceInterno _Cfg = null;
		protected CORESIP_Priority _Priority = CORESIP_Priority.CORESIP_PR_NONURGENT;
		protected TlfState _State = TlfState.Idle;
		protected TlfState _OldState = TlfState.Idle;
		protected List<SipChannel> _Channels = new List<SipChannel>();
#if DEBUG
        public SipCallInfo _SipCall = null;
#else
		protected SipCallInfo _SipCall = null;
#endif
		protected Timer _CallTout = new Timer(Settings.Default.TlfCallsTout);
        protected TlfConference _Conference = null;
		protected int _Tone = -1;
		protected IntPtr _EvSub = IntPtr.Zero;
		protected bool _RemoteHangUp = false;
        protected bool _IpDestinity = false;
        protected int _LastCode;
        /// <summary>
        /// Vale true si la configuración corresponde a un recurso no configurado, por ejemplo de otro SCV
        /// </summary>
        protected bool _UnknownResource = false;
        /// <summary>
        /// Vale true cuando se quiere aparcar la llamada cuando se establezca. 
        /// Se usa porque la llamada saliente está
        /// en vias de establecimiento (Out) en el momento que se quiere aparcar.
        /// </summary>
        protected bool _HoldOnEstablish = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefijo"></param>
        /// <param name="recibir"></param>
        /// <returns></returns>
		protected bool PermisoRed(uint prefijo, bool recibir)
		{
			foreach (ListaRedes lr in Top.Cfg.ListRedes)
			{
				if (lr.Prefijo == prefijo)
				{
					foreach (PermisosRedesSCV p in Top.Cfg.PermisosRedes)
					{
						if (p.IdRed == lr.IdRed)
							return recibir ? p.Recibir : p.Llamar;
					}
				}
			}

			return true;
		}

        /// <summary>
        /// Return TlfState having into account state of channels.
        /// If Channel is Idle (i.e available), by defaultkeep state with no changes
        /// or return idle if required in param
        /// </summary>
        /// <param name="defaultState"></param>
        /// <returns></returns>
		protected TlfState GetReachableState(bool setIdle = false )
		{
            SipChannel.DestinationState state;
			TlfState st = TlfState.Unavailable;

			foreach (SipChannel ch in _Channels)
			{
                state = ch.DestinationReachableState();
                switch (state)
				{
                    case SipChannel.DestinationState.Busy:
                        //No se envía PaP si está señalizando algo (por ejemplo In de captura)
                        //El estado PaP busy sólo se tiene en cuenta cuando está en reposo o
                        //y significa que está disponible, por eso no mantiene el unavailable
                        if (((_State == TlfState.Idle) || (_State == TlfState.Unavailable)) && !setIdle)
                            return TlfState.PaPBusy;
                        else return _State;
                    case SipChannel.DestinationState.Idle:
                        if ((_State == TlfState.Unavailable) || (_State == TlfState.PaPBusy) || setIdle)
                            return TlfState.Idle;
                        else return _State;
                    case SipChannel.DestinationState.NotReachable:
                        //Caso de 19+1 canal sin lineas encontradas en la configuracion, 
                        //siempre está en reposo
                            if ((this is TlfIaPosition) && (ch.ListLines.Count == 0))
                                return TlfState.Idle;
                            break;
				}
			}

			return st;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="path"></param>
        /// <param name="prio"></param>
        /// <returns></returns>
		protected virtual bool TryCall(SipChannel ch, SipPath path, int prio)
		{
			string dstParams = "";
			string remoteId = path.Remote.Ids[0];
            string dstUri;

            if (!path.Line.centralIP)
            {
                //Estos parametros son internos, sirven para dar información a la pasarela
                //En encaminamiento IP no se deben usar
			    if (!string.IsNullOrEmpty(path.Remote.SubId))
			    {
				    dstParams += string.Format(";isub={0}", path.Remote.SubId);
			    }
			    if ((ch.Prefix != Cd40Cfg.INT_DST) && (ch.Prefix != Cd40Cfg.IP_DST) && (ch.Prefix != Cd40Cfg.UNKNOWN_DST) &&
                        ((ch.Prefix != Cd40Cfg.PP_DST) || (string.Compare(remoteId, path.Line.Id, true /*ignoreCase*/) != 0)))
			    {
				    dstParams += string.Format(";cd40rs={0}", path.Line.Id);
			    }
			    if (ch.Prefix == Cd40Cfg.ATS_DST)
			    {
				    dstParams += string.Format(";cd40prio={0}", prio);
			    }
            }

            //Si el destino es un recurso ajeno a mi SCV (no configurado) utilizo la URI recibida.
            if (_UnknownResource)
                dstUri = ch.Uri;
            else 
                dstUri = string.Format("<sip:{0}@{1}{2}>", remoteId, path.Line.Ip, dstParams);
            //else if (ch.Prefix < Cd40Cfg.RTB_DST)
            //    dstUri = string.Format("<sip:{0}@{1}{2}>", remoteId, path.Line.Ip, dstParams);
            //else /*ch.Prefix >= Cd40Cfg.RTB_DST para que funcione con el plugin del proxy para lineas de redes externas */
            //    dstUri = string.Format("<sip:{3:D2}{0}@{1}{2}>", remoteId, path.Line.Ip, dstParams, ch.Prefix);
            
			try
			{
				int sipCallId;
                CORESIP_CallFlags flags = CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;
				if (_SipCall.Monitoring)
				{
                    if (path.ModoSinProxy == false)
                        flags |= CORESIP_CallFlags.CORESIP_CALL_EXTERNAL_IP;
                    sipCallId = SipAgent.MakeMonitoringCall(ch.AccId, dstUri, flags);
				}
                else if (_SipCall.Dialog != null)
                    {
                        sipCallId = SipAgent.MakeTlfCallReplaces(ch.AccId, dstUri, _SipCall.Priority, flags, _SipCall.Dialog.callId,
                            _SipCall.Dialog.toTag, _SipCall.Dialog.fromTag);
                    }
                else if (_SipCall.Redirect)
                {
                    sipCallId =_SipCall.Id;
                    SipAgent.CallProccessRedirect(_SipCall.Id, dstUri, CORESIP_REDIRECT_OP.CORESIP_REDIRECT_ACCEPT);
                }
                    else
                    {
                        flags = Settings.Default.EnableEchoCanceller &&
                            (ch.Prefix == Cd40Cfg.PP_DST || (ch.Prefix >= Cd40Cfg.RTB_DST && ch.Prefix < Cd40Cfg.EyM_DEST)) ?
                            CORESIP_CallFlags.CORESIP_CALL_EC : CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;
                        if (path.ModoSinProxy == false)
                            flags |= CORESIP_CallFlags.CORESIP_CALL_EXTERNAL_IP;
                        sipCallId = SipAgent.MakeTlfCall(ch.AccId, dstUri, _SipCall.ReferBy, _SipCall.Priority, flags);
                        try
                        {
                            Top.Tlf.SaveParamLastCall(sipCallId, ch.AccId, dstUri, _SipCall.ReferBy, _SipCall.Priority, flags);
                            _Logger.Debug("Guardo llamada "+ sipCallId.ToString()+ dstUri);
                        }
                        catch (Exception ex)
                        {
                            _Logger.Warn("Imposible salvar llamada" + ex);
                        }
                }

                _SipCall.Update(sipCallId, ch.AccId, remoteId, ch, path.Remote, path.Line);

                _Logger.Info("Making call to: {0} {1:X}", dstUri, sipCallId);
				return true;
			}
			catch (Exception ex)
			{
				ch.SetCallResult(path.Remote, path.Line, _SipCall.Priority, -1);
				_Logger.Warn("ERROR llamando a " + dstUri, ex);
			}

			return false;
		}
        public class tsalvado
        {
            public int sipCallId;
            public string _AccId;
            public string _dstUri;
            public string _ReferBy;
            public CORESIP_Priority _Priority1;
            public CORESIP_CallFlags _flags;
            public int cont;
            public tsalvado()
            {
                sipCallId = -1;
                _AccId = ""; ;
                _dstUri="";
                _ReferBy = "";
                _Priority1 = 0;
                _flags = 0;
                cont = 0;
            }
        };
        public List<tsalvado> lsalvado = new List<tsalvado>();
        public void SaveParamLastCall(int sipCallId, string AccId, string dstUri, string ReferBy, CORESIP_Priority Priority, CORESIP_CallFlags flags)
        {
            tsalvado salvado = new tsalvado();
            salvado.sipCallId = sipCallId;
            salvado._AccId = AccId;
            salvado._dstUri = dstUri;
            salvado._ReferBy = ReferBy;
            salvado._Priority1 = Priority;
            salvado._flags = flags;
            int c = lsalvado.Count;
            if (c > 2)
                lsalvado.RemoveAt(0);
            if (c > 1)
                salvado.cont = lsalvado[c - 1].cont + 1;
            else
                salvado.cont = 1;
            lsalvado.Add(salvado);
        }
        public tsalvado GetParamLastCall(int sipCallId)
        {
            tsalvado salvado = new tsalvado();
            foreach (tsalvado s in lsalvado)
            {
                if (s.sipCallId == sipCallId)
                    return s;
            }
            return salvado;
        }

/// <summary>
/// 
/// </summary>
/// LALM 211006
///#2629 Presentar via utilizada en llamada saliente.
/// <param name="ch"></param>
/// <param name="path"></param>
/// <param name="prio"></param>
/// <param name="remoteId"></param>
/// <returns></returns>
private static string getdstParams(SipChannel ch, SipPath path, int prio, string remoteId)
        {
            string dstParams = "";
            if (!path.Line.centralIP)
            {
                //Estos parametros son internos, sirven para dar información a la pasarela
                //En encaminamiento IP no se deben usar
                if (!string.IsNullOrEmpty(path.Remote.SubId))
                {
                    dstParams += string.Format(";isub={0}", path.Remote.SubId);
                }
                if ((ch.Prefix != Cd40Cfg.INT_DST) && (ch.Prefix != Cd40Cfg.IP_DST) && (ch.Prefix != Cd40Cfg.UNKNOWN_DST) &&
                        ((ch.Prefix != Cd40Cfg.PP_DST) || (string.Compare(remoteId, path.Line.Id, true /*ignoreCase*/) != 0)))
                {
                    dstParams += string.Format(";cd40rs={0}", path.Line.Id);
                  
                }
                if (ch.Prefix == Cd40Cfg.ATS_DST)
                {
                    dstParams += string.Format(";cd40prio={0}", prio);
                }
            }

            return dstParams;
        }

        public ArrayList GetUris()
        {
            if (_Channels[0].GetType() == typeof(IntChannel))
                return _Channels[0].GetUris;
           return null;
                        }
        /// <summary>
        /// Realiza un intento de llamada por el siguiente path que le corresponde
        /// </summary>
        /// <returns></returns>
		protected TlfState TryCall()
		{
			bool ats = false;

			TlfState estado = TlfState.Congestion;

            foreach (SipChannel ch in _Channels)
            {
                SipPath path = ch.GetPreferentPath(_SipCall.Priority);

                if (path != null)
                    Top.Tlf.PriorityCall = path.PriorityChannel;

                if (ch.Prefix == Cd40Cfg.PP_DST || ch.Prefix == Cd40Cfg.EyM_DEST)
                    estado = TlfState.Busy;
                else
                {
                    ats |= (ch.Prefix == Cd40Cfg.ATS_DST);
                    if (path != null && !PermisoRed(ch.Prefix, false))
                    {
                        if (!ats)
                            return TlfState.NotAllowed;

                        continue;
                    }
                }

                while (path != null)
                {

                    if (TryCall(ch, path, (int)_SipCall.Priority + 1))
                    {
                        path.Reset(ch.Uri);//220802 paso la uri para que se almacene
                        return TlfState.Out;
                    }

                    path = ch.GetPreferentPath(_SipCall.Priority);
                }
            }

            _LastCode = 0;

			if (_SipCall.Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
			{
				foreach (SipChannel ch in _Channels)
				{
					SipPath path = ch.GetInterrumpiblePath();
					if (ch.Prefix == Cd40Cfg.PP_DST)
						estado = TlfState.Busy;

					while (path != null)
					{
                        _Logger.Warn("Interrupcion por prioridad en curso {0}", path.Line.Id);
                        if (TryCall(ch, path, (int)_SipCall.Priority + 1))
						{
							return TlfState.Out;
						}

						path = ch.GetInterrumpiblePath();
					}
				}
			}

            int numPriority = Top.Cfg.SoyPrivilegiado ? (int)_SipCall.Priority + 1 : (int)_SipCall.Priority + 6;
            foreach (SipChannel ch in _Channels)
			{
				SipPath path = ch.GetDetourPath(_SipCall.Priority);
				if (ch.Prefix == Cd40Cfg.PP_DST)
					estado = TlfState.Busy;

				while (path != null && PermisoRed(ch.Prefix, false))
				{
                    _Logger.Debug("LLamada por alternativa {0} soyPriv:{1} prio {2}", path.Line.Id, Top.Cfg.SoyPrivilegiado, numPriority);
                    if (TryCall(ch, path, numPriority))
					{
                        //LALM 211007
                        //#2629 Presentar via utilizada en llamada saliente.
                        string remoteid = "";
                        string uri = getdstParams(ch,path,(int)( _SipCall.Priority + 1),remoteid);
                        path.Reset(uri);

                        return TlfState.Out;
					}

					path = ch.GetDetourPath(_SipCall.Priority);
				}
			}

			_CallTout.Enabled = false;
			return estado;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reason"></param>
		protected void MakeHangUp(int reason)
		{
			if (_SipCall != null)
			{
				if (_SipCall.IsActive)
				{
					Debug.Assert(_State != TlfState.Idle);

					switch (_State)
					{
						case TlfState.In:
						case TlfState.InPrio:
						case TlfState.RemoteIn:
							if (reason == 0)
							{
								reason = SipAgent.SIP_BUSY;
							}
							break;
						case TlfState.Out:
							_CallTout.Enabled = false;
							break;
						case TlfState.Set:
						case TlfState.Conf:
						case TlfState.RemoteHold:
							Top.Mixer.Unlink(_SipCall.Id);
							// Top.Recorder.Rec(CORESIP_CallType.CORESIP_CALL_DIA, false);
							break;
					}

					if (_Conference != null)
					{
						_Conference.Remove(this);
						_Conference = null;
					}

                    _Logger.Debug("HangupCall: {0}", _SipCall.RemoteId);
                    SipAgent.HangupCall(_SipCall.Id, reason);
				}
				else
				{
					Debug.Assert((_State == TlfState.Congestion) || (_State == TlfState.NotAllowed) || (_State == TlfState.Busy) || (_State == TlfState.OutOfService));
				}                

				_SipCall = null;
                _HoldOnEstablish = false;
                //Top.Tlf.PriorityCall = false;
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		protected void OnCallTimeout(object sender, ElapsedEventArgs e)
		{
			Top.WorkingThread.Enqueue("OnTlfCallTimeout", delegate()
			{
				if (_State == TlfState.Out)
				{
					Debug.Assert((_SipCall != null) && _SipCall.IsActive);

                    _Logger.Debug("OnCallTimeout HangupCall: {0}", _SipCall.RemoteId); 
                    SipAgent.HangupCall(_SipCall.Id);

					_SipCall.Id = -1;
					State = TlfState.Congestion;
				}
			});
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
		protected void OnRsChanged(object sender)
		{
			Resource rs = (Resource)sender;
            //Para evitar tratar eventos de pasarelas de otros SCVs en entorno de laboratorio
            //if (rs.Content is GwTlfRs)
            //    if (!Top.Cfg.BelongsToMyConfig(Tipo_Elemento_HW.TEH_TIFX, ((GwTlfRs)rs.Content).GwIp) &&
            //        !Top.Cfg.BelongsToMyConfig(Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA, ((GwTlfRs)rs.Content).GwIp))
            //        return;

			if (!rs.IsValid)
			{
				TlfState st = GetReachableState();

				if (st == TlfState.Unavailable)
				{
					MakeHangUp(0);
				}
				else
				{
					if (_SipCall != null)
					{
						if ((_State == TlfState.Out) || (_State == TlfState.Busy) || (_State == TlfState.Congestion) || (_State == TlfState.OutOfService))
						{
							if (_SipCall.Line.RsLine == rs)
							{
								_SipCall.Ch.ResetCallResults(rs);
							}
							else
							{
								foreach (SipChannel ch in _Channels)
								{
									if (ch.ResetCallResults(rs))
									{
										break;
									}
								}
							}
						}

						if (_SipCall.IsActive && (_SipCall.Line.RsLine == rs))
						{
							if (_State == TlfState.Out)
							{
                                _Logger.Debug("OnRsChanged HangupCall: {0}", _SipCall.RemoteId); 
								SipAgent.HangupCall(_SipCall.Id);
								_SipCall.Id = -1;

								st = (_CallTout.Enabled ? TryCall() : TlfState.Congestion);
							}
							else
							{
								MakeHangUp(0);
                                st = TlfState.Unavailable;
							}
						}
						else
						{
							st = _State;
						}
					}
				}

				State = st;
			}
            else //rs.IsValid
            {
				GwTlfRs.State rsState = rs.Content is GwTlfRs ? ((GwTlfRs)rs.Content).St : GwTlfRs.State.Idle;
                //Actualiza los cambios de proxy en los destinos ATS externos
                //RQF-49 Esta funcion, cambia todas las lineas de todos los recursos que se llaman igual al id pasado.
                // si no se hace la llamada a todos los proxies.
                if (rs.Content is GwTlfRs)
                    ResetIpLinesOfChannels(rs.Id, ((GwTlfRs)rs.Content).GwIp);

				if (_SipCall != null) 
				{
					if (((_State == TlfState.Out) && (_SipCall.Line.RsLine != rs)) || 
						(_State == TlfState.Busy) || (_State == TlfState.Congestion) || (_State == TlfState.Congestion) )
					{
						foreach (SipChannel ch in _Channels)
						{
							if (ch.ResetCallResults(rs))
							{
								break;
							}
						}
					}

                    if (_SipCall.InterruptionWarning && (_State == TlfState.Out) && (rsState == GwTlfRs.State.Idle))
                    {
                        Debug.Assert(_SipCall.IsActive);
                        Debug.Assert(_SipCall.Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY);
                        TryCallWithoutInterruption();
                    }
                    // 16/12/2016
                    // Este else if se comenta porque se ha observado que teniendo una conversación prioritaria ATS 
                    // por un recurso R1 del troncal T1, si el recurso que cambia pertenece a T1, la llamada no debe colgarse.
                    //else if (rsState == GwTlfRs.State.Idle && _SipCall.Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
                    //{
                    //    if (_SipCall.Id != -1)
                    //        SipAgent.HangupCall(_SipCall.Id);

                    //    State = TlfState.Idle;
                    //}
                }
				else
				{
                    //Pone a idle la tecla sin tener en cuenta todos sus canales y recursos
                    //No funciona para proxy propio en una tecla de un TopRs
                    //bool setIdle = true;
                    //if ((rsState == GwTlfRs.State.BusyInterruptionAllow) ||
                    //   (rsState == GwTlfRs.State.BusyInterruptionNotAllow))
                    //    setIdle = false;
                    //State = GetReachableState(setIdle);
                    State = GetReachableState();
                }
            }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param namFe="id"></param>
        /// <param name="gwIp"></param>
		protected void ResetIpLinesOfChannels(string id, string gwIp)
		{
			foreach (SipChannel ch in _Channels)
			{
				ch.ResetLine(id, gwIp);
			}
		}

        /// <summary>
        /// Se utiliza cuando se está realizando una interrupción por prioridad y aún no se ha completado
        /// para abortar la interrupción y realizar la llamada por el camino libre si lo hay
        /// Si no hay camino disponible, la interrupción continuará.
        /// </summary>
        /// <returns>true si se puede seguir intentando</returns>
        private void TryCallWithoutInterruption()
        {
            foreach (SipChannel ch in _Channels)
            {
                SipPath path = ch.GetPreferentPath(_SipCall.Priority);

                if (path != null)
                {
                    SipAgent.HangupCall(_SipCall.Id);
                    _Logger.Warn("Interrupcion abortada en {0} por liberación de camino {1}", _SipCall.Line.Id, path.Line.Id);
                    _SipCall.Id = -1; // Aqui se pone _SipCall.InterruptionWarning = false
                    State = TryCall();
                    break;
                }
            }
        }

        /// <summary>
        /// Se utiliza para borrar de la pantalla la tecla. 
        /// Es el caso de entrantes por 19+1 no encontradas en la configuración 
        /// que se deben aceptar pero no tienen linea por donde realizar una saliente, 
        /// asi que no se conservan #3522
        /// <returns>true si se puede seguir intentando</returns>
        private void DeleteInScreen()
        {
            int linesCount = 0;
            foreach (SipChannel ch in _Channels)
                linesCount += ch.ListLines.Count;
            if ((this is TlfIaPosition) && (_State == TlfState.Idle) && linesCount == 0)
            {
                _Literal = "";
            }
        }

        #endregion
    }
}
