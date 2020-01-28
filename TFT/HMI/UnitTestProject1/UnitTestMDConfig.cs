using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using U5ki.Infrastructure;
using HMI.Model.Module.Messages;
namespace UnitTest
{
    [TestClass]
    public class UnitTestMDConfig
    {
        private static Cd40Cfg cfg = new Cd40Cfg();
        private static CfgEnlaceInterno link = new CfgEnlaceInterno();
        public event EventHandler<RangeMsg> TlfChanged;

        [TestMethod]
        public void TestMethod1()
        {
            Cd40Cfg cfg = ConfiguraciónProxy();

            //cfg = ConfiguracionGrupoUnMiembro(cfg);
            //    _CfgManager = new CfgManager();
            //    _SipManager = new SipManager();
            //    _MixerManager = new MixerManager();
            //    _TlfManager = new TlfManager();
            //Tlf tlf = new Tlf();
            //TlfInfo posInfo = new TlfInfo(link.Literal, TlfState.Idle, true, TlfType.Md);
            //RangeMsg<TlfInfo> tlfPositions = new RangeMsg<TlfInfo>(0, 1);
            //tlfPositions.Info[0] = posInfo;
            ////tlf.Reset(tlfPositions);
            ////General.SafeLaunchEvent(TlfChanged, this, (RangeMsg)tlfPositions);
            //OnTlfChanged(this, RangeMsg e)
            
        }
        private static Cd40Cfg ConfiguraciónProxy()
        {
            Cd40Cfg cfg = new Cd40Cfg();
            cfg.Version = "9.9";
            cfg.ConfiguracionGeneral = new ConfiguracionSistema();
            DireccionamientoIP i = new DireccionamientoIP();
            i.IdHost = "SCV2_LABO";
            i.IpRed1 = "192.168.2.112";
            i.TipoHost = Tipo_Elemento_HW.TEH_EXTERNO_TELEFONIA;
            i.Interno = true;
            i.Max = 218000;
            i.Min = 218008;
            i.EsCentralIP = true;

            cfg.ConfiguracionGeneral.PlanDireccionamientoIP.Add(i);
            return cfg;
        }

        private static Cd40Cfg ConfiguracionGrupoUnMiembro(Cd40Cfg cfg)
        {
            //Usuario S2
            ConfiguracionUsuario userCfg = new ConfiguracionUsuario();
            //Tecla de grupo con 1 miembro
            CfgEnlaceInterno link = new CfgEnlaceInterno();
            link.Literal = "grupotest1";
            link.PosicionHMI = 45;
            link.TipoEnlaceInterno = "MD";
            link.Prioridad = 4;
            link.OrigenR2 = "218002";
            CfgRecursoEnlaceInterno rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 0;
            rec.NombreRecurso = "218001";
            link.ListaRecursos.Add(rec);
            userCfg.TlfLinks.Add(link);

            //Tecla de AD normal
            CfgEnlaceInterno link2 = new CfgEnlaceInterno();
            link2.Literal = "S1_SCV2";
            link2.PosicionHMI = 44;
            link2.TipoEnlaceInterno = "DA";
            link2.Prioridad = 4;
            link2.OrigenR2 = "218002";
            userCfg.TlfLinks.Add(link2);

            cfg.ConfiguracionUsuarios.Add(userCfg);

            //Usuario S1
            ConfiguracionUsuario userCfg2 = new ConfiguracionUsuario();
            //Tecla de grupo con 1 miembro
            CfgEnlaceInterno link3 = new CfgEnlaceInterno();
            link3.Literal = "grupotest1";
            link3.PosicionHMI = 45;
            link3.TipoEnlaceInterno = "MD";
            link3.Prioridad = 4;
            link3.OrigenR2 = "218001";
            CfgRecursoEnlaceInterno rec2 = new CfgRecursoEnlaceInterno();
            rec2.Prefijo = 0;
            rec2.NombreRecurso = "218001";
            link3.ListaRecursos.Add(rec2);
            userCfg2.TlfLinks.Add(link3);

            //Tecla de AD normal
            CfgEnlaceInterno link4 = new CfgEnlaceInterno();
            link4.Literal = "S2_SCV2";
            link4.PosicionHMI = 44;
            link4.TipoEnlaceInterno = "DA";
            link4.Prioridad = 4;
            link4.OrigenR2 = "218001";
            userCfg2.TlfLinks.Add(link4);

            cfg.ConfiguracionUsuarios.Add(userCfg2);

            //Configuracion de sistema, numeracion de usuarios
            DireccionamientoSIP dirSIP = new DireccionamientoSIP();
            StrNumeroAbonado num = new StrNumeroAbonado();
            num.NumeroAbonado = "218001";
            num.Prefijo = 0;
            dirSIP.IdUsuario = "S1_SCV2";
            dirSIP.NumerosAbonadoQueAtiende.Add(num);
            cfg.ConfiguracionGeneral.PlanDireccionamientoSIP.Add(dirSIP);

            num = new StrNumeroAbonado();
            num.NumeroAbonado = "218002";
            num.Prefijo = 0;
            dirSIP = new DireccionamientoSIP();
            dirSIP.IdUsuario = "S2_SCV2";
            dirSIP.NumerosAbonadoQueAtiende.Add(num);
            cfg.ConfiguracionGeneral.PlanDireccionamientoSIP.Add(dirSIP);

            num = new StrNumeroAbonado();
            num.NumeroAbonado = "218003";
            num.Prefijo = 0;
            dirSIP = new DireccionamientoSIP();
            dirSIP.IdUsuario = "S2_SCV3";
            dirSIP.NumerosAbonadoQueAtiende.Add(num);
            cfg.ConfiguracionGeneral.PlanDireccionamientoSIP.Add(dirSIP);
            return cfg;
        }
        private static void ConfigExample1()
        {
            cfg.Version = "9.9";
            ConfiguracionUsuario confUsuario = new ConfiguracionUsuario();
            link.TipoEnlaceInterno = "MD";
            link.Literal = "grupoTest";
            link.PosicionHMI = 5;
            link.Prioridad = 4;
            link.OrigenR2 = "314453";
            CfgRecursoEnlaceInterno rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 3;
            rec.NombreRecurso = "14R";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
        }
    }
}
