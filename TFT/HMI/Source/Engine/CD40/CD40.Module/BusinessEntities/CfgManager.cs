using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;

using HMI.Model.Module.BusinessEntities;
using HMI.CD40.Module.Properties;

using U5ki.Infrastructure;
using Utilities;
using NLog;
namespace HMI.CD40.Module.BusinessEntities
{
    class RsIdxType
    {
        public int indexToLine;
        public int ruta;
        public TipoInterface TipoInteface;

        public RsIdxType(int index, int r, TipoInterface tipo)
        {
            indexToLine = index;
            ruta = r;
            TipoInteface = tipo;
        }
    }

	class TlfNet
	{
		public string Id;
		public List<SipLine> Lines = new List<SipLine>();
		public List<RsIdxType> RsTypes = new List<RsIdxType>();
		public List<int> Routes = new List<int>();
	}

	class CfgManager
	{
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private string STR_SECTOR_FS = "**FS**";
        private string STR_PUESTO_FS = "__FS__";

		public event GenericEventHandler ConfigChanged;

		public string MainId
		{
			get 
            {
                if (_UserCfg != null && _UserCfg.User.IdIdentificador == STR_SECTOR_FS)
                    return STR_PUESTO_FS;
                    
                return _UserCfg != null ? _UserCfg.User.IdIdentificador : Top.HostId; 
            }
		}

		public string PositionId
		{
			get { return _UserCfg != null ? _UserCfg.User.Nombre : Top.HostId; }
		}

		public uint NumFrecByPage
		{
			get { return _UserCfg != null ? _UserCfg.User.ParametrosDelSector.NumFrecPagina : 0; }
		}

		public Permissions Permissions
		{
			get
			{
				Permissions p = Permissions.None;

				if (_UserCfg != null)
				{
					if (_UserCfg.User.TeclasDelSector.TeclaPrioridad)
					{
						p |= Permissions.Priority;
					}
					if (_UserCfg.User.TeclasDelSector.Escucha)
					{
						p |= Permissions.Listen;
					}
					if (_UserCfg.User.TeclasDelSector.Retener)
					{
						p |= Permissions.Hold;
					}
					if (_UserCfg.User.TeclasDelSector.TransDirecta)
					{
						p |= Permissions.Transfer;
					}
					if (_UserCfg.User.ParametrosDelSector.Intruido)
					{
						p |= Permissions.Intruded;
					}
                    if (_UserCfg.User.TeclasDelSector.Conferencia)
                    {
                        p |= Permissions.Conference;
                    }
                    if (_UserCfg.User.TeclasDelSector.Glp)
                    {
                        p |= Permissions.Replay;
                    }
                }

				return p;
			}
		}

		public uint Priority
		{
			get { return _UserCfg != null ? _UserCfg.User.Sector.PrioridadR2 : (uint)CORESIP_Priority.CORESIP_PR_NONURGENT; }
		}

		public IEnumerable<StrNumeroAbonado> HostAddresses
		{
			get { return _HostAddresses; }
		}

		public IEnumerable<CfgEnlaceInterno> TlfLinks
		{
			get
			{
				if (_UserCfg != null)
				{
					foreach (CfgEnlaceInterno link in _UserCfg.TlfLinks)
					{
						if (link.TipoEnlaceInterno == "DA")
						{
							yield return link;
						}
					}
				}
			}
		}

		public IEnumerable<CfgEnlaceInterno> AgLinks
		{
			get
			{
				if (_UserCfg != null)
				{
					foreach (CfgEnlaceInterno link in _UserCfg.TlfLinks)
					{
						if (link.TipoEnlaceInterno == "AG")
						{
							yield return link;
						}
					}
				}
			}
		}

		public IEnumerable<CfgEnlaceInterno> LcLinks
		{
			get
			{
				if (_UserCfg != null)
				{
					foreach (CfgEnlaceInterno link in _UserCfg.TlfLinks)
					{
						if (link.TipoEnlaceInterno == "IA")
						{
							yield return link;
						}
					}
				}
			}
		}

		public IEnumerable<CfgEnlaceExterno> RdLinks
		{
			get
			{
				if (_UserCfg != null)
				{
					return _UserCfg.RdLinks;
				}

				return new List<CfgEnlaceExterno>();
			}
		}

		public IEnumerable<PermisosRedesSCV> PermisosRedes
		{
			get
			{
				if (_UserCfg != null)
				{
					return _UserCfg.User.PermisosRedDelSector;
				}

				return new List<PermisosRedesSCV>();
			}
		}

