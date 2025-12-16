namespace BillingAutomation.Models;

public class RejectedCustomerDto
{
    public string CustomerId { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public object? OriginalData { get; set; }
}
