using System;
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
    }
}
