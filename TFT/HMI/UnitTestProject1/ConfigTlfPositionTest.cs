using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HMI.CD40.Module.BusinessEntities;
using U5ki.Infrastructure;
using HMI.Model.Module.BusinessEntities;

namespace UnitTest
{
    /// <summary>
    /// NOTA:
    /// Los test deben pasarse en modo debug, 
    /// que hace que el acceso a muchas clases sea public en lugar de internal.
    /// </summary>
    [TestClass]
    public class ConfigTlfPositionTest
    {
        static Cd40Cfg cfg = new Cd40Cfg();

        public ConfigTlfPositionTest()
        {
            Top.Init();
            ConfiguracionProxy();
            ConfiguracionBL();
            ConfiguraUsuario();
            Top.Cfg.OnNewConfig(this, cfg);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Obtiene o establece el contexto de las pruebas que proporciona
        ///información y funcionalidad para la ejecución de pruebas actual.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Atributos de prueba adicionales
        //
        // Puede usar los siguientes atributos adicionales conforme escribe las pruebas:
        //
        // Use ClassInitialize para ejecutar el código antes de ejecutar la primera prueba en la clase
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup para ejecutar el código una vez ejecutadas todas las pruebas en una clase
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Usar TestInitialize para ejecutar el código antes de ejecutar cada prueba 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup para ejecutar el código una vez ejecutadas todas las pruebas
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        [TestMethod]
        ///Tests para probar cuando se corta la llamada y pasa la tecla a reposo
        ///por una sectorización con cambios.
        ///La llamada no tiene canal
        public void ChangeConfig1()
        {
            //Initial configuration
            TlfPosition tlfTest = new TlfPosition(12);
            CfgEnlaceInterno link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            CfgRecursoEnlaceInterno rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);

            tlfTest.Reset(link);

            //Give it an incomplete call, , no channel
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            Assert.IsNotNull(tlfTest._SipCall);

            //1.Cambio configuracion origen R2
            link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218002"; // Cambio
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //Se corta la llamada
            Assert.IsNull(tlfTest._SipCall);

            //Give it an incomplete call, no channel
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            Assert.IsNotNull(tlfTest._SipCall);

            //2.Cambio configuracion Prioridad, literal
            link = new CfgEnlaceInterno();
            link.Literal = "S1_";  //Cambio
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4; // Cambio
            link.OrigenR2 = "218002"; 
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //No se corta la llamada
            Assert.IsNotNull(tlfTest._SipCall);

            //Give it an incomplete call, no channel
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            Assert.IsNotNull(tlfTest._SipCall);

            //3.Cambio configuracion prefijo recurso, nombre rec
            link = new CfgEnlaceInterno();
            link.Literal = "S1_";  
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4; 
            link.OrigenR2 = "218002";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 3; // Cambio
            rec.NombreRecurso = "S1_"; //Cambio
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //No Se corta la llamada
            Assert.IsNotNull(tlfTest._SipCall);

        }

        [TestMethod]
        ///Tests para probar que no se corta la llamada ni cambia de estado 
        ///por una sectorización sin cambios
        public void ChangeConfig2()
        {
            //Initial configuration
            TlfPosition tlfTest = new TlfPosition(12);
            CfgEnlaceInterno link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            CfgRecursoEnlaceInterno rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);

            tlfTest.Reset(link);
            //Give it an incomplete call, estado unavailable
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            Assert.IsNotNull(tlfTest._SipCall);
            Assert.AreEqual(tlfTest.State, TlfState.Unavailable);

            //1.sin cambios, en estado unavailable
            link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //No se corta la llamada
            Assert.IsNotNull(tlfTest._SipCall);
            Assert.AreEqual(tlfTest.State, TlfState.Unavailable);

            //2. Tecla en congestion, no cambia
            tlfTest.State = TlfState.Congestion;
            link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //No se corta la llamada
            Assert.IsNotNull(tlfTest._SipCall);
            Assert.AreEqual(tlfTest.State, TlfState.Congestion);

            //3. Tecla en busy, no cambia
            tlfTest.State = TlfState.Busy;
            link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //No se corta la llamada
            Assert.IsNotNull(tlfTest._SipCall);
            Assert.AreEqual(tlfTest.State, TlfState.Busy);

            //3. Tecla en InProcess, no cambia
            tlfTest.State = TlfState.InProcess;
            link = new CfgEnlaceInterno();
            link.Literal = "S1";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "S1";
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //No se corta la llamada
            Assert.IsNotNull(tlfTest._SipCall);
            Assert.AreEqual(tlfTest.State, TlfState.InProcess);

        }

