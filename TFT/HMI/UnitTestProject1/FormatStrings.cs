using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System;

namespace UnitTestProject1
{
    [TestClass]
    public class FormatStrings
    {
        [TestMethod]
        public void TestMethod1()
        {
            String s1 = "A12345678Z";
            String s2 = "B12345678901234Y";
            String s3 = "C123456789012345678X";
            String s4 = "juan" + s1 + s2 + s3 + "pepe";

            Debug.WriteLine(GenIdAgrupacion(s1));
            Debug.WriteLine(GenIdAgrupacion(s2));
            Debug.WriteLine(GenIdAgrupacion(s3));
            Debug.WriteLine(GenIdAgrupacion(s4));
        }
        //LALM 210618
        // Funcion Que limita el numero maximo de caracteres de una agrupacion a 16.
        private String GenIdAgrupacion(String agrupacion)
        {
            int longmax = 16;
            String IdAgrupacion = agrupacion;
            int len = agrupacion.Length;
            if (len > longmax)
            {
                int mitad = longmax / 2;
                IdAgrupacion = agrupacion.Substring(0, mitad - 1) + ".." + agrupacion.Substring(len - (mitad - 1), mitad-1);
            }
            return IdAgrupacion;
        }
    }
}
