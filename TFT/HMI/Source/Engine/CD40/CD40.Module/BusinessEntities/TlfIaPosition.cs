using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;

namespace HMI.CD40.Module.BusinessEntities
{
	class TlfIaPosition : TlfPosition
	{
		public string Number
		{
            get { return _NumberWithPrefix; }
		}

		public TlfIaPosition(int pos)
			: base(pos)
		{
		}

		public TlfIaPosition(TlfPosition tlf)
			: base(1000)
		{
			Debug.Assert(tlf.Cfg != null);
			base.Reset(tlf.Cfg);
		}

		public TlfIaPosition(uint prefix, string dst)
			: base(1000)
		{
			CfgEnlaceInterno link = new CfgEnlaceInterno();

			CfgRecursoEnlaceInterno rs;
			string literal;
			TlfManager.EncapsuleIaInfo(prefix, dst, out literal, out rs);

			link.Literal = literal;
			link.ListaRecursos.Add(rs);
			link.Prioridad = Top.Cfg.Priority;
			link.OrigenR2 = Top.Cfg.MainId;

			base.Reset(link);
		}

		public void Update()
		{
			if (_Cfg != null)
			{
				base.Reset(_Cfg);
			}
		}

		public override void Reset(CfgEnlaceInterno cfg)
		{
			foreach (SipChannel ch in _Channels)
			{
				ch.RsChanged -= OnRsChanged;
			}

			base.Reset(cfg);
		}

