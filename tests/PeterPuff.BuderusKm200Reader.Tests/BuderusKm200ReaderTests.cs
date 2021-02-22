using FluentAssertions;
using NSubstitute;
using PeterPuff.BuderusKm200Reader;
using System;
using System.IO;
using System.Net;
using System.Text;
using Xunit;

public class BuderusKm200ReaderTests
{
    public class Constructor
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowsIfHostIsNullOrWhitespace(string host)
        {
            int port = IPEndPoint.MinPort;
            string gatewayPassword = nameof(gatewayPassword);
            string privatePassword = nameof(privatePassword);

            Action act = () => _ = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*invalid*host*")
                .And.ParamName.Should().Be("host");
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void ThrowsIfPortIsOutOfRange(int port)
        {
            string host = nameof(host);
            string gatewayPassword = nameof(gatewayPassword);
            string privatePassword = nameof(privatePassword);

            Action act = () => _ = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*invalid*port*")
                .And.ParamName.Should().Be("port");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowsIfGatewayPasswordIsNullOrWhitespace(string gatewayPassword)
        {
            string host = nameof(host);
            int port = IPEndPoint.MinPort;
            string privatePassword = nameof(privatePassword);

            Action act = () => _ = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*invalid*gateway*password*")
                .And.ParamName.Should().Be("gatewayPassword");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ThrowsIfPrivatePasswordIsNullOrWhitespace(string privatePassword)
        {
            string host = nameof(host);
            int port = IPEndPoint.MinPort;
            string gatewayPassword = nameof(gatewayPassword);

            Action act = () => _ = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*invalid*private*password*")
                .And.ParamName.Should().Be("privatePassword");
        }

        [Fact]
        public void StoresHostInProperty()
        {
            string host = nameof(host);
            int port = IPEndPoint.MinPort;
            string gatewayPassword = nameof(gatewayPassword);
            string privatePassword = nameof(privatePassword);

            var reader = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            reader.Host.Should().Be(host);
        }

        [Fact]
        public void StoresPortInProperty()
        {
            string host = nameof(host);
            int port = IPEndPoint.MinPort;
            string gatewayPassword = nameof(gatewayPassword);
            string privatePassword = nameof(privatePassword);

            var reader = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            reader.Port.Should().Be(port);
        }
    }

    public abstract class Read
    {
        protected string Host { get; } = "192.168.1.2";
        protected int Port { get; } = 80;
        protected string GatewayPassword { get; } = "aB12-34cD-Ef56-78Gh";
        protected string PrivatePassword { get; } = "ThisIsThePrivatePassword";
        protected BuderusKm200Reader Reader { get; }

        public Read()
            => Reader = new BuderusKm200Reader(Host, Port, GatewayPassword, PrivatePassword);

        protected string GetUrlToMock(string dataPointToRead)
            => $"http://{Host}:{Port}{dataPointToRead}";

        protected static HttpWebRequest MockRequest(string url, string responseText)
        {
            // web request/response mocking taken from here: https://gist.github.com/ronsun/378c25277206b1761defc0a589e08b71

            var responseStream = new MemoryStream(Encoding.UTF8.GetBytes(responseText));
            var response = Substitute.For<HttpWebResponse>();
            response.GetResponseStream().Returns(responseStream);
            var request = Substitute.ForPartsOf<HttpWebRequest>();
            request.When(request => request.GetResponse()).DoNotCallBase();
            request.GetResponse().Returns(response);
            // Must init header collection here to avoid NullReferenceExcetpion while trying to access 
            // ContentType or other properties dependent on private field '_HttpRequestHeaders' in framework.
            // Headers also depend on it, thats why we init it via Headers.
            request.Headers = new WebHeaderCollection();
            // This mock IWebRequestCreate makes WebRequest.Create(url) returning our mocked request,
            // but it's unable to apply to WebRequest.CreateHttp(url)
            var requestCreator = Substitute.For<IWebRequestCreate>();
            requestCreator.Create(Arg.Any<Uri>()).Returns(request);
            WebRequest.RegisterPrefix(url, requestCreator);

            return request;
        }

        /*
         * The encrypted response can be generated by temporary inserting the encryption method "EncryptStringToOpenSslAes" in the reader class.
         * At the end of the constructor following lines were inserted:
         * ...
         * var unencrypted = "<content of expected response>";
         * var encrypted = $"\r\n{Convert.ToBase64String(EncryptStringToOpenSslAes(unencrypted, _key))}";
         * ...
         * An instance of the reader class has to be created with the parameters to be used in the tests.
         * ...
         * private static byte[] EncryptStringToOpenSslAes(string unencrypted, byte[] key)
         * {
         *     // OpenSSL uses empty IV
         *     var iv = new byte[16];
         *
         *     // Create a RijndaelManaged object with the specified key and IV
         *     using (var aesAlg = new RijndaelManaged
         *     {
         *         Mode = CipherMode.ECB,
         *         Padding = PaddingMode.Zeros,
         *         KeySize = 256,
         *         BlockSize = 128,
         *         Key = key,
         *         IV = iv
         *     })
         *     // Create a encryptor to perform the stream transform
         *     using (ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV))
         *     using (MemoryStream msEncrypt = new MemoryStream())
         *     {
         *         using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
         *         using (StreamWriter srEncrypt = new StreamWriter(csEncrypt))
         *             srEncrypt.Write(unencrypted);
         *         return msEncrypt.ToArray();
         *     }
         * }
         */
    }

    public class ReadDatapointData : Read
    {
        [Fact]
        public void ReturnsCorrectResult()
        {
            // original dataoint id for this response: /system/sensors/temperatures/outdoor_t1
            // We have to use an unique datapoint per test to enable concurrent test runs of different tests.
            var dataPointToRead = $"/{nameof(ReadDatapointData)}/{nameof(ReturnsCorrectResult)}";
            var expectedUserAgent = "TeleHeater/2.2.3";
            var encryptedDeviceResponse = "\r\nWsjYjFOV4L8oyAjxi8GGfZ2uTf3PPKKYfdoQkr3zd/frRDQ5fFnaIbZXQR9gUAkawNpUOjlM0Ela+3z79+gSpzkB5eIqef669LR3/JtUv59U+6Amc6CGtr6yy9iGEPetwpW8F15HpHOhxTr9IrehMPVYAs7TKCq0ce6Xe4wiEs300GOj/GTV5k1UKo+Y2p27dmbHAzBirj19+k2pgw60/W0SZ6Lvip3HhJMfrDGxivw=";
            var expectedDatapointData = "{\"id\":\"/system/sensors/temperatures/outdoor_t1\",\"type\":\"floatValue\",\"writeable\":0,\"recordable\":1,\"value\":9.3,\"unitOfMeasure\":\"C\",\"state\":[{\"open\":-3276.8},{\"short\":3276.7}]}";
            var url = GetUrlToMock(dataPointToRead);
            var request = MockRequest(url, encryptedDeviceResponse);

            var result = Reader.ReadDatapointData(dataPointToRead);

            request.Received().UserAgent = expectedUserAgent;
            result.Should().Be(expectedDatapointData);
        }
    }

    public class ReadDatapointValueAsFloat : Read
    {
        [Fact]
        public void ReturnsCorrectResult()
        {
            // Original dataoint id for this response: /system/sensors/temperatures/outdoor_t1
            // We have to use an unique datapoint per test to enable concurrent test runs of different tests.
            var dataPointToRead = $"/{nameof(ReadDatapointValueAsFloat)}/{nameof(ReturnsCorrectResult)}";
            var encryptedDeviceResponse = "\r\nWsjYjFOV4L8oyAjxi8GGfZ2uTf3PPKKYfdoQkr3zd/frRDQ5fFnaIbZXQR9gUAkawNpUOjlM0Ela+3z79+gSpzkB5eIqef669LR3/JtUv59U+6Amc6CGtr6yy9iGEPetwpW8F15HpHOhxTr9IrehMPVYAs7TKCq0ce6Xe4wiEs300GOj/GTV5k1UKo+Y2p27dmbHAzBirj19+k2pgw60/W0SZ6Lvip3HhJMfrDGxivw=";
            var expectedDatapointValue = 9.3f;
            var url = GetUrlToMock(dataPointToRead);
            MockRequest(url, encryptedDeviceResponse);

            var result = Reader.ReadDatapointValueAsFloat(dataPointToRead);

            result.Should().Be(expectedDatapointValue);
        }

        [Fact]
        public void ThrowsIfDatatypeDoesNotMatch()
        {
            // The decrypted response of the below written encrypted response is:
            // "{\"id\":\"/system/healthStatus\",\"type\":\"stringValue\",\"writeable\":0,\"recordable\":0,\"value\":\"ok\",\"allowedValues\":[\"error\",\"maintenance\",\"ok\"]}"
            // Original dataoint id for this response: /system/healthStatus
            // We have to use an unique datapoint per test to enable concurrent test runs of different tests.
            var dataPointToRead = $"/{nameof(ReadDatapointValueAsFloat)}/{nameof(ThrowsIfDatatypeDoesNotMatch)}";
            var encryptedDeviceResponse = "\r\n5lFtxRGRpgiqvS3HZTzpc6/aaOVxpus3KTyOieiyRedD1ooIhZlUS/HnXZhVlgUKOc8hSDZJ2L+r9d3jFMWFuBFBHawwv8GgjtdGVFMwOVI9k+/3tEDNrTTFvmQXs1QFdaj6Y6Dar9bTg7oACiFE0QkljkshrljWy/lldZyvJ6GIlfGU2nFstBIVP8DcgOVy";
            var url = GetUrlToMock(dataPointToRead);
            MockRequest(url, encryptedDeviceResponse);

            Action act = () => Reader.ReadDatapointValueAsFloat(dataPointToRead);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*type*not*expected*")
                .WithMessage("*string*")
                .WithMessage("*float*");
        }
    }
}
