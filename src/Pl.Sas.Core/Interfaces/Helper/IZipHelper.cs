namespace Pl.Sas.Core.Interfaces
{
    public interface IZipHelper
    {
        /// <summary>
        /// Zip a byte array data
        /// </summary>
        /// <param name="data">data to zio</param>
        /// <returns>byte array</returns>
        byte[] ZipByte(byte[] data);

        /// <summary>
        /// UnZip a byte array data
        /// </summary>
        /// <param name="data">data to unzio</param>
        /// <returns>byte array</returns>
        byte[] UnZipByte(byte[] data);
    }
}