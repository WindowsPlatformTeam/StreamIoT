using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace StreamIoT.IoTHub
{
    public class IoTHubProvider
    {
        static DeviceClient deviceClient;
        static string iotHubUri = "{iotHubUri}";
        static string connectionString = "{connectionString}";
        static IoTHubProvider _instance;
        static RegistryManager registryManager;

        private IoTHubProvider()
        {
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        }

        public static IoTHubProvider GetInstance()
        {
            if (_instance == null)
            {
                _instance = new IoTHubProvider();
            }
            return _instance;
        }

        public async Task SendDeviceToCloudMessagesAsync()
        {
            if(deviceClient == null)
            {
                await CreateDeviceClient();
            }

            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    deviceId = "myFirstDevice",
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                await deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(1000);
            }
        }

        private async Task CreateDeviceClient()
        {
            var deviceKey = await AddDeviceAsync();
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey("myFirstDevice", deviceKey), Microsoft.Azure.Devices.Client.TransportType.Mqtt);
        }

        private async Task<string> AddDeviceAsync()
        {
            string deviceId = "myFirstDevice";
            Device device = null;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }
            return device.Authentication.SymmetricKey.PrimaryKey;
        }
    }
}
