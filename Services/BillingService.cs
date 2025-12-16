using System.Globalization;
using BillingAutomation.Models;
using MongoDB.Driver;

namespace BillingAutomation.Services;

public class BillingService : IBillingService
{
    private readonly IAIService _aiService;
    private readonly MongoDB.Driver.IMongoCollection<CustomerMaster> _customerCollection;
    private readonly MongoDB.Driver.IMongoCollection<CustomerUsage> _usageCollection;
    private readonly MongoDB.Driver.IMongoCollection<BatchAnalysisLog> _batchCollection;

    public BillingService(IAIService aiService, MongoDB.Driver.IMongoDatabase database)
    {
        _aiService = aiService;
        _customerCollection = database.GetCollection<CustomerMaster>("CustomerMaster");
        _usageCollection = database.GetCollection<CustomerUsage>("CustomerUsage");
        _batchCollection = database.GetCollection<BatchAnalysisLog>("BatchAnalysisLog");
    }

    public BillingResponseDto ProcessBilling(List<CustomerInputDto> customers)
    {
        var response = new BillingResponseDto();
        var processed = new List<ProcessedCustomerDto>();
        var rejected = new List<RejectedCustomerDto>();
        var customerIds = new HashSet<string>();

        foreach (var customer in customers)
        {
            var errors = new List<string>();

            // Validation 1: CustomerId unique
            if (string.IsNullOrWhiteSpace(customer.CustomerId))
                errors.Add("CustomerId is required.");
            else if (!customerIds.Add(customer.CustomerId))
                errors.Add($"Duplicate CustomerId: {customer.CustomerId}");

            // Validation 2: Usage numeric and > 0
            if (customer.Usage <= 0)
                errors.Add("Usage must be greater than 0.");

            // Normalization
            var normalizedPlan = NormalizePlanName(customer.PlanName);
            if (normalizedPlan == null)
            {
                errors.Add($"Invalid Plan Name: {customer.PlanName}. Must be Basic, Standard, or Premium.");
            }

            var normalizedCity = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(customer.CityRegion.Trim().ToLower());

            if (errors.Any())
            {
                rejected.Add(new RejectedCustomerDto
                {
                    CustomerId = customer.CustomerId,
                    Errors = errors,
                    OriginalData = customer
                });
                continue;
            }

            // Billing Calculation
            decimal billingAmount = 0;
            switch (normalizedPlan)
            {
                case "Basic":
                    billingAmount = (decimal)(customer.Usage * 1.2);
                    break;
                case "Standard":
                    billingAmount = (decimal)(customer.Usage * 1.0) + 50m;
                    break;
                case "Premium":
                    billingAmount = (decimal)(customer.Usage * 0.8) + 100m;
                    break;
            }

            processed.Add(new ProcessedCustomerDto
            {
                CustomerId = customer.CustomerId,
                PlanName = normalizedPlan!,
                CityRegion = normalizedCity,
                Usage = customer.Usage,
                BillingAmount = Math.Round(billingAmount, 2)
            });
        }

        // Call AI for final summary and analysis
        var aiAnalysis = _aiService.AnalyzeData(processed, rejected);

        response.ProcessedCustomers = processed;
        response.RejectedCustomers = rejected;
        response.AiSummary = aiAnalysis;

        // Save to Database (MongoDB)
        try
        {
            foreach (var p in processed)
            {
                // 1. Upsert Customer Master
                var filter = Builders<CustomerMaster>.Filter.Eq(c => c.CustomerId, p.CustomerId);
                var update = Builders<CustomerMaster>.Update.Combine(
                    Builders<CustomerMaster>.Update.Set(c => c.CityRegion, p.CityRegion),
                    Builders<CustomerMaster>.Update.Set(c => c.Plan, p.PlanName),
                    Builders<CustomerMaster>.Update.SetOnInsert(c => c.CreatedDate, DateTime.UtcNow)
                );
                
                _customerCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = true });

                // 2. Insert Usage Record
                _usageCollection.InsertOne(new CustomerUsage
                {
                    CustomerId = p.CustomerId,
                    TotalUsage = (int)p.Usage,
                    CalculatedBillAmount = p.BillingAmount,
                    ProcessedDate = DateTime.UtcNow
                });
            }

            // 3. Save AI Batch Log
            // Convert generic object (JsonElement) to BsonDocument
            var jsonString = System.Text.Json.JsonSerializer.Serialize(aiAnalysis);
            var bsonDoc = MongoDB.Bson.BsonDocument.Parse(jsonString);

            _batchCollection.InsertOne(new BatchAnalysisLog
            {
                AiAnalysis = bsonDoc
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database Save Failed: {ex.Message}");
        }

        return response;
    }

    private string? NormalizePlanName(string planName)
    {
        if (string.IsNullOrWhiteSpace(planName)) return null;
        var lower = planName.Trim().ToLower().Replace(" ", "");
        
        // Exact matches
        if (lower == "basic") return "Basic";
        if (lower == "standard") return "Standard";
        if (lower == "premium") return "Premium";

        // Mappings for imported data
        if (lower == "plana" || lower == "plane") return "Basic";
        if (lower == "planb" || lower == "planc") return "Standard";
        if (lower == "pland") return "Premium";

        return null;
    }

    public IEnumerable<CustomerUsage> GetBillingHistory()
    {
        // Join logic is not native in Mongo basic driver without aggregates.
        // For simplicity, we'll fetch usages and populate customer manually (client-side join).
        // Or just return usages.
        
        var usages = _usageCollection.Find(MongoDB.Driver.Builders<CustomerUsage>.Filter.Empty)
            .SortByDescending(u => u.ProcessedDate)
            .ToList();

        // Simple client-sdie join
        foreach (var u in usages)
        {
            var customer = _customerCollection.Find(c => c.CustomerId == u.CustomerId).FirstOrDefault();
            u.Customer = customer;
        }

        return usages;
    }

    public IEnumerable<BatchAnalysisLog> GetAnalysisHistory()
    {
        return _batchCollection.Find(MongoDB.Driver.Builders<BatchAnalysisLog>.Filter.Empty)
            .SortByDescending(b => b.ProcessedAt)
            .ToList();
    }
}
