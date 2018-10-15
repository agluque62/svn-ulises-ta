using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using LoggerImp = Microsoft.Practices.EnterpriseLibrary.Logging.Logger;
using LogEntry = Microsoft.Practices.EnterpriseLibrary.Logging.LogEntry;
using ExceptionFormatter = Microsoft.Practices.EnterpriseLibrary.Logging.ExceptionFormatter;
using LoggingException = Microsoft.Practices.EnterpriseLibrary.Logging.LoggingException;

namespace HMI.Infrastructure.Interface
{
	public static class Logger
	{
		// Fields
		private static bool _IsFatalEnabled = false;
		private static bool _IsErrorEnabled = false;
		private static bool _IsWarnEnabled = false;
		private static bool _IsInfoEnabled = false;
		private static bool _IsDebugEnabled = false;
		private static bool _IsTraceEnabled = false;
		private static List<string> _Categories;

		// Properties
		public static bool IsFatalEnabled { get { return _IsFatalEnabled; } }
		public static bool IsErrorEnabled { get { return _IsErrorEnabled; } }
		public static bool IsWarnEnabled { get { return _IsWarnEnabled; } }
		public static bool IsInfoEnabled { get { return _IsInfoEnabled; } }
		public static bool IsDebugEnabled { get { return _IsDebugEnabled; } }
		public static bool IsTraceEnabled { get { return _IsTraceEnabled; } }

		// Methods
		static Logger()
		{
			if (LoggerImp.IsLoggingEnabled())
			{
				_Categories = new List<string>();
				_Categories.Add("General");

				LogEntry ev = new LogEntry();
				ev.EventId = 100;
				ev.Categories = _Categories;

				ev.Severity = TraceEventType.Critical;
				ev.Priority = 0;
				_IsFatalEnabled = LoggerImp.ShouldLog(ev);

				ev.Severity = TraceEventType.Error;
				ev.Priority = 1;
				_IsErrorEnabled = LoggerImp.ShouldLog(ev);

				ev.Severity = TraceEventType.Warning;
				ev.Priority = 2;
				_IsWarnEnabled = LoggerImp.ShouldLog(ev);

				ev.Severity = TraceEventType.Information;
				ev.Priority = 3;
				_IsInfoEnabled = LoggerImp.ShouldLog(ev);

				ev.Severity = TraceEventType.Verbose;
				ev.Priority = 4;
				_IsDebugEnabled = LoggerImp.ShouldLog(ev);

				ev.Severity = TraceEventType.Transfer;
				ev.Priority = 5;
				_IsTraceEnabled = LoggerImp.ShouldLog(ev);
			}
		}

