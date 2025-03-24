namespace Shared.Models
{
    public class ApprovalLog
    {
        public string ApprovedBy { get; set; } = default!;
        public DateTime Timestamp { get; set; }
    }
}
