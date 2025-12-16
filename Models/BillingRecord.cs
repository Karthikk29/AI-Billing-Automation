namespace BillingAutomation.Models;

public class BillingRecord
{
    public required string CustomerId { get; set; }
    public required string CustomerName { get; set; } // Enriched from DB or input
    public required string PlanType { get; set; }
    public double Usage { get; set; }
    public decimal BillingAmount { get; set; }
    public string Status { get; set; } = "Processed";
    public List<string> Notes { get; set; } = new();
}
