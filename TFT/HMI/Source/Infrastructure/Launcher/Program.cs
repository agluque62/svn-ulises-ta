using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace Launcher
{
	class Program
	{
		static int Main(string[] args)
		{
            NLogConfigure();

			if (args.Length < 1)
			{
				return 1;
			}

			try
			{
				string process = Path.GetFileNameWithoutExtension(args[0]);
				Process[] running = Process.GetProcessesByName(process);

				foreach (Process p in running)
				{
					p.CloseMainWindow();
				}

				for (int i = 0; i < 10; i++)
				{
					foreach (Process p in running)
					{
						if (!p.HasExited)
						{
							Thread.Sleep(500);
							break;
						}
					}
				}

				foreach (Process p in running)
				{
					if (!p.HasExited)
					{
						p.Kill();
					}
				}
                /** 20170720. AGL. TODO. Ejecutar la Tarea Programada, si no existe o da algun tipo de error, ejecutar el programa.. */
                using (Process task = Process.Start(new ProcessStartInfo() {
                    FileName = "schtasks.exe", 
                    Arguments = " /RUN /TN UV5K-HMI-START", 
                    UseShellExecute = false, 
                    CreateNoWindow=true, 
                    RedirectStandardOutput=true,
                    RedirectStandardError=true
                }))
                {
                    string error = task.StandardError.ReadToEnd();
                    if (String.IsNullOrEmpty(error)==false)
                    {
                        Log(String.Format("Launcher UV5K-HMI-START Error: {0}. Starting {1}", error, args[0]));
                        Process app = (args.Length == 1) ? Process.Start(args[0]) : Process.Start(args[0], args[1]);
                        app.Close();
                    }
                    else
                    {
                        string result = task.StandardOutput.ReadToEnd();
                        Log(String.Format("Launcher UV5K-HMI-START Success: {0}", result));
                    }
                }
                //Process app = (args.Length == 1) ? Process.Start(args[0]) : Process.Start(args[0], args[1]);
                //app.Close();
			}
			catch (Exception x) 
			{
                Log("Excepcion: " + x.Message);
				return 1;
			}

			return 0;
		}

        static void NLogConfigure()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var ftarget = new NLog.Targets.FileTarget() { FileName = "${basedir}/launcher.txt", Layout = "${longdate}: ${message}" };
            var rule = new NLog.Config.LoggingRule("*", NLog.LogLevel.Debug, ftarget);
            config.AddTarget("file", ftarget);
            config.LoggingRules.Add(rule);
            NLog.LogManager.Configuration = config;
        }

        static void Log(string msg)
        {
            NLog.LogManager.GetLogger("launcher").Info(msg);
        }
    }
}
