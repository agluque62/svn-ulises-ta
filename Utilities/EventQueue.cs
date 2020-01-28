using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using NLog;

namespace Utilities
{
	public delegate void QEventHandler();

	public class EventQueue
	{
		public void Start()
		{
			Debug.Assert(_Stop);

			_NewEvent = new AutoResetEvent(false);
			_StopEvent = new ManualResetEvent(false);
			_WorkingThread = new Thread(ProcessEvents);
            _WorkingThread.IsBackground = true;

			_Stop = false;
			_WorkingThread.Start();
		}
        public void Start(string name, int overloadThreshold)
        {
            Debug.Assert(_Stop);

            _NewEvent = new AutoResetEvent(false);
            _StopEvent = new ManualResetEvent(false);
            _WorkingThread = new Thread(ProcessEvents);
            _WorkingThread.IsBackground = true;

            _Stop = false;
            _WorkingThread.Start();
            _Name = name;
            _OverloadThreshold = overloadThreshold;
        }

		public void Stop()
		{
			lock (_Queue)
			{
				if (_Stop)
				{
					return;
				}

				_Stop = true;
			}

			_StopEvent.Set();
			_WorkingThread.Join(5000);
			_Queue.Clear();
			_NewEvent.Close();
			_StopEvent.Close();
		}

		public void InternalStop()
		{
			lock (_Queue)
			{
				_Stop = true;
			}

			_Queue.Clear();
			_NewEvent.Close();
			_StopEvent.Close();
		}

		public void Enqueue(string id, QEventHandler handler)
		{
			if (Thread.CurrentThread.ManagedThreadId == _WorkingThread.ManagedThreadId)
			{
				if (!_Stop)
				{
					try
					{
						handler();
					}
					catch (Exception x)
					{
                        LogManager.GetCurrentClassLogger().Error("Event Queue ERROR {1} ejecutando {0}", id, x.Message);
					}
				}
			}
			else
			{
				lock (_Queue)
				{
					if (!_Stop)
					{
						_Queue.Enqueue(new Event(id, handler));
						_NewEvent.Set();
                        string listaEventos = "";
                        if (_Queue.Count > _OverloadThreshold)
                        {
							//Do not print too much
                            if (_Queue.Count % 100 == 0 && _Queue.Count < 1000)
                            {
                                LogManager.GetCurrentClassLogger().Info("Event Queue {1} overloaded {0} with {2}", _Queue.Count, _Name, id);
                                foreach (Event evento in _Queue.ToArray())
                                {
                                    listaEventos += " ";
                                    listaEventos += evento.Id;
                                }
                            //LogManager.GetCurrentClassLogger().Trace("ev:{0} ", listaEventos);                           
                            }
                        }
					}
				}
			}
		}

		#region Private Members

		class Event
		{
			public string Id;
			public QEventHandler Handler;
			public bool Valid;

			public Event(string id, QEventHandler handler)
			{
				Id = id;
				Handler = handler;
				Valid = true;
			}
		}

		// private static Logger _Logger = LogManager.GetCurrentClassLogger();

		private bool _Stop = true;
		private Queue<Event> _Queue = new Queue<Event>(100);
		private AutoResetEvent _NewEvent;
		private ManualResetEvent _StopEvent;
		private Thread _WorkingThread;
        private string _Name = "NoName";
        private int _OverloadThreshold = 200;
		private void ProcessEvents()
		{
			WaitHandle[] waitHandles = new WaitHandle[] { _StopEvent, _NewEvent };

			while (!_Stop)
			{
				Event ev = null;

				lock (_Queue)
				{
					if (_Queue.Count > 0)
					{
						ev = _Queue.Dequeue();
					}
					else
					{
						_NewEvent.Reset();
					}
				}

				if (ev == null)
				{
					if (WaitHandle.WaitAny(waitHandles) == 0)
					{
						break;
					}
					else
					{
						lock (_Queue)
						{
							Debug.Assert(_Queue.Count > 0);
							ev = _Queue.Dequeue();
						}
					}
				}

				if (ev.Valid)
				{
 					try
					{
						ev.Handler();
					}
					catch (Exception x)
					{
						LogManager.GetCurrentClassLogger().Error("EventQueue ERROR {1} ejecutando {0} en {2}", ev.Id, x.Message, x.StackTrace);
					}
				}
			}
		}

		#endregion
	}
}
