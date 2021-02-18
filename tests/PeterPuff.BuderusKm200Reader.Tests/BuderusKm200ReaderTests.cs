using FluentAssertions;
using PeterPuff.BuderusKm200Reader;
using System;
using System.Net;
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
}
