using System.Net.Sockets;
using NModbus;

namespace Harvestry.Edge.Adapters.Modbus;

public class SkidLinkClient : IDisposable
{
    private readonly string _ipAddress;
    private readonly int _port;
    private TcpClient? _tcpClient;
    private IModbusMaster? _modbusMaster;

    public SkidLinkClient(string ipAddress, int port = 502)
    {
        _ipAddress = ipAddress;
        _port = port;
    }

    public async Task ConnectAsync()
    {
        if (_tcpClient != null && _tcpClient.Connected) return;

        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(_ipAddress, _port);
        
        var factory = new ModbusFactory();
        _modbusMaster = factory.CreateMaster(_tcpClient);
        
        // Settings for reliability
        _modbusMaster.Transport.ReadTimeout = 2000;
        _modbusMaster.Transport.WriteTimeout = 2000;
    }

    public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort numPoints)
    {
        if (_modbusMaster == null) throw new InvalidOperationException("Not Connected");
        return await _modbusMaster.ReadHoldingRegistersAsync(slaveId, startAddress, numPoints);
    }

    public async Task WriteSingleRegisterAsync(byte slaveId, ushort registerAddress, ushort value)
    {
        if (_modbusMaster == null) throw new InvalidOperationException("Not Connected");
        await _modbusMaster.WriteSingleRegisterAsync(slaveId, registerAddress, value);
    }

    public async Task WriteMultipleRegistersAsync(byte slaveId, ushort startAddress, ushort[] data)
    {
        if (_modbusMaster == null) throw new InvalidOperationException("Not Connected");
        await _modbusMaster.WriteMultipleRegistersAsync(slaveId, startAddress, data);
    }

    // High Level "Anderson" specific method example
    public async Task SetTargetEcPhAsync(double ec, double ph)
    {
        // HE Anderson Map (Hypothetical):
        // 40001: Target EC (x100)
        // 40002: Target pH (x100)
        ushort ecVal = (ushort)(ec * 100);
        ushort phVal = (ushort)(ph * 100);
        
        await WriteMultipleRegistersAsync(1, 0, new[] { ecVal, phVal });
    }

    public void Dispose()
    {
        _modbusMaster?.Dispose();
        _tcpClient?.Dispose();
    }
}