		public IEnumerable<ListaRedes> ListRedes
		{
			get
			{
				if (_SystemCfg != null)
				{
					return _SystemCfg.PlanRedes;
				}

				return new List<ListaRedes>();
			}
		}

		public bool ResetUsuario
		{
			get { return _ResetUsuario; }
		}

        public bool SoyPrivilegiado
        {
            get
            {
                ulong result;
                foreach (StrNumeroAbonado host in _HostAddresses)
                {
                    if (ulong.TryParse(host.NumeroAbonado, out result))
                      if (_MiScv.IsPrivilegiado(result))
                        return true;
                }
                return false;
            }
        }
		public void Init()
		{
			Top.Registry.NewConfig += OnNewConfig;

			_HostAddresses = new List<StrNumeroAbonado>();
		}

		public void Start()
		{
		}

		public void End()
		{
		}

		public string GetHostIp(string hostId)
		{
			foreach (DireccionamientoIP host in _SystemCfg.PlanDireccionamientoIP)
			{
				if (string.Compare(host.IdHost, hostId, true) == 0)
				{					
                    return host.IpRed1;
				}
			}

			return null;
		}

		public string GetHostMainUser(string host)
		{
			foreach (AsignacionUsuariosDominantesTV tv in _SystemCfg.PlanAsignacionUsuariosDominantes)
			{
				if (string.Compare(tv.IdHost, host, true) == 0)
				{
                    return tv.IdUsuario;
				}
			}

			return null;
		}

        public string GetGwRsIp(string resourceId, out string gw)
		{
			gw = GetGwRsHost(resourceId);
			if (gw != null)
			{
				return GetHostIp(gw);
			}

			return null;
		}

        // Devuelve la dirección del Proxy que está activa
        public string GetProxyIpAddress(out string idEquipo)
        {
            idEquipo = string.Empty;

            if (_MiScv != null)
                return _MiScv.GetProxyIpAddress(out idEquipo);

            return null;
        }

        // Devuelve en end point del Proxy que está activa
        public string GetProxyIp(out string idEquipo)
        {
            idEquipo = string.Empty;

            if (_MiScv != null)
                return _MiScv.GetProxyIp(out idEquipo);

            return null;
        }

        //Devuelve la dirección del proxy activo del SCV que contiene el número en sus rangos.
        private string GetExtScvIp(ulong num, out string idEquipo)
        {
            idEquipo = null;

            foreach (Scv elem in _OtrosScv.Values)
            {
                if (elem.IsInRangeScv(num))
                {
                    return elem.GetProxyIp(out idEquipo);
                }
            }

            return null;
        }

        /// <summary>
        /// Devuelve true si encuentra un equipo/pasarela en la configuracion con esa IP
        /// false en caso contrario. 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public bool BelongsToMyConfig(U5ki.Infrastructure.Tipo_Elemento_HW tipo, string ip, out string equipo)
        {
            equipo = null;
            foreach (DireccionamientoIP host in _SystemCfg.PlanDireccionamientoIP)
            {
                if ((tipo == host.TipoHost) && (string.Compare(host.IpRed1, ip, true) == 0))
                {
                    equipo = host.IdHost;
                    return true;
                }
            }
            return false;
        }

