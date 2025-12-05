namespace Harvestry.Telemetry.Application.DTOs;

/// <summary>
/// Result of telemetry ingestion operation.
/// </summary>
public class IngestResultDto
{
    public int TotalReceived { get; set; }
    public int Accepted { get; set; }
    public int Rejected { get; set; }
    public int Duplicates { get; set; }
    public long ProcessingTimeMs { get; set; }
    
    public double SuccessRate => TotalReceived == 0 ? 0 : (Accepted / (double)TotalReceived) * 100.0;
    public double Throughput => ProcessingTimeMs == 0 ? 0 : (TotalReceived / (ProcessingTimeMs / 1000.0));
}

