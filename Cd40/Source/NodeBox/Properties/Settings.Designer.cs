﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.42000
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace U5ki.NodeBox.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ControlRemoto {
            get {
                return ((bool)(this["ControlRemoto"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1023")]
        public int PuertoControlRemoto {
            get {
                return ((int)(this["PuertoControlRemoto"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1022")]
        public int PuertoPresencia {
            get {
                return ((int)(this["PuertoPresencia"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int MinutosSesion {
            get {
                return ((int)(this["MinutosSesion"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("departamento")]
        public string IdSistema {
            get {
                return ((string)(this["IdSistema"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".1.1.100.1")]
        public string HistBaseOid {
            get {
                return ((string)(this["HistBaseOid"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("public")]
        public string HistCommunity {
            get {
                return ((string)(this["HistCommunity"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>HistBaseOid</string>\r\n  <string>HistCommunity</string>\r\n  <string>IdSistem" +
            "a</string>\r\n  <string>DefaultCodec</string>\r\n  <string>DefaultDelayBufPframes</s" +
            "tring>\r\n  <string>DefaultJBufPframes</string>\r\n  <string>InvProceedingDiaTout</s" +
            "tring>\r\n  <string>InvProceedingIaTout</string>\r\n  <string>InvProceedingMonitorin" +
            "gTout</string>\r\n  <string>InvProceedingRdTout</string>\r\n  <string>KAMultiplier</" +
            "string>\r\n  <string>KAPeriod</string>\r\n  <string>RxLevel</string>\r\n  <string>SndS" +
            "amplingRate</string>\r\n  <string>FiltroSettings</string>\r\n  <string>TsxTout</stri" +
            "ng>\r\n  <string>TxLevel</string>\r\n  <string>ConectionRetryTimer</string>\r\n  <stri" +
            "ng>HFBalanceoAsignacion</string>\r\n  <string>HFMonitorTimer</string>\r\n  <string>H" +
            "FSnmpReadCommunity</string>\r\n  <string>HFSnmpSimula</string>\r\n  <string>HFSnmpWr" +
            "iteCommunity</string>\r\n  <string>ListenIp</string>\r\n  <string>ListenPort</string" +
            ">\r\n  <string>McastIp</string>\r\n  <string>McastPortBegin</string>\r\n  <string>Moni" +
            "torCarrierTimeOut</string>\r\n  <string>SipPort</string>\r\n  <string>SipUser</strin" +
            "g>\r\n  <string>tifxMcastIp</string>\r\n  <string>tifxMcastPort</string>\r\n  <string>" +
            "tifxPresenceTout</string>\r\n  <string>PabxSimulada</string>\r\n  <string>TifxMcastI" +
            "p</string>\r\n  <string>TifxMcastPort</string>\r\n  <string>PabxPollTime</string>\r\n " +
            " <string>PabxSaPwd</string>\r\n  <string>PabxWsPort</string>\r\n  <string>ConfigByte" +
            "</string>\r\n  <string>Cd40_CfgService_SoapCfg_InterfazSOAPConfiguracion</string>\r" +
            "\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection FiltroSettings {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["FiltroSettings"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("20")]
        public int NetworkOnDelay {
            get {
                return ((int)(this["NetworkOnDelay"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool MNSimulating {
            get {
                return ((bool)(this["MNSimulating"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int IgmpQueryPeriodSeconds {
            get {
                return ((int)(this["IgmpQueryPeriodSeconds"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10000")]
        public uint ListenPort {
            get {
                return ((uint)(this["ListenPort"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6060")]
        public uint SipPort {
            get {
                return ((uint)(this["SipPort"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("UV5KI")]
        public string SipUser {
            get {
                return ((string)(this["SipUser"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("192.168.2.3")]
        public string HistServer {
            get {
                return ((string)(this["HistServer"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("192.168.2.202")]
        public string IpPrincipal {
            get {
                return ((string)(this["IpPrincipal"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("7060")]
        public uint SipPortPhone {
            get {
                return ((uint)(this["SipPortPhone"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("256")]
        public uint MaxCalls {
            get {
                return ((uint)(this["MaxCalls"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("224.10.10.51")]
        public string ListenIp {
            get {
                return ((string)(this["ListenIp"]));
            }
        }
    }
}
