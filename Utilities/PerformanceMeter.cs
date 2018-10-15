using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

using NLog;

namespace Utilities
{
    public class PerformanceMeter
    {
    }
    public class TimeMeasurement : PerformanceMeter
    {
        Stopwatch watch;
        String Id { get; set; }
        public TimeMeasurement(String name = "Generico")
        {
            Id = name;
            watch = new Stopwatch();
            watch.Start();
        }
        public void StopAndPrint(string etiqueta = "", [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            watch.Stop();
#if DEBUG
            LogLevel level = LogLevel.Warn;
#else
            LogLevel level = LogLevel.Trace;
#endif
            //StackTrace stackTrace = new StackTrace();           // get call stack
            //StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)
            //StackFrame callingFrame = stackFrames[1];
            //MethodBase method = callingFrame.GetMethod();

            //LogManager.GetCurrentClassLogger().Warn(String.Format("From [{0}.{1} [line {2}]]: Tiempo Transcurrido: {3} ms.",
            //    method.ReflectedType,
            //    method.Name,
            //    lineNumber,
            //    watch.ElapsedMilliseconds));

            LogManager.GetCurrentClassLogger().Log(level, String.Format("[{0,-16}<{1,-8}>]: Tiempo Medido: {2,6} ms.",
                Id,
                etiqueta,
                watch.ElapsedMilliseconds));
        }
    }
}
