public class Notification
{
    public int NotificationId { get; set; }
    public int PatientId { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Time { get; set; }
    public string Type { get; set; } = null!;
    public DateTime CreatedAt { get; internal set; }
}
