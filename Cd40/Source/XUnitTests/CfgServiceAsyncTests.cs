using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Moq;
using Xunit;

using Utilities;
using U5ki.Infrastructure;
using U5ki.CfgService;
using System.Xml.Linq;
using System.Diagnostics;
using ProtoBuf;
using System.Security.Cryptography;
using static XUnitTests.CfgServiceAsyncTests;
using System.Net.NetworkInformation;

namespace XUnitTests
{
    public class CfgServiceAsyncTests
    {
        [Fact]
        public void StartStopTest()
        {
            PrepareTest((service, soap, mcast) =>
            {
                service.Start();
                Task.Delay(100).Wait();
                mcast.Raise(m => m.MasterStatusChanged += null, this, true);
                Wait(50);
                mcast.Raise(m => m.MasterStatusChanged += null, this, false);
                Wait(50);
                mcast.Raise(m => m.MasterStatusChanged += null, this, true);
                Wait(50);
                service.Stop();
            });
        }
        [Fact]
        public void ChangeConfigTest()
        {
            PrepareTest((service, soap, mcast) =>
            {
                service.Start();
                Task.Delay(1000).Wait();
                mcast.Raise(m => m.MasterStatusChanged += null, this, true);
                Wait(5);
                SendConfigChangeNotification();
                Wait(180);
                service.Stop();
            });
        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnReceiveCfgAndUserMsg(bool master)
        {
            PrepareTest((service, soap, mcast) =>
            {
                var cfgData = new CfgMoked();

                service.Start();
                Task.Delay(100).Wait();
                mcast.Raise(m => m.MasterStatusChanged += null, this, master);
                Wait(20);
                mcast.Raise(e => e.ResourceChanged += null, this, cfgData.cfgChange());
                Wait(25);
                mcast.Raise(m => m.UserMsgReceived += null, this, CFG_SAVE_AS_DEFAULT());
                Wait(25);
                mcast.Raise(m => m.UserMsgReceived += null, this, CFG_DEL_DEFAULT());
                Wait(10);
                service.Stop();
            });

        }
        [Fact]
        public void ProtobufTest01 ()
        {
            var cfgMoked = new CfgMoked();
            MemoryStream rsMs = new MemoryStream();
            ProtoBuf.Serializer.Serialize<Cd40Cfg>(rsMs, cfgMoked.cfg);
            var dataRaw = rsMs.ToArray();

            MemoryStream ms = new MemoryStream(dataRaw);
            var cfg1 = Serializer.Deserialize<Cd40Cfg>(ms);

        }
        void PrepareTest(Action<IService, Mock<IUlisesSoapService>, Mock<IUlisesMcastService>> deploy)
        {
            using (var service = new CfgServiceAsync())
            {
                var cfgData = new CfgMoked();
                var mockSoapService = new Mock<IUlisesSoapService>();
                mockSoapService.Setup(x => x.GetVersionConfiguracion())
                    .Returns(() => cfgData.getVersion());
                mockSoapService.Setup(x => x.GetConfigSistema())
                    .Returns(() => cfgData.configuracionSistema());
                mockSoapService.Setup(x => x.GetConferenciasPreprogramadas())
                    .Returns(() => cfgData.conferencias());
                mockSoapService.Setup(x => x.LoginTop(It.IsAny<string>()))
                    .Returns<string>(p => cfgData.usersOnPict(p));
                mockSoapService.Setup(x => x.GetCfgUsuario(It.IsAny<string>()))
                    .Returns<string>(u => cfgData.userData(u));
                mockSoapService.Setup(x => x.GetListaEnlacesExternos(It.IsAny<string>()))
                    .Returns<string>(u => cfgData.enlacesExternos(u));
                mockSoapService.Setup(x => x.GetListaEnlacesInternos(It.IsAny<string>()))
                    .Returns<string>(u => cfgData.enlacesInternos(u));
                mockSoapService.Setup(x => x.GetPoolHfElement())
                    .Returns(() => cfgData.hfElements());
                mockSoapService.Setup(x => x.GetPoolNMElements(It.IsAny<string>()))
                    .Returns<string>(t => cfgData.nodeElements(t));
                mockSoapService.Setup(x => x.GetParametrosMulticast())
                    .Returns(() => new U5ki.CfgService.SoapCfg.ParametrosMulticast() { GrupoMulticastConfiguracion = "224.100.10.1", PuertoMulticastConfiguracion = 1000 });

                var mockMcastService = new Mock<IUlisesMcastService>();
                mockMcastService.Setup(x => x.SetValue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Cd40Cfg>()))
                    .Callback<string, string, Cd40Cfg>((p1, p2, cfg) => 
                    {
                        Debug.WriteLine($"Settings CFGDATA ({cfg?.Version})");
                        MemoryStream rsMs = new MemoryStream();
                        Serializer.Serialize(rsMs, cfg);
                    });
                service.SoapService = mockSoapService.Object;
                service.McastService = mockMcastService.Object;
                deploy(service, mockSoapService, mockMcastService);
            }
        }

