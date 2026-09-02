using HR.LeaveManagement.Domain.Common;

namespace HR.LeaveManagement.Domain;

/// <summary>
/// Represents a type of leave available to employees,
/// such as annual leave, sick leave, or unpaid leave.
/// </summary>
public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int DefaultDays { get; set; }
}
