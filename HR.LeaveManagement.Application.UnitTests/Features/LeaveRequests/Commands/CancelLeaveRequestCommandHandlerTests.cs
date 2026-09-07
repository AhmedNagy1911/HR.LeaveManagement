using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest;
using HR.LeaveManagement.Application.Models.Identity;
using HR.LeaveManagement.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace HR.LeaveManagement.Application.UnitTests.Features.LeaveRequests.Commands;

public class CancelLeaveRequestCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _mockLeaveRequestRepo;
    private readonly Mock<ILeaveAllocationRepository> _mockLeaveAllocationRepo;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IEmailSender> _mockEmailSender;

    public CancelLeaveRequestCommandHandlerTests()
    {
        _mockLeaveRequestRepo = MockLeaveRequestRepository.GetMockLeaveRequestRepository();
        _mockLeaveAllocationRepo = MockLeaveAllocationRepository.GetMockLeaveAllocationRepository();

        _mockUserService = new Mock<IUserService>();
        _mockUserService.Setup(u => u.GetEmployee(It.IsAny<string>()))
            .ReturnsAsync(new Employee { Id = "employee-1", Email = "employee1@test.com", Firstname = "Test", Lastname = "Employee" });

        _mockEmailSender = new Mock<IEmailSender>();
        _mockEmailSender.Setup(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()))
            .ReturnsAsync(true);
    }

    private CancelLeaveRequestCommandHandler CreateHandler() =>
        new(_mockLeaveRequestRepo.Object,
            _mockLeaveAllocationRepo.Object,
            _mockUserService.Object,
            _mockEmailSender.Object);

    [Fact]
    public async Task Cancel_ApprovedRequest_RefundsAllocationDays()
    {
        // Arrange - request Id 2: Approved = true, EndDate - StartDate = 2 days
        var handler = CreateHandler();
        var command = new CancelLeaveRequestCommand { Id = 2 };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(MediatR.Unit.Value);

        var updatedRequest = await _mockLeaveRequestRepo.Object.GetByIdAsync(2);
        updatedRequest.Cancelled.ShouldBeTrue();

        var updatedAllocation = await _mockLeaveAllocationRepo.Object.GetUserAllocations("employee-1", 1);
        updatedAllocation.NumberOfDays.ShouldBe(12); // 10 + 2

        _mockEmailSender.Verify(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()), Times.Once);
    }

    [Fact]
    public async Task Cancel_PendingUnapprovedRequest_DoesNotChangeAllocationDays()
    {
        // Arrange - request Id 1: Approved = null (still pending)
        var handler = CreateHandler();
        var command = new CancelLeaveRequestCommand { Id = 1 };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedRequest = await _mockLeaveRequestRepo.Object.GetByIdAsync(1);
        updatedRequest.Cancelled.ShouldBeTrue();

        var allocation = await _mockLeaveAllocationRepo.Object.GetUserAllocations("employee-1", 1);
        allocation.NumberOfDays.ShouldBe(10); // unchanged since it was never approved
    }

    [Fact]
    public async Task Handle_RequestNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new CancelLeaveRequestCommand { Id = 999 };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ApprovedRequestWithMissingAllocation_ThrowsNotFoundException()
    {
        // Arrange
        var mockLeaveRequestRepo = MockLeaveRequestRepository.GetMockLeaveRequestRepository();
        var approvedRequestNoAllocation = new HR.LeaveManagement.Domain.LeaveRequest
        {
            Id = 4,
            StartDate = new DateTime(2026, 11, 1),
            EndDate = new DateTime(2026, 11, 3),
            LeaveTypeId = 2, // no allocation exists for this leave type
            Approved = true,
            RequestingEmployeeId = "employee-1"
        };
        mockLeaveRequestRepo.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(approvedRequestNoAllocation);

        var handler = new CancelLeaveRequestCommandHandler(
            mockLeaveRequestRepo.Object,
            _mockLeaveAllocationRepo.Object,
            _mockUserService.Object,
            _mockEmailSender.Object);

        var command = new CancelLeaveRequestCommand { Id = 4 };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}