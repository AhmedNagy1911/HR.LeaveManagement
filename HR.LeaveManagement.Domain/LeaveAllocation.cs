using HR.LeaveManagement.Domain.Common;

namespace HR.LeaveManagement.Domain;

/// <summary>
/// Represents the number of leave days allocated to an employee
/// for a specific leave type and period.
/// </summary>
public class LeaveAllocation : BaseEntity
{
    public int NumberOfDays { get; set; }
     
    public LeaveType? LeaveType { get; set; }
    public int LeaveTypeId { get; set; }

    public int Period { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
}