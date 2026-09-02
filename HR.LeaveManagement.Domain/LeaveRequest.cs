using HR.LeaveManagement.Domain.Common;

namespace HR.LeaveManagement.Domain;

/// <summary>
/// Represents a leave request submitted by an employee,
/// including the requested dates, leave type, approval status,
/// and any additional comments.
/// </summary>
public class LeaveRequest : BaseEntity
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public LeaveType? LeaveType { get; set; }
    public int LeaveTypeId { get; set; }

    public DateTime DateRequested { get; set; }
    public string? RequestComments { get; set; }

    public bool? Approved { get; set; }
    public bool Cancelled { get; set; }

    public string RequestingEmployeeId { get; set; } = string.Empty;

}