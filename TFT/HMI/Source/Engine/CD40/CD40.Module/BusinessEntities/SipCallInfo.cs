using System;
using System.Collections.Generic;
using System.Text;

using U5ki.Infrastructure;
using Utilities;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class SipCallInfo
#else
	class SipCallInfo
#endif		
	{
		public int Id
		{
			get { return _Id; }
			set 
			{ 
				_Id = value;
				_InterruptionWarning = false;
			}
		}

		public CORESIP_Priority Priority
		{
			get { return _Priority; }
		}

		public string ReferBy
		{
			get { return _ReferBy; }
		}

		public SipChannel Ch
		{
			get { return _Ch; }
		}

		public SipLine Line
		{
			get { return _Line; }
		}

		public int LastCallResult
		{
			set { _Ch.SetCallResult(_Remote, _Line, _Priority, value); }
		}

		public bool IsActive
		{
			get { return (Id >= 0); }
		}

		public bool InterruptionWarning
		{
			get { return _InterruptionWarning; }
			set { _InterruptionWarning = value; }
		}

		public bool Monitoring
		{
			get { return _Monitoring; }
		}

        public TlfPickUp.DialogData Dialog
        {
            get { return _Dialog; }
        }

        public bool Redirect
        {
            get { return _Redirect; }
        }
        public bool LastErrorInIP()
        {
            if (_Ch == null)
                return false;
            else 
                return _Ch.LastErrorInIP(_Remote, _Priority);
        }

        public string RemoteId
        {
            get { return _RemoteId; }
        }
        public bool IsValid(IEnumerable<SipChannel> channels)
		{
            //Si el Id es -1, la clase no tiene todos sus miembros rellenos (crash en _Ch)
            //es el caso de llamadas que no han prosperado y están en espera de cuelgue
            if (_Ch != null) 
            {   
                foreach (SipChannel ch in channels)
                {
                    if (ch.Prefix == _Ch.Prefix)
                    {
                        _Remote = ch.ContainsRemote(_RemoteId, _Remote.SubId);
                        if ((_Remote != null) && (_Line !=null))
                        {
                            if (!_LocalId.Equals(ch.AccId))
                                break;
                            SipLine  line = ch.ContainsLine(_Line.Id, _Line.Ip);
                            if (line != null)
                            {
                                return _Line.IsAvailable;
                            }
                        }
                    }
                }
            }

			return false;
		}

		public void Update(int callId, string localId, string remoteId, SipChannel ch, SipRemote remote, SipLine line)
		{
			_Id = callId;
			_LocalId = localId;
			_RemoteId = remoteId;
			_Ch = ch;
			_Remote = remote;
			_Line = line;
		}

		public void UpdateOutgoingCall(IEnumerable<SipChannel> channels, CORESIP_Priority priority)
		{
			_Priority = priority;

			if (_Priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
			{
				foreach (SipChannel ch in channels)
				{
					ch.ResetCallResults(false);
				}
			}
		}

		public static SipCallInfo NewLcCall(IEnumerable<SipChannel> channels)
		{
            SipCallInfo info = NewCall(channels);
			info._Priority = CORESIP_Priority.CORESIP_PR_URGENT;

			return info;
		}

        public static SipCallInfo NewReplacesCall(IEnumerable<SipChannel> channels, TlfPickUp.DialogData dialog)
        {
            SipCallInfo info = NewCall(channels);
            info._Dialog = dialog;

            return info;
        }

        public static SipCallInfo NewRedirectCall(IEnumerable<SipChannel> channels, CORESIP_Priority priority, int callId)
        {
            SipCallInfo info = NewCall(channels);
            info._Redirect = true;
            info._Id = callId;
            info._Priority = priority;

            return info;
        }

        private static SipCallInfo NewCall(IEnumerable<SipChannel> channels)
        {
            foreach (SipChannel ch in channels)
            {
                ch.ResetCallResults(true);
            }

            SipCallInfo info = new SipCallInfo();
            return info;
        }

        public static SipCallInfo NewMonitoringCall(IEnumerable<SipChannel> channels)
		{
            SipCallInfo info = NewCall(channels);
			info._Monitoring = true;

			return info;
		}

		public static SipCallInfo NewTlfCall(IEnumerable<SipChannel> channels, CORESIP_Priority priority, string referBy)
		{
            SipCallInfo info = NewCall(channels);
			info._Priority = priority;
			info._ReferBy = referBy;

			return info;
		}

		// LALM anado reason
		public static SipCallInfo NewIncommingCall(IEnumerable<SipChannel> channels, int callId, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo, bool findNoConfigured,out string reason)
		{
            SipPath path = null;
            SipChannel channel = null;
			reason = "";
			foreach (SipChannel ch in channels)
			{
				ch.First = false;
			}

			foreach (SipChannel ch in channels)
			{
				path = ch.FindPath(inInfo.SrcId, inInfo.SrcIp, inInfo.SrcSubId, inInfo.SrcRs);
                if (path != null)
                {
                    channel = ch;
                    break;
                }
                else
                {
					if (ch.ReasonDecline!="")
                    {
						reason= ch.ReasonDecline;
                    }
				}
            }
            //Si no se encuentra path en todos los canales,
            //se hace una busqueda sin comparar con el recurso.
            if ((path == null) && (findNoConfigured))
            {
                foreach (SipChannel ch in channels)
                {
                    path = ch.FindPathNoConfigured(inInfo.SrcId, inInfo.SrcSubId);
                    if (path != null)
                    {
                        channel = ch;
                        break;
                    }
                }
            }
            if (path != null)
            {
                channel.First = true;
                return new SipCallInfo(callId, inInfo.DstId, inInfo.SrcId, info.Priority, info.Type, channel, path.Remote, path.Line);
            }

            return null;
        }

		#region Private Members

		private int _Id = -1;
		private string _LocalId;
		private string _RemoteId;
		private string _ReferBy;
		private CORESIP_Priority _Priority = CORESIP_Priority.CORESIP_PR_NORMAL;
		private SipChannel _Ch;
		private SipRemote _Remote;
		private SipLine _Line;
        //Vale true durante el tiempo en que hay tonos de aviso de interrupción hasta que se establece la llamada
        //en una interrupción por prioridad
		private bool _InterruptionWarning;
		private bool _Monitoring;
        private bool _Redirect;
        private TlfPickUp.DialogData _Dialog = null;

		private SipCallInfo()
		{
		}

		private SipCallInfo(int callId, string localId, string remoteId, CORESIP_Priority priority, CORESIP_CallType type, SipChannel ch, SipRemote remote, SipLine line)
		{
			_Id = callId;
			_LocalId = localId;
			_RemoteId = remoteId;
			_Priority = priority;
			_Ch = ch;
			_Remote = remote;
			_Line = line;
			
			switch (type)
			{
				case CORESIP_CallType.CORESIP_CALL_MONITORING:
                case CORESIP_CallType.CORESIP_CALL_GG_MONITORING:           
				case CORESIP_CallType.CORESIP_CALL_AG_MONITORING:
					_Monitoring = true;
					break;
			}
		}

		#endregion
	}
}
