using MQTTnet;
using MQTTnet.Client;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Harvestry.Edge.Adapters.Mqtt;

public class CloudBridge
{
    private IMqttClient? _mqttClient;
    private readonly string _endpoint;
    private readonly string _clientId;
    private readonly string _certPath;
    private readonly string _keyPath;

    public CloudBridge(string endpoint, string clientId, string certPath, string keyPath)
    {
        _endpoint = endpoint;
        _clientId = clientId;
        _certPath = certPath;
        _keyPath = keyPath;
    }

    public async Task ConnectAsync()
    {
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();

        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(_endpoint, 8883)
            .WithClientId(_clientId)
            .WithCleanSession(false)
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30));

        if (File.Exists(_certPath)) 
        {
            // Simplified certificate loading
            // var cert = X509Certificate2.CreateFromPemFile(_certPath, _keyPath);
            // optionsBuilder.WithTls(new MqttClientOptionsBuilderTlsParameters
            // {
            //    UseTls = true,
            //    Certificates = new[] { cert }
            // });
        }

        var options = optionsBuilder.Build();

        _mqttClient.DisconnectedAsync += async e =>
        {
            Console.WriteLine("[MQTT] Disconnected. Reconnecting...");
            await Task.Delay(5000);
            try { await _mqttClient.ConnectAsync(options); } catch { }
        };

        await _mqttClient.ConnectAsync(options);
    }

    public async Task PublishTelemetryAsync(string topic, string payload)
    {
        if (_mqttClient == null || !_mqttClient.IsConnected) return;

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();

        await _mqttClient.PublishAsync(message);
    }

    public async Task SubscribeAsync(string topic, Func<string, Task> handler)
    {
        if (_mqttClient == null) throw new InvalidOperationException("Not Connected");

        await _mqttClient.SubscribeAsync(topic);
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            if (e.ApplicationMessage.Topic == topic)
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                await handler(payload);
            }
        };
    }
}
