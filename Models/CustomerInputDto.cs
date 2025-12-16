using System.ComponentModel.DataAnnotations;

namespace BillingAutomation.Models;

public class CustomerInputDto
{
    [Required]
    public string CustomerId { get; set; } = string.Empty;

    [Required]
    public string PlanName { get; set; } = string.Empty;

    [Required]
    public string CityRegion { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Usage must be greater than 0")]
    public double Usage { get; set; }
}
