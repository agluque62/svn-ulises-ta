using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HMI.CD40.Module.BusinessEntities;
using System.IO;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using U5ki.Infrastructure;

namespace UnitTestProject1
{
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

    [TestClass]
    public class ForwardUnitTest
    {
        const string source = "A";
        const string destA = "A";
        const string destB = "B";
        const string destC = "C";
        const string destD = "D";
        const string uriDestA = "sip:A@10.68.60.132";
        const string uriDestB = "sip:B@10.68.60.132";
        const string uriDestC = "sip:C@10.68.60.132";
        const string uriDestD = "sip:D@10.68.60.132";

        private ForwardManager.TlfForward _A_UA ;
        private ForwardManager.TlfForward _B_UA ;
        private ForwardManager.TlfForward _C_UA;
        private ForwardManager.TlfForward _D_UA;
        private TlfPosition tlfA;
        private TlfPosition tlfB;
        private TlfPosition tlfC;
        private TlfPosition tlfD;

        private Mock<TlfManager> mockTlf;
        private Mock<TlfPosition> mockTlfA;
        private Mock<TlfPosition> mockTlfB;
        private Mock<TlfPosition> mockTlfC;
        private Mock<TlfPosition> mockTlfD;
        public ForwardUnitTest()
        {
            //
            // Prepara el mock del TlfManager
            //
            //user TlfManager constructor for testing
            mockTlf = new Mock<TlfManager>(true);
            mockTlfA = new Mock<TlfPosition>(MockBehavior.Strict,  0);
            mockTlfA.SetupGet(m => m.NumerosAbonado).Returns(new List<string> { "A" });
            mockTlfB = new Mock<TlfPosition>(MockBehavior.Strict, 0);
            mockTlfB.SetupGet(m => m.NumerosAbonado).Returns(new List<string> { "B" });
            mockTlfC = new Mock<TlfPosition>(MockBehavior.Strict, 0);
            mockTlfC.SetupGet(m => m.NumerosAbonado).Returns(new List<string> { "C" });
            mockTlfD = new Mock<TlfPosition>(MockBehavior.Strict, 0);
            mockTlfD.SetupGet(m => m.NumerosAbonado).Returns(new List<string> { "D" });

            //SortedList<int, TlfPosition> ADConfigured = new SortedList<int, TlfPosition>();
            tlfA = new TlfPosition(0);
            //tlfA.Literal = "A";          
            //ADConfigured.Add(0, tlfA);
            tlfB = new TlfPosition(1);
            //tlfB.Literal = "B";
            //ADConfigured.Add(1, tlfB);
            tlfC = new TlfPosition(2);
            //ADConfigured.Add(2, tlfC);
            //tlfC.Literal = "C";
            //mockTlf.SetupProperty<SortedList<int, TlfPosition>>(m => m._TlfPositions, ADConfigured);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestA, destA, false, true)).Returns(mockTlfA.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestA, null, false, true)).Returns(mockTlfA.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestB, destB, false, true)).Returns(mockTlfB.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestB, null, false, true)).Returns(mockTlfB.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestC, destC, false, true)).Returns(mockTlfC.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestC, null, false, true)).Returns(mockTlfC.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestD, destD, false, true)).Returns(mockTlfD.Object);
            mockTlf.Setup(m => m.GetTlfPosition(uriDestD, null, false, true)).Returns(mockTlfD.Object);
            

            _A_UA = new ForwardManager.TlfForward("A", mockTlf.Object);
            _B_UA = new ForwardManager.TlfForward("B", mockTlf.Object);
            _C_UA = new ForwardManager.TlfForward("C", mockTlf.Object);
            _D_UA = new ForwardManager.TlfForward("D", mockTlf.Object);

        }
        [TestMethod]
        public void DesvioSimple()
        {

            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");

            DesvioNegociacionAB();

            //Cancelacion desvio
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");

            _A_UA.Cancel();
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            _B_UA.ReceiveRelease(source);

            _A_UA.AnswerRelease(destB);

            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
           
        }
        [TestMethod]
        public void CancelacionSinDesvio()
        {

            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");

            DesvioNegociacionAB();

            //Cancelacion desvio por error a C
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _A_UA.Cancel();
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            string heads =_C_UA.ReceiveRelease(source);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(heads, "");
            _A_UA.AnswerRelease(destC);

            
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);

        }
        [TestMethod]
        public void DesvioRepetido()
        {

            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");

            DesvioNegociacionAB();
            File.Delete("Forward_" + destB + ".xml");

            //B recibe el desvio de nuevo
            string heads = _B_UA.ReceiveRequest(source, destA);
            File.Copy("Forward_" + destB + ".xml", "ForwardAnswerReq_" + destB + ".xml", true);

            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, "sip:" + source + "@10.68.60.132");
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(heads, "A");
            //Cancelacion desvio
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");

