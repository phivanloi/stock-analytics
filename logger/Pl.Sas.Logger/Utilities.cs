namespace Pl.Sas.Logger
{
    public static class Utilities
    {
        private static readonly Random random = new();

        /// <summary>
        /// Generate short guid
        /// </summary>
        /// <returns>string</returns>
        public static string GenerateShortGuid()
        {
            return Shorter(Convert.ToBase64String(Guid.NewGuid().ToByteArray())).ToUpper();
            static string Shorter(string base64String)
            {
                base64String = base64String.Split('=')[0];
                base64String = base64String.Replace('+', Convert.ToChar(random.Next(65, 91)));
                base64String = base64String.Replace('/', Convert.ToChar(random.Next(65, 91)));
                return base64String;
            }
        }

    }
}