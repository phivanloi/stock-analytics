using Pl.Sas.Core.Interfaces;
using System.IO.Compression;

namespace Pl.Sas.Infrastructure.Helper
{
    public class GZipHelper : IZipHelper
    {
        public virtual byte[] ZipByte(byte[] zipData)
        {
            using var inputStream = new MemoryStream(zipData);
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                CopyTo(inputStream, gzipStream);
            }
            return outputStream.ToArray();
        }

        public virtual byte[] UnZipByte(byte[] unZipData)
        {
            using var inputStream = new MemoryStream(unZipData);
            using var outputStream = new MemoryStream();
            using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                CopyTo(gzipStream, outputStream);
            }
            return outputStream.ToArray();
        }

        private static void CopyTo(Stream sourceStream, Stream destinationStream)
        {
            byte[] bytes = new byte[4096];
            int cnt;
            while ((cnt = sourceStream.Read(bytes, 0, bytes.Length)) != 0)
            {
                destinationStream.Write(bytes, 0, cnt);
            }
        }
    }
}