            _A_UA.Cancel();
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            heads = _B_UA.ReceiveRelease(source);
            Assert.AreEqual(heads, "");

            _A_UA.AnswerRelease(destB);

            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);

        }
        private void DesvioNegociacionAB()
        {
            //A desvia a B
            _A_UA.RequestForward(uriDestB);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            File.Copy("Forward_" + source + ".xml", "ForwardReq_" + source + ".xml", true);

            string heads = _B_UA.ReceiveRequest(source, destA);
            File.Copy("Forward_" + destB + ".xml", "ForwardAnswerReq_" + destB + ".xml", true);
            Assert.AreEqual(heads, "A");

            _A_UA.AnswerRequest(destB, out string hopTail);

            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, "sip:" + source + "@10.68.60.132");
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(hopTail, "");
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, "sip:" + source + "@10.68.60.132");
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfA.Object);
        }

        [TestMethod]
        public void DesvioEncadenado()
        {
            DesvioEncadenadoABC();

            //Cancelación del desvío encadenado 
            //A cancela desvío a B
            _A_UA.Cancel();
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            string heads = _C_UA.ReceiveRelease(destA);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(heads, "B");
            _A_UA.AnswerRelease(destC);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);

            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);

            //B cancela desvío a C
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _B_UA.Cancel();
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);

            heads = _C_UA.ReceiveRelease(destB);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(heads, "");
            _B_UA.AnswerRelease(destC);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);
        }

        private void DesvioEncadenadoABC()
        {
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            //A desvia a B
            DesvioNegociacionAB();

            //B desvia a C
            _B_UA.RequestForward(uriDestC);
            File.Copy("Forward_" + destB + ".xml", "ForwardReq_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 1);

            string heads = _C_UA.ReceiveRequest(destB, destB);
            File.Copy("Forward_" + destC + ".xml", "ForwardAnswerReq_" + destC + ".xml", true);

            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(heads, "B");

            _B_UA.AnswerRequest(destC, out string hopTail);
            File.Copy("Forward_" + destB + ".xml", "ForwardUpdate_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 0);

            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(hopTail, "");
            _A_UA.ReceiveUpdate(destB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].ListDiv[1], uriDestB);

            heads = _C_UA.ReceiveRequest(destA, destA);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 2);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].Head, uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].ListDiv[1], uriDestB);

            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj.Count, 2);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj[0], mockTlfA.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj[1], mockTlfB.Object);
            Assert.AreEqual(heads, "B, A");

            _A_UA.AnswerRequest(destC, out hopTail);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].ListDiv[1], uriDestB);

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 2);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[1], mockTlfB.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(hopTail, "C");
        }

        [TestMethod]
        public void DesvioEncadenadoInverso()
        {
            string hopTail;
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            //B desvia a C
            _B_UA.RequestForward(uriDestC);
            File.Copy("Forward_" + destB + ".xml", "ForwardReq_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 1);
            string heads = _C_UA.ReceiveRequest(destB, destB);
            File.Copy("Forward_" + destC + ".xml", "ForwardAnswerReq_" + destC + ".xml", true);
            Assert.AreEqual(heads, "B");

            _B_UA.AnswerRequest(destC, out hopTail);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(hopTail, "");

            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            //A desvia a B
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _A_UA.RequestForward(uriDestB);
            File.Copy("Forward_" + destA + ".xml", "ForwardReq_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Tail, uriDestB);

            heads = _B_UA.ReceiveRequest(destA, destA);
            File.Copy("Forward_" + destB + ".xml", "ForwardAnswerReq_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(heads, "");

            _A_UA.AnswerRequest(destB, out hopTail);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].ListDiv[1], uriDestB);
            Assert.AreEqual(hopTail, "");

            heads = _C_UA.ReceiveRequest(destA, destA);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 2);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].Head, uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].ListDiv[1], uriDestB);
            Assert.AreEqual(heads, "B, A");

            _A_UA.AnswerRequest(destC, out hopTail);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].ListDiv[1], uriDestB);
            Assert.AreEqual(hopTail, "C");

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 2);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[1], mockTlfB.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj.Count, 2);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj[0], mockTlfA.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj[1], mockTlfB.Object);

            //Cancelacion
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            //B cancela desvío a C
            _B_UA.Cancel();
            File.Copy("Forward_" + destB + ".xml", "ForwardRel_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 2);

            _C_UA.ReceiveRelease(destB);
            File.Copy("Forward_" + destC + ".xml", "ForwardRelAnswer1_" + destC + ".xml", true);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);

            File.Delete("Forward_" + destB + ".xml");

            _B_UA.AnswerRelease(destC);
            File.Copy("Forward_" + destB + ".xml", "ForwardUpdate_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);

            _A_UA.ReceiveUpdate(destB);
            File.Copy("Forward_" + destA + ".xml", "ForwardRequest_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);

            heads = _B_UA.ReceiveRequest(destA, destA);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(heads, "A");

            _A_UA.AnswerRequest(destB, out hopTail);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(hopTail, "");

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfA.Object);

            //A cancela desvío a B
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _A_UA.Cancel();
            File.Copy("Forward_" + destA + ".xml", "ForwardRel_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);

            _B_UA.ReceiveRelease(destA);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);

            _A_UA.AnswerRelease(destB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
        }
        [TestMethod]
        public void DesvioDoble()
        {
            string hopTail;
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");
            //A desvia a C
            _A_UA.RequestForward(uriDestC);
            File.Copy("Forward_" + destA + ".xml", "ForwardReq_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);

            string heads = _C_UA.ReceiveRequest(destA, destA);
            File.Copy("Forward_" + destC + ".xml", "ForwardAnswerReq_" + destC + ".xml", true);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(heads, "A");
            _A_UA.AnswerRequest(destC, out hopTail);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(hopTail, "");

            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);

            //B desvia a C
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _B_UA.RequestForward(uriDestC);
            File.Copy("Forward_" + destB + ".xml", "ForwardReq_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);

            heads = _C_UA.ReceiveRequest(destB, destB);
            File.Copy("Forward_" + destC + ".xml", "ForwardAnswerReq_" + destC + ".xml", true);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 2);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].Tail, uriDestC);
            Assert.AreEqual(heads, "A, B");
            _B_UA.AnswerRequest(destC, out hopTail);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(hopTail, "");

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfA.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[1].TlfParticipantsObj[0], mockTlfB.Object);

            //Cancelacion
            //B cancela desvia a C
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _B_UA.Cancel();
            File.Copy("Forward_" + destB + ".xml", "ForwardRel_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 2);

            _C_UA.ReceiveRelease(destB);
            File.Copy("Forward_" + destC + ".xml", "ForwardRelAnswer1_" + destC + ".xml", true);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);

            _B_UA.AnswerRelease(destC);
            File.Copy("Forward_" + destB + ".xml", "ForwardUpdate_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);

            //A cancela desvia a C
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            _A_UA.Cancel();
            File.Copy("Forward_" + destA + ".xml", "ForwardRel_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);

            _C_UA.ReceiveRelease(destA);
            File.Copy("Forward_" + destC + ".xml", "ForwardRelAnswer2_" + destC + ".xml", true);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);

            _A_UA.AnswerRelease(destC);
            File.Copy("Forward_" + destA + ".xml", "ForwardUpdate_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);
        }
        [TestMethod]
        public void DesvioEnBucle()
        {
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            //A desvia a B
            DesvioNegociacionAB();
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");

            //B desvia a A
            _B_UA.RequestForward(uriDestA);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);

            string heads = _A_UA.ReceiveRequest(destB, destB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(heads, "");

            _B_UA.AnswerRequest(destA, out string hopTail, 482);
            Assert.AreEqual(_B_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(hopTail, "");

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfA.Object);

        }
        [TestMethod]
        public void DesvioSinRespuesta()
        {
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            //A desvia a B
            _A_UA.RequestForward(uriDestB);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);

            string heads =_B_UA.ReceiveRequest(source, destA);
            Assert.AreEqual(heads, "A");

            _A_UA.AnswerRequest(destB, out string hopTail, 408);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(hopTail, "");
        }
        [TestMethod]
        public void DesvioCancelSinRespuesta()
        {
            File.Delete("Forward_" + source + ".xml");
            File.Delete("Forward_" + destB + ".xml");

            DesvioNegociacionAB();

            //Cancelacion desvio sin respuesta
            _A_UA.Cancel();
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            _B_UA.ReceiveRelease(source);

            _A_UA.AnswerRelease(destB, 400);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
        }
        [TestMethod]
        public void CaidaDesvioSimple()
        {
            //El cambio de estado lo hago antes de que se suscriban tlfForward para
            //Evitar pasar por OnTlfParticipantsObjObjtateChange
            tlfA.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfB.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;

            DesvioNegociacionAB();

            //Se cae puesto A y se borra el desvío en B           
            _B_UA.AutoRemoveDiversionSet(destA);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);

            //Se cae B y se borra el desvío en A
            _A_UA.AutoRemoveDiversionSet(destB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
        }

        [TestMethod]
        public void CaidaADesvioEncadenado()
        {
            //El cambio de estado lo hago antes de que se suscriban tlfForward para
            //Evitar pasar por OnTlfParticipantsObjObjtateChange
            tlfA.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfB.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfC.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            DesvioEncadenadoABC();

            //Se cae puesto A y se borra el desvío en C y en B se mantiene su desvio a C
            _B_UA.AutoRemoveDiversionSet( mockTlfA.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestC);

            _C_UA.AutoRemoveDiversionSet(mockTlfA.Object);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
        }
        [TestMethod]
        public void CaidaBDesvioEncadenado()
        {
            //El cambio de estado lo hago antes de que se suscriban tlfForward para
            //Evitar pasar por OnTlfParticipantstateChange
            tlfA.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfB.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfC.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            DesvioEncadenadoABC();

            //Se cae puesto B y se borra el desvío en C y en A se mantiene su desvio a C
            _A_UA.AutoRemoveDiversionSet(destB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].ListDiv[1], uriDestB);

            _C_UA.AutoRemoveDiversionSet(destB);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].ListDiv[1], uriDestB);

        }

        [TestMethod]
        public void CaidaCDesvioEncadenado()
        {
            //El cambio de estado lo hago antes de que se suscriban tlfForward para
            //Evitar pasar por OnTlfParticipantstateChange
            tlfA.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfB.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfC.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            DesvioEncadenadoABC();

            //Se cae puesto C y se borra el desvío en A y en B. Se negocia el desvio de A a B.
            _B_UA.AutoRemoveDiversionSet(mockTlfC.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);

            _A_UA.AutoRemoveDiversionSet(mockTlfC.Object);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Tail, uriDestB);

            File.Copy("Forward_" + source + ".xml", "ForwardReq_" + source + ".xml", true);
            string heads = _B_UA.ReceiveRequest(source, destA);
            Assert.AreEqual(heads, "A");

            _A_UA.AnswerRequest(destB, out string hopTail);

            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, "sip:" + source + "@10.68.60.132");
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(hopTail, "");

            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Head, "sip:" + source + "@10.68.60.132");
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].Tail, uriDestB);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);

            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfB.Object);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 1);
            Assert.AreEqual(_B_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfA.Object);

        }

        [TestMethod]
        public void CaidaCDesvioEncadenadoVariante()
        {
            string hopTail;
            //El cambio de estado lo hago antes de que se suscriban tlfForward para
            //Evitar pasar por OnTlfParticipantsObjtateChange
            tlfA.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfB.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            tlfC.State = HMI.Model.Module.BusinessEntities.TlfState.Unavailable;
            DesvioEncadenadoABC();

            //Se cae puesto C y se borra el desvío en A y en B. Se negocia el desvio de A a B.
            _A_UA.AutoRemoveDiversionSet(destC);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(_A_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._PendingDiversionSet[0].Tail, uriDestB);

            File.Copy("Forward_" + source + ".xml", "ForwardReq_" + source + ".xml", true);
            string heads = _B_UA.ReceiveRequest(source, destA);
            Assert.AreEqual(heads, "");

            File.Copy("Forward_" + destB + ".xml", "ForwardAnswer_" + destB + ".xml", true);
            _A_UA.AnswerRequest(destB, out hopTail);
            Assert.AreEqual(hopTail, "");

            File.Copy("Forward_" + destA + ".xml", "ForwardReqAnswer_" + destC + ".xml", true);
            _A_UA.AnswerRequest(destC, out hopTail, 400);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(hopTail, "");

            _B_UA.AutoRemoveDiversionSet(destC);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);

        }
        [TestMethod]
        public void DesvioEncadenadoCuadruple()
        {
            string hopTail;
            File.Delete("Forward_" + destD + ".xml");

            DesvioEncadenadoABC();
            //D desvia a A
            _D_UA.RequestForward(uriDestA);
            File.Copy("Forward_" + destD + ".xml", "ForwardReq_" + destD + ".xml", true);
            Assert.AreEqual(_D_UA._PendingDiversionSet.Count, 1);

            string heads = _A_UA.ReceiveRequest(destD, destD);
            File.Copy("Forward_" + destA + ".xml", "ForwardAnswerReq_" + destA + ".xml", true);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Head, uriDestA);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_A_UA._LocalDiversionSet[0].ListDiv[1], uriDestB);
            Assert.AreEqual(_A_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(heads, "");

            _D_UA.AnswerRequest(destA, out hopTail);
            File.Copy("Forward_" + destD + ".xml", "ForwardUpdate_" + destD + ".xml", true);
            Assert.AreEqual(_D_UA._PendingDiversionSet.Count, 1);
            Assert.AreEqual(_D_UA._PendingDiversionSet[0].Head, uriDestD);
            Assert.AreEqual(_D_UA._PendingDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_D_UA._PendingDiversionSet[0].ListDiv[1], uriDestB);
            Assert.AreEqual(_D_UA._PendingDiversionSet[0].ListDiv[2], uriDestA);
            Assert.AreEqual(_D_UA._LocalDiversionSet.Count, 0);
            Assert.AreEqual(hopTail, "");

            heads = _C_UA.ReceiveRequest(destD, destD);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 3);
            Assert.AreEqual(_C_UA._LocalDiversionSet[2].Head, uriDestD);
            Assert.AreEqual(_C_UA._LocalDiversionSet[2].Tail, uriDestC);
            Assert.AreEqual(_C_UA._LocalDiversionSet[2].ListDiv[1], uriDestB);
            Assert.AreEqual(_C_UA._LocalDiversionSet[2].ListDiv[2], uriDestA);
            Assert.AreEqual(_C_UA._LocalDiversionSet[2].TlfParticipantsObj.Count, 3);
            Assert.AreEqual(heads, "B, A, D");
            _D_UA.AnswerRequest(destC, out hopTail);
            Assert.AreEqual(_D_UA._LocalDiversionSet.Count, 1);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].Head, uriDestD);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].Tail, uriDestC);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].ListDiv[1], uriDestB);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].ListDiv[2], uriDestA);
            Assert.AreEqual(hopTail, "C");

            Assert.AreEqual(_D_UA._LocalDiversionSet[0].TlfParticipantsObj.Count, 3);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].TlfParticipantsObj[0], mockTlfC.Object);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].TlfParticipantsObj[1], mockTlfB.Object);
            Assert.AreEqual(_D_UA._LocalDiversionSet[0].TlfParticipantsObj[2], mockTlfA.Object);

            //Cancelacion
            File.Delete("Forward_" + destA + ".xml");
            File.Delete("Forward_" + destB + ".xml");
            File.Delete("Forward_" + destC + ".xml");
            File.Delete("Forward_" + destD + ".xml");

            //B cancela desvío a C
            _B_UA.Cancel();
            File.Copy("Forward_" + destB + ".xml", "ForwardRel_" + destB + ".xml", true);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 1);

            _C_UA.ReceiveRelease(destB);
            File.Copy("Forward_" + destC + ".xml", "ForwardRelAnswer1_" + destC + ".xml", true);
            Assert.AreEqual(_C_UA._LocalDiversionSet.Count, 0);

            _B_UA.AnswerRelease(destC);
            Assert.AreEqual(_B_UA._LocalDiversionSet.Count, 0);
        }
    }
    }