        void Wait(int seconds)
        {
            Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();
        }
        SpreadDataMsg CFG_SAVE_AS_DEFAULT()
        {
            var data = CfgServiceAsyncTests.ProtoEncode("cfgDef");
            return new SpreadDataMsg("Testing", 200, data, data.Length, "Testing");
        }
        SpreadDataMsg CFG_DEL_DEFAULT()
        {
            var data = CfgServiceAsyncTests.ProtoEncode($"u5ki.DefaultCfg.cfgDef.json");
            return new SpreadDataMsg("Testing", 202, data, data.Length, "Testing");
        }

        internal class CfgMoked
        {
            public string getVersion()
            {
                LoadCfgFromFile();
                return cfg.Version;
            }
            public U5ki.CfgService.SoapCfg.ConfiguracionSistema configuracionSistema()
            {
                var newSoapCfg = new U5ki.CfgService.SoapCfg.ConfiguracionSistema();
                
                newSoapCfg.ParametrosGenerales = new U5ki.CfgService.SoapCfg.ParametrosGeneralesSistema()
                {
                    TamLiteralEmplazamiento = cfg.ConfiguracionGeneral.ParametrosGenerales.TamLiteralEmplazamiento,
                    TamLiteralEnlAG = cfg.ConfiguracionGeneral.ParametrosGenerales.TamLiteralEnlAG,
                    TamLiteralEnlDA = cfg.ConfiguracionGeneral.ParametrosGenerales.TamLiteralEnlDA,
                    TamLiteralEnlExt = cfg.ConfiguracionGeneral.ParametrosGenerales.TamLiteralEnlExt,
                    TamLiteralEnlIA = cfg.ConfiguracionGeneral.ParametrosGenerales.TamLiteralEnlIA,
                    TiempoMaximoPTT = cfg.ConfiguracionGeneral.ParametrosGenerales.TiempoMaximoPTT,
                    TiempoSinJack1 = cfg.ConfiguracionGeneral.ParametrosGenerales.TiempoSinJack1,
                    TiempoSinJack2 = cfg.ConfiguracionGeneral.ParametrosGenerales.TiempoSinJack2
                };
                newSoapCfg.PlanNumeracionATS = cfg.ConfiguracionGeneral.PlanNumeracionATS.Select(p => new U5ki.CfgService.SoapCfg.NumeracionATS()
                {
                    Central = p.Central,
                    CentralPropia = p.CentralPropia,
                    Throwswitching = p.Throwswitching,
                    ListaRutas = p.ListaRutas.Select(r => new U5ki.CfgService.SoapCfg.PlanRutas()
                    {
                        TipoRuta = r.TipoRuta,
                        ListaTroncales = r.ListaTroncales.Select(t => t).ToArray()
                    }).ToArray(),
                    NumTest="",
                    RangosOperador = p.RangosOperador.Select(r => new U5ki.CfgService.SoapCfg.Rangos()
                    {
                        Inicial = r.Inicial.ToString(),
                        Final = r.Final.ToString(),
                        IdAbonado = r.IdAbonado,
                        IdPrefijo = r.IdPrefijo,
                        InsertedId = 0
                    }).ToArray(),
                    RangosPrivilegiados = p.RangosPrivilegiados.Select(r => new U5ki.CfgService.SoapCfg.Rangos()
                    {
                        Inicial = r.Inicial.ToString(),
                        Final = r.Final.ToString(),
                        IdAbonado = r.IdAbonado,
                        IdPrefijo = r.IdPrefijo,
                        InsertedId = 0
                    }).ToArray()
                }).ToArray();
                newSoapCfg.PlanAsignacionRecursos = cfg.ConfiguracionGeneral.PlanAsignacionRecursos.Select(p => new U5ki.CfgService.SoapCfg.AsignacionRecursosGW()
                {
                    IdHost = p.IdHost,
                    IdRecurso = p.IdRecurso,
                    SipPort = p.SipPort
                }).ToArray();
                newSoapCfg.PlanAsignacionUsuarios = cfg.ConfiguracionGeneral.PlanAsignacionUsuarios.Select(p => new U5ki.CfgService.SoapCfg.AsignacionUsuariosTV()
                {
                    IdHost = p.IdHost,
                    EnableGrabacionAnalogica = p.EnableGrabacionAnalogica,
                    EnableGrabacionEd137 = p.EnableGrabacionEd137,
                    IdUsuario = p.IdUsuario,
                    IpGrabador1 = p.IpGrabador1,
                    IpGrabador2 = p.IpGrabador2,
                    RtspPort = p.RtspPort,
                    RtspPort1 = p.RtspPort2,
                    TipoGrabacionAnalogica = p.TipoGrabacionAnalogica
                }).ToArray();

                return newSoapCfg;
            }
            public U5ki.CfgService.SoapCfg.ConferenciasPreprogramadas conferencias()
            {
                var conf = new U5ki.CfgService.SoapCfg.ConferenciasPreprogramadas()
                {
                    ConferenciaProgramada = cfg.ConferenciasPreprogramadas.Select(c => new U5ki.CfgService.SoapCfg.Conferencia()
                    {
                        Alias = c.Alias,
                        IdSalaBkk = c.IdSalaBkk,
                        ParticipantesConferencia = c.participantesConferencia.Select(p => new U5ki.CfgService.SoapCfg.Participantes()
                        {
                            Descripcion = p.Descripcion,
                            SipUri = p.SipUri,
                        }).ToArray(),
                        PosHMI = c.PosHMI,
                        TipoConferencia = c.TipoConferencia
                    }).ToArray()
                };
                return conf;
            }
            public U5ki.CfgService.SoapCfg.LoginTerminalVoz usersOnPict(string pict)
            {
                var users = cfg.ConfiguracionGeneral.PlanAsignacionUsuarios
                    .Where(i => i.IdHost == pict)
                    .Select(i => i.IdUsuario)
                    .Aggregate((a, b) => a + b);
                Debug.WriteLine($"LoginTop => {pict}:{users}");
                return new U5ki.CfgService.SoapCfg.LoginTerminalVoz()
                {
                    IdUsuario = users,
                    ModoLogin = "A"
                };
            }
            public U5ki.CfgService.SoapCfg.CfgUsuario userData(string idUser)
            {
                var userCfg = cfg.ConfiguracionUsuarios
                    .Where(c => c.User.Nombre == idUser)
                    .Select(c => c.User)
                    .Select(u => new U5ki.CfgService.SoapCfg.CfgUsuario()
                    {
                        IdIdentificador = u.IdIdentificador,
                        ListaAbonados = u.ListaAbonados.Select(l => new U5ki.CfgService.SoapCfg.NumerosAbonado()
                        {
                            Numero = l.Numero,
                            Prefijo = l.Prefijo
                        }).ToArray(),
                        NivelesDelSector = new U5ki.CfgService.SoapCfg.NivelesSCV()
                        {
                            CICL = u.NivelesDelSector.CICL,
                            CIPL = u.NivelesDelSector.CIPL,
                            CPICL = u.NivelesDelSector.CPIPL,
                            CPIPL = u.NivelesDelSector.CPIPL,
                            InsertedId = 0
                        },
                        Nombre = u.Nombre,
                        ParametrosDelSector = new U5ki.CfgService.SoapCfg.ParametrosSectorSCVKeepAlive()
                        {
                            NumLlamadasEntrantesIda = u.ParametrosDelSector.NumLlamadasEntrantesIda,
                            NumLlamadasEnIda = u.ParametrosDelSector.NumLlamadasEnIda,
                            NumFrecPagina = u.ParametrosDelSector.NumFrecPagina,
                            NumPagFrec = u.ParametrosDelSector.NumPagFrec,
                            NumEnlacesInternosPag = u.ParametrosDelSector.NumEnlacesInternosPag,
                            NumPagEnlacesInt = u.ParametrosDelSector.NumPagEnlacesInt,
                            Intrusion = u.ParametrosDelSector.Intrusion,
                            Intruido = u.ParametrosDelSector.Intruido,
                            KeepAlivePeriod = u.ParametrosDelSector.KeepAlivePeriod,
                            KeepAliveMultiplier = u.ParametrosDelSector.KeepAliveMultiplier
                        },
                        NumeroEnlacesExternos = u.NumeroEnlacesExternos,
                        NumeroEnlacesInternos = u.NumeroEnlacesInternos,
                        PermisosRedDelSector = u.PermisosRedDelSector.Select(p => new U5ki.CfgService.SoapCfg.PermisosRedesSCV()
                        {
                            IdRed = p.IdRed,
                            Llamar = p.Llamar,
                            Recibir = p.Recibir
                        }).ToArray(),
                        Sector = new U5ki.CfgService.SoapCfg.SectoresSCV()
                        {
                            IdParejaUCS = u.Sector.IdParejaUCS,
                            InsertedId = 0,
                            PrioridadR2 = u.Sector.PrioridadR2,
                            TipoHMI = u.Sector.TipoHMI,
                            TipoPosicion = u.Sector.TipoPosicion
                        },
                        TeclasDelSector = new U5ki.CfgService.SoapCfg.TeclasSectorSCV()
                        {
                            TransConConsultaPrev = u.TeclasDelSector.TransConConsultaPrev,
                            TransDirecta = u.TeclasDelSector.TransDirecta,
                            Conferencia = u.TeclasDelSector.Conferencia,
                            Escucha = u.TeclasDelSector.Escucha,
                            Retener = u.TeclasDelSector.Retener,
                            Captura = u.TeclasDelSector.Captura,
                            Redireccion = u.TeclasDelSector.Redireccion,
                            RepeticionUltLlamada = u.TeclasDelSector.RepeticionUltLlamada,
                            RellamadaAut = u.TeclasDelSector.RellamadaAut,
                            TeclaPrioridad = u.TeclasDelSector.TeclaPrioridad,
                            Tecla55mas1 = u.TeclasDelSector.Tecla55mas1,
                            Monitoring = u.TeclasDelSector.Monitoring,
                            CoordinadorTF = u.TeclasDelSector.CoordinadorTF,
                            CoordinadorRD = u.TeclasDelSector.CoordinadorRD,
                            IntegracionRDTF = u.TeclasDelSector.IntegracionRDTF,
                            LlamadaSelectiva = u.TeclasDelSector.LlamadaSelectiva,
                            GrupoBSS = u.TeclasDelSector.GrupoBSS,
                            LTT = u.TeclasDelSector.LTT,
                            SayAgain = u.TeclasDelSector.SayAgain,
                            InhabilitacionRedirec = u.TeclasDelSector.InhabilitacionRedirec,
                            Glp = u.TeclasDelSector.Glp,
                            PermisoRTXSQ = u.TeclasDelSector.PermisoRTXSQ,
                            PermisoRTXSect = u.TeclasDelSector.PermisoRTXSect
                        }
                    }).FirstOrDefault();
                Debug.WriteLine($"GetCfgUsuario for => {idUser}");
                return userCfg;
            }
            public U5ki.CfgService.SoapCfg.CfgEnlaceExterno[] enlacesExternos(string idUser)
            {
                var userExtLinks = cfg.ConfiguracionUsuarios
                    .Where(c => c.User.Nombre == idUser)
                    .SelectMany(c => c.RdLinks)
                    .Select(e => new U5ki.CfgService.SoapCfg.CfgEnlaceExterno()
                    {
                        Literal = e.Literal,
                        AliasEnlace = e.Alias,
                        DescDestino = e.DescDestino,
                        TipoFrecuencia = (uint)e.TipoFrecuencia,
                        ExclusividadTxRx = e.ExclusividadTxRx,
                        EstadoAsignacion = e.EstadoAsignacion,
                        Prioridad = e.Prioridad,
                        SupervisionPortadora = e.SupervisionPortadora,
                        FrecuenciaSintonizada = e.FrecuenciaSintonizada,
                        ListaPosicionesEnHmi = e.ListaPosicionesEnHmi.Select(i=>i).ToArray(),
                        DestinoAudio = e.DestinoAudio.Select(d=>d).ToArray(),
                        MetodoCalculoClimax = e.MetodoCalculoClimax,
                        VentanaSeleccionBss = e.VentanaSeleccionBss,
                        SincronizaGrupoClimax = e.SincronizaGrupoClimax,
                        AudioPrimerSqBss = e.AudioPrimerSqBss,
                        FrecuenciaNoDesasignable = e.FrecuenciaNoDesasignable,
                        VentanaReposoZonaTxDefecto = e.VentanaReposoZonaTxDefecto,
                        PrioridadSesionSIP = e.PrioridadSesionSip,
                        CldSupervisionTime = e.CldSupervisionTime,
                        MetodosBssOfrecidos = e.MetodosBssOfrecidos,
                        PasivoRetransmision = e.PasivoRetransmision,
                        SelectableFrequencies = e.SelectableFrequencies.Select(f=>f).ToArray(),
                        DefaultFrequency = e.DefaultFrequency,
                        ListaRecursos = e.ListaRecursos.Select(r => new U5ki.CfgService.SoapCfg.CfgRecursoEnlaceExterno()
                        {
                            IdRecurso = r.IdRecurso,
                            Tipo = r.Tipo,
                            Estado = r.Estado,
                            ModoConfPTT = r.ModoConfPTT,
                            NumFlujosAudio = r.NumFlujosAudio,
                            IdEmplazamiento = r.IdEmplazamiento,
                            NombreZona = r.NombreZona,
                            GrsDelay = r.GrsDelay,
                            OffSetFrequency = r.OffSetFrequency,
                            EnableEventPttSq = r.EnableEventPttSq,
                            RedundanciaRol = r.RedundanciaRol,
                            RedundanciaIdPareja = r.RedundanciaIdPareja,
                            Telemando = r.Telemando
                        }).ToArray(),
                        ModoTransmision = "M",
                        EmplazamientoDefecto = "",
                        TiempoVueltaADefecto = "0",
                        PorcentajeRSSI = "0"
                    }).ToArray();
                Debug.WriteLine($"GetRdLinks for => {idUser}");
                return userExtLinks;
            }
            public U5ki.CfgService.SoapCfg.CfgEnlaceInterno[] enlacesInternos(string idUser)
            {
                var userIntLinks = cfg.ConfiguracionUsuarios
                    .Where(c => c.User.Nombre == idUser)
                    .SelectMany(c => c.TlfLinks)
                    .Select(e => new U5ki.CfgService.SoapCfg.CfgEnlaceInterno()
                    {
                        Literal = e.Literal,
                        PosicionHMI = e.PosicionHMI,
                        TipoEnlaceInterno = e.TipoEnlaceInterno,
                        Dependencia = e.Dependencia,
                        Prioridad = e.Prioridad + 1,
                        OrigenR2 = e.OrigenR2,
                        Dominio = e.Dominio,
                        ListaRecursos = e.ListaRecursos.Select(r => new U5ki.CfgService.SoapCfg.CfgRecursoEnlaceInternoConInterface()
                        {
                            NombreRecurso = r.NombreRecurso,
                            NumeroAbonado = r.NumeroAbonado,
                            Interface = (U5ki.CfgService.SoapCfg.TipoInterface)r.Interface,
                            NombreMostrar = r.NombreMostrar,
                            Prefijo = translateprefijo(r.Prefijo)
                        }).ToArray()
                    }).ToArray();
                Debug.WriteLine($"GetTlfLinks for => {idUser}");
                return userIntLinks;
            }
            public U5ki.CfgService.SoapCfg.PoolHfElement[] hfElements()
            {
                var elements = cfg.PoolHf.Select(i => new U5ki.CfgService.SoapCfg.PoolHfElement()
                {
                    Id = i.Id,
                    IpGestor = i.IpGestor,
                    Oid = i.Oid,
                    SipUri = i.SipUri,
                    Frecs = i.Frecs.Select(f => new U5ki.CfgService.SoapCfg.HfRangoFrecuencias()
                    {
                        FMin = f.fmin,
                        FMax = f.fmax
                    }).ToArray()
                }).ToArray();
                return elements;
            }
            public U5ki.CfgService.SoapCfg.Node[] nodeElements(string tipo)
            {
                var source = tipo == "0" ? cfg.NodesMN : cfg.NodesEE;
                var nodes = source.Select(n => new U5ki.CfgService.SoapCfg.Node()
                {
                    Id = n.Id,
                    Canalizacion = (U5ki.CfgService.SoapCfg.GearChannelSpacings)n.Canalizacion,
                    EsReceptor = n.EsReceptor,
                    EsTransmisor = n.EsTransmisor,
                    FormaDeTrabajo = (U5ki.CfgService.SoapCfg.Tipo_Formato_Trabajo)n.FormaDeTrabajo,
                    FormatoFrecuenciaPrincipal = (U5ki.CfgService.SoapCfg.Tipo_Formato_Frecuencia)n.FormatoFrecuenciaPrincipal,
                    Frecs = n.Frecs.Select(f => new U5ki.CfgService.SoapCfg.HfRangoFrecuencias()
                    {
                        FMax = f.fmax,
                        FMin = f.fmin
                    }).ToArray(),
                    FrecuenciaPrincipal = n.FrecuenciaPrincipal,
                    IdEmplazamiento = n.idEmplazamiento,
                    IpGestor = n.IpGestor,
                    ModeloEquipo = n.ModeloEquipo,
                    Modulacion = (U5ki.CfgService.SoapCfg.GearModulations)n.Modulacion,
                    NivelDePotencia = (U5ki.CfgService.SoapCfg.GearPowerLevels)n.Modulacion,
                    Offset = (U5ki.CfgService.SoapCfg.GearCarrierOffStatus)n.Modulacion,
                    Oid = n.Oid,
                    Prioridad = n.Prioridad,
                    Puerto = n.Puerto,
                    SipUri = n.SipUri,
                    TipoDeCanal = (U5ki.CfgService.SoapCfg.Tipo_Canal)n.TipoDeCanal,
                    TipoDeFrecuencia = (U5ki.CfgService.SoapCfg.Tipo_Frecuencia)n.TipoDeFrecuencia
                }).ToArray();
                return nodes;
            }
            public RsChangeInfo cfgChange()
            {
                MemoryStream rsMs = new MemoryStream();
                ProtoBuf.Serializer.Serialize<Cd40Cfg>(rsMs, cfg);
                var dataRaw = rsMs.ToArray();
                var data = Tools.Compress(dataRaw);
                return new RsChangeInfo("Testing", Identifiers.CfgTopic, Identifiers.CfgRsId, Identifiers.CfgRsId, data);
            }
            public CfgMoked()
            {
                LoadCfgFromFile();
            }
            public Cd40Cfg cfg {  get; set; }
            string Filename => Path.Combine(AppContext.BaseDirectory, "u5ki.LastCfg.json");
            uint translateprefijo(uint id)
            {
                if (id == Cd40Cfg.INT_DST) return 2;
                if (id == Cd40Cfg.PP_DST) return 32;
                return id;
            }
            void LoadCfgFromFile()
            {
                var txtData = File.ReadAllText(Filename);
                cfg = JsonConvert.DeserializeObject<Cd40Cfg>(txtData);
            }
        }