        [TestMethod]
        ///Tests para probar cuando se corta la llamada y pasa la tecla a reposo
        ///por una sectorización con cambios.
        ///Hay una llamada con canal, pero sin callId
        public void ChangeConfig3()
        {
            //Initial configuration
            TlfPosition tlfTest = new TlfPosition(12);
            CfgEnlaceInterno link = new CfgEnlaceInterno();
            link.Literal = "BL";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218001";
            CfgRecursoEnlaceInterno rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "prubl";
            rec.Interface = TipoInterface.TI_BL;
            link.ListaRecursos.Add(rec);

            tlfTest.Reset(link);

            //Give it an incomplete call, no callID
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            SipRemote remote = new SipRemote(rec.NombreRecurso);
            tlfTest._SipCall.Update(-1, link.OrigenR2, rec.NombreRecurso, tlfTest.Channels[0], remote, tlfTest.Channels[0].ListLines[0]);
            Assert.IsNotNull(tlfTest._SipCall);

            //1.Cambio configuracion origen R2
            link = new CfgEnlaceInterno();
            link.Literal = "BL";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 4;
            link.OrigenR2 = "218002"; // Cambio
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "prubl";
            rec.Interface = TipoInterface.TI_BL;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //Se corta la llamada
            Assert.IsNull(tlfTest._SipCall);

            //Give it an incomplete call, no callID
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            remote = new SipRemote(rec.NombreRecurso);
            tlfTest._SipCall.Update(-1, link.OrigenR2, rec.NombreRecurso, tlfTest.Channels[0], remote, tlfTest.Channels[0].ListLines[0]);
            tlfTest.State = TlfState.Busy;

            //2.Cambio configuracion Prioridad, literal
            link = new CfgEnlaceInterno();
            link.Literal = "BL_";  //Cambio
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 3; // Cambio
            link.OrigenR2 = "218002";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 2;
            rec.NombreRecurso = "prubl";
            rec.Interface = TipoInterface.TI_BL;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //Se corta la llamada porque no esta disponible la linea
            Assert.IsNull(tlfTest._SipCall);
            Assert.AreEqual(tlfTest.State, TlfState.Unavailable);

            //Give it an incomplete call, no channel
            tlfTest._SipCall = SipCallInfo.NewTlfCall(tlfTest.Channels, CORESIP_Priority.CORESIP_PR_NORMAL, null);
            remote = new SipRemote(rec.NombreRecurso);
            tlfTest._SipCall.Update(-1, link.OrigenR2, rec.NombreRecurso, tlfTest.Channels[0], remote, tlfTest.Channels[0].ListLines[0]);
            tlfTest.State = TlfState.Busy;
            Assert.IsNotNull(tlfTest._SipCall);

            //3.Cambio configuracion prefijo recurso, nombre rec
            link = new CfgEnlaceInterno();
            link.Literal = "BL_";
            link.TipoEnlaceInterno = "DA";
            link.Prioridad = 3;
            link.OrigenR2 = "218002";
            rec = new CfgRecursoEnlaceInterno();
            rec.Prefijo = 0; // Cambio
            rec.NombreRecurso = "S4_SCV2"; //Cambio
            rec.Interface = TipoInterface.TI_Radio;
            link.ListaRecursos.Add(rec);
            tlfTest.Reset(link);
            //Se corta la llamada
            Assert.IsNull(tlfTest._SipCall);

        }
        private static void ConfiguracionProxy()
        {
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
        }
        private static void ConfiguracionBL()
        {
            // Pasarela
            DireccionamientoIP i = new DireccionamientoIP();
            i.IdHost = "TIFX18";
            i.IpRed1 = "192.168.2.18";
            i.TipoHost = Tipo_Elemento_HW.TEH_TIFX;
            i.Interno = false;
            i.Max = 0;
            i.Min = 0;
            i.EsCentralIP = false;
            cfg.ConfiguracionGeneral.PlanDireccionamientoIP.Add(i);

            //Recurso BL
            AsignacionRecursosGW a = new AsignacionRecursosGW();
            a.IdRecurso = "prubl";
            a.IdHost = "TIFX18";
            a.SipPort = 5060;
            cfg.ConfiguracionGeneral.PlanAsignacionRecursos.Add(a);
        }
        private static void ConfiguraUsuario()
        {
            // Sector
            DireccionamientoIP i = new DireccionamientoIP();
            i.IdHost = "PICT04";
            i.IpRed1 = "192.168.2.204";
            i.TipoHost = Tipo_Elemento_HW.TEH_TOP;
            i.Interno = false;
            i.Max = 0;
            i.Min = 0;
            i.EsCentralIP = false;
            cfg.ConfiguracionGeneral.PlanDireccionamientoIP.Add(i);

            //Recurso interno Usuario 
            AsignacionUsuariosTV a = new AsignacionUsuariosTV();
            a.IdUsuario = "S4_SCV2";
            a.IdHost = "PICT04";
            cfg.ConfiguracionGeneral.PlanAsignacionUsuarios.Add(a);
        }
    }
}
