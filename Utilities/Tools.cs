using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
namespace Utilities
{
    public static class Tools
    {
        private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Compress buffer in Gzip format. It appends original buffer length at the beginning of compressed buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] buffer)
        {
            byte[] gzBuffer = null;
            try
            {
                //Compress buffer into Stream
                MemoryStream ms = new MemoryStream();
                using (GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(buffer, 0, buffer.Length);
                    zip.Close();
                }
                ms.Position = 0;

                //Copy compressed Stream into array of bytes with length
                byte[] compressed = new byte[ms.Length];
                gzBuffer = new byte[compressed.Length + 4];

                ms.Read(compressed, 0, compressed.Length);

                Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
                Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            }
            catch (Exception exc)
            {
                _Logger.Error(String.Format("Error Compressing: {0}", exc.Message)); 
            }
            return gzBuffer;
        }

        /// <summary>
        /// Decompress buffer if it identifies Gzipmagic number in header, otherwise returns input buffer.
        /// It is valid for both compressed and uncompressed buffers
        /// </summary>
        /// <param name="gzBuffer"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] gzBuffer)
        {
            byte[] buffer = gzBuffer;
            try
            {
                MemoryStream ms = new MemoryStream();
                int msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);
                ushort magicNumber = BitConverter.ToUInt16(gzBuffer, 4);
                if (magicNumber == 0x8b1f)
                {
                    buffer = new byte[msgLength];

                    ms.Position = 0;
                    using (GZipStream zip = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        zip.Read(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception exc)
            {
                _Logger.Error(String.Format("Error Decompressing: {0}", exc.Message));
                return buffer;
            }
            return buffer;
        }
	}
}
