using BillingAutomation.Models;
using BillingAutomation.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillingAutomation.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;

    public BillingController(IBillingService billingService)
    {
        _billingService = billingService;
    }

    [HttpPost("process")]
    public ActionResult<BillingResponseDto> Process([FromBody] List<CustomerInputDto> customers)
    {
        if (customers == null || !customers.Any())
        {
            return BadRequest("No customer data provided.");
        }

        var result = _billingService.ProcessBilling(customers);
        return Ok(result);
    }

    [HttpGet("history")]
    public ActionResult<IEnumerable<CustomerUsage>> GetHistory()
    {
        var history = _billingService.GetBillingHistory();
        return Ok(history);
    }

    [HttpGet("analysis")]
    public ActionResult<IEnumerable<BatchAnalysisLog>> GetAnalysis()
    {
        var logs = _billingService.GetAnalysisHistory();
        return Ok(logs);
    }
}
