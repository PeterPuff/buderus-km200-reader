using PeterPuff.BuderusKm200Reader;
using System;

namespace ReadOutdoorTemperature
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: <host address> <host port> <gateway password> <private password>");
                return;
            }

            var host = args[0];
            var port = int.Parse(args[1]);
            var gatewayPassword = args[2];
            var privatePassword = args[3];
            var reader = new BuderusKm200Reader(host, port, gatewayPassword, privatePassword);

            var datapointOutdoorTemperature = "/system/sensors/temperatures/outdoor_t1";
            var datapointSystemHealth = "/system/healthStatus";

            ReadAndShow(reader.ReadDatapointValueAsFloat, datapointOutdoorTemperature);
            ReadAndShow(reader.ReadDatapointValueAsString, datapointSystemHealth);
        }

        private static void ReadAndShow<T>(Func<string, T> readMethod, string datapoint)
        {
            try
            {
                var value = readMethod(datapoint);
                Console.WriteLine($"Value of '{datapoint}': {value}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occured while reading datapoint value of '{datapoint}': {ex}");
            }
        }
    }
}
