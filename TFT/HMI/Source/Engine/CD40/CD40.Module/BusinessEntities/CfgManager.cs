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
#if DEBUG
    public class RsIdxType
#else
    class RsIdxType
#endif
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

#if DEBUG
    public class TlfNet
#else
	class TlfNet
#endif
    {
		public string Id;
		public List<SipLine> Lines = new List<SipLine>();
		public List<RsIdxType> RsTypes = new List<RsIdxType>();
		public List<int> Routes = new List<int>();
	}

#if DEBUG
    public class OperatorData
#else
	class OperatorData
#endif
    {
        /// <summary>
        /// PICT host name
        /// </summary>
        public string idHost;   
        /// <summary>
        /// sector name having in account if it's a group of sectors
        /// </summary>
        public string idGroupName;
        /// <summary>
        /// data of each sector
        /// </summary>
        public List <SectorData> sectorData = new List<SectorData> ();
    }

    public class SectorData
    {
        public string idUser;
        public List<CfgRecursoEnlaceInterno> numberData = new List<CfgRecursoEnlaceInterno>();
    }
#if DEBUG
    public class CfgManager
#else
	class CfgManager
#endif
    {
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        private string STR_SECTOR_FS = "**FS**";
        private string STR_PUESTO_FS = "__FS__";

		public event GenericEventHandler ConfigChanged;
        public event GenericEventHandler<bool> ProxyStateChangeCfg;
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
                    if (_UserCfg.User.TeclasDelSector.Captura)
                        p |= Permissions.Capture;
                    if (_UserCfg.User.TeclasDelSector.Redireccion)
                        p |= Permissions.Forward;
                    if (_UserCfg.User.TeclasDelSector.RepeticionUltLlamada)
                        p |= Permissions.ReplayOnlyRadio;
                    if (_UserCfg.User.TeclasDelSector.PermisoRTXSQ )//RQF36  //deberia estar en cd40cfg.proto.cs
                        p |= Permissions.PermisoRTXSQ;
                    if (_UserCfg.User.TeclasDelSector.PermisoRTXSect )//RQF36 //deberia estar en cd40cfg.proto.cs
                        p |= Permissions.PermisoRTXSect;
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
        public IEnumerable<CfgEnlaceInterno> MdTlfLinksPropios
        {
            get
            {
                if (_UserCfg != null)
                {
                    foreach (CfgEnlaceInterno link in _UserCfg.TlfLinks)
                    {
                        if (link.TipoEnlaceInterno == "MD" && link.Dominio == "PROPIO")
                        {
                            yield return link;
                        }
                    }
                }
            }
        }
        public IEnumerable<CfgEnlaceInterno> MdTlfLinksAjeno
        {
            get
            {
                if (_UserCfg != null)
                {
                    foreach (CfgEnlaceInterno link in _UserCfg.TlfLinks)
                    {
                        if (link.TipoEnlaceInterno == "MD" && link.Dominio == "AJENO")
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
            //LALM 210507 Comprobacion necesaria.
            if (Top.Registry!=null)
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
            return _SystemCfg.GetHostIp(hostId);
				}

        public string GetMainUser(string host)
		{
            return _SystemCfg.GetMainUser(host);
				}

        public string GetGwRsIp(string resourceId, out string gw)
		{
            return _SystemCfg.GetGwRsIp(resourceId, out gw);
        }

        // Devuelve la dirección del Proxy que está activa
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

        public string GetUserHost(string name)
        {
            foreach (OperatorData oper in _Operators)
            {
                if (string.Compare(oper.idGroupName, name, true) == 0)
                     return oper.idHost;
                foreach (SectorData sector in oper.sectorData)
                {
                    if (string.Compare (sector.idUser, name, true) == 0)
                        return oper.idHost;
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

        public AsignacionUsuariosTV GetHostTv(string IdHost)
        {
            foreach (AsignacionUsuariosTV tv in _SystemCfg.PlanAsignacionUsuarios)
            {
                if (string.Compare(tv.IdHost, IdHost, true) == 0)
                {
                    return tv;
                }
            }

            return null;
        }


        public List<StrNumeroAbonado> GetHostAddresses(string host)
		{
            return _SystemCfg.GetHostAddresses(host);
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
                                    foreach (PlanRecursos recurso in _SystemCfg.GetTrunkResources(trunk))
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
        public List<PlanRecursos> GetNetResources(uint prefix, string numero)
        {
            return _SystemCfg.GetNetResources(prefix, numero);
        }

        public TlfNet GetIPNet(uint prefix, string number)
        {
            TlfNet net = null;
            if (prefix == Cd40Cfg.ATS_DST)
            {
                ulong num;
                if (ulong.TryParse(number, out num))
                {
                    // Se comprueba si existe un equipo externo con capacidad
                    // para dar servicio a números ATS, un encaminamiento tipo central IP
                    foreach (Scv scv in _OtrosScv.Values)
                        if (scv.EsCentralIp && scv.IsInRangeScv(num))
                        {
                            net = CreateIpNet(number, scv);
                        }
                    //Puede ser el caso de una numeración que pertenece a mi SCV (y no sea un puesto de operación)
                    if ((net == null) && _MiScv.EsCentralIp && _MiScv.IsInRangeScv(num))
                    {
                        net = CreateIpNet(number, _MiScv);
                    }
                }
            }
            return net;
        }

        private static TlfNet CreateIpNet(string number, Scv scv)
        {
            TlfNet net = new TlfNet();
            net.Id = "Net_ATS_IP_" + scv.Id;

            Rs<GwTlfRs> rs = Top.Registry.GetRs<GwTlfRs>(number);
            string ScvIp;
            if (rs.Content == null)
            {
                //Si el recurso no tiene contenido actualizado, la IP es de los datos de configuración (principal)                            
                string id;
               
                ScvIp = scv.GetProxyIp(out id);
                if (rs.IsUnreferenced)
                {
                    //Para que recién creado por defecto aparezca sin aspa, le añado un recurso 
                    GwTlfRs proxyRs = new GwTlfRs();
                    proxyRs.GwIp = ScvIp;
                    rs.Reset(null, proxyRs);
                }
            }
            else
                //Utilizo los datos del recurso (actualizado con el activo)
                ScvIp = ((GwTlfRs)rs.Content).GwIp;

            //RQF-49
            //  Borro las lineas existentes
            int index1 = 0;
            if (true)
            {
                List < SipLine > listLines = net.Lines.FindAll(line1 => line1.centralIP == true);
                foreach (SipLine line1 in listLines)
                {
                    int index = net.Lines.IndexOf(line1);
                    net.Lines.RemoveAt(index);
                    net.Routes.RemoveAt(index);
                    net.RsTypes.RemoveAt(index);
                }
                string idDependencia;
                do
                {
                    ScvIp = scv.GetProxyIp(out idDependencia, index1++);
                } while (ScvIp == "" && index1 < 2);
                //index1 -= 1;
            }


            SipLine line = new SipLine(rs, ScvIp, true);
            //net.Lines.Add(line);
            //net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, 0, TipoInterface.TI_IP_PROXY));
            //net.Routes.Add(0);


            //RQF-49 habria que crear 2 adicionales  proxies o 2 recursos.
            // LALM Supongo que habria que crear una ruta por cada proxy que exista.
            for (int i=0,j=0;i<3;i++)
            {
                string id;
                ScvIp = scv.GetProxyIp(out id,i);
                if (ScvIp != "")
                {
                    line = new SipLine(rs, ScvIp, true);
                    net.Lines.Add(line);
                    net.RsTypes.Add(new RsIdxType(net.Lines.Count - 1, 0, TipoInterface.TI_IP_PROXY));
                    net.Routes.Add(0);
                    j++;
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

        // RQF36 Permisos RTX
        public bool PermisoRTXSQ()
        {
            // Esperando a que se defina.
            return _UserCfg == null ? false : _UserCfg.User.TeclasDelSector.PermisoRTXSQ;
        }

        public bool PermisoRTXSect()
        {
            // Esperando a que se defina.
            return _UserCfg == null ? false : _UserCfg.User.TeclasDelSector.PermisoRTXSect;
        }

        #region Private Members
        /// <summary>
        /// Flag que indica si el puesto de operador ha cambiado de usuario por configuración
        /// </summary>
        private bool _ResetUsuario = false;
		private ConfiguracionSistema _SystemCfg;
		private ConfiguracionUsuario _UserCfg;
        private List<PoolHfElement> _PoolHf;
		private List<StrNumeroAbonado> _HostAddresses;
        private Scv _MiScv;
        // La key es el id del Scv, para facilitar las búsquedas
        private Dictionary<string, Scv> _OtrosScv = new Dictionary<string,Scv>();
        /// <summary>
        /// Compact structure to optimize searchs of sectors o internal users.
        /// Unifies data from queries DireccionamientoSIP and AsignacionUsuarios
        /// Suitable for searches of groups of users                                                                                                                                                                                                                                                                                                                                                            º37t689
        /// </summary>
        private List<OperatorData> _Operators = new List<OperatorData>();

        private void OnProxyStateChangeCfg(object sender, bool state)
        {
            General.SafeLaunchEvent(ProxyStateChangeCfg, this, state);
        }

#if DEBUG
    public void OnNewConfig(object sender, Cd40Cfg cfg)
#else
	private void OnNewConfig(object sender, Cd40Cfg cfg)
#endif
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
                _HostAddresses = _SystemCfg.GetHostAddresses(Top.HostId);
                ClearChildrenObjects();
            }
            else
            {
                _HostAddresses = _SystemCfg.GetHostAddresses(Top.HostId);
                CreateChildrenObjects();                
            }
            CreateOperatorsData();
            //RQF-22
            CheckCfgAnalogica();
            //RQF35
            
            General.SafeLaunchEvent(ConfigChanged, this);
        }
        /// <summary>
        /// Fills _Operators struct with  config received
        /// </summary>
        private void CreateOperatorsData()
        {
            OperatorData operatorTA = null;
            List<OperatorData> operatorsCopy = new List<OperatorData>();

            foreach (DireccionamientoSIP dirSip in _SystemCfg.PlanDireccionamientoSIP)
            {
                SectorData sector = new SectorData();
                operatorTA = null;
                foreach (StrNumeroAbonado num in dirSip.NumerosAbonadoQueAtiende)
                {
                    if (operatorTA == null)
                    {
                        operatorTA = operatorsCopy.Find(x => x.idGroupName.Equals(num.IdAgrupacion));
                        if (operatorTA == null)
                        {
                            operatorTA = new OperatorData();
                            operatorTA.idGroupName = num.IdAgrupacion;
                            foreach (AsignacionUsuariosTV tv in _SystemCfg.PlanAsignacionUsuarios)
                            {
                                if ((tv.IdUsuario != STR_SECTOR_FS) && (tv.IdUsuario != STR_PUESTO_FS))
                                {
                                    if (tv.IdUsuario.Equals(dirSip.IdUsuario))
                                    {
                                        operatorTA.idHost = tv.IdHost;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    CfgRecursoEnlaceInterno cfg = new CfgRecursoEnlaceInterno();
                    cfg.NombreRecurso = dirSip.IdUsuario;
                    cfg.NumeroAbonado = num.NumeroAbonado;
                    cfg.Prefijo = num.Prefijo;
                    sector.numberData.Add(cfg);
                    sector.idUser = dirSip.IdUsuario;
                }
                operatorTA.sectorData.Add(sector);
                if (operatorsCopy.Contains(operatorTA) == false)
                    operatorsCopy.Add(operatorTA);
            }

            _Operators = operatorsCopy;
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
                {
                    _MiScv = new Scv(scvAts);
                    _MiScv.ProxyStateChange += OnProxyStateChangeCfg;
                }
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
            string idUser = cfg.ConfiguracionGeneral.GetMainUser(Top.HostId);

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

            if (_MiScv.GetProxyIp(out idEquipo) == ip)
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
                    // RQF-49 busco entre todos los proxies.
                    for (int i=0;i<3; i++)
                    {
                        string ipAddScv = scv.GetProxyIp(out idEquipo,i);
                        if ((scv.EsCentralIp) && (ipAddScv == ip))
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
                if (ip == host.IpRed1)
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
                else if (ip == host.IpRed1)
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
                            foreach (PlanRecursos recInPlan in _SystemCfg.GetTrunkResources(trunk))
                            {
                                if (string.Compare(recInPlan.IdRecurso, foundRsId, true) == 0)
                                {
                                    recurso = new CfgRecursoEnlaceInterno();
                                    recurso.Prefijo = Cd40Cfg.ATS_DST;
                                    recurso.NumeroAbonado = id;
                                    recurso.NombreRecurso = foundRsId;
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

        /// <summary>
        /// Devuelve el nombre del host que contiene ese recurso
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        private string GetGwRsHost(string resourceId)
        {
            string[] hostId = _SystemCfg.GetGwRsHost(resourceId).Split(':');  // idHost:SipPort
            return hostId[0];
        }

        //RQF-22
        public static string GetSeccionClave(string seccion, string clave, string fullRecorderFileName)
        {
            uint result = 0;
            //String fullRecorderFileName = Settings.Default.RecorderServicePath + "\\" + SipAgent.UG5K_REC_CONF_FILE;
            StringBuilder retorno = new StringBuilder(50);
            try
            {
                result = Native.Kernel32.GetPrivateProfileString(seccion, clave, "", retorno, 50, fullRecorderFileName);
            }
            catch (Exception /*exc*/)
            {
                //_Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} exception {1} !!!", Marshal.GetLastWin32Error(), exc.Message);
            }
            //_Logger.Debug("Leyendo fichero {1} :{0}", retorno, fullRecorderFileName);
            if (result != 1)
            {
                //_Logger.Error("Error leyendo fichero UG5K_REC_CONF_FILE: {0} !!!", Marshal.GetLastWin32Error());
            }

            return retorno.ToString();
        }


        // RQF-22
        private static string UG5K_REC_ANALOGIC_FILE = "ug5krec-analogic.ini";
        public static void LoadGrabacionAnalogica()
        {
            String fullRecorderAnalogicaFileName = ".\\" + UG5K_REC_ANALOGIC_FILE;
            int TipoGrabacionAnalogica = int.Parse(GetSeccionClave("GrabacionAnalogica", "TipoGrabacionAnalogica", fullRecorderAnalogicaFileName));
            bool EnableGrabacionAnalogica = bool.Parse(GetSeccionClave("GrabacionAnalogica", "EnableGrabacionAnalogica", fullRecorderAnalogicaFileName));

        }

        public static bool PictGrabacionAnalogicaCfg(int TipoGrabacionAnalogica, bool EnableGrabacionAnalogica)
        {
            bool result = false;
            bool changes = false;

            String fullRecorderAnalogicaFileName = ".\\" + UG5K_REC_ANALOGIC_FILE;
            // Actualizar el fichero INI que maneja el módudo de grabación Analogica
            try
            {
                int tipograbacionanalogica = 0;
                bool enablegrabacionanalogica = false;
                string retorno;
                retorno = GetSeccionClave("GrabacionAnalogica", "TipoGrabacionAnalogica", fullRecorderAnalogicaFileName);
                if (retorno.Length > 0)
                {
                    tipograbacionanalogica = int.Parse(GetSeccionClave("GrabacionAnalogica", "TipoGrabacionAnalogica", fullRecorderAnalogicaFileName));
                }
                else
                {
                    Native.Kernel32.WritePrivateProfileString("GrabacionAnalogica ", "TipoGrabacionAnalogica ", TipoGrabacionAnalogica.ToString(), fullRecorderAnalogicaFileName);
                    Native.Kernel32.WritePrivateProfileString("GrabacionAnalogica ", "EnableGrabacionAnalogica ", EnableGrabacionAnalogica.ToString(), fullRecorderAnalogicaFileName);
                }
                enablegrabacionanalogica = bool.Parse(GetSeccionClave("GrabacionAnalogica", "EnableGrabacionAnalogica", fullRecorderAnalogicaFileName));
                if (tipograbacionanalogica != TipoGrabacionAnalogica ||
                        enablegrabacionanalogica != EnableGrabacionAnalogica)
                    changes = true;

                //RQF22
                result |= Native.Kernel32.WritePrivateProfileString("GrabacionAnalogica ", "TipoGrabacionAnalogica ", TipoGrabacionAnalogica.ToString(), fullRecorderAnalogicaFileName);
                result |= Native.Kernel32.WritePrivateProfileString("GrabacionAnalogica ", "EnableGrabacionAnalogica ", EnableGrabacionAnalogica.ToString(), fullRecorderAnalogicaFileName);
            }
            catch (Exception /*exc*/)
            {
                //_Logger.Error("Error leyendo/escribiendo fichero UG5K_REC_ANALOGICA_CONF_FILE: {0} exception {1} !!!", Marshal.GetLastWin32Error(), exc.Message);
            }
            if (result == false)
            {
                //_Logger.Error("Error escribiendo fichero UG5K_REC_ANALOGICA_CONF_FILE: {0} !!!", Marshal.GetLastWin32Error());
            }
            if (changes == true)
            {
                // Notificar al módulo de propio de grabación analogica que ha cambiado la configuración.
                {
                    // TODO LALM, lo notifica el llamante.
                }
                _Logger.Debug("Notificado al módulo de grabación analogica Tipo:{0},Enable:{1}", TipoGrabacionAnalogica, EnableGrabacionAnalogica);
            }
            return changes;
        }

        //RQF-22
        void CheckCfgAnalogica()
        {
            //AsignacionUsuariosTV tv = Top.Cfg.GetUserTv(Top.Cfg.MainId);
            AsignacionUsuariosTV tv = Top.Cfg.GetHostTv(Top.HostId);
            //LALM 220208 si no hay configuración no hago nada.
            if (tv != null)
            {
                bool changes = PictGrabacionAnalogicaCfg(tv.TipoGrabacionAnalogica, tv.EnableGrabacionAnalogica);

                //RQF-22
                if (changes)//comento para pruebas
                {
                    Top.WorkingThread.Enqueue("TipoGrabacionAnalogica", delegate ()
                    {
                        Top.Mixer.SetTipoGrabacionAnalogica(tv.TipoGrabacionAnalogica);
                        Top.Mixer.SetGrabacionAnalogica(tv.TipoGrabacionAnalogica, tv.EnableGrabacionAnalogica);
                        //Top.Mixer.Init();
                        //Top.Mixer.Start();
                    });
                }
            }
        }

        #endregion
    }
}
