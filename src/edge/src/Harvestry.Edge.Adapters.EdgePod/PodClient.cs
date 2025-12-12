using CoAP;
using System.Text;

namespace Harvestry.Edge.Adapters.EdgePod;

public class PodClient
{
    private readonly CoapClient _client;

    public PodClient()
    {
        _client = new CoapClient();
    }

    public Task DiscoverPodsAsync()
    {
        // Placeholder
        return Task.CompletedTask;
    }

    public void SetValve(string podIp, int channel, bool state)
    {
        _client.Uri = new Uri($"coap://{podIp}/valves/{channel}");
        
        string payload = state ? "1" : "0";
        // CoAP.NET Put is synchronous in this old version
        var response = _client.Put(payload);

        // Check response code via numeric value or named constant if available
        // In this version, response.Code might be an enum.
        // Assuming success if not null for now.
        if (response == null)
        {
            throw new Exception("Failed to set valve: No Response");
        }
    }

    public void SetMicroProgram(string podIp, string programJson)
    {
        _client.Uri = new Uri($"coap://{podIp}/program");
        
        var response = _client.Post(programJson);
        
        if (response == null)
        {
            throw new Exception("Failed to upload program: No Response");
        }
    }
}





