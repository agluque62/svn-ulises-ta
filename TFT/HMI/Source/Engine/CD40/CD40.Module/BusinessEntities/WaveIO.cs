using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace HMI.CD40.Module.BusinessEntities
{
    class WaveIO
    {
        public int length;
        public short channels;
        public int samplerate;
        public int DataLength;
        public short BitsPerSample = 16;

        private void WaveHeaderIN(string spath)
        {
            FileStream fs = new FileStream(spath, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs);
            length = (int)fs.Length - 8;
            fs.Position = 22;
            channels = br.ReadInt16();
            fs.Position = 24;
            samplerate = br.ReadInt32();
            fs.Position = 34;

            BitsPerSample = br.ReadInt16();
            DataLength = (int)fs.Length - 44;
            br.Close();
            fs.Close();

        }

        private void WaveHeaderOUT(string sPath)
        {
            FileStream fs = new FileStream(sPath, FileMode.Create, FileAccess.Write);

            BinaryWriter bw = new BinaryWriter(fs);
            fs.Position = 0;
            bw.Write(new char[4] { 'R', 'I', 'F', 'F' });

            bw.Write(length);

            bw.Write(new char[8] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });

            bw.Write((int)16);

            bw.Write((short)1);
            bw.Write(channels);

            bw.Write(samplerate);

            bw.Write((int)(samplerate * ((BitsPerSample * channels) / 8)));

            bw.Write((short)((BitsPerSample * channels) / 8));

            bw.Write(BitsPerSample);

            bw.Write(new char[4] { 'd', 'a', 't', 'a' });
            bw.Write(DataLength);
            bw.Close();
            fs.Close();
        }
        //public void Merge(string[] files, string outfile)
        public void Merge(List <string>  files, string outfile)
        {
            WaveIO wa_IN = new WaveIO();
            WaveIO wa_out = new WaveIO();

            wa_out.DataLength = 0;
            wa_out.length = 0;


            //Gather header data
            foreach (string path in files)
            {
                wa_IN.WaveHeaderIN(@path);
                wa_out.DataLength += wa_IN.DataLength;
                wa_out.length += wa_IN.length;

            }

            //Recontruct new header
            wa_out.BitsPerSample = wa_IN.BitsPerSample;
            wa_out.channels = wa_IN.channels;
            wa_out.samplerate = wa_IN.samplerate;
            wa_out.WaveHeaderOUT(@outfile);

            foreach (string path in files)
            {
                FileStream fs = new FileStream(@path, FileMode.Open, FileAccess.Read);
                byte[] arrfile = new byte[fs.Length - 44];
                fs.Position = 44;
                fs.Read(arrfile, 0, arrfile.Length);
                fs.Close();

                FileStream fo = new FileStream(@outfile, FileMode.Append, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fo);
                bw.Write(arrfile);
                bw.Close();
                fo.Close();
            }
        }
        /// <summary>
        /// Recorta
        /// </summary>
        /// <param name="pathfile"></param> fichero de entrada wav
        /// <param name="outfile"></param>Fichero de aslida wav
        /// <param name="inicio"></param>posicion de inicio, normalmente no se usa
        /// <param name="lonmax"></param>maxima longitud a extraer en ms
        /// <param name="from_end"></param> parte ultima grabada.
        public void Recorta(string pathfile, string outfile, int inicio, long lonmax, bool from_end)
        {
            WaveIO wa_IN = new WaveIO();
            WaveIO wa_out = new WaveIO();

            wa_out.DataLength = 0;
            wa_out.length = 0;


            //Gather header data
            int cab = 44;
            wa_IN.WaveHeaderIN(pathfile);
            long inicio1 = inicio * wa_IN.BitsPerSample;
            int londata = (int)(lonmax * wa_IN.BitsPerSample);// tiempo por muestras
            int londataycab = (int)(lonmax * wa_IN.BitsPerSample + cab);// tiempo por muestas mas cabecera.
            if (londata > wa_IN.DataLength)
            {
                londata = (int)(wa_IN.DataLength);
                londataycab = (int)(londata + cab);
            }
            wa_out.DataLength = londata;//+= wa_IN.DataLength;
            wa_out.length = londataycab-8;//+= wa_IN.length;

            //Recontruct new header
            wa_out.BitsPerSample = wa_IN.BitsPerSample;
            wa_out.channels = wa_IN.channels;
            wa_out.samplerate = wa_IN.samplerate;
            wa_out.WaveHeaderOUT(@outfile);

            FileStream fs = new FileStream(pathfile, FileMode.Open, FileAccess.Read);
            //byte[] arrfile = new byte[fs.Length - cab];
            byte[] arrfile = new byte[londata];
            fs.Position = cab;
            //fs.Read(arrfile, inicio, arrfile.Length);
            if (londata > fs.Length)
            {// por aqui no deberia pasar.
                londata = (int)(wa_IN.DataLength);
                londataycab = (int)(londata + cab);
            }
            if (from_end)
                fs.Seek(-londata, SeekOrigin.End);
            else
                fs.Seek(inicio1, SeekOrigin.Begin);
            fs.Read(arrfile, 0, (int)londata);
            fs.Close();

            FileStream fo = new FileStream(@outfile, FileMode.Append, FileAccess.Write);
            BinaryWriter bw = new BinaryWriter(fo);
            bw.Write(arrfile);
            bw.Close();
            fo.Close();
        }
    }
}
