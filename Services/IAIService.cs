using BillingAutomation.Models;

namespace BillingAutomation.Services;

public interface IAIService
{
    object AnalyzeData(List<ProcessedCustomerDto> processed, List<RejectedCustomerDto> rejected);
}
