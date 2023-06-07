using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;
namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
	public class TlfConferencePrepro : IDisposable
#else
	class TlfConferencePrepro : IDisposable
#endif	
	{
        /// <summary>
        /// 
        /// </summary>
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

		public event GenericEventHandler Finished;
		public event GenericEventHandler<Member> MemberAdded;
		public event GenericEventHandler<Member> MemberRemoved;

		public CORESIP_Priority Priority
		{
			get { return _Priority; }
		}

		public List<Member> Members
		{
			get { return _Members; }
		}

		public List<Member> DesiredMembers
		{
			get { return _DesiredMembers; }
		}

        public ConfState ConferenceState
        {
            get { return _ConferenceState; }
        }

        public TlfConferencePrepro()
        {
            Top.Sip.IncomingSubscribeConf += OnIncomingSubscribeConf;
        }

		public bool TryAddExisting(Member member)
		{
			Debug.Assert(!_Members.Contains(member));
			Debug.Assert(!_DesiredMembers.Contains(member));

            if (_DesiredMembers.Count < 16)
            {
			
                //tlf.AddToConference(tlf.Pos,this);

                //_Priority = (CORESIP_Priority)Math.Min((int)_Priority, (int)tlf.CallPriority);
                _DesiredMembers.Add(member);

                return true;
            }

            return false;
		}

		//public void TryAddIncoming(Member member)
		//{
		//	Debug.Assert(!_Members.Contains(member));
		//	//Top.Tlf.Accept(tlf.Pos, this);

		//	if (!_DesiredMembers.Contains(member))
		//	{
		//		_Priority = (CORESIP_Priority)Math.Min((int)_Priority, (int)tlf.CallPriority);
		//		_Intruder = tlf;
		//		_DesiredMembers.Add(tlf);
		//	}
		//}

		public void Add(string sala,string user)
        {
			_ConferenceState = ConfState.Idle;
			if (!_DesiredConferencias.Contains(sala))
				_DesiredConferencias.Add(sala);
			var a = _DesiredMembers.FindAll(x => x.conferencia == sala && x.uri == user);
			if (_DesiredMembers.FindAll(x => x.conferencia == sala && x.uri == user).Count == 0)
				_DesiredMembers.Add(new Member(user, sala));
		
			Debug.Assert(_DesiredConferencias.Contains(sala));
			_Logger.Trace("Add to conf ");
			Member participante = new Member(user, sala);
			if (_Members.FindAll(x => x.conferencia == sala && x.uri == user) == null)
			{
				_Members.Add(participante);

				General.SafeLaunchEvent(MemberAdded, this, participante);
			}

		}

		public void Add(Member participante)
		{
			Debug.Assert(_DesiredMembers.Contains(participante));
            _Logger.Trace("Add to conf ");
			if (!_Members.Contains(participante))
			{
				_Members.Add(participante);
                //NotifyConfInfo();

				General.SafeLaunchEvent(MemberAdded, this, participante);
			}

		}

		public void Remove(Member participante)
		{
			if (_DesiredMembers.Remove(participante))
			{
				if (_DesiredMembers.Count == 1)
				{
					Dispose();
				}
				else
				{
					if (_Members.Remove(participante))
					{
                        _Logger.Trace("Remove from conf ");
						NotifyConfInfo();
					}

					General.SafeLaunchEvent(MemberRemoved, this, participante);
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			General.SafeLaunchEvent(Finished, this);
		}

        #endregion
        public struct Member
        {
            public string uri;
            public string conferencia;
			public TlfState State;
			public Member(string Uri,string Conf)
            {
				uri = Uri;
				conferencia = Conf;
				State = TlfState.Idle;
            }
        };
        #region Private Members

        private CORESIP_Priority _Priority = CORESIP_Priority.CORESIP_PR_UNKNOWN;
		private List<Member> _Members = new List<Member>();
		private List<Member> _DesiredMembers = new List<Member>();
		private List<string> _DesiredConferencias = new List<string>();
		private uint _Version = 0;
		private TlfPosition _Intruder = null;
        private ConfState _ConferenceState = ConfState.Idle;

        /// <summary>
        /// Construye y envía el notify con los miembros de la conferencia. 
        /// El primer miembro soy yo mismo, con todos los numeros a los que atiendo.
        /// Se llama cuando alguien se suscribe (1) y cuando se añaden miembros a la conferencia (2).
        /// Tiene un parametro opcional callId. Si viene, se envia el notify al miembro que 
        /// contiene ese callId (1). El comportamiento por defecto es que si no viene el parámetro 
        /// se envia a todos los miembros de la conferencia (2).
        /// </summary>
        /// <param name="callId" parámetro opcional, callId ></param>
        /// <returns> </returns>
        private void NotifyConfInfo(int callId = -1)
		{
            _Logger.Trace("NotifyConfInfo ");
			if (_Members.Count > 0)
			{
				CORESIP_ConfInfo info = new CORESIP_ConfInfo();

				info.Version = ++_Version;
				info.Users = new CORESIP_ConfInfo.ConfUser[SipAgent.CORESIP_MAX_CONF_USERS];

                int j = 0;
                foreach (StrNumeroAbonado num in Top.Cfg.HostAddresses)
                {
                    info.Users[j].Id = string.Format("<sip:{0}@{1}>", num.NumeroAbonado, Top.SipIp); ;
                    info.Users[j++].Name = Top.Cfg.PositionId;
                }
                int i;
                info.UsersCount = (uint)(_Members.Count + j); ;


			}
		}
        /// <summary>
        /// Se llama cuando CORESIP ha recibido una suscripcion al evento 'conference'
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="call"></param>
        /// <param name="info"></param>
        /// <param name="lenInfo"></param>
        public void OnIncomingSubscribeConf(object sender, int callId, string info, uint lenInfo)
        {            
            _Logger.Trace("OnIncomingSubscribeConf ");
            NotifyConfInfo(callId);
        }

		#endregion
	}
}
