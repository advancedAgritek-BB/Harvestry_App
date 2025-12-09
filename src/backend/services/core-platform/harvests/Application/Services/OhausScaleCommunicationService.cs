using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Harvestry.Harvests.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Harvestry.Harvests.Application.Services;

/// <summary>
/// Ohaus scale communication service - supports MT-SICS protocol
/// Ohaus scales commonly use MT-SICS (Mettler-Toledo Standard Interface Command Set)
/// </summary>
public class OhausScaleCommunicationService : IScaleCommunicationService, IDisposable
{
    private readonly ILogger<OhausScaleCommunicationService> _logger;
    private IScaleConnection? _connection;
    private bool _isConnected;
    private bool _disposed;

    public OhausScaleCommunicationService(ILogger<OhausScaleCommunicationService> logger)
    {
        _logger = logger;
    }

    public string ConnectionType => _connection?.ConnectionType ?? "unknown";
    public bool IsConnected => _isConnected && (_connection?.IsConnected ?? false);

    #region Connection Management

    public async Task<bool> ConnectAsync(ScaleConnectionConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Connecting to Ohaus scale via {ConnectionType}", config.ConnectionType);

            // Create appropriate connection based on type
            _connection = config.ConnectionType.ToLower() switch
            {
                "serial" => new SerialScaleConnection(_logger),
                "network" => new NetworkScaleConnection(_logger),
                "usb" => new UsbScaleConnection(_logger),
                _ => throw new NotSupportedException($"Connection type '{config.ConnectionType}' is not supported")
            };

            _isConnected = await _connection.ConnectAsync(config, cancellationToken);

            if (_isConnected)
            {
                _logger.LogInformation("Successfully connected to Ohaus scale");
                
                // Send initial identification command to verify connection
                var response = await SendCommandAsync("I1", cancellationToken);
                _logger.LogDebug("Scale identification: {Response}", response);
            }

            return _isConnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Ohaus scale");
            _isConnected = false;
            return false;
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (_connection != null)
        {
            await _connection.DisconnectAsync(cancellationToken);
            _isConnected = false;
            _logger.LogInformation("Disconnected from Ohaus scale");
        }
    }

    #endregion

    #region Scale Operations

    public async Task<ScaleReadingData> GetCurrentReadingAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        try
        {
            // MT-SICS command: S = Send stable weight, SI = Send weight immediately
            var response = await SendCommandAsync("SI", cancellationToken);
            return ParseWeightResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get weight reading from scale");
            throw;
        }
    }

    public async Task<bool> TareAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        try
        {
            // MT-SICS command: T = Tare
            var response = await SendCommandAsync("T", cancellationToken);
            return response.Contains("S S") || response.Contains("S A"); // S = Success
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to tare scale");
            return false;
        }
    }

    public async Task<bool> ZeroAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        try
        {
            // MT-SICS command: Z = Zero
            var response = await SendCommandAsync("Z", cancellationToken);
            return response.Contains("S S") || response.Contains("S A");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to zero scale");
            return false;
        }
    }

    public async IAsyncEnumerable<ScaleReadingData> GetContinuousReadingsAsync(
        int intervalMs = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureConnected();

        while (!cancellationToken.IsCancellationRequested && IsConnected)
        {
            ScaleReadingData? reading = null;
            
            try
            {
                reading = await GetCurrentReadingAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting continuous reading, will retry");
            }

            if (reading != null)
            {
                yield return reading;
            }

            await Task.Delay(intervalMs, cancellationToken);
        }
    }

    #endregion

    #region Private Methods

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Scale is not connected");
    }

    private async Task<string> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        if (_connection == null)
            throw new InvalidOperationException("No connection established");

        return await _connection.SendCommandAsync(command, cancellationToken);
    }

    /// <summary>
    /// Parse MT-SICS weight response
    /// Format: S S      123.45 g (S=stable, D=dynamic, +/-=overload/underload)
    /// </summary>
    private ScaleReadingData ParseWeightResponse(string response)
    {
        var reading = new ScaleReadingData
        {
            RawResponse = response,
            Timestamp = DateTime.UtcNow
        };

        if (string.IsNullOrWhiteSpace(response))
        {
            _logger.LogWarning("Empty response from scale");
            return reading;
        }

        // MT-SICS response parsing
        // Format: [Command echo] [Stability] [Weight] [Unit]
        // Example: "S S      234.56 g"
        // S = stable, D = dynamic (unstable)
        // + = overload, - = underload

        var parts = response.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length >= 2)
        {
            // First part is command echo (S, SI, etc.)
            // Second part is stability indicator
            var stabilityIndicator = parts.Length > 1 ? parts[1] : "";
            reading.IsStable = stabilityIndicator == "S";
            reading.IsOverload = stabilityIndicator == "+";
            reading.IsUnderload = stabilityIndicator == "-";

            // Find the weight value (should be numeric)
            for (int i = 2; i < parts.Length; i++)
            {
                if (decimal.TryParse(parts[i], out var weight))
                {
                    reading.NetWeight = weight;
                    reading.GrossWeight = weight; // Gross = Net when tared
                    
                    // Next part should be unit
                    if (i + 1 < parts.Length)
                    {
                        reading.Unit = NormalizeUnit(parts[i + 1]);
                    }
                    break;
                }
            }
        }

        // Alternative parsing using regex for more complex formats
        var weightMatch = Regex.Match(response, @"([+-]?\d+\.?\d*)\s*(g|kg|lb|oz)", RegexOptions.IgnoreCase);
        if (weightMatch.Success)
        {
            reading.NetWeight = decimal.Parse(weightMatch.Groups[1].Value);
            reading.GrossWeight = reading.NetWeight;
            reading.Unit = NormalizeUnit(weightMatch.Groups[2].Value);
        }

        _logger.LogDebug(
            "Parsed weight: {Weight}{Unit}, Stable: {Stable}",
            reading.NetWeight, reading.Unit, reading.IsStable);

        return reading;
    }

    private static string NormalizeUnit(string unit)
    {
        return unit.ToLower() switch
        {
            "g" or "grams" => "Grams",
            "kg" or "kilograms" => "Kilograms",
            "lb" or "lbs" or "pounds" => "Pounds",
            "oz" or "ounces" => "Ounces",
            _ => unit
        };
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }

    #endregion
}