        public override bool CanHandleOutputCall(uint prefix, string dst, string number, string lit = null, bool unknownResource = false)
		{
            if ((_State == TlfState.In) || (_State == TlfState.InPrio) ||
				(_State == TlfState.RemoteIn) || (_State == TlfState.Hold))
			{
                return base.CanHandleOutputCall(prefix, dst, number, lit, unknownResource);
			}

			if (_SipCall == null)
			{
				CfgEnlaceInterno link = new CfgEnlaceInterno();

				CfgRecursoEnlaceInterno rs;
				string literal;
				TlfManager.EncapsuleIaInfo(prefix, dst, out literal, out rs);
                _UnknownResource = unknownResource;
                if (_UnknownResource)
                    rs.NumeroAbonado = number;
                link.Literal = lit; // literal;
				link.ListaRecursos.Add(rs);
				link.Prioridad = Top.Cfg.Priority;
                link.OrigenR2 = Top.Cfg.GetNumeroAbonado(Top.Cfg.MainId, Cd40Cfg.ATS_DST) ?? Top.Cfg.MainId;
				Reset(link);
                //Nuevo inicio de llamada saliente
                //Limpia el código recibido en la llamada anterior
                _LastCode = 0;
                _NumberWithPrefix = number;
				return true;
			}

			return false;
		}
        /// <summary>
        /// Fill de data structure for an AID Position
        /// Search for resource in configuration if exist or create from scratch
        /// </summary>
        /// <param name="info"></param>
        /// <param name="inInfo"></param>
        /// <returns></returns>
        public bool FillData(CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
        {
                CfgEnlaceInterno link = new CfgEnlaceInterno();
                string literal = null;

                CfgRecursoEnlaceInterno rs = Top.Cfg.GetResourceFromUri(inInfo.SrcId, inInfo.SrcIp, inInfo.SrcSubId, inInfo.SrcRs);
                if (rs != null)
                {
                    _UnknownResource = false;
                    //El literal, es el display name que procede del SIP o el numero en su defecto
                    TlfManager.ExtractIaInfo(rs, out literal, out _NumberWithPrefix);
                    _Logger.Debug("Incoming: {0} {1} {2} lit {3} numbPre {4}", rs.Prefijo, rs.NumeroAbonado, rs.NombreRecurso, literal, _NumberWithPrefix);
                }
                else  // para que funcionen llamadas entrantes no configuradas de transito de un SCV
                {
                    rs = Top.Cfg.GetATSResourceFromUri(inInfo.SrcId);
                    if (rs != null)
                    {
                        _UnknownResource = true;
                        //El literal, es el display name que procede del SIP o el numero en su defecto
                        TlfManager.ExtractIaInfo(rs, out literal, out _NumberWithPrefix);
                    }
                }
                if (rs == null)
                //No encuentro configurado el recurso
                {
                    rs = new CfgRecursoEnlaceInterno();
                    rs.Prefijo = Cd40Cfg.UNKNOWN_DST;
                    rs.NombreRecurso = inInfo.SrcId;
                    rs.NumeroAbonado = string.Format("sip:{0}@{1}", inInfo.SrcId, inInfo.SrcIp);
                    _UnknownResource = true;
                    _NumberWithPrefix = string.Format("{0:D2}{1}", rs.Prefijo, inInfo.SrcId);
                    literal = inInfo.SrcId;
                }
                if (!PermisoRed((uint)rs.Prefijo, true))
                    return false;


                if (!String.IsNullOrEmpty(inInfo.DisplayName))
                    literal = inInfo.DisplayName;

                link.Literal = literal;
                link.ListaRecursos.Add(rs);
                link.Prioridad = Top.Cfg.Priority;
                link.OrigenR2 = Top.Cfg.GetNumeroAbonado(Top.Cfg.MainId, Cd40Cfg.ATS_DST) ?? Top.Cfg.MainId;

                Reset(link);

                return true;
         }

		public override int HandleIncomingCall(int sipCallId, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
		{
            bool replacedCall = false;
            if ((Top.ScreenSaverEnabled) || (Top.Mixer.RxTlfAudioVia == TlfRxAudioVia.Speaker && !Top.Hw.LCSpeaker))
            {
                return SipAgent.SIP_DECLINE;
            }

			if ((call2replace >= 0) && (CallId == call2replace))
			{
				MakeHangUp(SipAgent.SIP_GONE);
                replacedCall = true;
			}

			if ((info.Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY) &&
				((Top.Cfg.Permissions & Permissions.Intruded) == Permissions.Intruded) && (_SipCall != null))
			{
				if (_State == TlfState.Out)
				{
                    //En teclas de 19+1,  se admiten llamada con los datos que no coincidan con configuracion
					SipCallInfo inCall = SipCallInfo.NewIncommingCall(_Channels, sipCallId, info, inInfo, true);

					if (inCall != null)
					{
						if (((int)inCall.Priority < (int)_SipCall.Priority) ||
							((inCall.Priority == _SipCall.Priority) && (string.Compare(inInfo.SrcIp, Top.SipIp) < 0)))
						{
							_CallTout.Enabled = false;
							SipAgent.HangupCall(_SipCall.Id);

							_SipCall = inCall;
                            CORESIP_CallFlags flags = Settings.Default.EnableEchoCanceller &&
                                (_SipCall.Ch.Prefix == Cd40Cfg.PP_DST || (_SipCall.Ch.Prefix >= Cd40Cfg.RTB_DST && _SipCall.Ch.Prefix < Cd40Cfg.EyM_DEST)) ?
                                CORESIP_CallFlags.CORESIP_CALL_EC : CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS;

							return (SipAgent.SIP_OK | ((int)flags << 16));
						}
					}
				}

                if (EfectivePriority != CORESIP_Priority.CORESIP_PR_EMERGENCY && info.Priority != CORESIP_Priority.CORESIP_PR_EMERGENCY)
				{
					MakeHangUp(0);
				}
			}

			if ((_SipCall == null) && !InOperation())	// && PermisoRed((uint)(((SipChannel)inCall.Ch).Type),true))
			{
                if (FillData(info, inInfo) == false)
                    return SipAgent.SIP_DECLINE;
			}
            int result = base.HandleIncomingCall(sipCallId, call2replace, info, inInfo);
            //Si no se acepta llamada entrante y se trataba de un replace que ya se ha colgado, se señaliza con error
            if (replacedCall && 
                ((result == SipAgent.SIP_DECLINE) || (result == SipAgent.SIP_BUSY)))
                State = TlfState.Busy;
            return result;
		}

		#region Private Members
        //Numero de abonado con prefijo
        private string _NumberWithPrefix = "";

		#endregion
	}
}
