using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain;
using Moq;

namespace HR.LeaveManagement.Application.UnitTests.Mocks;

public class MockLeaveAllocationRepository
{
    public static Mock<ILeaveAllocationRepository> GetMockLeaveAllocationRepository()
    {
        var leaveAllocations = new List<LeaveAllocation>
        {
            new()
            {
                Id = 1,
                NumberOfDays = 10,
                LeaveTypeId = 1,
                Period = DateTime.Now.Year,
                EmployeeId = "employee-1"
            }
        };

        var mockRepo = new Mock<ILeaveAllocationRepository>();

        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => leaveAllocations.FirstOrDefault(q => q.Id == id));

        mockRepo.Setup(r => r.GetUserAllocations(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string userId, int leaveTypeId) =>
                leaveAllocations.FirstOrDefault(q => q.EmployeeId == userId && q.LeaveTypeId == leaveTypeId));

        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<LeaveAllocation>()))
            .Returns((LeaveAllocation allocation) =>
            {
                var existing = leaveAllocations.FirstOrDefault(q => q.Id == allocation.Id);
                if (existing != null)
                {
                    leaveAllocations.Remove(existing);
                }
                leaveAllocations.Add(allocation);
                return Task.CompletedTask;
            });

        return mockRepo;
    }
}