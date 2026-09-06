using System.Security.Cryptography;

namespace App.Utils
{
    public static class Md5Hash
    {
        public async static Task<string> ComputeAsync(Stream assetStream)
        {
            using var md5 = MD5.Create();

            byte[] hash = await md5.ComputeHashAsync(assetStream);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