#region Connection Implementations

/// <summary>
/// Abstract base for scale connections
/// </summary>
public interface IScaleConnection : IDisposable
{
    string ConnectionType { get; }
    bool IsConnected { get; }
    Task<bool> ConnectAsync(ScaleConnectionConfig config, CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    Task<string> SendCommandAsync(string command, CancellationToken cancellationToken);
}

/// <summary>
/// Serial port connection (RS-232)
/// </summary>
internal class SerialScaleConnection : IScaleConnection
{
    private readonly ILogger _logger;
    private object? _serialPort; // Would be System.IO.Ports.SerialPort in real implementation
    private bool _isConnected;

    public SerialScaleConnection(ILogger logger)
    {
        _logger = logger;
    }

    public string ConnectionType => "serial";
    public bool IsConnected => _isConnected;

    public Task<bool> ConnectAsync(ScaleConnectionConfig config, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Opening serial connection on {Port} at {BaudRate} baud",
            config.PortName, config.BaudRate);

        // In real implementation:
        // _serialPort = new SerialPort(config.PortName, config.BaudRate, ...);
        // _serialPort.Open();
        
        // Placeholder for actual serial port implementation
        _isConnected = true;
        return Task.FromResult(true);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        // _serialPort?.Close();
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<string> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        // In real implementation:
        // _serialPort.WriteLine(command + "\r\n");
        // return _serialPort.ReadLine();
        
        // Simulated response
        return Task.FromResult($"SI S      {123.45m + new Random().NextDouble() * 10:F2} g");
    }

    public void Dispose()
    {
        // _serialPort?.Dispose();
    }
}

/// <summary>
/// Network/TCP connection
/// </summary>
internal class NetworkScaleConnection : IScaleConnection
{
    private readonly ILogger _logger;
    private object? _tcpClient; // Would be System.Net.Sockets.TcpClient
    private bool _isConnected;

    public NetworkScaleConnection(ILogger logger)
    {
        _logger = logger;
    }

    public string ConnectionType => "network";
    public bool IsConnected => _isConnected;

    public async Task<bool> ConnectAsync(ScaleConnectionConfig config, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Opening network connection to {Host}:{Port}",
            config.Host, config.Port);

        // In real implementation:
        // _tcpClient = new TcpClient();
        // await _tcpClient.ConnectAsync(config.Host, config.Port, cancellationToken);
        
        await Task.Delay(100, cancellationToken); // Simulate connection time
        _isConnected = true;
        return true;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        // _tcpClient?.Close();
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<string> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        // In real implementation, send via NetworkStream
        return Task.FromResult($"SI S      {123.45m + new Random().NextDouble() * 10:F2} g");
    }

    public void Dispose()
    {
        // _tcpClient?.Dispose();
    }
}

/// <summary>
/// USB HID connection
/// </summary>
internal class UsbScaleConnection : IScaleConnection
{
    private readonly ILogger _logger;
    private bool _isConnected;

    public UsbScaleConnection(ILogger logger)
    {
        _logger = logger;
    }

    public string ConnectionType => "usb";
    public bool IsConnected => _isConnected;

    public Task<bool> ConnectAsync(ScaleConnectionConfig config, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Opening USB connection (VID: {VID}, PID: {PID})",
            config.UsbVendorId, config.UsbProductId);

        // In real implementation, would use HidSharp or similar library
        // to connect to USB HID device
        
        _isConnected = true;
        return Task.FromResult(true);
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        _isConnected = false;
        return Task.CompletedTask;
    }

    public Task<string> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        // USB HID typically reads weight reports directly
        // rather than command/response
        return Task.FromResult($"SI S      {123.45m + new Random().NextDouble() * 10:F2} g");
    }

    public void Dispose()
    {
        // Close USB connection
    }
}

#endregion
