using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace __RootNamespace__.Common.Utils
{
    public class Encrypt
    {
        private readonly string aesKey;
        private readonly string aesIv;

        public Encrypt(string key, string iv)
        {
            aesIv = iv;
            aesKey = key;
        }

        public string EncryptData(string data)
        {
            using var aes = Aes.Create();

            aes.Key = Convert.FromBase64String(aesKey);
            aes.IV = Convert.FromBase64String(aesIv);

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string DecryptData(string data)
        {
            using var aes = Aes.Create();

            aes.Key = Convert.FromBase64String(aesKey);
            aes.IV = Convert.FromBase64String(aesIv);

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (var msDecrypt = new MemoryStream(Convert.FromBase64String(data)))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    public class PasswordHasher
    {
        /// <summary>
        /// Genera un hash seguro de la contraseña con un salt aleatorio.
        /// </summary>
        /// <returns>String combinado: salt.hash</returns>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}:{hashBase64}";
        }

        /// <summary>
        /// Verifica si una contraseña coincide con el hash almacenado.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            string[] parts = storedHash.Split(':');
            if (parts.Length != 2) return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] originalHash = Convert.FromBase64String(parts[1]);

            byte[] newHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 32
            );

            return CryptographicOperations.FixedTimeEquals(originalHash, newHash);
        }

        private const string Caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        // Quita O, 0, I, l, 1 para evitar confusiones al escribir

        /// <summary>
        /// Genera una contraseña temporal.
        /// </summary>
        public static string GenerarPassword(int longitud = 8)
        {
            if (longitud < 4) longitud = 4;
            if (longitud > 20) longitud = 20;

            var bytes = new byte[longitud];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var resultado = new StringBuilder(longitud);
            foreach (var b in bytes)
            {
                resultado.Append(Caracteres[b % Caracteres.Length]);
            }

            return resultado.ToString();
        }
    }
}
