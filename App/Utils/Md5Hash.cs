using System.Security.Cryptography;

namespace App.Utils
{
    public static class Md5Hash
    {
        public async static Task<string> ComputeAsync(string filePath)
        {
            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(filePath);

            byte[] hash = await md5.ComputeHashAsync(stream);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
