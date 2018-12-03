using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using Newtonsoft.Json;
using NLog;

namespace Utilities
{
    public class ServicesHelpers
    {
        public class ManagedSemaphore
        {
            public ManagedSemaphore(int initialCount, int maxCount, String id, int maxTime)
            {
                _semaphore = new Semaphore(initialCount, maxCount);
                Id = id;
                Maxtime = maxTime;
                OccupiedBy = "";
            }
            public bool Acquire([CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
            {
#if !DEBUG1
                if (_semaphore == null)
                    return Throw("SEM not Found", false, lineNumber, caller);
                if (_semaphore.WaitOne(Maxtime) == false)
                    return Throw("SEM Timeout", false, lineNumber, caller);
#endif
                OccupiedBy = String.Format("[{0}-{1}]", caller, lineNumber);
                return true;
            }
            public void Release(bool launch = false, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
            {
#if !DEBUG1
                if (_semaphore == null)
                    Throw("SEM not Found", false, lineNumber, caller);
                if (_semaphore.WaitOne(0) == true)
                    Throw("SEM Release Error", false, lineNumber, caller);
                _semaphore.Release();
#endif
                OccupiedBy = "";
            }
            protected bool Throw(string msg, bool launch = false, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
            {
                Logger _logger = LogManager.GetLogger("ManagedSemaphore");
                _logger.Log(LogLevel.Fatal, String.Format("{0}: {1}, From: [{2}-{3}], Last: [{4}]", Id, msg, caller, lineNumber, OccupiedBy));
#if DEBUG
                if (launch)
                    throw new Exception(msg);
#endif
                return false;
            }
            /** */
            protected string Id { get; set; }
            protected string OccupiedBy { get; set; }
            protected int Maxtime { get; set; }
            protected Semaphore _semaphore { get; set; }
        }

        public class DummySemaphore
        {
            public DummySemaphore(int initialCount, int maxCount, String id, int maxTime) { }
            public bool Acquire() { return true; }
            public void Release(bool launch = false) { }
        }

        public class TraceInOut<T>
        {
            public TraceInOut()
            {
#if !DEBUG
                _logger = LogManager.GetLogger(typeof(T).Name);
#endif
                Counter = 0;
            }
            public Int64 TraceIn([System.Runtime.CompilerServices.CallerMemberName] string caller = null)
            {
                String msg = String.Format("{0}: {1} Invoked", Counter, caller);
#if DEBUG
                Debug.WriteLine(msg);
#else
                _logger.Trace(msg);
#endif
                return Counter++;
            }
            public void TraceOut(Int64 Val, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
            {
                String msg = String.Format("{0}: {1} Executed", Counter, caller);
#if DEBUG
                Debug.WriteLine(msg);
#else
                _logger.Trace(msg);
#endif
            }
            protected Int64 Counter { get; set; }
            Logger _logger = null;
        }

        public static string SerializeObject(object data) { return JsonConvert.SerializeObject(data, Formatting.Indented); }
        public static T DeserializeObject<T>(string data) { return JsonConvert.DeserializeObject<T>(data); }

        /// <summary>
        /// Establece si un string contiene algun elemento de una lista....
        /// </summary>
        /// <param name="val"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool ListElementInString(string val, List<string> list)
        {
            var res = list.Where(e => val.ToLower().Contains(e.ToLower())).ToList().Count;
            return res > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        const string ficheroversiones = "versiones.json";
        const string oldName = "nodebox";
        public static void VersionsFileAdjust(string newModuleName, List<string> newServicesNamesList)
        {
            string ficheroversionesbackup = ficheroversiones + ".bkp";
            if (File.Exists(ficheroversiones))
            {
                VersionDetails.VersionData actualVersion = new VersionDetails(ficheroversiones, false).version;

                if (actualVersion.Version != "")
                {
                    VersionDetails.VersionData backupVersion = new VersionDetails(ficheroversionesbackup, false).version;

                    if (actualVersion.Version != backupVersion.Version)
                    {
                        File.WriteAllText(ficheroversionesbackup, File.ReadAllText(ficheroversiones));
                    }
                }

                if (newModuleName.ToLower() == oldName.ToLower())
                {
                    // Recupero el fichero original...
                    File.WriteAllText(ficheroversiones, File.ReadAllText(ficheroversionesbackup));
                }
                else
                {
                    actualVersion.Components.ForEach(c =>
                    {
                        // Cambiar los titulos de los modulos.
                        if (c.Name.ToLower().Contains(oldName.ToLower()) == true)
                        {
                            c.Name = c.Name.ToUpper().Replace(oldName.ToUpper(), newModuleName.ToUpper());
                        }

                        // Cambiar los path de ficheros que correspondan y
                        c.Files.ForEach(file =>
                        {
                            if (file.Path.ToLower().Contains(oldName.ToLower()) == true)
                            {
                                file.Path = file.Path.ToLower().Replace(oldName.ToLower(), newModuleName.ToLower());
                            }
                        });

                        // Eliminar los que correspondan.
                        if (c.Name.ToUpper().Contains("SERVICES"))
                        {
                            c.Files = c.Files.Where(f => ServicesHelpers.ListElementInString(f.Path, newServicesNamesList)).ToList();
                        }
                    });

                    // Escribir el real...                
                    File.WriteAllText(ficheroversiones, actualVersion.ToString());
                }
            }
            else
            {
                File.WriteAllText("Errores.out.txt", "No encuentro el fichero de versiones...");
            }
        }
    }
}
