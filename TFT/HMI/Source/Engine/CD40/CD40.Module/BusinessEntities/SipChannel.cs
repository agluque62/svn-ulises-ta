using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using U5ki.Infrastructure;
using Utilities;
using NLog;
using System.Collections;

namespace HMI.CD40.Module.BusinessEntities
{
#if DEBUG
    public class SipLine
#else
	class SipLine
#endif	
	{
		public string Id
		{
			get { return (RsLine != null ? RsLine.Id : ""); }
		}

		public bool IsAvailable
		{
            get { return (RsLine != null ? RsLine.IsValid : true); }
		}

        public bool centralIP
        {
            get { return _centralIP; }
        }
        public string Ip;
        /// <summary>
        /// Recurso que representa el estado de la linea
        /// </summary>
		public readonly Resource RsLine;
        //Indica que es un encaminamiento tipo CentralIP externa (otro SCV)
        private bool _centralIP = false;

		public SipLine(Resource rs, string ip, bool centralIP = false)
		{
			RsLine = rs;
			Ip = ip;
            _centralIP = centralIP;
		}

		public bool Equals(string id, string ip)
		{
			return (SipUtilities.EqualSIPIPAddress(Ip, ip) && (string.Compare(Id, id, true) == 0));
		}

		public bool Find(string id, string ip)
		{
            //Comentado para permitir recibir transf directa de PP interna SCV
            return (SipUtilities.EqualSIPIPAddress(Ip, ip)/* && (string.IsNullOrEmpty(id) || (string.Compare(Id, id, true) == 0))*/);
		}
	}

#if DEBUG
    public class SipRemote
#else
	class SipRemote
#endif	
	{
		public readonly List<string> Ids;
		public readonly string SubId = null;

		public SipRemote(string id)
		{
			Ids = new List<string>(1);
			Ids.Add(id);
		}

		public SipRemote(List<string> ids)
		{
			Ids = ids;
		}

		public SipRemote(string id, string subId)
		{
			Ids = new List<string>(1);
			Ids.Add(id);

			SubId = subId;
		}

		public bool Equals(string remote, string subRemote)
		{
			if (string.Compare(SubId, subRemote, true) == 0)
			{
				foreach (string id in Ids)
				{
					if (string.Compare(id, remote, true) == 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		public bool Find(string remote, string subRemote)
		{
			if (string.IsNullOrEmpty(SubId))
			{
				if (string.IsNullOrEmpty(subRemote))
				{
					foreach (string id in Ids)
					{
						if (string.Compare(id, remote, true) == 0)
						{
							return true;
						}
					}
				}
			}
			else
			{
				return (string.Compare(SubId, subRemote, true) == 0);
			}

			return false;
		}
	}

#if DEBUG
	public class SipPath
#else
	class SipPath
#endif	
	{
		public readonly SipLine Line;
		public readonly SipRemote Remote;
        public readonly bool PriorityChannel;
        /// <summary>
        ///  indica que la llamada debe hacerse en modo sin proxy, es el modo emergencia, para llamadas internas
        ///  directas cuando no hay proxy o ha fallado
        /// </summary>
        private bool modoSinProxy;
        public bool ModoSinProxy
        {
            set {modoSinProxy = value;}
            get { return modoSinProxy; }
        }
        public string Uri
        {
            get
            {
                string dstParams = "";

                if (!string.IsNullOrEmpty(Remote.SubId))
                {
                    return string.Format("<sip:{0}@{1}{2}>", Remote.SubId, Line.Ip, dstParams);
                }
                else
                {
                    return string.Format("<sip:{0}@{1}{2}>", Remote.Ids[0], Line.Ip, dstParams);
                }
            }
        }
        public SipPath(SipRemote remote, SipLine line, Resource rsProxyPropio)
        {
            Line = line;
            if (remote.Ids[0].Contains("0*"))
                remote.Ids[0] = remote.Ids[0].Substring(2);
            Remote = remote;
            PriorityChannel = false;
            ModoSinProxy = true;

            if ((rsProxyPropio != null) && (rsProxyPropio.IsValid))
                ModoSinProxy = false;
        }
    
        public SipPath(SipRemote remote, SipLine line, bool priority, Resource rsProxyPropio)
        {
            Line = line;
            Remote = remote;
            PriorityChannel = priority;

            if (priority && !Remote.Ids[0].Contains("0*"))
                Remote.Ids[0] = "0*" + remote.Ids[0];
            else if (!priority && Remote.Ids[0].Contains("0*"))
                Remote.Ids[0] = Remote.Ids[0].Substring(2);
            ModoSinProxy = true;

            if ((rsProxyPropio != null) && (rsProxyPropio.IsValid))
                ModoSinProxy = false;
        }

        public void Reset()
        {
            if (Remote.Ids[0].Contains("0*"))
                Remote.Ids[0] = Remote.Ids[0].Substring(2);
        }

        //LALM 211012
        //#2629 Presentar via utilizada en llamada saliente.
        public void Reset(string uri)
        {
            if (Remote.Ids[0].Contains("0*"))
                Remote.Ids[0] = Remote.Ids[0].Substring(2);
        }
    }

#if DEBUG
	public struct SipResult
#else
    struct SipResult
#endif
    {
		public int Result;
		public int PrioResult;
	}

#if DEBUG
	public abstract class SipChannel
#else
	abstract class SipChannel
#endif	
	{
        public enum DestinationState {Idle, NotReachable, Busy};
		public virtual event GenericEventHandler RsChanged
		{
			add
			{
				foreach (SipLine line in _Lines)
				{
					if (line.RsLine != null)
						line.RsLine.Changed += value;
				}
                if (RsProxyPropio != null)
                    RsProxyPropio.Changed += value;
            }
			remove 
			{
				foreach (SipLine line in _Lines)
				{
					if (line.RsLine != null)
						line.RsLine.Changed -= value;
                }
                if (RsProxyPropio != null)
                    RsProxyPropio.Changed -= value;
            }
		}

		public string Id
		{
			get { return _Id; }
		}

		public uint Prefix
		{
			get { return _Prefix; }
		}

		public string AccId
		{
			get { return _AccId; }
		}

		public virtual string Uri
		{
			get { return null; }
		}
        public virtual string UriPropia
        {
            get { return null; }
        }

        public virtual string Domain
		{
			get { return null; }
		}
        public virtual ArrayList GetUris
		{
			get { return null; }
		}

		public virtual TipoInterface Type
		{
			get
			{
				return _Type;
			}
		}

		public IEnumerable<SipLine> Lines
		{
			get { return _Lines; }
		}

        public List<SipLine> ListLines
        {
            get { return _Lines; }
        }

		public List<SipRemote> RemoteDestinations
		{
			get { return _RemoteDestinations; }
		}

		public bool First
		{
			get { return _First; }

			set { _First = value; }
		}

        public bool PriorityAllowed
        {
            get { return _PriorityAllowed; }
        }

        public bool IsPP
        {
            get { return _IsPP; }
        }


        public SipChannel()
        {
            string idEquipo;

            string rsIp = Top.Cfg.GetProxyIp(out idEquipo);
            if (rsIp != null)
            {
                RsProxyPropio = Top.Registry.GetRs<GwTlfRs>(idEquipo);
            }
         }
		public void AddRemoteDestination(string dst, string subDst)
		{
			foreach (SipRemote remote in _RemoteDestinations)
			{
				if (remote.Equals(dst, subDst))
				{
					return;
				}
			}

			_RemoteDestinations.Add(new SipRemote(dst, subDst));
			_Results = new SipResult[_RemoteDestinations.Count, _Lines.Count];
		}

		public SipRemote ContainsRemote(string id, string subId)
		{
			foreach (SipRemote remote in _RemoteDestinations)
			{
				if (remote.Equals(id, subId))
				{
					return remote;
				}
			}

			return null;
		}

		public SipLine ContainsLine(string id, string ip)
		{
			foreach (SipLine line in _Lines)
			{
				if (line.Equals(id, ip))
				{
					return line;
				}
			}

			return null;
		}

		public SipPath FindPath(string id, string ip, string subId, string rsId)
		{
            if (_UnknownUri != null)
                return new SipPath(_RemoteDestinations[0], _Lines[0], RsProxyPropio);
            else
            {
                //Busco un path que coincida con todos los datos (excepto el rsId)
                foreach (SipRemote remote in _RemoteDestinations)
                {
                    if (remote.Find(id, subId))
                    {
                        foreach (SipLine line in _Lines)
                        {
                            if (line.Find(rsId, ip))
                            {
                                return new SipPath(remote, line, RsProxyPropio);
                            }
                        }
                    }
                }
            }
			return null;
		}
        public SipPath FindPath(string uri)
        {
            SipPath sipPath = null;
            string sipUri = new SipUtilities.SipUriParser(uri).UlisesFormat;
            foreach (SipRemote remote in _RemoteDestinations)
            {
                foreach (SipLine line in _Lines)
                {
                    if (line.IsAvailable)
                    {
                        sipPath = new SipPath(remote, line, RsProxyPropio);
                        string pathUri = new SipUtilities.SipUriParser(sipPath.Uri).UlisesFormat;
                        if (sipUri.CompareTo(pathUri) == 0)
                            return sipPath;
                    }
                }
            }

            return null;
        }

        public SipPath FindPathNoConfigured(string id, string subId)
        {
            //Busco un path que contenga en RemoteDestination el id aunque no coincida la ip y rs
            //Este caso es una llamada entrante de que llega de tránsito desde un SCV intermedio
            //donde no conozco la IP de origen, ni el recurso (pasarela de otro SCV)
            foreach (SipRemote remote in _RemoteDestinations)
            {
                if (remote.Find(id, subId))
                {
                    foreach (SipLine line in _Lines)
                    {
                        if (line.IsAvailable)
                        {
                            return new SipPath(remote, line, RsProxyPropio);
                        }
                    }
                    //Si el canal no tiene lineas, se trata de una llamada entrante desconocida
                    //no identificada en la configuración, devuelvo un path sin línea
                    if (_Lines.Count == 0)
                        return new SipPath(remote, null, RsProxyPropio);
                }
            }
            return null;
        }

		public void ResetCallResults(bool allResults)
		{
			for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
			{
				for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
				{
					if (allResults)
					{
						_Results[i, j].Result = 0;
					}
					_Results[i, j].PrioResult = 0;
				}
			}
		}

		public bool ResetCallResults(Resource rs)
		{
			for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
			{
				if (_Lines[j].RsLine == rs)
				{
					if (!rs.IsValid)
					{
						switch (_Prefix)
						{
							case Cd40Cfg.INT_DST:
								for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
								{
									_Results[i, j].Result = 0;
									_Results[i, j].PrioResult = 0;
								}
								break;

							default:
								for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
								{
									switch (_Results[i, j].Result)
									{
										case SipAgent.SIP_BUSY:
										case SipAgent.SIP_NOT_FOUND:
										case SipAgent.SIP_CONGESTION:
											break;
										default:
											_Results[i, j].Result = 0;
											_Results[i, j].PrioResult = 0;
											break;
									}
								}
								break;
						}
					}
					else if (rs.Content is GwTlfRs)
					{
						switch (((GwTlfRs)rs.Content).St)
						{
							case GwTlfRs.State.Idle:
								for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
								{
									if (_Results[i, j].Result == SipAgent.SIP_TEMPORARILY_UNAVAILABLE)
									{
										_Results[i, j].Result = 0;
										_Results[i, j].PrioResult = 0;
									}
								}
								break;
							case GwTlfRs.State.BusyInterruptionAllow:
								for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
								{
									if (_Results[i, j].Result == SipAgent.SIP_TEMPORARILY_UNAVAILABLE)
									{
										_Results[i, j].PrioResult = 0;
									}
								}
								break;
						}
					}

					return true;
				}
			}

			return false;
		}

		public void SetCallResult(SipRemote remote, SipLine line, CORESIP_Priority priority, int result)
		{
			for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
			{
				if (_RemoteDestinations[i] == remote)
				{
					for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
					{
						if (_Lines[j] == line)
						{
							_Results[i, j].Result = result;

							if (priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
							{
								_Results[i, j].PrioResult = result;
							}

							break;
						}
					}

					break;
				}
			}
		}

        /// <summary>
        /// Devuelve true si el último error en los resultados era de una linea tipo centralIP
        /// </summary>
        /// <returns></returns>
        public bool LastErrorInIP(SipRemote remote, CORESIP_Priority priority)
        {
            for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
            {
                if (_RemoteDestinations[i] == remote)
                {
                    for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
                    {
                        if (priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)
                        {
                            if (_Results[i, j].PrioResult != 0)
                                return _Lines[j].centralIP;
                        }
                        else
                            if (_Results[i, j].Result != 0)
                                return _Lines[j].centralIP;
                    }
                }
            }
            return false;
        }

        protected bool IsAvailable(SipLine line)
        {
            if (!line.centralIP)
                return line.IsAvailable;
            else
                return (line.IsAvailable && RsProxyPropio != null && RsProxyPropio.IsValid);
        }

		public virtual SipPath GetPreferentPath(CORESIP_Priority priority)
		{
			return null;
		}

		public virtual SipPath GetInterrumpiblePath()
		{
			return null;
		}

		public virtual SipPath GetDetourPath(CORESIP_Priority priority)
		{
			return null;
		}

		public virtual void ResetLine(string id, string ip)
		{
			return;
		}

        /// <summary>
        /// Devuelve el estado del destino
        /// En el caso general, el estado lo da la disponibilidad de cualquiera de sus lineas
        /// </summary>
        public virtual DestinationState DestinationReachableState()
        {
            DestinationState st = DestinationState.NotReachable;
            foreach (SipLine line in Lines)
            {
                if (IsAvailable(line))
                {
                    return DestinationState.Idle;
                }
            }
            return st;
        }
 
 		#region Protected Members
        protected static Logger _Logger = LogManager.GetCurrentClassLogger();

		protected string _Id;
		protected uint _Prefix;
        //Nombre de la cuenta SIP, coincide con el numero de abonado del sector
		protected string _AccId;
		protected List<SipLine> _Lines = new List<SipLine>();
		protected List<SipRemote> _RemoteDestinations = new List<SipRemote>();
		protected SipResult[,] _Results = null;
		protected bool _First = false;

		protected TipoInterface _Type = 0;
        //Recurso adicional que representa el estado del proxy propio
        //Se utiliza para activar el modo emergencia y afecta a la presencia de los encaminamientos por IP
        public Resource RsProxyPropio = null;
        //Se utiliza para detectar canales de tipo PP de pasarela que no admiten prioridad, o lineas de otra red no ATS
        protected bool _PriorityAllowed = true;
        protected bool _IsPP = false;
        protected string _UnknownUri = null;


		#endregion
	}

    /// <summary>
    /// Not used
    /// </summary>
	class IpChannel : SipChannel
	{
		public override string Uri
		{
			get { return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip); }
		}

        public override string Domain
        {
            get { return _Lines[0].Ip; }
        }

		public IpChannel(string localId, string remote, uint prefix)
		{
            string rsIpExt;
            string idEquipo;

			_Id = remote;
			_Prefix = prefix;
			_AccId = Top.Cfg.GetNumeroAbonado(localId, Cd40Cfg.ATS_DST) ?? localId;

			string[] idIp = remote.Split('@');
			_RemoteDestinations.Add(new SipRemote(idIp[0]));
            // Usar esta en modo sin proxy 
            rsIpExt = idIp[1];

             rsIpExt = Top.Cfg.GetProxyIp(out idEquipo);
            _Lines.Add(new SipLine(null, rsIpExt));
			_Results = new SipResult[1, 1];
		}

		public override SipPath GetPreferentPath(CORESIP_Priority priority)
		{
			SipLine line = _Lines[0];
			SipRemote remote = _RemoteDestinations[0];

			if (_Results[0, 0].PrioResult == 0)
			{
				int result = _Results[0, 0].Result;

				if ((result == 0) ||
					((result == SipAgent.SIP_BUSY) && (priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)))
				{
					return new SipPath(remote, line, RsProxyPropio);
				}
			}
            _Logger.Debug("IpChannel not available, line {0}, results: {1}, {2}", line.RsLine.Id, _Results[0, 0].PrioResult, _Results[0, 0].Result);
			return null;
		}
	}

	class IntChannel : SipChannel
	{
        ///<summary>
        ///Devuelve la uri formada con la dirección del proxy si está disponible, 
        ///Si el proxy no está disponible se envía la dirección propia del sector
        /// </summary>
        public override string Uri
        {
            get
            {
                return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], Domain);
            }
        }
        /// <summary>
        /// Devuelve la uri formada propia del sector
        /// </summary>
        public override string UriPropia
        {   
            get
            {
                return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Lines[1].Ip);
            }
        }

        public override ArrayList GetUris
        {
            get
            {
                ArrayList listaUris = new ArrayList();
                foreach (SipRemote dest in _RemoteDestinations)
                    foreach (string idDest in dest.Ids)
                        listaUris.Add(string.Format("<sip:{0}@{1}>", idDest, Domain));
                return listaUris;
            }
        }

        public override string Domain
        {
            get { return _Lines[0].IsAvailable ? _Lines[0].Ip : _Lines[1].Ip; }
        }

		public IntChannel(string localId, string hostId, string userId, uint prefix)
		{
            string rsIp;
            string idEquipo;

			_Id = hostId;
			_Prefix = prefix;
			//_Local = Top.Cfg.GetNumeroAbonado(localId, Cd40Cfg.ATS_DST) ?? localId;
            _AccId = localId;

			foreach (StrNumeroAbonado num in Top.Cfg.GetHostAddresses(hostId))
			{
				if (string.Compare(num.NumeroAbonado, userId, true) != 0)
				{
                    _RemoteDestinations.Add(new SipRemote(num.NumeroAbonado));
				}
			}

            //Primera línea por el proxy
            //y última línea en caso de no disponibilidad de proxy, la directa entre puestos
            rsIp = Top.Cfg.GetProxyIp(out idEquipo);
            if (rsIp != null)
            {
                Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(idEquipo);
                _Lines.Add(new SipLine(rs, rsIp));
            }
            rsIp = Top.Cfg.GetHostIp(hostId);
            Rs<TopRs> rsTop = Top.Registry.GetRs<TopRs>(hostId);
            _Lines.Add(new SipLine(rsTop, rsIp));

            _Results = new SipResult[_RemoteDestinations.Count, _Lines.Count];
		}

        /// <summary>
        /// Para puestos, se usa siempre la primera línea por el proxy
        /// a excepción de que no esté disponible, entonces se usa la línea directa
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
		public override SipPath GetPreferentPath(CORESIP_Priority priority)
		{
             SipPath sipPath = null;

            for (int i = 0; i< _Lines.Count; i++)
            {
                if (_Lines[i].IsAvailable)
                    if (_Results[0, i].PrioResult == 0)
                    {

                        if ((_Results[0, i].Result == 0) ||
                            ((_Results[0, i].Result == SipAgent.SIP_BUSY) && (priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)))
                        {
                            return new SipPath(_RemoteDestinations[0], _Lines[i], RsProxyPropio);
                        }
                    }
                _Logger.Debug("IntChannel not available, line {0}, results:{1}, {2}", _Lines[i].RsLine.Id, _Results[0, i].PrioResult, _Results[0, i].Result);
            }

            return sipPath;
		}

        /// <summary>
        /// Devuelve el estado del destino. 
        /// En el caso de IntChannel (usuarios TOP internos), el estado lo da la
        /// disponibilidad de la linea del recurso TopRs, no la linea que va por el proxy
        /// </summary>
        /// <returns> Estado Idle o Reachable, el Top, nunca se considera ocupado</returns>
        public override DestinationState DestinationReachableState()
        {
           foreach (SipLine line in _Lines)
            {
                if (line.RsLine is Rs<TopRs>)
                {
                    if (line.IsAvailable)
                        return DestinationState.Idle;
                }
            } 
            return DestinationState.NotReachable;
        }
	}

	class LcChannel : SipChannel
	{
		public override string Uri
		{
			get { return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip); }
		}
        public override string Domain
        {
            get { return _Lines[0].Ip; }
        }

		public LcChannel(string localId, string rsId, uint prefix)
		{
			Rs<GwLcRs> rs = Top.Registry.GetRs<GwLcRs>(rsId);
            string idEquipo;

			_Id = rsId;
			_Prefix = prefix;
			_AccId = Top.Cfg.GetNumeroAbonado(localId, Cd40Cfg.ATS_DST) ?? localId;
            _RemoteDestinations.Add(new SipRemote(rsId));
			_Lines.Add(new SipLine(rs, Top.Cfg.GetGwRsIp(rsId, out idEquipo)));
			_Results = new SipResult[1, 1];
        }

		public override SipPath GetPreferentPath(CORESIP_Priority priority)
		{
			SipLine line = _Lines[0];
			SipRemote remote = _RemoteDestinations[0];
            SipPath sipPath = null;

            if (line.IsAvailable && (_Results[0, 0].PrioResult == 0) && (_Results[0, 0].Result == 0))
			{
				sipPath = new SipPath(remote, line, RsProxyPropio);
			}
            _Logger.Debug("LcChannel not available, line {0}, results:{1}, {2}", line.RsLine.Id, _Results[0, 0].PrioResult, _Results[0, 0].Result);

            return sipPath;
		}
		public override void ResetLine(string id, string ip)
		{
			foreach (SipLine line in Lines)
			{
				if (line.Id == id)
				{
					line.Ip = ip;
				}
			}
		}
	}

