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
	class TlfConference : IDisposable
	{
        /// <summary>
        /// 
        /// </summary>
        private static Logger _Logger = LogManager.GetCurrentClassLogger();

		public event GenericEventHandler Finished;
		public event GenericEventHandler<TlfPosition> MemberAdded;
		public event GenericEventHandler<TlfPosition> MemberRemoved;

		public CORESIP_Priority Priority
		{
			get { return _Priority; }
		}

		public List<TlfPosition> Members
		{
			get { return _Members; }
		}

		public List<TlfPosition> DesiredMembers
		{
			get { return _DesiredMembers; }
		}

        public ConfState ConferenceState
        {
            get { return _ConferenceState; }
        }

        public TlfConference()
        {
            Top.Sip.IncomingSubscribeConf += OnIncomingSubscribeConf;
        }

		public bool TryAddExisting(TlfPosition tlf)
		{
			Debug.Assert(!_Members.Contains(tlf));
			Debug.Assert(!_DesiredMembers.Contains(tlf));

            if (_DesiredMembers.Count < 5)
            {
                tlf.AddToConference(this);

                _Priority = (CORESIP_Priority)Math.Min((int)_Priority, (int)tlf.CallPriority);
                _DesiredMembers.Add(tlf);

                return true;
            }

            return false;
		}

		public void TryAddIncoming(TlfPosition tlf)
		{
			Debug.Assert(!_Members.Contains(tlf));
			Top.Tlf.Accept(tlf.Pos, this);

			if (!_DesiredMembers.Contains(tlf))
			{
				_Priority = (CORESIP_Priority)Math.Min((int)_Priority, (int)tlf.CallPriority);
				_Intruder = tlf;
				_DesiredMembers.Add(tlf);
			}
		}

		public void Add(TlfPosition tlf)
		{
			Debug.Assert(_DesiredMembers.Contains(tlf));
            _Logger.Trace("Add to conf ");
			if (!_Members.Contains(tlf))
			{
				_Members.Add(tlf);
                //NotifyConfInfo();

				General.SafeLaunchEvent(MemberAdded, this, tlf);
			}

			if ((tlf.State == TlfState.Conf) && (_Members.Count > 1))
			{
                _ConferenceState = ConfState.Executing;

                NotifyConfInfo();

				foreach (TlfPosition member in _Members)
				{
					if ((member != tlf) && (member.State == TlfState.Conf))
					{
						Top.Mixer.Link(tlf.CallId, member.CallId, MixerDir.SendRecv, FuentesGlp.Telefonia);
					}
				}
			}
		}

		public void Remove(TlfPosition tlf)
		{
			if (_DesiredMembers.Remove(tlf))
			{
				if (_DesiredMembers.Count == 1)
				{
					Dispose();
				}
				else
				{
					if (_Members.Remove(tlf))
					{
                        _Logger.Trace("Remove from conf ");
						NotifyConfInfo();
					}

					_Priority = CORESIP_Priority.CORESIP_PR_UNKNOWN;

					foreach (TlfPosition member in _DesiredMembers)
					{
						_Priority = (CORESIP_Priority)Math.Min((int)_Priority, (int)tlf.CallPriority);
					}
					General.SafeLaunchEvent(MemberRemoved, this, tlf);
				}
			}
		}

		public void HangUp()
		{
			List<TlfPosition> desiredMembers = new List<TlfPosition>(_DesiredMembers);

			_DesiredMembers.Clear();
			_Members.Clear();

			foreach (TlfPosition tlf in desiredMembers)
			{
				tlf.HangUp(0);
			}

            _ConferenceState = ConfState.Idle;

			General.SafeLaunchEvent(Finished, this);
		}

        public void Hold(bool on)
        {
            _ConferenceState= on ? ConfState.Hold : ConfState.Executing;
        }

		#region IDisposable Members

		public void Dispose()
		{
			foreach (TlfPosition tlf in _DesiredMembers)
			{
				tlf.RemoveFromConference();
			}
			_DesiredMembers.Clear();
			_Members.Clear();

			General.SafeLaunchEvent(Finished, this);
		}

		#endregion

		#region Private Members

		private CORESIP_Priority _Priority = CORESIP_Priority.CORESIP_PR_UNKNOWN;
		private List<TlfPosition> _Members = new List<TlfPosition>();
		private List<TlfPosition> _DesiredMembers = new List<TlfPosition>();
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

				for (i = j; i < info.UsersCount; i++)
				{
					TlfPosition member = _Members[i - j];

					info.Users[i].Id = member.Uri;
					info.Users[i].Name = member.Literal;

					if (member == _Intruder)
					{
						info.Users[i].Role = "Intruder";
					}
				}

                foreach (TlfPosition member in _Members)
				{
                    if ((callId == -1) || (callId == member.CallId))
					   SipAgent.SendConfInfo(member.CallId, info);
                }
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
