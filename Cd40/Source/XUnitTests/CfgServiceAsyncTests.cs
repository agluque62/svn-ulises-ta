using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Moq;
using Xunit;

using U5ki.Infrastructure;
using U5ki.CfgService;
using System.Xml.Linq;

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
        public void OnSlaveCfgReceivedTest()
        {
            PrepareTest((service, soap, mcast) =>
            {
                var cfgData = new CfgMoked();

                service.Start();
                Wait(20);
                mcast.Raise(e => e.ResourceChanged += null, this, cfgData.cfgChange());
                Wait(20);
                service.Stop();
                Wait(1);
            });
        }
        void PrepareTest(Action<IService, Mock<IUlisesSoapService>, Mock<IUlisesMcastService>> deploy)
        {
            using (var service = new CfgServiceAsync())
            {
                var cfgData = new CfgMoked();
                var mockSoapService = new Mock<IUlisesSoapService>();
                mockSoapService.Setup(x => x.GetVersionConfiguracion()).Returns(cfgData.Version);
                mockSoapService.Setup(x => x.GetConfigSistema()).Returns(cfgData.configuracionSistema());
                mockSoapService.Setup(x => x.GetConferenciasPreprogramadas()).Returns(cfgData.conferencias());

                var mockMcastService = new Mock<IUlisesMcastService>();

                service.SoapService = mockSoapService.Object;
                service.McastService = mockMcastService.Object;
                deploy(service, mockSoapService, mockMcastService);
            }
        }

        void Wait(int seconds)
        {
            Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();
        }

        internal class CfgMoked
        {
            public string Version => cfg.Version;
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
            
            public RsChangeInfo cfgChange()
            {
                return new RsChangeInfo("", "", "", "", new byte[] { });
            }
            public CfgMoked()
            {
                var txtData = File.ReadAllText(Filename);
                cfg = JsonConvert.DeserializeObject<Cd40Cfg>(txtData);
            }
            Cd40Cfg cfg {  get; set; }
            string Filename => Path.Combine(AppContext.BaseDirectory, "u5ki.LastCfg.json");

        }
    }
}
