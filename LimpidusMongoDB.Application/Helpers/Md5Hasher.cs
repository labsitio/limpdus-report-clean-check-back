using System.Security.Cryptography;
using System.Text;

namespace LimpidusMongoDB.Application.Helpers
{
    /// <summary>
    /// Replica o hash usado em Business.Security.Criptografar (MD5 hex lowercase),
    /// com Encoding Windows-1252 para alinhar ao .NET Framework legado.
    /// </summary>
    public static class Md5Hasher
    {
        private static readonly Encoding LegacyEncoding;

        static Md5Hasher()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            LegacyEncoding = Encoding.GetEncoding(1252);
        }

        public static string HashHex(string text)
        {
            var bytes = LegacyEncoding.GetBytes(text ?? string.Empty);
            var hash = MD5.HashData(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}
