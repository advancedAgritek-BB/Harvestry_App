namespace Harvestry.Analytics.Domain.ValueObjects;

public record DashboardWidget(
    string Id, 
    string ReportId, 
    int X, 
    int Y, 
    int W, 
    int H, 
    string Title, 
    string VisualizationType
);





