namespace CCM.API.Models;
public class CustomerRequest {
    public int Id { get; set; }
    public string RequestNo { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerType { get; set; } = "";
    public string ContactPerson { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? GSTIN { get; set; }
    public string? PAN { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? Industry { get; set; }
    public string BillingAddress { get; set; } = "";
    public string? Remarks { get; set; }
    public string? GstFileName { get; set; }
    public string? GstFilePath { get; set; }
    public string Status { get; set; } = "Pending L1";
    public int SubmittedBy { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public int? L1ApprovedBy { get; set; }
    public DateTime? L1ApprovedAt { get; set; }
    public string? L1Comment { get; set; }
    public int? L2ApprovedBy { get; set; }
    public DateTime? L2ApprovedAt { get; set; }
    public string? L2Comment { get; set; }
    public string? SapCustomerId { get; set; }
    public DateTime? SapCreatedAt { get; set; }
    public string? SapRawResponse { get; set; }
    public User? Submitter { get; set; }
    public User? L1Approver { get; set; }
    public User? L2Approver { get; set; }
    public List<ApprovalTimeline> Timeline { get; set; } = new();
}