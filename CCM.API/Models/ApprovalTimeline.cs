namespace CCM.API.Models;
public class ApprovalTimeline {
    public int Id { get; set; }
    public int RequestId { get; set; }
    public int? ActionById { get; set; }
    public string Role { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime ActionAt { get; set; } = DateTime.UtcNow;
    public CustomerRequest? Request { get; set; }
    public User? ActionBy { get; set; }
}