		public DireccionamientoIP GetGwRsHostInfo(string resourceId)
		{
			foreach (AsignacionRecursosGW gw in _SystemCfg.PlanAsignacionRecursos)
			{
				if (string.Compare(gw.IdRecurso, resourceId, true) == 0)
				{
					foreach (DireccionamientoIP host in _SystemCfg.PlanDireccionamientoIP)
					{
						if (string.Compare(gw.IdHost, host.IdHost, true) == 0)
						{
							return host;
						}
					}
				}
			}

			return null;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">es el user_name de la uri</param>
        /// <param name="ip">dominio o ip de la uri</param>
        /// <param name="subId"></param>
        /// <param name="rsId">campo rs que viene en la uri</param>
        /// <returns>null si no ha encontrado el recurso configurado</returns>
		public CfgRecursoEnlaceInterno GetResourceFromUri(string id, string ip, string subId, string rsId)
		{
            CfgRecursoEnlaceInterno recurso = null;

			if (!string.IsNullOrEmpty(subId))
			{
                recurso = new CfgRecursoEnlaceInterno();
				recurso.Prefijo = Cd40Cfg.ATS_DST;
				recurso.NumeroAbonado = subId;
			}

            //recurso = lookForResourceInScv(id, ip);
            if (recurso == null)
            {
                foreach (DireccionamientoIP host in _SystemCfg.PlanDireccionamientoIP)
                {
                    if (recurso != null)
                        break;
                    if (host.TipoHost == Tipo_Elemento_HW.TEH_TOP)
                    {
                        recurso = lookForResourceInTop(id);
                    }
                    else if ((recurso == null) &&
                        (host.TipoHost == Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA))
                    {
                        recurso = lookForResourceExternal(id, ip, host);
                    }
                    else if ((recurso == null) && (host.TipoHost == Tipo_Elemento_HW.TEH_TIFX))
                    {
                        //recurso de pasarela
                        if (host.IpRed1 == ip)
                            recurso = lookForResourceGw(id, rsId, host);
                    }
                }
            }
			return recurso;
		}

		public string GetUserHost(string userId)
		{
			foreach (AsignacionUsuariosTV tv in _SystemCfg.PlanAsignacionUsuarios)
			{
				if (string.Compare(tv.IdUsuario, userId, true) == 0)
				{
					return tv.IdHost;
				}
			}

			return null;
		}

        public AsignacionUsuariosTV GetUserTv(string userId)
        {
            foreach (AsignacionUsuariosTV tv in _SystemCfg.PlanAsignacionUsuarios)
            {
                if (string.Compare(tv.IdUsuario, userId, true) == 0)
                {
                    return tv;
                }
            }

            return null;
        }

		public List<StrNumeroAbonado> GetHostAddresses(string host)
		{
			List<StrNumeroAbonado> addresses = new List<StrNumeroAbonado>();

			foreach (AsignacionUsuariosTV tv in _SystemCfg.PlanAsignacionUsuarios)
			{
				if (string.Compare(tv.IdHost, host, true) == 0)
				{
                    /* 
                     * JCAM 06/10/2015
                     * Se elimina de la lista los identificadores de usuario
                     * para evitar que se registren en el proxy con el 
                     * identificador y con el número de abonado
					StrNumeroAbonado num = new StrNumeroAbonado();

					num.Prefijo = 2;
					num.NumeroAbonado = tv.IdUsuario;

					addresses.Add(num);
                     */
					addresses.AddRange(GetNumerosAbonado(tv.IdUsuario));
				}
			}

			return addresses;
		}

		public string GetNumeroAbonado(string userId, uint prefix)
		{
			List<StrNumeroAbonado> nums = GetNumerosAbonado(userId);

			foreach (StrNumeroAbonado num in nums)
			{
				if (num.Prefijo == prefix)
				{
					return num.NumeroAbonado;
				}
			}

			return null;
		}

		public string[] GetUserFromAddress(uint prefix, string address)
		{
			foreach (DireccionamientoSIP sip in _SystemCfg.PlanDireccionamientoSIP)
			{
				foreach (StrNumeroAbonado num in sip.NumerosAbonadoQueAtiende)
				{
                    if ((num.Prefijo == prefix) && (string.Compare(num.NumeroAbonado, address, true) == 0))
                    {
                        return new string[] { num.IdAgrupacion, sip.IdUsuario };
                    }
				}
			}

            return null;
		}

		public TlfNet GetNet(uint prefix, string number, ref StrNumeroAbonado altRoute)
		{
            string idEquipo;
            if (prefix == Cd40Cfg.ATS_DST)
			{
				ulong num;

				if (ulong.TryParse(number, out num))
				{
					int centralId = -1;

					foreach (NumeracionATS centralAts in _SystemCfg.PlanNumeracionATS)
					{
						bool foundCentralAts = false;
						centralId++;
                       
                        foreach (RangosSCV range in centralAts.RangosOperador)
						{
							if ((range.Inicial <= num) && (range.Final >= num))
							{
								if (!string.IsNullOrEmpty(range.IdAbonado))
								{
									altRoute = new StrNumeroAbonado();
									altRoute.Prefijo = range.IdPrefijo;
									altRoute.NumeroAbonado = range.IdAbonado;
								}

								foundCentralAts = true;
								break;
							}
						}

						if (!foundCentralAts)
						{
							foreach (RangosSCV range in centralAts.RangosPrivilegiados)
							{
								if ((range.Inicial <= num) && (range.Final >= num))
								{
									if (!string.IsNullOrEmpty(range.IdAbonado))
									{
										altRoute = new StrNumeroAbonado();
										altRoute.Prefijo = range.IdPrefijo;
										altRoute.NumeroAbonado = range.IdAbonado;
									}

									foundCentralAts = true;
									break;
								}
							}
						}

                        if (foundCentralAts)
                        {
                            int route = 0;
                            TlfNet net = new TlfNet();
                            net.Id = "Net_ATS_" + centralId;

                            foreach (PlanRutas ruta in centralAts.ListaRutas)
                            {
                                // Primero van las rutas directas y luego el resto
                                route = (ruta.TipoRuta == "D") ? route : ++route;

                                foreach (string trunk in ruta.ListaTroncales)
                                {
                                    foreach (PlanRecursos recurso in GetTrunkResources(trunk))
                                    {
                                        Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(recurso.IdRecurso);
                                        string rsIp = GetGwRsIp(recurso.IdRecurso, out idEquipo);
                                        SipLine line = new SipLine(rs, rsIp);

                                        net.Lines.Add(line);
                                        net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, route, recurso.Tipo));
                                        net.Routes.Add(route);
                                    }
                                }
                            }

                            /*
                             * 
                             *  Los recursos dentro de un troncal ya no se ordenan primero los de R2 
                             *  y luego los de N-5 (tal y como se hizo para ACC). Se ordenan primero lado A
                             *  y luego lado B (Según Encamina (02/06) ). 
                             *  Incidencia #2521
                            net.RsTypes.Sort(delegate(RsIdxType a, RsIdxType b)
                            {
                                if (a.ruta == b.ruta)
                                    return a.TipoInteface - b.TipoInteface;
                                if (a.ruta < b.ruta)
                                    return -1;

                                return 1;
                            });
                            */
                            return net;
                        }
					}
                    //Devuelve una red vacía si no lo encuentra en la configuración
                    _Logger.Info("Linea no encontrada para prefijo {0} {1}", prefix, number);
                    TlfNet emptyNet = new TlfNet();
                    emptyNet.Id = "Net_ATS_" + centralId;
                    return emptyNet;
				}
			}
            // Por acceso indirecto, el prefijo PP
            // se comporta como un abonado con marcación
            else if (prefix == Cd40Cfg.PP_DST)   
            {
                TlfNet net = new TlfNet();
                Rs<GwTlfRs> rs = null;
                net.Id = "Net_" + "SEGURIDAD";
                //Busca el equipo que le da servicio, pasarela o proxy
                string rsIp = GetGwRsIp(number, out idEquipo);
                if (idEquipo != null)
                    rs = Top.Registry.GetRs<GwTlfRs>(number);
                if (rs == null)
                {
                    //o nuestro proxy si es un teléfono IP no configurado
                    if ((rsIp == null) || (idEquipo == null))
                    {
                        rsIp = GetProxyIp(out idEquipo);
                        rs = Top.Registry.GetRs<GwTlfRs>(idEquipo);
                    }
                    _Logger.Warn("Number not found in configurated resources {0}, use {1} instead", number, idEquipo);
                }
                //rs.Reset(null, new GwTlfRs());
                if (rs != null)
                {
                    _Logger.Debug("Resource found for {0}: {1}", number, rs.Id);

                    SipLine line = new SipLine(rs, rsIp);
                    net.Lines.Add(line);
                    net.Routes.Add(0);
                    net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, 0, TipoInterface.TI_BL));
                }
                else
                    _Logger.Error("{0} not found in conf resources {0} and proxy not found either.", number, idEquipo);
                return net;
            }
            else if (prefix == Cd40Cfg.UNKNOWN_DST) //Destinos entrantes que no están configurados
            {
                TlfNet net = new TlfNet();
                net.Id = "Net_" + "SEGURIDAD";
                string rsIp = GetProxyIp(out idEquipo);
                if (rsIp != null)
                {
                    Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(idEquipo);
                    GwTlfRs proxyRs = new GwTlfRs();
                    proxyRs.GwIp = rsIp;
                    rs.Reset(null, proxyRs);

                    SipLine line = new SipLine(rs, rsIp);
                    net.Lines.Add(line);
                    net.Routes.Add(0);
                    net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, 0, TipoInterface.TI_BL));

                    return net;
                }
            }
           else if (prefix == Cd40Cfg.IP_DST)
            {
                TlfNet net = new TlfNet();
                net.Id = "Net_" + "SEGURIDAD";
                string rsIp = GetProxyIp(out idEquipo);
                if (rsIp != null)
                {
                    Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(idEquipo);
                    GwTlfRs proxyRs = new GwTlfRs();
                    proxyRs.GwIp = rsIp;
                    rs.Reset(null, proxyRs);

                    SipLine line = new SipLine(rs, rsIp);
                    net.Lines.Add(line);
                    net.Routes.Add(0);
                    net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, 0, TipoInterface.TI_BL));

                    return net;
                }
            }
            else
            {
                foreach (ListaRedes red in _SystemCfg.PlanRedes)
                {
                    if (red.Prefijo == prefix)
                    {
                        TlfNet net = new TlfNet();
                        net.Id = "Net_" + red.IdRed;

                        foreach (PlanRecursos recurso in red.ListaRecursos)
                        {
                            string rsIp;
                            Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(recurso.IdRecurso);
                            rsIp = GetGwRsIp(recurso.IdRecurso, out idEquipo);
                            SipLine line = new SipLine(rs, rsIp);

                            net.Lines.Add(line);
                            net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, altRoute != null ? 1000 : 0, TipoInterface.TI_AB));
                            // Si altRoute es distinto de null es que estamos obteniendo la red
                            // de salida alternativa para un numero ATS. Esta red sólo se tiene
                            // que usar cuando no podemos salir por la propia red ATS, por eso
                            // ponemos un valor elevado de ruta de modo que se coja en GetDetourPath
                            net.Routes.Add(altRoute != null ? 1000 : 0);
                        }

                        return net;
                    }
                }
            }

			return null;
		}

        public TlfNet GetIPNet(uint prefix, string number)
        {
            TlfNet net = null;
            if (prefix == Cd40Cfg.ATS_DST)
            {
                ulong num;
                if (ulong.TryParse(number, out num))
                    // Se comprueba si existe un equipo externo con capacidad
                    // para dar servicio a números ATS, un encaminamiento tipo central IP
                    foreach (Scv scv in _OtrosScv.Values)
                    if (scv.EsCentralIp && scv.IsInRangeScv(num))
                    {
                        net = new TlfNet();
                        net.Id = "Net_ATS_IP_"+scv.Id;

                        Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(number);
                        string ScvIp;
                        if (rs.Content == null)
                        {
                            //Si el recurso no tiene contenido actualizado, lo creo con los datos de configuración (principal)
                            GwTlfRs proxyRs = new GwTlfRs();
                            string id;
                            proxyRs.GwIp = ScvIp = scv.GetProxyIp(out id);
                            rs.Reset(null, proxyRs);
                        }
                        else
                            //Utilizo los datos del recurso (actualizado con el activo)
                            ScvIp = ((GwTlfRs)rs.Content).GwIp;
                        SipLine line = new SipLine(rs, ScvIp, true);
                        net.Lines.Add(line);
                        net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, 0, TipoInterface.TI_IP_PROXY));
                        net.Routes.Add(0);
                    }
            }
            return net;
        }
		public bool ExistNet(uint prefix, string number)
		{
            if (prefix == Cd40Cfg.ATS_DST)
			{
				ulong num;

				if (ulong.TryParse(number, out num))
				{
                    // Comprobar si existe un equipo externo telefónico con 
                    // rango de llamados válido para 'number'
                    string idEquipo;
                    GetExtScvIp(num, out idEquipo);
                    if (idEquipo != null)
                        return true;
                    //Este bucle se mantiene para las búsquedas de centrales noIP
					foreach (NumeracionATS centralAts in _SystemCfg.PlanNumeracionATS)
					{
						foreach (RangosSCV range in centralAts.RangosOperador)
						{
							if ((range.Inicial <= num) && (range.Final >= num))
							{
								return true;
							}
						}

						foreach (RangosSCV range in centralAts.RangosPrivilegiados)
						{
							if ((range.Inicial <= num) && (range.Final >= num))
							{
								return true;
							}
						}
					}
				}
			}
			else
			{
				foreach (ListaRedes red in _SystemCfg.PlanRedes)
				{
					if (red.Prefijo == prefix)
					{
						return true;
					}
				}
			}

			return false;
		}

        public bool SitesConfiguration()
        {
            /** 20180315. AGL. Cuando desaparecia el PICT de la config, _UserCfg es null, 
             en estas circustancias esta funcion daba una excepcion y no progresaba el limpiado
             de los recursos en PICT. */
            return  _UserCfg == null ? false : _UserCfg.User.TeclasDelSector.LTT;
        }

        public CfgRecursoEnlaceInterno GetATSResourceFromUri(string id)
        {
            CfgRecursoEnlaceInterno recurso = null;
            //Vemos si es un destino ATS externo que viene de tránsito por un SCV no IP
            if (ExistNet(Cd40Cfg.ATS_DST, id))
            {
                recurso = new CfgRecursoEnlaceInterno();
                recurso.Prefijo = Cd40Cfg.ATS_DST;
                recurso.NumeroAbonado = id;
            }
            return recurso;
        }

		#region Private Members

		private bool _ResetUsuario;
		private ConfiguracionSistema _SystemCfg;
		private ConfiguracionUsuario _UserCfg;
        private List<PoolHfElement> _PoolHf;
		private List<StrNumeroAbonado> _HostAddresses;
        private Scv _MiScv;
        // La key es el id del Scv, para facilitar las búsquedas
        private Dictionary<string, Scv> _OtrosScv = new Dictionary<string,Scv>();

		private void OnNewConfig(object sender, Cd40Cfg cfg)
		{
			string _idIdenticador = MainId;

			if (_UserCfg != null)
			{
				_SystemCfg = cfg.ConfiguracionGeneral;
				_UserCfg = GetUserCfg(cfg);
                _PoolHf = cfg.PoolHf;

				_ResetUsuario = MainId != _idIdenticador;
			}
			else
			{
				_SystemCfg = cfg.ConfiguracionGeneral;
				_UserCfg = GetUserCfg(cfg);
                _PoolHf = cfg.PoolHf;
            }

            /** 20180315. AGL. Si _UserCfg es null, es porque el ID HOST configurado no forma parte de
             la configuracion de BDT, aunque si de la configuracion de SPREAD. */
            if (_UserCfg == null)
            {
                _HostAddresses = GetHostAddresses(Top.HostId);
                ClearChildrenObjects();
            }
            else
            {
                _HostAddresses = GetHostAddresses(Top.HostId);
                CreateChildrenObjects();                
            }
            General.SafeLaunchEvent(ConfigChanged, this);
        }

        /// <summary>
        /// Crea los objetos SCV en base a los nuevos datos de configuración recibidos
        /// </summary>
        private void CreateChildrenObjects()
        {
            //delete old SCVs
            _MiScv = null;
            _OtrosScv.Clear();

            //create SCVs
            CreateScvs();

        }

        private void CreateScvs()
        {
            //delete old SCVs
            _MiScv = null;
            _OtrosScv.Clear();
            foreach (NumeracionATS scvAts in _SystemCfg.PlanNumeracionATS)
            {
                if (scvAts.CentralPropia)
                    _MiScv = new Scv(scvAts);
                    //Esta protección es porque pueden llegar de configuración SCV sin rango y sin nombre
                else if (scvAts.Central.Length > 0)
                   _OtrosScv.Add(scvAts.Central, new Scv(scvAts));
            }

            foreach (DireccionamientoIP obj in _SystemCfg.PlanDireccionamientoIP)
            {
                if ((obj.TipoHost == Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA) && obj.EsCentralIP)
                {
                    if (obj.Interno)
                    {
                        if (_MiScv == null)
                            _MiScv = new Scv(obj);  // No debería entrar aquí
                        else
                            _MiScv.SetIpData(obj);
                    }
                    else
                    {
                        Scv outScv;
                        if (!_OtrosScv.TryGetValue(obj.IdHost, out outScv))
                            _OtrosScv.Add(obj.IdHost, new Scv(obj)); // No debería entrar aquí
                        else
                            outScv.SetIpData(obj);
                    }
                }
            }
        }

        private void ClearChildrenObjects()
        {
            _MiScv = null;
            _OtrosScv.Clear();
        }
		
        private ConfiguracionUsuario GetUserCfg(Cd40Cfg cfg)
		{
			string idUser = GetHostMainUser(Top.HostId);

			if (!string.IsNullOrEmpty(idUser))
			{
				foreach (ConfiguracionUsuario conf in cfg.ConfiguracionUsuarios)
				{
					if (conf.User.IdIdentificador == idUser)
					{
                        if (conf.User.IdIdentificador == STR_SECTOR_FS)
                        {
                            conf.User.Nombre = conf.User.IdIdentificador = STR_PUESTO_FS;
                        }
                        
                        return conf;
					}
				}
			}

			return null;
		}

        /// <summary>
        /// Dado el ID (nombre lógico) devuelve todos los numeros de abonado
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
		private List<StrNumeroAbonado> GetNumerosAbonado(string userId)
		{
			foreach (DireccionamientoSIP sip in _SystemCfg.PlanDireccionamientoSIP)
			{
				if (string.Compare(sip.IdUsuario, userId, true) == 0)
				{
					return sip.NumerosAbonadoQueAtiende;
				}
			}

			return new List<StrNumeroAbonado>();
		}

		private string GetGwRsHost(string resourceId)
		{
			foreach (AsignacionRecursosGW gw in _SystemCfg.PlanAsignacionRecursos)
			{
				if (string.Compare(gw.IdRecurso, resourceId, true) == 0)
				{
					return gw.IdHost;
				}
			}

			return null;
		}

		private List<PlanRecursos> GetTrunkResources(string trunkId)
		{
			foreach (ListaTroncales trunk in _SystemCfg.PlanTroncales)
			{
				if (string.Compare(trunk.IdTroncal, trunkId, true) == 0)
				{
					return trunk.ListaRecursos;
				}
			}

			return new List<PlanRecursos>();
		}

        /// <summary>
        /// Dado un user name y la ip (procedentes de una URI por ejemplo)
        /// la busca entre los Tops.
        /// Si no lo encuentra, devuelve null.
        /// </summary>
        /// <param name="id"></param>
       /// <returns></returns>
        private CfgRecursoEnlaceInterno lookForResourceInTop(string id)
        {
            CfgRecursoEnlaceInterno recurso = new CfgRecursoEnlaceInterno();
            recurso.Prefijo = Cd40Cfg.INT_DST;

            foreach (DireccionamientoSIP sip in _SystemCfg.PlanDireccionamientoSIP)
            {
                //Lo busca por nombre
                if (string.Compare(sip.IdUsuario, id, true) == 0)
                {
                    recurso.NombreRecurso = sip.IdUsuario;
                    recurso.NumeroAbonado = id;
                    return recurso;
                }
                //y si no, lo busca por nummero al que atiende
                foreach (StrNumeroAbonado number in sip.NumerosAbonadoQueAtiende)
                {
                    if (string.Compare(number.NumeroAbonado, id, true) == 0)
                    {
                         recurso.NombreRecurso = sip.IdUsuario;
                         recurso.NumeroAbonado = id;
                         return recurso;
                    }
                }
            }
            return null;

        }
        /// <summary>
        /// Dado un user name y una ip, busca el recurso entre los SCV IP
        /// </summary>
        /// <param name="id"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        private CfgRecursoEnlaceInterno lookForResourceInScv(string id, string ip)
        {
            string idEquipo;
            CfgRecursoEnlaceInterno recurso = null;
            ulong resultNumber = 0;

            if (SipUtilities.SipEndPoint.EqualSipEndPoint(_MiScv.GetProxyIp(out idEquipo),ip))
            //Pertenece a mi SCV
            {
                //Es un TOP o un recurso de telefonía IP interno
                recurso = new CfgRecursoEnlaceInterno();
                recurso.Prefijo = Cd40Cfg.PP_DST;
                recurso.NombreRecurso = id;
                recurso.NumeroAbonado = id; //string.Format("{0}@{1}", id, host.IpRed1);
            }
            else
            {
                foreach (Scv scv in _OtrosScv.Values)
                {
                    string ipAddScv = scv.GetProxyIp(out idEquipo);
                    if ((scv.EsCentralIp) && (SipUtilities.SipEndPoint.EqualSipEndPoint(ipAddScv, ip)))
                    //Pertenece a otro SCV IP
                    {
                        try
                        {
                            resultNumber = Convert.ToUInt64(id);
                            if (scv.IsInRangeScv(resultNumber))
                            {
                                recurso = new CfgRecursoEnlaceInterno();
                                recurso.Prefijo = Cd40Cfg.ATS_DST;
                                recurso.NumeroAbonado = id;
                                recurso.NombreRecurso = string.Format("{0}@{1}", id, ipAddScv);
                                break;
                            }
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            return recurso;
        }
        /// <summary>
        /// Dado un user name y una ip, busca el recurso entre los equipos externos.
        /// Es el caso de los recursos ATS de encaminamientos IP
        /// Si no lo encuentra, devuelve null.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="host"></param>
        /// <returns></returns>
        private CfgRecursoEnlaceInterno lookForResourceExternal(string id, string ip, DireccionamientoIP host)
        {
            long resultNumber = 0;
            CfgRecursoEnlaceInterno recurso = null;
            if (host.Interno)
            {
                if (SipUtilities.SipEndPoint.EqualSipEndPoint(ip, host.IpRed1))
                //Pertenece a la centralita interna
                {
                    recurso = new CfgRecursoEnlaceInterno();
                    recurso.Prefijo = Cd40Cfg.PP_DST;
                    recurso.NombreRecurso = id;
                    recurso.NumeroAbonado = id; //string.Format("{0}@{1}", id, host.IpRed1);
                }
            }
            else
            {
                if (host.EsCentralIP)
                {
                    try
                    {
                        resultNumber = Convert.ToInt64(id);
                        if ((resultNumber >= host.Min) && (resultNumber <= host.Max))
                        {
                            recurso = new CfgRecursoEnlaceInterno();
                            recurso.Prefijo = Cd40Cfg.ATS_DST;
                            recurso.NumeroAbonado = id;
                            recurso.NombreRecurso = string.Format("{0}@{1}", id, host.IpRed1);
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
                else if (SipUtilities.SipEndPoint.EqualSipEndPoint(ip, host.IpRed1))
                {
                    //Este es el caso de la telefonía de equipos externos a una PABX externa 
                    string nombreRecurso = null;
                    foreach (AsignacionRecursosGW gw in _SystemCfg.PlanAsignacionRecursos)
                    {
                        if (string.Compare(gw.IdHost, host.IdHost, true) == 0)
                        {
                            nombreRecurso = gw.IdRecurso;
                        }
                    }
                    if (nombreRecurso != null)
                    {
                        foreach (ListaRedes red in _SystemCfg.PlanRedes)
                        {
                            foreach (PlanRecursos recInPlan in red.ListaRecursos)
                            {
                                if (string.Compare(recInPlan.IdRecurso, nombreRecurso, true) == 0)
                                {
                                    recurso = new CfgRecursoEnlaceInterno();
                                    recurso.Prefijo = red.Prefijo;
                                    recurso.NumeroAbonado = id;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
 
            return recurso;
        }

        private CfgRecursoEnlaceInterno lookForResourceGw(string id, string rsId, DireccionamientoIP host)
        {
            CfgRecursoEnlaceInterno recurso = null;
            string foundRsId = null;
            foreach (AsignacionRecursosGW gw in _SystemCfg.PlanAsignacionRecursos)
            {
                if (string.Compare(gw.IdHost, host.IdHost, true) == 0)
                {
                    if ((string.Compare(gw.IdRecurso, id, true) == 0) || (string.Compare(gw.IdRecurso, rsId, true) == 0))
                    {
                        foundRsId = gw.IdRecurso;
                        break;
                    }
                }
            }

            //if (string.Compare(id, rsId, true) != 0)
            // Si no entra a mirar si el recurso pertenece a una red, las llamadas entrantes
            // distintas de la red RTB no entran en el puesto.
            if (string.IsNullOrEmpty(foundRsId) == false)
            {
                foreach (ListaRedes red in _SystemCfg.PlanRedes)
                {
                    foreach (PlanRecursos recInPlan in red.ListaRecursos)
                    {
                        if (string.Compare(recInPlan.IdRecurso, foundRsId, true) == 0)
                        {
                            recurso = new CfgRecursoEnlaceInterno();
                            recurso.Prefijo = red.Prefijo;
                            recurso.NumeroAbonado = id;
                            break;
                        }
                    }
                }

                foreach (NumeracionATS centralAts in _SystemCfg.PlanNumeracionATS)
                {
                    foreach (PlanRutas ruta in centralAts.ListaRutas)
                    {
                        foreach (string trunk in ruta.ListaTroncales)
                        {
                            foreach (PlanRecursos recInPlan in GetTrunkResources(trunk))
                            {
                                if (string.Compare(recInPlan.IdRecurso, foundRsId, true) == 0)
                                {
                                    recurso = new CfgRecursoEnlaceInterno();
                                    recurso.Prefijo = Cd40Cfg.ATS_DST;
                                    recurso.NumeroAbonado = id;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if ((recurso == null) && (foundRsId != null))
            {
                recurso = new CfgRecursoEnlaceInterno();
                recurso.Prefijo = Cd40Cfg.PP_DST;
                recurso.NombreRecurso = foundRsId;
                recurso.NumeroAbonado = foundRsId;
                recurso.Interface = TipoInterface.TI_AB;
            }
            return recurso;
        }

		#endregion
	}
}
