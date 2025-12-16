using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BillingAutomation.Models;

[BsonIgnoreExtraElements]
public class CustomerMaster
{
// ... (omitting middle to focus on adding using)

    [Key]
    public string CustomerId { get; set; } = string.Empty;
    public string? Plan { get; set; }
    public string? CityRegion { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class CustomerUsage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UsageId { get; set; }
    
    public string CustomerId { get; set; } = string.Empty;
    public int TotalUsage { get; set; }
    public decimal CalculatedBillAmount { get; set; }
    public DateTime ProcessedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey("CustomerId")]
    public CustomerMaster? Customer { get; set; }
}

[BsonIgnoreExtraElements]
public class BatchAnalysisLog
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public BsonDocument? AiAnalysis { get; set; }
}
