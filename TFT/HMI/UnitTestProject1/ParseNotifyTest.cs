using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HMI.CD40.Module.BusinessEntities;

namespace UnitTest
{
    /// <summary>
    /// Descripción resumida de ParseNotify
    /// </summary>
    [TestClass]
    public class ParseNotifyTest
    {
        public ParseNotifyTest()
        {
            //
            // TODO: Agregar aquí la lógica del constructor
            //
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
        public void ParseNotifyTest1()
        {
            //Llamada early entrante
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<dialog-info xmlns=\"urn:ietf:params:xml:ns:dialog-info\" version=\"1\" state=\"full\" entity=\"sip:314451@10.12.60.129\">\n <dialog id=\"ced4d1159dd44276b93e723d441c5d4a\" call-id=\"ced4d1159dd44276b93e723d441c5d4a\" local-tag=\"f3a0edc4f1c14b3d89485e185df9ca20\" remote-tag=\"e0ed684d5c9348a48e22c4627773121e\" direction=\"recipient\">\n  <state>early</state>\n  <duration>0</duration>\n  <local>\n   <identity display=\"14L\">sip:314451@10.12.60.129</identity>\n   <target uri=\"sip:314451@10.12.60.129:5060\" />\n  </local>\n  <remote>\n   <identity>sip:314453@10.12.60.132</identity>\n  </remote>\n </dialog>\n</dialog-info>\n";
            TlfPickUp pickIpClass = new TlfPickUp(false);
            string source = null;
            List<TlfPickUp.DialogData> list = pickIpClass.NotifyDialogParse(xml, out source);
            Assert.AreEqual(list.Count, 1);
            Assert.AreEqual(list[0].callId, "ced4d1159dd44276b93e723d441c5d4a");
            Assert.AreEqual(list[0].remoteId, "sip:314453@10.12.60.132");
            Assert.AreEqual(list[0].state, "early");
            Assert.AreEqual(list[0].toTag, "f3a0edc4f1c14b3d89485e185df9ca20");
            Assert.AreEqual(list[0].fromTag, "e0ed684d5c9348a48e22c4627773121e");

            //Llamada early saliente
            xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<dialog-info xmlns=\"urn:ietf:params:xml:ns:dialog-info\" version=\"1\" state=\"full\" entity=\"sip:314451@10.12.60.129\">\n <dialog id=\"ced4d1159dd44276b93e723d441c5d4a\" call-id=\"ced4d1159dd44276b93e723d441c5d4a\" local-tag=\"f3a0edc4f1c14b3d89485e185df9ca20\" remote-tag=\"e0ed684d5c9348a48e22c4627773121e\" direction=\"initiator\">\n  <state>early</state>\n  <duration>0</duration>\n  <local>\n   <identity display=\"14L\">sip:314451@10.12.60.129</identity>\n   <target uri=\"sip:314451@10.12.60.129:5060\" />\n  </local>\n  <remote>\n   <identity>sip:314453@10.12.60.132</identity>\n  </remote>\n </dialog>\n</dialog-info>\n";
            source = null;
            list = pickIpClass.NotifyDialogParse(xml, out source);
            Assert.AreEqual(list.Count, 0);
        }

        [TestMethod]
        public void ParseNotifyTest2()
        {
            //Dos dialogos
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<dialog-info xmlns=\"urn:ietf:params:xml:ns:dialog-info\" version=\"1\" state=\"full\" entity=\"sip:314451@10.12.60.129\">\n <dialog id=\"ced4d1159dd44276b93e723d441c5d4a\" call-id=\"ced4d1159dd44276b93e723d441c5d4a\" local-tag=\"f3a0edc4f1c14b3d89485e185df9ca20\" remote-tag=\"e0ed684d5c9348a48e22c4627773121e\" direction=\"recipient\">\n  <state>early</state>\n  <duration>0</duration>\n  <local>\n   <identity display=\"14L\">sip:314451@10.12.60.129</identity>\n   <target uri=\"sip:314451@10.12.60.129:5060\" />\n  </local>\n  <remote>\n   <identity>sip:314453@10.12.60.132</identity>\n  </remote>\n </dialog>\n<dialog id=\"ffd48b98199840479ac78509f01785c4\" call-id=\"ffd48b98199840479ac78509f01785c4\" local-tag=\"606246bf3b3b44bda19fa1ef2ad6fa89\" remote-tag=\"e0ed684d5c9348a48e22c4627773121e\" direction=\"recipient\">\n<state>confirmed</state>\n<duration>0</duration>\n<local>\n<identity>sip:314451@10.12.60.129</identity>\n<target uri=\"sip:314451@10.12.60.129:5060\" />\n</local>\n<remote>\n<identity display=\"18L\">sip:314453@10.12.60.132</identity>\n<target uri=\"sip:314453@10.12.60.132:5060\" />\n</remote>\n</dialog>\n</dialog-info>\n";
            TlfPickUp pickIpClass = new TlfPickUp(false);
            string source = null;
            List<TlfPickUp.DialogData> list = pickIpClass.NotifyDialogParse(xml, out source);
            Assert.AreEqual(source, "sip:314451@10.12.60.129");
            Assert.AreEqual(list.Count, 2);
            Assert.AreEqual(list[0].callId, "ced4d1159dd44276b93e723d441c5d4a");
            Assert.AreEqual(list[0].remoteId, "sip:314453@10.12.60.132");
            Assert.AreEqual(list[0].state, "early");
            Assert.AreEqual(list[0].toTag, "f3a0edc4f1c14b3d89485e185df9ca20");
            Assert.AreEqual(list[0].fromTag, "e0ed684d5c9348a48e22c4627773121e");

            Assert.AreEqual(list[1].callId, "ffd48b98199840479ac78509f01785c4");
            Assert.AreEqual(list[1].state, "confirmed");
            Assert.AreEqual(list[1].remoteId, "sip:314453@10.12.60.132");
            Assert.AreEqual(list[1].toTag, "606246bf3b3b44bda19fa1ef2ad6fa89");
            Assert.AreEqual(list[1].fromTag, "e0ed684d5c9348a48e22c4627773121e");

        }
    }
}
