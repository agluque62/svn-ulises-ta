using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using U5ki.Infrastructure;

namespace HMI.CD40.Module.BusinessEntities
{
	class SipCallInfo
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

        public bool LastErrorInIP()
        {
            if (_Ch == null)
                return false;
            else 
                return _Ch.LastErrorInIP(_Remote, _Priority);
        }

        public bool IsValid(IEnumerable<SipChannel> channels)
		{
            //Si el Id es -1, la clase no tiene todos sus miembros rellenos (crash en _Ch)
            //es el caso de llamadas que no han prosperado y están en espera de cuelgue
            if (Id > 0)
            {   
                foreach (SipChannel ch in channels)
                {
                    if (ch.Prefix == _Ch.Prefix)
                    {
                        _Remote = ch.ContainsRemote(_RemoteId, _Remote.SubId);
                        if (_Remote != null)
                        {
                            _Line = ch.ContainsLine(_Line.Id, _Line.Ip);
                            if (_Line != null)
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
			foreach (SipChannel ch in channels)
			{
				ch.ResetCallResults(true);
			}

			SipCallInfo info = new SipCallInfo();
			info._Priority = CORESIP_Priority.CORESIP_PR_URGENT;

			return info;
		}

		public static SipCallInfo NewMonitoringCall(IEnumerable<SipChannel> channels)
		{
			foreach (SipChannel ch in channels)
			{
				ch.ResetCallResults(true);
			}

			SipCallInfo info = new SipCallInfo();
			info._Monitoring = true;

			return info;
		}

		public static SipCallInfo NewTlfCall(IEnumerable<SipChannel> channels, CORESIP_Priority priority, string referBy)
		{
			foreach (SipChannel ch in channels)
			{
				ch.ResetCallResults(true);
			}

			SipCallInfo info = new SipCallInfo();
			info._Priority = priority;
			info._ReferBy = referBy;

			return info;
		}

		public static SipCallInfo NewIncommingCall(IEnumerable<SipChannel> channels, int callId, CORESIP_CallInfo info, CORESIP_CallInInfo inInfo, bool findNoConfigured)
		{
            SipPath path = null;
			foreach (SipChannel ch in channels)
			{
				ch.First = false;
			}
            IPEndPoint sipEP = new IPEndPoint(IPAddress.Parse(inInfo.SrcIp), (int)inInfo.SrcPort);
			foreach (SipChannel ch in channels)
			{
				path = ch.FindPath(inInfo.SrcId, sipEP.ToString(), inInfo.SrcSubId, inInfo.SrcRs);
                if (path != null)
                {
                    ch.First = true;
                    return new SipCallInfo(callId, inInfo.DstId, inInfo.SrcId, info.Priority, info.Type, ch, path.Remote, path.Line);
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
                        ch.First = true;
                        return new SipCallInfo(callId, inInfo.DstId, inInfo.SrcId, info.Priority, info.Type, ch, path.Remote, path.Line);
                    }
                }
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
