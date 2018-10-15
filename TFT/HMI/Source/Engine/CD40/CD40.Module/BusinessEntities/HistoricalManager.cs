using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

using HMI.CD40.Module.Properties;
using HMI.Model.Module.BusinessEntities;

namespace HMI.CD40.Module.BusinessEntities
{
    class HistoricalManager
    {
        public enum AccessType { AD, AI };

        private static string _Doc = Settings.Default.PathHistoricalOfLocalCalls;

        /// <summary>
        /// Anade un registro al fichero XML
        /// </summary>
        /// <param name="llamadas"></param>
        /// <param name="user"></param>
        /// <param name="access"></param>
        /// <param name="target"></param>
        private static void AddRecord(ref XElement llamadas, AccessType access, string target)
        {
            // Eliminar el registro con la llamada más antigua
            var calls = from el in llamadas.Elements() select el;
            if (calls.Count() >= 10)
                calls.ElementAt(calls.Count() - 1).Remove();

            // Actualizar valor de la última llamada
            llamadas.SetAttributeValue("Ultima", target);

            // El último elemento es el más reciente
            llamadas.AddFirst(new XElement("Call", new XAttribute("Fecha_Hora", DateTime.Now.ToString()),
                                               new XAttribute("Acceso", access.ToString()),
                                               new XAttribute("Colateral", target)));

        }

        /// <summary>
        /// Añade una llamada del tipo callType al histórico local de llamadas.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="access"></param>
        /// <param name="target"></param>
        public static void AddCall(string callType, string user, AccessType access, string target)
        {
            try
            {
                XElement file = XElement.Load(_Doc);

                var iPuesto = from el in file.Elements("Puesto")
                              where (string)el.Attribute("Valor") == user
                              select el;

                // No existen llamadas para este usuario, se crea la lista de llamadas vacía
                if (iPuesto.Count() == 0)
                {
                    file.Add(new XElement("Puesto", new XAttribute("Valor", user),
                                                new XElement("Llamadas", new XAttribute("Tipo", "Entrantes"), new XAttribute("Ultima", "")),
                                                new XElement("Llamadas", new XAttribute("Tipo", "Salientes"), new XAttribute("Ultima", "")),
                                                new XElement("Llamadas", new XAttribute("Tipo", "NoAtendidas"), new XAttribute("Ultima", ""))));

                    file.Save(_Doc);
                    AddCall(callType, user, access, target);

                    //iPuesto = from el in file.Elements("Puesto")
                    //          where (string)el.Attribute("Valor") == target
                    //          select el;
                }

                // Llamadas del puesto=<user> y del Tipo=<callType>
                var llamadas = from l in iPuesto.Elements("Llamadas")
                               where (string)l.Attribute("Tipo") == callType
                               select l;

                foreach (var llamada in llamadas)
                {
                    XElement elemento = llamada;
                    AddRecord(ref elemento, access, target);
                }

                file.Save(_Doc);
            }
            catch (System.IO.FileNotFoundException)
            {
                XDocument doc = new XDocument(new XElement("HistoricoLocalDeLlamadas",
                                                    new XElement("Puesto", new XAttribute("Valor", user),
                                                        new XElement("Llamadas", new XAttribute("Tipo", "Entrantes"), new XAttribute("Ultima", "")),
                                                        new XElement("Llamadas", new XAttribute("Tipo", "Salientes"), new XAttribute("Ultima", "")),
                                                        new XElement("Llamadas", new XAttribute("Tipo", "NoAtendidas"), new XAttribute("Ultima", "")))));
                doc.Save(_Doc);

                AddCall(callType, user, access, target);
            }
        }

        public static List<LlamadaHistorica> GetHistoricalCalls(string puesto)
        {
            List<LlamadaHistorica> historyList = new List<LlamadaHistorica>();

            try
            {
                XElement file = XElement.Load(_Doc);

                var iPuesto = from el in file.Elements("Puesto")
                              where (string)el.Attribute("Valor") == puesto
                              select el;

                foreach (var callType in iPuesto.Elements("Llamadas"))
                {
                    foreach (var call in callType.Elements("Call"))
                    {
                        LlamadaHistorica llamada = new LlamadaHistorica();

                        llamada.Tipo = callType.Attribute("Tipo").Value;
                        llamada.Ultima = callType.Attribute("Ultima").Value;

                        llamada.Fecha_Hora = call.Attribute("Fecha_Hora").Value;
                        llamada.Acceso = call.Attribute("Acceso").Value;
                        llamada.Colateral = call.Attribute("Colateral").Value;

                        historyList.Add(llamada);
                    }
                }
            }
            catch (System.IO.FileNotFoundException )
            {
            }

            return historyList;
        }
    }
}
