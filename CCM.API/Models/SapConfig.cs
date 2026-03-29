namespace CCM.API.Models;
public class SapConfig {
    public int Id { get; set; }
    public string BaseUrl { get; set; } = "";
    public string ApiPath { get; set; } = "/sap/opu/odata/sap/API_BUSINESS_PARTNER";
    public string? SapUsername { get; set; }
    public string? SapPassword { get; set; }
    public string? ClientNo { get; set; }
    public string AccountGrp { get; set; } = "KUNA";
    public string Mode { get; set; } = "mock";
    public int TimeoutMs { get; set; } = 5000;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}