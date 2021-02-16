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
    }
}
