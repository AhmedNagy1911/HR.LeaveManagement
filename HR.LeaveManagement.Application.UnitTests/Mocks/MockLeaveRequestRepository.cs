using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain;
using Moq;

namespace HR.LeaveManagement.Application.UnitTests.Mocks;

public class MockLeaveRequestRepository
{
    // تواريخ ثابتة عشان التستات متبقاش هشة مع مرور الوقت
    public static readonly DateTime PendingRequestStartDate = new(2026, 10, 1);
    public static readonly DateTime PendingRequestEndDate = new(2026, 10, 4);   // 3 days
    public static readonly DateTime ApprovedRequestStartDate = new(2026, 10, 10);
    public static readonly DateTime ApprovedRequestEndDate = new(2026, 10, 12); // 2 days
    public static readonly DateTime DateRequested = new(2026, 9, 1);

    public static Mock<ILeaveRequestRepository> GetMockLeaveRequestRepository()
    {
        var leaveRequests = new List<LeaveRequest>
        {
            new()
            {
                Id = 1,
                StartDate = PendingRequestStartDate,
                EndDate = PendingRequestEndDate, // 3 days
                LeaveTypeId = 1,
                DateRequested = DateRequested,
                RequestComments = "Test request",
                Approved = null,
                Cancelled = false,
                RequestingEmployeeId = "employee-1"
            },
            new()
            {
                Id = 2,
                StartDate = ApprovedRequestStartDate,
                EndDate = ApprovedRequestEndDate, // 2 days
                LeaveTypeId = 1,
                DateRequested = DateRequested,
                RequestComments = "Already approved request",
                Approved = true,
                Cancelled = false,
                RequestingEmployeeId = "employee-1"
            }
        };

        var mockRepo = new Mock<ILeaveRequestRepository>();

        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => leaveRequests.FirstOrDefault(q => q.Id == id));

        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<LeaveRequest>()))
            .Returns((LeaveRequest leaveRequest) =>
            {
                var existing = leaveRequests.FirstOrDefault(q => q.Id == leaveRequest.Id);
                if (existing != null)
                {
                    leaveRequests.Remove(existing);
                }
                leaveRequests.Add(leaveRequest);
                return Task.CompletedTask;
            });

        return mockRepo;
    }
}