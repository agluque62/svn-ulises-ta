using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMI.Model.Module.BusinessEntities;
using U5ki.Infrastructure;
namespace HMI.CD40.Module.BusinessEntities
{
    /// <summary>
    /// Teclas para los miembros de un un grupo. 
    /// Tienen comportamiento diferente respecto a sus estados. Están desactivadas (Inactive), hasta que el foco
    /// envía notificación de activación del grupo.
    /// Utilizan TlfFocusChannel
    /// </summary>
    class TlfPositionMd: TlfPosition
    {
        private bool _ActiveState;
        public void EndMDCall (string fromUri)
        {
            if (_Member)
            {
                _ActiveState = false;
                State = GetReachableState();
            }
            _Subscribed = false;
            SipAgent.DestroyConferenceSubscription(fromUri);
        }
        private bool _Member;
        private bool _Subscribed = false;
        public TlfPositionMd(int pos): base( pos)
        {
            _ActiveState = true;
            State = TlfState.Idle;
        }

        override public TlfState State
        {
            get
            {
                if (!_ActiveState)
                    return TlfState.Inactive;
                else return base.State;
            }
            set
            {
                if (_ActiveState) base.State = value;
                else base.State = TlfState.Inactive;
            }
        }

        public override void Reset(CfgEnlaceInterno cfg)
        {
            _Cfg = cfg;
            _Literal = cfg.Literal;
            _Priority = (CORESIP_Priority)cfg.Prioridad;
            _Channels.Clear();
            TlfFocusChannel ch = null;
            ch = new TlfFocusChannel(cfg.OrigenR2, cfg.Literal);
            _ActiveState = true;
            _Member = false;            
            foreach (CfgRecursoEnlaceInterno dst in cfg.ListaRecursos)
            {
                if (Top.Cfg.GetMainUser(Top.HostId) == dst.NombreRecurso)
                {
                    //Soy miembro del grupo
                    _Member = true;
                    _ActiveState = false;
                    ch.ImMember = true;
                }
                else
                    ch.AddFinalDestination(dst.NombreRecurso, dst.NumeroAbonado, dst.Prefijo);
            }
            ch.RsChanged += OnRsChanged;
           _Channels.Add(ch);
            if ((_SipCall != null) && !_SipCall.IsValid(_Channels))
            {
                MakeHangUp(0);	// Cortamos llamadas y quitamos busy y congestion
            }

            if (_SipCall == null)
            {
                State = GetReachableState();
            }
         }
        protected override bool TryCall(SipChannel ch, SipPath path, int prio)
        {
            //Esta comprobación no se hace porque a veces la CORESIP pierde el NOTIFY del "deleted"
            //TODO Pendiente de hacer cuando se resuelva la CORESIP
            if (!_Subscribed)
            {
                SipAgent.CreateConferenceSubscription(ch.AccId, ch.Uri);
                _Subscribed = true;
            }
            return base.TryCall(ch, path, prio);
        }

        public override int HandleIncomingCall(int sipCallId, int call2replace, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
        {
            int ret = SipAgent.SIP_DECLINE;
            if (inInfo.SrcId == _Literal)
            {
                //it's for me
                _ActiveState = true;
                ret = base.HandleIncomingCall(sipCallId, call2replace, info, inInfo);
                //Esta comprobación no se hace porque a veces la CORESIP pierde el NOTIFY del "deleted"
                //TODO Pendiente de hacer cuando se resuelva la CORESIP
                if (!_Subscribed)
                {
                    SipAgent.CreateConferenceSubscription(_SipCall.Ch.AccId, _SipCall.Ch.Uri);
                    _Subscribed = true;
                }
            }
            return ret;
        }
        /// <summary>
        /// Handler received from CORESIP when there is a call state change.
        /// There is a special case CORESIP sends an error code in stateInfo.LastReason instead of stateInfo.LastCode
        /// It is when focus wants to redirect an error code from final end (e.g. format SIP; cause=486)
        /// </summary>
        /// <param name="sipCallId"></param>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        public override bool HandleChangeInCallState(int sipCallId, CORESIP_CallStateInfo stateInfo)
        {
            if (!String.IsNullOrEmpty(stateInfo.LastReason))
            {
                string [] delimiters = {"cause=", ";"};
                string[] code = stateInfo.LastReason.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (String.Compare("sip", code[0], true) == 0)
                    int.TryParse(code[code.Length-1], out stateInfo.LastCode);
            }
            return base.HandleChangeInCallState(sipCallId, stateInfo);
        }

        public override bool IsForMe(CORESIP_CallInfo info, CORESIP_CallInInfo inInfo)
        {
            return (inInfo.SrcId == _Literal);
        }

    }
}
