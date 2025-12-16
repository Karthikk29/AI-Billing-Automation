namespace BillingAutomation.Models;

public class BillingResponseDto
{
    public List<ProcessedCustomerDto> ProcessedCustomers { get; set; } = new();
    public List<RejectedCustomerDto> RejectedCustomers { get; set; } = new();
    public object AiSummary { get; set; } = new();
}
