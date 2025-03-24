namespace Shared.Models
{
    public class ApprovalArguments
    {
        public string UserId { get; set; } = default!;
        public string RequestType { get; set; } = default!;
        public bool IsLongVacation { get; set; }

        // Optional: target director
        public string? TargetDirector { get; set; }
    }
}
