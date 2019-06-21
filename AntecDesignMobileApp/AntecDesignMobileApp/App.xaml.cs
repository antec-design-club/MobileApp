using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AntecDesignMobileApp.Services;
using AntecDesignMobileApp.Views;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;

namespace AntecDesignMobileApp
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            // Handle when your app starts

            var factory = new MqttFactory();
            var mqttClient = factory.CreateMqttClient();
            var options = new MqttClientOptionsBuilder()
                .WithClientId("App1")
                .WithTcpServer("test.mosquitto.org")
                .Build();

            mqttClient.UseConnectedHandler(async _ =>
            {
                Debug.WriteLine("MQTT client connected");

                await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("/flow-sensor/value").Build());

                Debug.WriteLine("MQTT client subscribed to topic /flow-sensor/value");
            });

            mqttClient.UseApplicationMessageReceivedHandler(e =>
            {
                Debug.WriteLine($"MQTT message received: Topic: {e.ApplicationMessage.Topic}, Payload: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            });

            mqttClient.UseDisconnectedHandler(async _ =>
            {
                Debug.WriteLine("MQTT client disconnected");
                await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

                try
                {
                    await mqttClient.ConnectAsync(options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"MQTT client failed to reconnect to the server: {ex.Message}");
                }
            });

            await mqttClient.ConnectAsync(options).ConfigureAwait(false);
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