		public static void Fatal(object obj)
		{
			if (_IsFatalEnabled)
			{
				LogEntry ev = new LogEntry(obj, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message)
		{
			if (_IsFatalEnabled)
			{
				LogEntry ev = new LogEntry(message, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, bool argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, char argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, int argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, byte argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, decimal argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, double argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, long argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LoggerImp.Write(msg, "General", 0, 100, TraceEventType.Critical);
			}
		}
		public static void Fatal(string message, object argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, sbyte argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, float argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, string argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, uint argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, ulong argument)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, object arg1, object arg2)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, arg1, arg2);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, object arg1, object arg2, object arg3)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, arg1, arg2, arg3);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Fatal(string message, params object[] args)
		{
			if (_IsFatalEnabled)
			{
				string msg = string.Format(message, args);
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void FatalException(string message, Exception exception)
		{
			if (_IsFatalEnabled)
			{
				ExceptionFormatter formater = new ExceptionFormatter();
				string msg = formater.GetMessage(new LoggingException(message, exception));
				LogEntry ev = new LogEntry(msg, _Categories, 0, 100, TraceEventType.Critical, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}

		public static void Error(object obj)
		{
			if (_IsErrorEnabled)
			{
				LogEntry ev = new LogEntry(obj, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message)
		{
			if (_IsErrorEnabled)
			{
				LogEntry ev = new LogEntry(message, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, bool argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, byte argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, char argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, decimal argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, double argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, int argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, string argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, long argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, object argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, sbyte argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, float argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, uint argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, ulong argument)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, object arg1, object arg2)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, arg1, arg2);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, object arg1, object arg2, object arg3)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, arg1, arg2, arg3);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Error(string message, params object[] args)
		{
			if (_IsErrorEnabled)
			{
				string msg = string.Format(message, args);
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void ErrorException(string message, Exception exception)
		{
			if (_IsErrorEnabled)
			{
				ExceptionFormatter formater = new ExceptionFormatter();
				string msg = formater.GetMessage(new LoggingException(message, exception));
				LogEntry ev = new LogEntry(msg, _Categories, 1, 100, TraceEventType.Error, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}

		public static void Warn(object obj)
		{
			if (_IsWarnEnabled)
			{
				LogEntry ev = new LogEntry(obj, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message)
		{
			if (_IsWarnEnabled)
			{
				LogEntry ev = new LogEntry(message, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, bool argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, byte argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, char argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, decimal argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, double argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, int argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, long argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, ulong argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, string argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, uint argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, float argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, object argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, sbyte argument)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, object arg1, object arg2)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, arg1, arg2);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, object arg1, object arg2, object arg3)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, arg1, arg2, arg3);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Warn(string message, params object[] args)
		{
			if (_IsWarnEnabled)
			{
				string msg = string.Format(message, args);
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void WarnException(string message, Exception exception)
		{
			if (_IsWarnEnabled)
			{
				ExceptionFormatter formater = new ExceptionFormatter();
				string msg = formater.GetMessage(new LoggingException(message, exception));
				LogEntry ev = new LogEntry(msg, _Categories, 2, 100, TraceEventType.Warning, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}

		public static void Info(object obj)
		{
			if (_IsInfoEnabled)
			{
				LogEntry ev = new LogEntry(obj, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message)
		{
			if (_IsInfoEnabled)
			{
				LogEntry ev = new LogEntry(message, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, bool argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, byte argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, char argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, decimal argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, double argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, int argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, long argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, object argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, sbyte argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, float argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, string argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, ulong argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, uint argument)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, object arg1, object arg2)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, arg1, arg2);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, object arg1, object arg2, object arg3)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, arg1, arg2, arg3);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Info(string message, params object[] args)
		{
			if (_IsInfoEnabled)
			{
				string msg = string.Format(message, args);
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void InfoException(string message, Exception exception)
		{
			if (_IsInfoEnabled)
			{
				ExceptionFormatter formater = new ExceptionFormatter();
				string msg = formater.GetMessage(new LoggingException(message, exception));
				LogEntry ev = new LogEntry(msg, _Categories, 3, 100, TraceEventType.Information, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}

		public static void Debug(object obj)
		{
			if (_IsDebugEnabled)
			{
				LogEntry ev = new LogEntry(obj, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message)
		{
			if (_IsDebugEnabled)
			{
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, bool argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, byte argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, char argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, decimal argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, double argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, int argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, long argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, object argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, sbyte argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, float argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, string argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, uint argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, ulong argument)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, object arg1, object arg2)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, arg1, arg2);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, object arg1, object arg2, object arg3)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, arg1, arg2, arg3);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Debug(string message, params object[] args)
		{
			if (_IsDebugEnabled)
			{
				string msg = string.Format(message, args);
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void DebugException(string message, Exception exception)
		{
			if (_IsDebugEnabled)
			{
				ExceptionFormatter formater = new ExceptionFormatter();
				string msg = formater.GetMessage(new LoggingException(message, exception));
				LogEntry ev = new LogEntry(message, _Categories, 4, 100, TraceEventType.Verbose, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}

		public static void Trace(object obj)
		{
			if (_IsTraceEnabled)
			{
				LogEntry ev = new LogEntry(obj, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message)
		{
			if (_IsTraceEnabled)
			{
				LogEntry ev = new LogEntry(message, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, bool argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, byte argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, char argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, decimal argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, double argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, int argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, long argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, object argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, sbyte argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, float argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, string argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, uint argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, ulong argument)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, argument);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, object arg1, object arg2)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, arg1, arg2);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, object arg1, object arg2, object arg3)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, arg1, arg2, arg3);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void Trace(string message, params object[] args)
		{
			if (_IsTraceEnabled)
			{
				string msg = string.Format(message, args);
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}
		public static void TraceException(string message, Exception exception)
		{
			if (_IsTraceEnabled)
			{
				ExceptionFormatter formater = new ExceptionFormatter();
				string msg = formater.GetMessage(new LoggingException(message, exception));
				LogEntry ev = new LogEntry(msg, _Categories, 5, 100, TraceEventType.Transfer, "", null);
				LoggerImp.Writer.Write(ev);
			}
		}

		//public static bool IsEnabled(LogLevel level);

		//public static void Log(LogLevel level, object obj);
		//public static void Log(LogLevel level, string message);
		//public static void Log(LogLevel level, string message, bool argument);
		//public static void Log(LogLevel level, string message, byte argument);
		//public static void Log(LogLevel level, string message, char argument);
		//public static void Log(LogLevel level, string message, decimal argument);
		//public static void Log(LogLevel level, string message, double argument);
		//public static void Log(LogLevel level, string message, int argument);
		//public static void Log(LogLevel level, string message, long argument);
		//public static void Log(LogLevel level, string message, object argument);
		//public static void Log(LogLevel level, string message, sbyte argument);
		//public static void Log(LogLevel level, string message, float argument);
		//public static void Log(LogLevel level, string message, string argument);
		//public static void Log(LogLevel level, string message, uint argument);
		//public static void Log(LogLevel level, string message, ulong argument);
		//public static void Log(LogLevel level, string message, object arg1, object arg2);
		//public static void Log(LogLevel level, string message, object arg1, object arg2, object arg3);
		//public static void Log(LogLevel level, string message, params object[] args);
		//public static void LogException(LogLevel level, string message, Exception exception);
	}
}
