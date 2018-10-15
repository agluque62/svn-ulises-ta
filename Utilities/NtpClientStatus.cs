using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Utilities
{
    public class NtpClientStatus : IDisposable
    {
        #region Public
        public enum NtpClient { Windows }
        public NtpClientStatus(NtpClient client = NtpClient.Windows)
        {
            Cliente = client;
        }
        public void Dispose()
        {
        }
        public List<string> Status
        {
            get
            {
                try
                {
                    switch (Cliente)
                    {
                        case NtpClient.Windows:
                            // 20170608. AGL. Se utilizará siempre el servicio de MEINBERG analogo al de las pasarelas
                            return NtpqPeers;
                        // return W32tmStatus;
                        default:
                            return ErrorStatus("No Implementado");
                    }
                }
                catch (Exception x)
                {
                    return ErrorStatus(x.Message);
                }
            }
        }
        #endregion

        #region Private
        private NtpClient Cliente { get; set; }
        private List<string> W32tmStatus
        {
            get
            {
                List<string> status = new List<string>();
                ProcessStartInfo psi = new ProcessStartInfo("w32tm", " /query /peers")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                var proc = Process.Start(psi);
                string Text = proc.StandardOutput.ReadToEnd();

                using (StringReader reader = new StringReader(Text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != String.Empty)
                            status.Add(Normalizar(line));
                    }
                }

                return status;
            }
        }
        private List<string> NtpqPeers
        {
            get
            {
                List<string> status = new List<string>();
                ProcessStartInfo psi = new ProcessStartInfo("ntpq", " -p")
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                var proc = Process.Start(psi);
                string Text = proc.StandardOutput.ReadToEnd();

                using (StringReader reader = new StringReader(Text))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line != String.Empty)
                            status.Add(Normalizar(line));
                    }
                }

                return status;
            }
        }
        private List<string> ErrorStatus(string error)
        {
                List<string> status = new List<string>()
                {
                    String.Format("Error: {0}", error) 
                };
                return status;
        }
        private String Normalizar(String inputString)
        {
            // var inputString = "Mañana será otro día";
            var normalizedString = inputString.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            for (int i = 0; i < normalizedString.Length; i++)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(normalizedString[i]);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(normalizedString[i]);
                }
            }
            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
        #endregion
    }
}