	class TlfPPChannel : SipChannel
	{
		public override string Uri
		{
			get 
			{
				if (string.Compare(_RemoteDestinations[0].Ids[0], _Lines[0].Id) != 0)
				{
                    string dstParams="";
                    if (!_Lines[0].centralIP)
                    {
                        dstParams += string.Format(";cd40rs={0}", _Lines[0].Id);
                    }
                    return string.Format("<sip:{0}@{1}{2}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip, dstParams); 
				}

				return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip);
			}
		}

        public override ArrayList GetUris
        {
            get
            {
                ArrayList listaUris = new ArrayList();
                foreach (SipRemote dest in _RemoteDestinations)
                    foreach (string idDest in dest.Ids)
                        listaUris.Add(string.Format("<sip:{0}@{1}>", idDest, Domain));
                return listaUris;
            }
        }

        public override string Domain
        {
            get { return _Lines[0].Ip; }
        }

		public TlfPPChannel(string localId, string number, string rsId, uint prefix, TipoInterface type)
		{
            string rsIp;
            string idEquipo;

			Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(rsId);

			_Id = rsId;
			_Prefix = prefix;
			_AccId = Top.Cfg.GetNumeroAbonado(localId, Cd40Cfg.ATS_DST) ?? localId;
			_Type = type;
			_RemoteDestinations.Add(new SipRemote(string.IsNullOrEmpty(number) ? rsId : number));
            rsIp = Top.Cfg.GetGwRsIp(rsId, out idEquipo);
            _Lines.Add(new SipLine(rs, rsIp));

			_Results = new SipResult[1, 1];

            //En lineas PP recurso de pasarela no se admite la llamada con prioridad
            //Sin embargo en los teléfonos de seguridad IP que llegan configurados de la misma forma, si se permite
            //Hago la distinción comparando la Ip de la línea con la del proxy
            //TODO Deberían llegar configurados de otra forma si tienen comportamiento diferente
            //#3595 y #3082 Cambio de criterio: No se permite la prioridad en AD no ATS, ni siquiera telefonos IP
            //En lineas PP recursos de pasarela se admiten facilidades como desvío
            if (rsIp != Top.Cfg.GetProxyIp(out idEquipo) &&
                ((type == TipoInterface.TI_BC) || (type == TipoInterface.TI_BL)))
            {
                //Es un recurso PP de pasarela
                _IsPP = true;
                //_PriorityAllowed = false;
            }
            _PriorityAllowed = false;
		}

