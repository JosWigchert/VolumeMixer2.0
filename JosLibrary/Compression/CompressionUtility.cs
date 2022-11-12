using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosLibrary.Compression
{
    /// 
    /// https://toreaurstad.blogspot.com/2014/01/compressing-byte-array-in-c-with.html
    /// Compresses or decompresses byte arrays using GZipStream
    /// 

    public static class CompressionUtility
    {

        private static int BUFFER_SIZE = 128 * 1024; //128kB

        public static byte[] Compress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressIntoMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs,
                 CompressionMode.Compress), BUFFER_SIZE))
                {
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return compressIntoMs.ToArray();
            }
        }

        public static byte[] Decompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressedMs = new MemoryStream(inputData))
            {
                using (var decompressedMs = new MemoryStream())
                {
                    using (var gzs = new BufferedStream(new GZipStream(compressedMs,
                     CompressionMode.Decompress), BUFFER_SIZE))
                    {
                        gzs.CopyTo(decompressedMs);
                    }
                    return decompressedMs.ToArray();
                }
            }
        }

        public static byte[] CompressString(string inputData)
        {
            byte[] toCompress = Encoding.ASCII.GetBytes(inputData);
            return Compress(toCompress);
        }

        public static string DecompressString(byte[] inputData)
        {
            byte[] decompressed = Decompress(inputData);
            return Encoding.ASCII.GetString(decompressed);
        }
    }
}
