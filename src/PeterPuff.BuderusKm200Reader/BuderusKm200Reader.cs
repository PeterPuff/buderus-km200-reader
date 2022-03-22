using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace PeterPuff.BuderusKm200Reader
{
    public class BuderusKm200Reader
    {
        private const string WebRequestUserAgent = "TeleHeater/2.2.3";

        private readonly byte[] _key;
        private static readonly byte[] _salt = new byte[]
        {
            0x86, 0x78, 0x45, 0xe9, 0x7c, 0x4e, 0x29, 0xdc,
            0xe5, 0x22, 0xb9, 0xa7, 0xd3, 0xa3, 0xe0, 0x7b,
            0x15, 0x2b, 0xff, 0xad, 0xdd, 0xbe, 0xd7, 0xf5,
            0xff, 0xd8, 0x42, 0xe9, 0x89, 0x5a, 0xd1, 0xe4
        };

        public string Host { get; }
        public int Port { get; }

        public BuderusKm200Reader(string host, int port, string gatewayPassword, string privatePassword)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Invalid host specified.", nameof(host));
            if (port < IPEndPoint.MinPort ||
                port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException(nameof(port), "Invalid port specified.");
            if (string.IsNullOrWhiteSpace(gatewayPassword))
                throw new ArgumentException("Invalid gateway password specified.", nameof(gatewayPassword));
            if (string.IsNullOrWhiteSpace(privatePassword))
                throw new ArgumentException("Invalid private password specified.", nameof(privatePassword));

            Host = host;
            Port = port;

            _key = CalculateKey(gatewayPassword, privatePassword);
        }

        private static byte[] CalculateKey(string gatewayPassword, string privatePassword)
        {
            // We need gateway password without dashes
            gatewayPassword = gatewayPassword.Replace("-", string.Empty);
            var gatewayPasswordBytes = Encoding.ASCII.GetBytes(gatewayPassword);
            var privatePasswordBytes = Encoding.ASCII.GetBytes(privatePassword);

            using (var md5 = new MD5CryptoServiceProvider())
            {
                // First Part of Key: MD5 of <gateway password> + <salt>
                var keyPart1Bytes = Concat(gatewayPasswordBytes, _salt);
                var keyPart1 = md5.ComputeHash(keyPart1Bytes);

                // Second part of Key: MD5 of <salt> + <private password>
                var keyPart2Bytes = Concat(_salt, privatePasswordBytes);
                var keyPart2 = md5.ComputeHash(keyPart2Bytes);

                var key = Concat(keyPart1, keyPart2);
                return key;
            }
        }

        private static byte[] Concat(byte[] firstArray, byte[] secondArray)
        {
            var buffer = new byte[firstArray.Length + secondArray.Length];
            Buffer.BlockCopy(firstArray, 0, buffer, 0, firstArray.Length);
            Buffer.BlockCopy(secondArray, 0, buffer, firstArray.Length, secondArray.Length);
            return buffer;
        }

        public string Decrypt(string encrypted)
        {
            var decrypted = DecryptStringFromOpenSslAes(encrypted, _key);
            decrypted = decrypted.TrimEnd('\0');
            return decrypted;
        }

        private static string DecryptStringFromOpenSslAes(string encrypted, byte[] key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encrypted);
            return DecryptStringFromOpenSslAes(encryptedBytes, key);
        }

        private static string DecryptStringFromOpenSslAes(byte[] encrypted, byte[] key)
        {
            // OpenSSL uses empty IV
            var iv = new byte[16];

            // Create a RijndaelManaged object with the specified key and IV
            using (var aesAlg = new RijndaelManaged
            {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.Zeros,
                KeySize = 256,
                BlockSize = 128,
                Key = key,
                IV = iv
            })
            // Create a decryptor to perform the stream transform
            using (ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
            using (MemoryStream msDecrypt = new MemoryStream(encrypted))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                return srDecrypt.ReadToEnd();
        }

        private static string GetTypeFromJson(JObject jObject)
            => jObject["type"].Value<string>();

        private static T GetValueFromJson<T>(JObject jObject)
            => jObject["value"].Value<T>();

        public string ReadDatapointData(string datapoint)
        {
            var uri = $"http://{Host}:{Port}{datapoint}";

            var req = (HttpWebRequest)WebRequest.Create(uri);
            req.UserAgent = WebRequestUserAgent;

            string responseText;
            using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            using (Stream dataStream = res.GetResponseStream())
            using (StreamReader reader = new StreamReader(dataStream))
                responseText = reader.ReadToEnd();

            return Decrypt(responseText);
        }

        private JObject ReadDatapointDataAsJson(string datapoint)
        {
            var jsonTextResponse = ReadDatapointData(datapoint);
            return JObject.Parse(jsonTextResponse);
        }

        public float ReadDatapointValueAsFloat(string datapoint)
        {
            JObject jObject = ReadDatapointDataAsJson(datapoint);
            ValidateJsonType(jObject, "floatValue");
            return GetValueFromJson<float>(jObject);
        }

        public string ReadDatapointValueAsString(string datapoint)
        {
            JObject jObject = ReadDatapointDataAsJson(datapoint);
            ValidateJsonType(jObject, "stringValue");
            return GetValueFromJson<string>(jObject);
        }

        private static void ValidateJsonType(JObject jObject, string expectedType)
        {
            string type = GetTypeFromJson(jObject);
            if (string.Compare(type, expectedType, StringComparison.OrdinalIgnoreCase) != 0)
                throw new InvalidOperationException($"The specified datapoint's type is not '{expectedType}' as expected, instead it is '{type}'.");
        }
    }
}
