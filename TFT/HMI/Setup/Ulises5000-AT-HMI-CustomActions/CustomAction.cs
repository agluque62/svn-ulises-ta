using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using HMI.CD40.Module.BusinessEntities;
using System.Threading;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;

namespace Ulises5000_AT_HMI_CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult Ed137RecVar(Session session)
        {
            session.Log("Begin Ed137RecVar");

            int varInt = int.Parse(session["ED137REC"]);

            session["ED137REC"] = (varInt & 0x01).ToString();
            session["RECDUAL"] = (varInt & 0x02) != 0 ? "1" : "0";

            return ActionResult.Success;
        }
        [CustomAction]
        public static ActionResult GenerateSpreadConf(Session session)
        {
            System.IO.StreamWriter conf;
            try
            {
                System.IO.Directory.CreateDirectory(session["U5ki.MCast.INSTALLFOLDER"]);
                int nItems = int.Parse(session["SPREAD_NUMBER"]);
                byte[] ipbase = IPAddress.Parse(session["SPREAD_IPBASE"]).GetAddressBytes();

                /** */
                View lView = session.Database.OpenView("DELETE FROM ComboBox WHERE ComboBox.Property='PICT'");
                lView.Execute();
                lView = session.Database.OpenView("SELECT * FROM ComboBox");
                lView.Execute();

                using (conf = new System.IO.StreamWriter(session["U5ki.MCast.INSTALLFOLDER"] + "spread.conf", false))
                {
                    conf.WriteLine(String.Format("Spread_Segment  239.255.10.50:4803 {{"));

                    for (int item = 0; item < nItems; item++)
                    {
                        IPAddress current = new IPAddress(ipbase);
                        conf.WriteLine(String.Format("\t PICT{0:00} {1} {{", item + 1, current.ToString()));

                        conf.WriteLine(String.Format("\t\tD {0}", current.ToString()));
                        conf.WriteLine(String.Format("\t\tC {0}", current.ToString()));
                        conf.WriteLine(String.Format("\t\tC 127.0.0.1"));
                        conf.WriteLine(String.Format("\t }}"));

                        /** */
                        Record lRecord = session.Database.CreateRecord(4);
                        lRecord.SetString(1, "PICT");
                        lRecord.SetInteger(2, item);
                        lRecord.SetString(3, String.Format("PICT{0:00}", item + 1));
                        lRecord.SetString(4, String.Format("PICT{0:00}, {1}", item + 1, current.ToString()));
                        lView.Modify(ViewModifyMode.InsertTemporary, lRecord);

                        ipbase[3]++;
                    }

                    conf.WriteLine(String.Format("}}"));

                    conf.WriteLine("TokenTimeout = 5000");
                    conf.WriteLine("HurryTimeout = 1500");
                    conf.WriteLine("AliveTimeout = 250");
                    conf.WriteLine("JoinTimeout = 250");
                    conf.WriteLine("RepTimeout = 1250");
                    conf.WriteLine("SegTimeout = 500");
                    conf.WriteLine("GatherTimeout = 2500");
                    conf.WriteLine("FormTimeout = 2500");
                    /** 20170903. AGL. Se cambia este parámetro para evitar transitorios en conmutación de NBX... */
                    conf.WriteLine(/*"LookupTimeout = 45000"*/"LookupTimeout = 15000");

                    conf.WriteLine("DebugFlags = { PRINT EXIT }");
                    conf.WriteLine("EventPriority =  INFO");
                    conf.WriteLine("#EventLogFile = testlog.out");
                    conf.WriteLine("EventTimeStamp = \"%Y-%m-%d %H:%M:%S:\"");
                    conf.WriteLine("#EventPreciseTimeStamp");
                    conf.WriteLine("#DebugInitialSequence");
                    conf.WriteLine("DangerousMonitor = true");
                    conf.WriteLine("#SocketPortReuse = AUTO");
                    conf.WriteLine("#MaxSessionMessages = 1000");
                    conf.WriteLine("RuntimeDir = /var/run/spread");
                    conf.WriteLine("DaemonUser = p");
                    conf.WriteLine("DaemonGroup = users");
                    conf.WriteLine("#RequiredAuthMethods = \"   \"");
                    conf.WriteLine("#AllowedAuthMethods = \"NULL\"");

                }
            }
            catch (Exception x)
            {
                session.Log("ERROR in custom action GenerateSpreadConf {0}", x.ToString());
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR in custom action GenerateSpreadConf {0}", x.ToString()));
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CustomActionPict2Ip(Session session)
        {
            session.Log("Begin CustomActionPict2Ip");
            try
            {
                int nItems = int.Parse(session["SPREAD_NUMBER"]);
                byte[] ipbase = IPAddress.Parse(session["SPREAD_IPBASE"]).GetAddressBytes();
                string pict = session["PICT"];

                for (int item = 0; item < nItems; item++)
                {
                    if (pict == String.Format("PICT{0:00}", item + 1))
                    {
                        session["DIRIP"] = (new IPAddress(ipbase)).ToString();
                        return ActionResult.Success;
                    }
                    ipbase[3]++;
                }

            }
            catch (Exception x)
            {
                session.Log("ERROR in custom action CustomActionPict2Ip {0}", x.ToString());
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR in custom action CustomActionPict2Ip {0}", x.ToString()));
                return ActionResult.Failure;
            }
            System.Windows.Forms.MessageBox.Show("ERROR en CustomAction CustomActionPict2Ip: No se ha encontrado una IP");
            return ActionResult.Failure;
        }

        [CustomAction]
        public static ActionResult LoadAsioChannels(Session session)
        {
            session.Log("Begin Custiom Action LoadAsioChannels");
            try
            {
                Thread thread = new Thread(AsioChannels.Init);
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join(); //Wait for the thread to end
                //AsioChannels.Init();
                session.Log("Custiom Action LoadAsioChannels num out channels {0}", AsioChannels.OutChannels.Count);
                /** */
                View lView = session.Database.OpenView("DELETE FROM ComboBox WHERE ComboBox.Property='RADIOSPEAKERDEV'");
                lView.Execute();
                lView = session.Database.OpenView("SELECT * FROM ComboBox");
                lView.Execute();
                string speakers = session["RADIOSPEAKERDEV"];
                int i = 0;
                foreach (String name in AsioChannels.OutChannels)
                {
                    /** */
                    Record lRecord = session.Database.CreateRecord(3);
                    lRecord.SetString(1, "RADIOSPEAKERDEV");
                    lRecord.SetInteger(2, i++);
                    lRecord.SetString(3, name);

                    lView.Modify(ViewModifyMode.InsertTemporary, lRecord);

                    session.Log("Custiom Action LoadAsioChannels {0}", name);
                }
                session["SAMPLERATE"] = AsioChannels.SampleRate.ToString();
            }
            catch (Exception x)
            {
                session.Log("ERROR in custom action LoadAsioChannels {0}", x.ToString());
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR in custom action LoadAsioChannels {0}", x.ToString()));
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }
        [CustomAction]
        public static ActionResult SetSampleRate(Session session)
        {
            session.Log("Begin Custiom Action SetSampleRate");
            try
            {
                string radioSepakerDevName = session["RADIOSPEAKERDEV"];
                MMDeviceCollection DevCol = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
                foreach (MMDevice dev in DevCol)
                {
                    if (dev.DeviceFriendlyName.Equals(radioSepakerDevName))
                    {
                        session["SAMPLERATE"] = dev.AudioClient.MixFormat.SampleRate.ToString();
                        return ActionResult.Success;
                    }
                }

            }
            catch (Exception x)
            {
                session.Log("ERROR in custom action SetSampleRate {0}", x.ToString());
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR in custom action SetSampleRate {0}", x.ToString()));
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }

        

        public const int CORESIP_MAX_FILE_PATH_LENGTH = 256;
        public const int CORESIP_MAX_ERROR_INFO_LENGTH = 512;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct CORESIP_Error
        {
            public int Code;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_FILE_PATH_LENGTH + 1)]
            public string File;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_ERROR_INFO_LENGTH + 1)]
            public string Info;
        }
        public const int CORESIP_MAX_SOUND_NAME_LENGTH = 512;
        public const int CORESIP_MAX_SOUND_NAMES = 16;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        struct CORESIP_SndWindowsDevices
        {
            public uint ndevices_found;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_SOUND_NAME_LENGTH * CORESIP_MAX_SOUND_NAMES)]
            public string DeviceNames; //array con los nombres, separados por '<###>'.
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_SOUND_NAME_LENGTH * CORESIP_MAX_SOUND_NAMES)]
            public string FriendlyName; //array con los nombres, separados por '<###>'
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_SOUND_NAME_LENGTH * CORESIP_MAX_SOUND_NAMES)]
            public string GUID;			//array con los nombres, separados por '<###>'
        }
        [DllImport("coresip-voter", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_GetWindowsSoundDeviceNames(int captureType, out CORESIP_SndWindowsDevices Devices, out CORESIP_Error error);

        
        
        [CustomAction]
        public static ActionResult CustomActionGetWindowsSndDevs(Session session)
        {
            
            CORESIP_Error err;
            CORESIP_SndWindowsDevices Devices;
            CORESIP_GetWindowsSoundDeviceNames(0, out Devices, out err);            
            
            session.Log("Begin CustomActionGetWindowsSndDevs");

            List<string> DeviceList = new List<string>();

            int index_name = 0;
            for (int i = 0; i < Devices.ndevices_found; i++)
            {
                int index_separator = Devices.DeviceNames.IndexOf("<###>", index_name);
                if (index_separator != -1)
                {
                    DeviceList.Add(Devices.DeviceNames.Substring(index_name, index_separator - index_name));
                    index_name = index_separator + 5;
                }
                else
                {
                    DeviceList.Add(Devices.DeviceNames.Substring(index_name));
                    break;
                }
            }

            PopulateComboBox(session, "STD_WIN_INSTRUCTOR_DEV", DeviceList);
            PopulateComboBox(session, "STD_WIN_ALUMN_DEV", DeviceList);
            PopulateComboBox(session, "STD_WIN_RD_DEV", DeviceList);
            PopulateComboBox(session, "STD_WIN_LC_DEV", DeviceList);

            try
            {
                Thread thread = new Thread(AsioChannels.Init);
                thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
                thread.Start();
                thread.Join(); //Wait for the thread to end
                //AsioChannels.Init();
                session.Log("Custiom Action LoadAsioChannels num out channels {0}", AsioChannels.OutChannels.Count);

                PopulateComboBox(session, "STD_ASIO_INSTRUCTOR_DEV", AsioChannels.OutChannels);
                PopulateComboBox(session, "STD_ASIO_ALUMN_DEV", AsioChannels.OutChannels);
                PopulateComboBox(session, "STD_ASIO_RD_DEV", AsioChannels.OutChannels);
                PopulateComboBox(session, "STD_ASIO_LC_DEV", AsioChannels.OutChannels);

            }
            catch (Exception x)
            {
                session.Log("ERROR in custom action CustomActionGetWindowsSndDevs {0}", x.ToString());
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR in custom action LoadAsioChannels {0}", x.ToString()));
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }

        private static void PopulateComboBox(Session session, string combobox_property, List<string> DeviceList)
        {
            string query;
            query = "DELETE FROM ComboBox WHERE ComboBox.Property='"+combobox_property+"'";
            View lView = session.Database.OpenView(query);
            lView.Execute();

            query = "SELECT * FROM ComboBox WHERE ComboBox.Property='" + combobox_property + "'";
            lView = session.Database.OpenView(query);
            lView.Execute();

            int order = 2;
            Record lRecord = session.Database.CreateRecord(4);
            session.Log("Setting record details");
            lRecord.SetString(1, combobox_property);
            lRecord.SetInteger(2, order);
            lRecord.SetString(3, "-none-");
            lRecord.SetString(4, "-none-");

            lView.Modify(ViewModifyMode.InsertTemporary, lRecord);
            order++;
            
            foreach (string dev in DeviceList)
            {
                lRecord = session.Database.CreateRecord(4);
                session.Log("Setting record details");
                lRecord.SetString(1, combobox_property);
                lRecord.SetInteger(2, order);
                lRecord.SetString(3, dev);
                lRecord.SetString(4, dev);

                lView.Modify(ViewModifyMode.InsertTemporary, lRecord);

                order++;
            }
        }

        [CustomAction]
        public static ActionResult CustomActionCheckSndDevs(Session session)
        {
            session.Log("Begin CustomActionCheckSndDevs");

            //System.Windows.Forms.MessageBox.Show(String.Format("ENTRA {0} ", session["STD_WIN_INSTRUCTOR_DEV"]));

            bool devices_iguales = false;
            bool windows_asio_no_seleccionados = false;
            string _none = "-none-";

            if (session["STD_WIN_INSTRUCTOR_DEV"] == _none &&
                session["STD_WIN_ALUMN_DEV"] == _none &&
                session["STD_WIN_RD_DEV"] == _none &&
                session["STD_WIN_LC_DEV"] == _none)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR: Debe seleccionar los dispositivos de Windows"));
                session["NEXT_BUTTON_DIALOG3"] = "False";
                return ActionResult.Success;
            }

            if (session["STD_ASIO_INSTRUCTOR_DEV"] == _none &&
                session["STD_ASIO_ALUMN_DEV"] == _none &&
                session["STD_ASIO_RD_DEV"] == _none &&
                session["STD_ASIO_LC_DEV"] == _none)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR: Debe seleccionar los dispositivos de ASIO"));
                session["NEXT_BUTTON_DIALOG3"] = "False";
                return ActionResult.Success;
            }

            if (session["STD_WIN_INSTRUCTOR_DEV"] != _none)
            {
                if (session["STD_ASIO_INSTRUCTOR_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_WIN_INSTRUCTOR_DEV"] == session["STD_WIN_ALUMN_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_INSTRUCTOR_DEV"] == session["STD_WIN_RD_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_INSTRUCTOR_DEV"] == session["STD_WIN_LC_DEV"]) devices_iguales = true;
            }

            if (session["STD_WIN_ALUMN_DEV"] != _none)
            {
                if (session["STD_ASIO_ALUMN_DEV"].Length == 0) windows_asio_no_seleccionados = true;
                if (session["STD_WIN_ALUMN_DEV"] == session["STD_WIN_INSTRUCTOR_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_ALUMN_DEV"] == session["STD_WIN_RD_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_ALUMN_DEV"] == session["STD_WIN_LC_DEV"]) devices_iguales = true;
            }

            if (session["STD_WIN_RD_DEV"] != _none)
            {
                if (session["STD_ASIO_RD_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_WIN_RD_DEV"] == session["STD_WIN_INSTRUCTOR_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_RD_DEV"] == session["STD_WIN_ALUMN_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_RD_DEV"] == session["STD_WIN_LC_DEV"]) devices_iguales = true;
            }

            if (session["STD_WIN_LC_DEV"] != _none)
            {
                if (session["STD_ASIO_LC_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_WIN_LC_DEV"] == session["STD_WIN_INSTRUCTOR_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_LC_DEV"] == session["STD_WIN_ALUMN_DEV"]) devices_iguales = true;
                else if (session["STD_WIN_LC_DEV"] == session["STD_WIN_RD_DEV"]) devices_iguales = true;
            }

            if (devices_iguales)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR: Hay nombres de dispositivos Windows repetidos"));
                session["NEXT_BUTTON_DIALOG3"] = "False";
            }
            else
            {
                session["NEXT_BUTTON_DIALOG3"] = "True";
            }

            devices_iguales = false;

            if (session["STD_ASIO_INSTRUCTOR_DEV"] != _none)
            {
                if (session["STD_WIN_INSTRUCTOR_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_ASIO_INSTRUCTOR_DEV"] == session["STD_ASIO_ALUMN_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_INSTRUCTOR_DEV"] == session["STD_ASIO_RD_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_INSTRUCTOR_DEV"] == session["STD_ASIO_LC_DEV"]) devices_iguales = true;
            }

            if (session["STD_ASIO_ALUMN_DEV"] != _none)
            {
                if (session["STD_WIN_ALUMN_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_ASIO_ALUMN_DEV"] == session["STD_ASIO_INSTRUCTOR_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_ALUMN_DEV"] == session["STD_ASIO_RD_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_ALUMN_DEV"] == session["STD_ASIO_LC_DEV"]) devices_iguales = true;
            }

            if (session["STD_ASIO_RD_DEV"] != _none)
            {
                if (session["STD_WIN_RD_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_ASIO_RD_DEV"] == session["STD_ASIO_INSTRUCTOR_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_RD_DEV"] == session["STD_ASIO_ALUMN_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_RD_DEV"] == session["STD_ASIO_LC_DEV"]) devices_iguales = true;
            }

            if (session["STD_ASIO_LC_DEV"] != _none)
            {
                if (session["STD_WIN_LC_DEV"] == _none) windows_asio_no_seleccionados = true;
                if (session["STD_ASIO_LC_DEV"] == session["STD_ASIO_INSTRUCTOR_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_LC_DEV"] == session["STD_ASIO_ALUMN_DEV"]) devices_iguales = true;
                else if (session["STD_ASIO_LC_DEV"] == session["STD_ASIO_RD_DEV"]) devices_iguales = true;
            }

            if (devices_iguales)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR: Hay nombres de dispositivos ASIO repetidos"));
                session["NEXT_BUTTON_DIALOG3"] = "False";
            }
            else
            {
                session["NEXT_BUTTON_DIALOG3"] = "True";
            }

            if (windows_asio_no_seleccionados)
            {
                System.Windows.Forms.MessageBox.Show(String.Format("ERROR: Si asigna un dispositivo de Windows debe asignar otro de Asio y viceversa, para el mismo dispositivo del puesto"));
                session["NEXT_BUTTON_DIALOG3"] = "False";
            }
            else
            {
                session["NEXT_BUTTON_DIALOG3"] = "True";
            }

            return ActionResult.Success;
        }


    }
    }