		public override SipPath GetPreferentPath(CORESIP_Priority priority)
		{
			SipLine line = _Lines[0];
			SipRemote remote = _RemoteDestinations[0];
            SipPath sipPath = null;

            if (line.IsAvailable &&
                (_Results[0, 0].PrioResult == 0))
                {
				int result = _Results[0, 0].Result;

				if ((result == 0) ||
					((result == SipAgent.SIP_BUSY) && (priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)))
				{
                    if (((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.Idle)
                    {
                        sipPath = new SipPath(remote, line, RsProxyPropio);
                    }                    
                    else if ((priority == CORESIP_Priority.CORESIP_PR_EMERGENCY) && _PriorityAllowed)
                        // Intento de llamada sobre una línea 
                        // PaP ocupada con prioridad
                    {
                        sipPath = new SipPath(remote, line, true, RsProxyPropio);
                    }
				}
			}
            if (sipPath == null)
                _Logger.Debug("TlfPPChannel not available, line {0}, results:{1}, {2}", line.RsLine.Id, _Results[0, 0].PrioResult, _Results[0, 0].Result);

            return sipPath;
		}

		public override SipPath GetInterrumpiblePath()
		{
			SipLine line = _Lines[0];
			SipRemote remote = _RemoteDestinations[0];

			if (line.IsAvailable && 
				((_Type == TipoInterface.TI_ATS_R2) || (_Type == TipoInterface.TI_ATS_N5) || (_Type == TipoInterface.TI_ATS_QSIG)) &&
				((((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.Idle) || (((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.BusyInterruptionAllow)) &&
				(_Results[0, 0].PrioResult == 0))
			{
				switch (_Results[0, 0].Result)
				{
					case 0:
					case SipAgent.SIP_BUSY:
					case SipAgent.SIP_CONGESTION:
					case SipAgent.SIP_TEMPORARILY_UNAVAILABLE:
                        return new SipPath(remote, line, RsProxyPropio);
				}
			}
            _Logger.Debug("TlfPPChannel not available, line {0}, results:{1}, {2}", line.RsLine.Id, _Results[0, 0].PrioResult, _Results[0, 0].Result);

			return null;
		}

		public override void ResetLine(string id, string ip)
		{
			foreach (SipLine line in Lines)
			{
				if (line.Id == id)
				{
					line.Ip = ip;
				}
			}
		}
        /// <summary>
        /// Devuelve el estado del destino. 
        /// En el caso general, el estado lo da la disponibilidad de cualquiera de sus lineas
        /// </summary>
        /// <returns> Estado Idle, Reachable, o Busy para las PP</returns>
        public override DestinationState DestinationReachableState()
        {
            DestinationState st = DestinationState.NotReachable;
            foreach (SipLine line in Lines)
            {
                if (line.IsAvailable)
                {
                    if ((Prefix == Cd40Cfg.PP_DST) &&
                        ((((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.BusyInterruptionAllow) ||
                        (((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.BusyInterruptionNotAllow)))
                    {
                        st = DestinationState.Busy;
                    }
                    else
                    {
                        return DestinationState.Idle;
                    }
                }
            }
            return st;
        }
    }
    class TlfFocusChannel : SipChannel
    {
        /// <summary>
        /// Lista de recurso que representan los destinos finales
        /// Se utiliza para calcular el estado global del focusChannel
        /// </summary>
        private List<Resource> _RsDstList = new List<Resource> ();
        private string _Host = null;
        private bool _ImMember = false;
        public bool ImMember
        { set { _ImMember = value; } }
        /// <summary>
        /// Este canal se utiliza para hacer llamadas al Phone Service Channel que esta en PhoneService y actua como foco.
        /// </summary>
        /// <param name="localId">own accountId</param>
        /// <param name="name">name of focus</param>
        public TlfFocusChannel(string localId, string name)
		{
            _Id = "Focus";
            _UnknownUri = name;
            _AccId = localId;
            _PriorityAllowed = false;
            _Prefix = Cd40Cfg.MD_DST;   //? para que no se identifique con otro tipo
            //Primera línea por el proxy, unica valida para crear un SipPath
            string idEquipo;
            string rsIp = Top.Cfg.GetProxyIp(out idEquipo);
            if (rsIp != null)
            {
                Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(idEquipo);
                _Lines.Add(new SipLine(rs, rsIp, true));
            }
			_RemoteDestinations.Add(new SipRemote(name));
			_Results = new SipResult[_RemoteDestinations.Count, _Lines.Count];
        }
        public TlfFocusChannel(string localId, string name, string host)
        {
            _Host = host;
            _Id = "PhoneService";
            _UnknownUri = name;
            _AccId = localId;
            _PriorityAllowed = false;
            _Prefix = Cd40Cfg.MD_DST;   //? para que no se identifique con otro tipo
            
            //Primera línea por el proxy, unica valida para crear un SipPath
            string idEquipo;
            string rsIp = Top.Cfg.GetProxyIp(out idEquipo);
            if (rsIp != null)
            {
                Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(idEquipo);
                _Lines.Add(new SipLine(rs, rsIp, true));
            }
            _RemoteDestinations.Add(new SipRemote(name));
            _Results = new SipResult[_RemoteDestinations.Count, _Lines.Count];
        }

        //Añade lineas que sirven unicamente para dar la disponibilidad (aspa)
        //No sirven para encaminar
        public void AddFinalDestination(string name, string number, uint prefix)
        {
            switch (prefix)
            {
                case Cd40Cfg.INT_DST:
                    string hostId = Top.Cfg.GetUserHost(name);
                    Rs<TopRs> rsTop = Top.Registry.GetRs<TopRs>(hostId);
                    _RsDstList.Add(rsTop);
                    break;
                case Cd40Cfg.ATS_DST:
                    Rs<GwTlfRs> rsAts = Top.Registry.GetRs<GwTlfRs>(number);
                    _RsDstList.Add(rsAts);
                    StrNumeroAbonado altNet = null;
                    TlfNet net = Top.Cfg.GetNet(prefix, number, ref altNet);
                    foreach (SipLine line in net.Lines)
                    {
                        _RsDstList.Add(line.RsLine);
                    }
                    if (altNet != null)
                    {
                         net = Top.Cfg.GetNet(altNet.Prefijo, altNet.NumeroAbonado, ref altNet);
                         foreach (SipLine line in net.Lines)
                         {
                             _RsDstList.Add(line.RsLine);
                         }
                    }
                    break;
                case Cd40Cfg.RTB_DST:
                    List <PlanRecursos> listRec = Top.Cfg.GetNetResources(prefix, number);
                    foreach (PlanRecursos recurso in listRec)
                    {
                        Rs<GwTlfRs> rsRtb = Top.Registry.GetRs<GwTlfRs>(recurso.IdRecurso);
                        _RsDstList.Add(rsRtb);
                    }
                    break;
                case Cd40Cfg.IP_DST:
                case Cd40Cfg.PP_DST:
                    Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(name);
                    _RsDstList.Add(rs);
                    break;
                default:
                    //TODO faltan otros tipos
                    break;
            }
        }
        public override string Uri
        {
            get
            {
                if (_Host == null)
                    return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip);
                else
                    return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Host);
            }
        }
        /// <summary>
        /// Para construir el path se usa unicamente la primera línea por el proxy
        /// </summary>
        /// <param name="priority"></param>
        /// <returns></returns>
        public override SipPath GetPreferentPath(CORESIP_Priority priority)
        {
            SipPath sipPath = null;

            for (int i = 0; i < _Lines.Count; i++)
            {
                if (_Lines[i].IsAvailable)
                    if (_Results[0, i].PrioResult == 0)
                    {
                        if ((_Results[0, i].Result == 0) ||
                            ((_Results[0, i].Result == SipAgent.SIP_BUSY) && (priority == CORESIP_Priority.CORESIP_PR_EMERGENCY)))
                        {
                            return new SipPath(_RemoteDestinations[0], _Lines[i], RsProxyPropio);
                        }
                    }
                _Logger.Debug("TlfFocusChannel not available, line {0}, results:{1}, {2}", _Lines[i].RsLine.Id, _Results[0, i].PrioResult, _Results[0, i].Result);
            }

            return sipPath;
        }
        /// <summary>
        /// Devuelve el estado del destino
        /// El estado lo da la disponibilidad de los destinos finales y el estado de las lineas
        /// </summary>
        public override DestinationState DestinationReachableState()
        {
            if (_ImMember)
                return base.DestinationReachableState();
            //TODO falta incluir la presencia del PhoneService
            DestinationState st = DestinationState.NotReachable;
            if (1 <= _RsDstList.FindAll(x => x.IsValid).Count)
                return base.DestinationReachableState();
            return st;
        }
        public override event GenericEventHandler RsChanged
        {
            add
            {
                foreach (SipLine line in _Lines)
                {
                    if (line.RsLine != null)
                        line.RsLine.Changed += value;
                }
                foreach (Resource res in _RsDstList)
                    res.Changed += value;
            }
            remove
            {
                foreach (SipLine line in _Lines)
                {
                    if (line.RsLine != null)
                        line.RsLine.Changed -= value;
                }
                foreach (Resource res in _RsDstList)
                    res.Changed -= value;
            }
        }
    }
	class TlfNetChannel : SipChannel
	{
		public override string Uri
		{
			get 
			{
                string dstParams = "";
                if (!_Lines[0].centralIP)
                {
                    dstParams += string.Format(";cd40rs={0}", _Lines[0].Id);
                }

                if (!string.IsNullOrEmpty(_UnknownUri))
                {
                    return _UnknownUri + dstParams;
                }
                if (!string.IsNullOrEmpty(_RemoteDestinations[0].SubId))
                {
                    return string.Format("<sip:{0}@{1}{2}>", _RemoteDestinations[0].SubId[0], _Lines[0].Ip, dstParams);
                }
                else 
                {
                    //Para redes externas, es necesario enviar el recurso
                    if (Prefix > Cd40Cfg.ATS_DST)
                        return string.Format("<sip:{0}@{1}{2}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip, dstParams);
                    return string.Format("<sip:{0}@{1}>", _RemoteDestinations[0].Ids[0], _Lines[0].Ip);
			}
		}
		}
        public override string Domain
        {
            get { return _Lines[0].Ip; }
        }

        /// <summary>
        /// Este canal se utiliza para los teléfonos IP y para sacar por IP hacia fuera recursos ajenos al SCV
        /// donde la IP del 'To' viene en number
        /// </summary>
        /// <param name="net"></param>
        /// <param name="localId"></param>
        /// <param name="number"></param>
        /// <param name="subNumber"></param>
        /// <param name="prefix"></param>
        /// <param name="unknownResource"></param>
        public TlfNetChannel(TlfNet net, string localId, string number, string subNumber, uint prefix, bool unknownResource = false)
		{            
            
            _Id = net.Id;
			_Prefix = prefix;
            if (unknownResource)
                _UnknownUri = number;
            if (prefix != Cd40Cfg.ATS_DST)
               _AccId = Top.Cfg.GetNumeroAbonado(localId, Cd40Cfg.ATS_DST) ?? localId;
            else
                _AccId = localId;
            if ((prefix > Cd40Cfg.ATS_DST) && (prefix < Cd40Cfg.UNKNOWN_DST))
                _PriorityAllowed = false;

			_RemoteDestinations.Add(new SipRemote(number, subNumber));
			_Lines = net.Lines;
			_Results = new SipResult[_RemoteDestinations.Count, _Lines.Count];
			_RsTypes = net.RsTypes;
            //0 (preferente)
            //1, 2, 3,...1000 rutas alternativas (detour)
			_Routes = net.Routes;
        }

		public override SipPath GetPreferentPath(CORESIP_Priority priority)
		{
            SipPath sipPath = null;

			for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
			{
				SipRemote remote = _RemoteDestinations[i];
				int preferentRoute = int.MaxValue;
				int skipRoute = -1;

				for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
				{
					SipLine line = _Lines[_RsTypes[j].indexToLine];
					int route = _Routes[j];
					int prioResult = _Results[i, j].PrioResult;

					if (IsAvailable(line) && (preferentRoute == int.MaxValue))
					{
						preferentRoute = route;
					}

					if (route > preferentRoute)
					{
						break;
					}
					else if (route == skipRoute)
					{
						continue;
					}

					if (prioResult == 0)
					{
						int result = _Results[i, j].Result;
                        if ((result == 0) ||
                            ((priority == CORESIP_Priority.CORESIP_PR_EMERGENCY) &&
                             ((result == SipAgent.SIP_BUSY) ||
                             (result == SipAgent.SIP_CONGESTION))))
                        {
                            if (IsAvailable(line) && (((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.Idle) &&
                                ((route == 0) || (_RsTypes[j].TipoInteface == TipoInterface.TI_ATS_QSIG)))
                            {
                                _Type = _RsTypes[j].TipoInteface;
                                sipPath = new SipPath(remote, line, RsProxyPropio);
                                break;
                            }
                        }
                        else if (SkipThisRoute(result, _RsTypes[j].TipoInteface))
                        // Debe saltarse esta ruta y buscar la siguiente
						{
							skipRoute = route;
						}
                        else if (SkipAllRoutes(result, _RsTypes[j].TipoInteface))
                        {
                            return null;
                        }
					}
					else if ((prioResult == SipAgent.SIP_NOT_FOUND) || (prioResult == SipAgent.SIP_CONGESTION))
					{
						skipRoute = route;
					}
                    _Logger.Debug("TlfNetChannel not available, line {0}, results:{1}, {2}", line.RsLine.Id, _Results[i, j].PrioResult, _Results[i, j].Result);
                }
			}

            return sipPath;
		}

		public override SipPath GetInterrumpiblePath()
		{
			for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
			{
				List<Utilities.Tuple<SipLine, int, uint>> interrumpibleLines = new List<Utilities.Tuple<SipLine, int, uint>>();
				SipRemote remote = _RemoteDestinations[i];
				int preferentRoute = int.MaxValue;
				int skipRoute = -1;

				for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
				{
					SipLine line = _Lines[j];
					int route = _Routes[j];
					int prioResult = _Results[i, j].PrioResult;

                    if (IsAvailable(line) && (preferentRoute == int.MaxValue))
					{
						preferentRoute = route;
					}

					if (route > preferentRoute)
					{
						break;
					}
					else if (route == skipRoute)
					{
						continue;
					}

					if (prioResult == 0)
					{
						switch (_Results[i, j].Result)
						{
							case 0:
							case SipAgent.SIP_BUSY:
							case SipAgent.SIP_CONGESTION:
							case SipAgent.SIP_TEMPORARILY_UNAVAILABLE:
								GwTlfRs rs = (GwTlfRs)line.RsLine.Content;

                                if (IsAvailable(line) &&
									((rs.St == GwTlfRs.State.Idle) || (rs.St == GwTlfRs.State.BusyInterruptionAllow)))
								{
									TipoInterface type = _RsTypes[j].TipoInteface;

									if ((type == TipoInterface.TI_ATS_R2) || (type == TipoInterface.TI_ATS_N5))
									{
										// Solo recursos en la ruta directa
										if (route == 0)
										{
											uint[] r2Priorities = { 10, 9, 4, 10, 8, 3, 10, 7, 2 };
											int sortKey = Array.IndexOf<uint>(r2Priorities, rs.Priority);

											if (sortKey >= 0)
											{
												interrumpibleLines.Add(new Utilities.Tuple<SipLine, int, uint>(line, sortKey, rs.CallBegin));
											}
										}
									}
									else if (type == TipoInterface.TI_ATS_QSIG)
									{
										uint[] qsigPriorities = { 0, 10, 10, 1, 10, 10, 2, 10, 10 };
										int sortKey = Array.IndexOf<uint>(qsigPriorities, rs.Priority);

										if (sortKey >= 0)
										{
											interrumpibleLines.Add(new Utilities.Tuple<SipLine, int, uint>(line, sortKey, rs.CallBegin));
										}
									}
								}
								break;
							case SipAgent.SIP_NOT_FOUND:
								skipRoute = route;
								break;
						}
					}
					else if ((prioResult == SipAgent.SIP_NOT_FOUND) || (prioResult == SipAgent.SIP_CONGESTION))
					{
						skipRoute = route;
					}
				}

				if (interrumpibleLines.Count > 0)
				{
					interrumpibleLines.Sort(delegate(Utilities.Tuple<SipLine, int, uint> a, Utilities.Tuple<SipLine, int, uint> b)
					{
                        //A igual prioridad se interrumpe la llamada mas antigua, con CallBegin menor
						if (a.Second == b.Second)
						{
							return (int)(a.Third - b.Third);
						}
                        //Se interrumpe la llamada con menor prioridad, menor sortKey
						return a.Second - b.Second;
					});

                    return new SipPath(remote, interrumpibleLines[0].First, RsProxyPropio);
				}
			}

			return null;
		}

		public override SipPath GetDetourPath(CORESIP_Priority priority)
		{
			for (int i = 0, iTo = _RemoteDestinations.Count; i < iTo; i++)
			{
				SipRemote remote = _RemoteDestinations[i];
				int preferentRoute = int.MaxValue;
				int skipRoute = -1;

				for (int j = 0, jTo = _Lines.Count; j < jTo; j++)
				{
					SipLine line = _Lines[j];
					int route = _Routes[j];
					int prioResult = _Results[i, j].PrioResult;

                    if (IsAvailable(line) && (preferentRoute == int.MaxValue))
					{
						preferentRoute = route;
					}

					if (route == skipRoute)
					{
						continue;
					}

					if (prioResult == 0)
					{
						int result = _Results[i, j].Result;
                        if ((result == 0) ||
                            ((priority == CORESIP_Priority.CORESIP_PR_EMERGENCY) &&
                             ((result == SipAgent.SIP_BUSY) ||
                             (result == SipAgent.SIP_CONGESTION))))
                        {
                            if (IsAvailable(line) && (((GwTlfRs)line.RsLine.Content).St == GwTlfRs.State.Idle) &&
								(((_RsTypes[j].TipoInteface != TipoInterface.TI_ATS_QSIG) && (route != 0)) ||
								((_RsTypes[j].TipoInteface == TipoInterface.TI_ATS_QSIG) && (route > preferentRoute))))
							{
                                _Type = _RsTypes[j].TipoInteface;
                                return new SipPath(remote, line, RsProxyPropio);
							}
						}
                        else if (SkipThisRoute(result, _RsTypes[j].TipoInteface))
                        // Debe saltarse esta ruta y buscar la siguiente
                        {
                            skipRoute = route;
                        }
                        else if (SkipAllRoutes(result, _RsTypes[j].TipoInteface))
                        {
                            return null;
                        }
					}
					else if ((prioResult == SipAgent.SIP_NOT_FOUND) || (prioResult == SipAgent.SIP_CONGESTION))
					{
						skipRoute = route;
					}
                    _Logger.Debug("TlfNetChannel detour not available, line {0}, results:{1}, {2}", line.RsLine.Id, _Results[i, j].PrioResult, _Results[i, j].Result);
                }
			}

			return null;
		}

        public override void ResetLine(string id, string ip)
        {
            foreach (SipLine line in Lines)
            {
                if (line.Id == id)
                {
                    line.Ip = ip;
                }
            }
        }

        private bool SkipThisRoute(int result, TipoInterface tipo)
        {
            bool ret = false;
            switch (result)
            {
                case SipAgent.SIP_CONGESTION:
                    // Casos solo producidos en el caso de proxy
                case SipAgent.SIP_ERROR:
                case SipAgent.SIP_SERVER_TIMEOUT:
                    ret = true;
                    break;
                default:
                    break;
            }
            return ret;
        }
        //Equivale a Fuera de Servicio:
        //debe saltar todas las rutas preferentes
        //e ir por la ruta desvio o backup
        private bool SkipAllRoutes(int result, TipoInterface tipo)
        {
            bool ret = false;
            switch (result)
            {
                case SipAgent.SIP_NOT_FOUND:
                    ret = true;
                    break;
                default:
                    break;
            }
            return ret;
        }
		#region Private Members

		private List<RsIdxType> _RsTypes;
		private List<int> _Routes;

		#endregion
	}
}
