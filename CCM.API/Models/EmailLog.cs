namespace CCM.API.Models;
public class EmailLog {
    public int Id { get; set; }
    public int? RequestId { get; set; }
    public string ToEmail { get; set; } = "";
    public string ToName { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Sent";
}