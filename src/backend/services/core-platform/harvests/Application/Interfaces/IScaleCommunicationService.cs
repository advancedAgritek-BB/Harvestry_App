namespace Harvestry.Harvests.Application.Interfaces;

/// <summary>
/// Abstract interface for scale communication - supports multiple connection types
/// </summary>
public interface IScaleCommunicationService
{
    /// <summary>
    /// Connection type identifier (usb, serial, network, bluetooth)
    /// </summary>
    string ConnectionType { get; }

    /// <summary>
    /// Whether the scale is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connect to the scale
    /// </summary>
    Task<bool> ConnectAsync(ScaleConnectionConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from the scale
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the current weight reading
    /// </summary>
    Task<ScaleReadingData> GetCurrentReadingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send tare command to scale
    /// </summary>
    Task<bool> TareAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send zero command to scale
    /// </summary>
    Task<bool> ZeroAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to continuous weight readings
    /// </summary>
    IAsyncEnumerable<ScaleReadingData> GetContinuousReadingsAsync(
        int intervalMs = 100,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for scale connection
/// </summary>
public class ScaleConnectionConfig
{
    /// <summary>
    /// Connection type: usb, serial, network, bluetooth
    /// </summary>
    public string ConnectionType { get; set; } = "usb";

    /// <summary>
    /// Serial port name (e.g., COM3, /dev/ttyUSB0)
    /// </summary>
    public string? PortName { get; set; }

    /// <summary>
    /// Baud rate for serial connection
    /// </summary>
    public int BaudRate { get; set; } = 9600;

    /// <summary>
    /// Data bits for serial connection
    /// </summary>
    public int DataBits { get; set; } = 8;

    /// <summary>
    /// Stop bits for serial connection
    /// </summary>
    public string StopBits { get; set; } = "One";

    /// <summary>
    /// Parity for serial connection
    /// </summary>
    public string Parity { get; set; } = "None";

    /// <summary>
    /// Network host address
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// Network port
    /// </summary>
    public int Port { get; set; } = 8000;

    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// USB vendor ID
    /// </summary>
    public string? UsbVendorId { get; set; }

    /// <summary>
    /// USB product ID
    /// </summary>
    public string? UsbProductId { get; set; }
}

/// <summary>
/// Data from a scale reading
/// </summary>
public class ScaleReadingData
{
    /// <summary>
    /// Gross weight (total weight on scale)
    /// </summary>
    public decimal GrossWeight { get; set; }

    /// <summary>
    /// Tare weight (container weight)
    /// </summary>
    public decimal TareWeight { get; set; }

    /// <summary>
    /// Net weight (gross - tare)
    /// </summary>
    public decimal NetWeight { get; set; }

    /// <summary>
    /// Unit of measurement from scale
    /// </summary>
    public string Unit { get; set; } = "g";

    /// <summary>
    /// Whether the reading is stable
    /// </summary>
    public bool IsStable { get; set; }

    /// <summary>
    /// Whether the scale is in overload condition
    /// </summary>
    public bool IsOverload { get; set; }

    /// <summary>
    /// Whether the scale is in underload/negative condition
    /// </summary>
    public bool IsUnderload { get; set; }

    /// <summary>
    /// Raw response string from scale
    /// </summary>
    public string? RawResponse { get; set; }

    /// <summary>
    /// Timestamp when reading was taken
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}





