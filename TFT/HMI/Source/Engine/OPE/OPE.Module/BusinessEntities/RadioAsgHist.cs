using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

using HMI.OPE.Module.BusinessEntities;

namespace HMI.OPE.Module.BusinessEntities
{
    class RadioAsgHist
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipto"></param>
        /// <param name="portto"></param>
        /// <param name="usu"></param>
        /// <param name="frec"></param>
        /// <param name="std"></param>
        /// <param name="last"></param>
        /// <summary>
        /// 
        /// </summary>
        public enum stdPos { Desasignado = 0x0e100, Monitor = 0x0e200, Trafico = 0x0e300 };
        [StructLayout(LayoutKind.Sequential)]
        struct stCmd
        {
            public Byte _tipo1;
            public Byte _cmd1;
            public UInt16 _dest;
            public UInt16 _org;
            public Byte _tipo;
            public Byte _cmd;
            public UInt16 _event;
            public UInt16 _usu;
            public UInt16 _fre;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public String _strfre;
            public UInt16 _last;
            public UInt16 _std;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public String _strusu;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipto"></param>
        /// <param name="portto"></param>
        /// <param name="usu"></param>
        /// <param name="frec"></param>
        /// <param name="std"></param>
        /// <param name="last"></param>
        static public byte[] SendCmdHistorico(string ipto, int portto, string usu, string frec, stdPos std, stdPos last)
        {
            stCmd cmd = new stCmd();

            cmd._tipo1 = 0;
            cmd._cmd1 = 4;
            cmd._dest = cmd._org = 0;
            cmd._tipo = 3;
            cmd._cmd = 0xae;
            cmd._event = 0x0800;        // big endian...
            cmd._usu = cmd._fre = 0xffff;

            cmd._std = (ushort)(std);
            cmd._last = (ushort)(last);
            cmd._strfre = frec;
            cmd._strusu = usu;
            try
            {
                UdpClient s = new UdpClient();
                IPEndPoint dst = new IPEndPoint(IPAddress.Parse(ipto), portto);

                byte[] cmdb = StructureToByteArray(cmd);

                s.Send(cmdb, cmdb.Count(), dst);

                return cmdb;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static byte[] StructureToByteArray(object obj)
        {
            int len = Marshal.SizeOf(obj);
            byte[] arr = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytearray"></param>
        /// <param name="obj"></param>
        static void ByteArrayToStructure(byte[] bytearray, ref object obj)
        {
            int len = Marshal.SizeOf(obj);
            IntPtr i = Marshal.AllocHGlobal(len);
            Marshal.Copy(bytearray, 0, i, len);
            obj = Marshal.PtrToStructure(i, obj.GetType());
            Marshal.FreeHGlobal(i);
        }
    }
}
