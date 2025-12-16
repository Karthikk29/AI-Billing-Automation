namespace BillingAutomation.Models;

public class AiResponse
{
    public List<FlaggedRecord> FlaggedRecords { get; set; } = new();
    public List<PlanCorrection> SuggestedPlanCorrections { get; set; } = new();
    public string SummaryText { get; set; } = string.Empty;
}

public class FlaggedRecord
{
    public required string CustomerId { get; set; }
    public required string Issue { get; set; }
}

public class PlanCorrection
{
    public required string CustomerId { get; set; }
    public required string SuggestedPlan { get; set; }
}