        public static byte[] ProtoEncode<T>(T data)
        {
            MemoryStream rsMs = new MemoryStream();
            ProtoBuf.Serializer.Serialize<T>(rsMs, data);
            var dataRaw = rsMs.ToArray();
            return dataRaw;
        }
        void SendConfigChangeNotification()
        {
            var endp = new IPEndPoint(IPAddress.Parse("224.100.10.1"), 1000);
            //var endp = new IPEndPoint(IPAddress.Parse("11.12.60.33"), 1000);
            var sck = new UdpClient();
            sck.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, IPAddress.HostToNetworkOrder(NetworkInterfaceIndex("11.12.60.33")));
            sck.Connect(endp);
            var data = Encoding.ASCII.GetBytes("12-3-2-3");
            sck.SendAsync(data, data.Length).Wait();
            Task.Delay(500).Wait();
            Debug.WriteLine("ConfigChangeNotification sent!");
        }
        static public int NetworkInterfaceIndex(string ip)
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                IPInterfaceProperties ip_properties = adapter.GetIPProperties();

                if (!adapter.GetIPProperties().MulticastAddresses.Any())
                    continue; // most of VPN adapters will be skipped

                if (!adapter.SupportsMulticast)
                    continue; // multicast is meaningless for this type of connection

                if (OperationalStatus.Up != adapter.OperationalStatus)
                    continue; // this adapter is off or not connected

                foreach (UnicastIPAddressInformation inf in ip_properties.UnicastAddresses)
                {
                    if (inf.Address.Equals(IPAddress.Parse(ip)) == true)
                    {

                        IPv4InterfaceProperties p = adapter.GetIPProperties().GetIPv4Properties();
                        if (p != null)
                            return p.Index;
                    }
                }

            }
            return 0;
        }

    }
}
