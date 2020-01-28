using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading.Tasks;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class GenericTest
    {
        [TestMethod]
        public void ConcurrentLockTest()
        {
            object locker = new object();
            lock (locker)
            {
                Debug.WriteLine("Lock Primer Nivel..");
                lock (locker)
                {
                    Debug.WriteLine("Lock Segundo Nivel..");
                }
                Task.Factory.StartNew(() =>
                {
                    lock (locker)
                    {
                        Debug.WriteLine("Lock Tercer Nivel..");
                        Task.Delay(1000).Wait();

                        Debug.WriteLine("Provocando Salida..");
                        return;
//                        throw new Exception("Excepcion Provocada....");
                    }
                });
            }
            Task.Delay(100).Wait();
            lock (locker)
            {
                Debug.WriteLine("Lock Cuarto Nivel..");
            }
        }
    }
}
