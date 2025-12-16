namespace BillingAutomation.Models;

public class ProcessedCustomerDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string CityRegion { get; set; } = string.Empty;
    public double Usage { get; set; }
    public decimal BillingAmount { get; set; }
}
