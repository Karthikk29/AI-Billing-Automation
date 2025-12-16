using BillingAutomation.Models;

namespace BillingAutomation.Services;

public interface IBillingService
{
    BillingResponseDto ProcessBilling(List<CustomerInputDto> customers);
    IEnumerable<CustomerUsage> GetBillingHistory();
    IEnumerable<BatchAnalysisLog> GetAnalysisHistory();
}
