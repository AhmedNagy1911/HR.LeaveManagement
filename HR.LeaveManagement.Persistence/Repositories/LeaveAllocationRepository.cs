using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Persistence.DatabaseContext;

namespace HR.LeaveManagement.Persistence.Repositories;

public class LeaveAllocationRepository(HrDatabaseContext context) 
    : GenericRepository<LeaveAllocation>(context), ILeaveAllocationRepository
{
}
