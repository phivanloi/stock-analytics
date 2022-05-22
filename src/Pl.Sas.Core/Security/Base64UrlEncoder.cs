using Ardalis.GuardClauses;
using System.Text;

namespace Pl.Sas.Core.Entities.Security
{
    public static class Base64UrlEncoder
    {
        private static readonly char base64PadCharacter = '=';
        private static readonly string doubleBase64PadCharacter = "==";
        private static readonly char base64Character62 = '+';
        private static readonly char base64Character63 = '/';
        private static readonly char _base64UrlCharacter62 = '-';
        private static readonly char _base64UrlCharacter63 = '_';

        /// <summary>
        /// Chuyển một mảng dữ liệu thành kiều base 64
        /// </summary>
        /// <param name="data">Chuỗi cần chuyển</param>
        /// <returns>base64 string
        /// VD: YmFzZSBzdHJpbmc=
        /// </returns>
        public static string Encode(string data)
        {
            var inArray = Encoding.UTF8.GetBytes(data);
            return Encode(inArray);
        }

        /// <summary>
        /// Chuyển một mảng dữ liệu thành kiều base 64
        /// </summary>
        /// <param name="inArray">Mảng dữ liệu</param>
        /// <returns>base64 string
        /// VD: YmFzZSBzdHJpbmc=
        /// </returns>
        public static string Encode(byte[] inArray)
        {
            string s = Convert.ToBase64String(inArray);
            s = s.Split(base64PadCharacter)[0];
            s = s.Replace(base64Character62, _base64UrlCharacter62);
            s = s.Replace(base64Character63, _base64UrlCharacter63);

            return s;
        }

        /// <summary>
        /// Chuyển một chuỗi base64 thành chuỗi bình thường
        /// </summary>
        /// <param name="str">Chuỗi base 64</param>
        /// <returns>Chuỗi bình thường</returns>
        /// <exception cref="FormatException">nếu chuỗi không đúng định dạng base64 sẽ lỗi</exception>
        public static string DecodeToString(string str)
        {
            var byteArray = Decode(str);
            return Encoding.UTF8.GetString(byteArray);
        }

        /// <summary>
        /// Chuyển một chuỗi base64 thành mảng byte
        /// </summary>
        /// <param name="str">Chuỗi base 64</param>
        /// <returns>mảng dữ liệu</returns>
        /// <exception cref="FormatException">nếu chuỗi không đúng định dạng base64 sẽ lỗi</exception>
        public static byte[] Decode(string str)
        {
            Guard.Against.Null(str, nameof(str));

            str = str.Replace(_base64UrlCharacter62, base64Character62);
            str = str.Replace(_base64UrlCharacter63, base64Character63);
            switch (str.Length % 4)
            {
                case 0:
                    break;
                case 2:
                    str += doubleBase64PadCharacter;
                    break;
                case 3:
                    str += base64PadCharacter;
                    break;
                default:
                    throw new FormatException("Base64 string format error");
            }

            return Convert.FromBase64String(str);
        }
    }
}
