using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval;
using HR.LeaveManagement.Application.MappingProfiles;
using HR.LeaveManagement.Application.Models.Identity;
using HR.LeaveManagement.Application.UnitTests.Mocks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace HR.LeaveManagement.Application.UnitTests.Features.LeaveRequests.Commands;

public class ChangeLeaveRequestApprovalCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _mockLeaveRequestRepo;
    private readonly Mock<ILeaveTypeRepository> _mockLeaveTypeRepo;
    private readonly Mock<ILeaveAllocationRepository> _mockLeaveAllocationRepo;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly IMapper _mapper;

    public ChangeLeaveRequestApprovalCommandHandlerTests()
    {
        _mockLeaveRequestRepo = MockLeaveRequestRepository.GetMockLeaveRequestRepository();
        _mockLeaveTypeRepo = MockLeaveTypeRepository.GetMockLeaveTypeRepository();
        _mockLeaveAllocationRepo = MockLeaveAllocationRepository.GetMockLeaveAllocationRepository();

        _mockUserService = new Mock<IUserService>();
        _mockUserService.Setup(u => u.GetEmployee(It.IsAny<string>()))
            .ReturnsAsync(new Employee { Id = "employee-1", Email = "employee1@test.com", Firstname = "Test", Lastname = "Employee" });

        _mockEmailSender = new Mock<IEmailSender>();
        _mockEmailSender.Setup(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()))
            .ReturnsAsync(true);

        var mapperConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<LeaveRequestProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = mapperConfig.CreateMapper();
    }

    private ChangeLeaveRequestApprovalCommandHandler CreateHandler() =>
        new(_mockLeaveRequestRepo.Object,
            _mockLeaveTypeRepo.Object,
            _mockLeaveAllocationRepo.Object,
            _mockUserService.Object,
            _mapper,
            _mockEmailSender.Object);

    [Fact]
    public async Task Approve_PendingRequest_DeductsAllocationDays()
    {
        // Arrange - request Id 1: StartDate +1, EndDate +4 => 3 days requested, initial allocation = 10 days
        var handler = CreateHandler();
        var command = new ChangeLeaveRequestApprovalCommand { Id = 1, Approved = true };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(MediatR.Unit.Value);

        var updatedAllocation = await _mockLeaveAllocationRepo.Object.GetUserAllocations("employee-1", 1);
        updatedAllocation.NumberOfDays.ShouldBe(7); // 10 - 3

        var updatedRequest = await _mockLeaveRequestRepo.Object.GetByIdAsync(1);
        updatedRequest.Approved.ShouldBe(true);

        _mockEmailSender.Verify(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()), Times.Once);
    }

    [Fact]
    public async Task Unapprove_PreviouslyApprovedRequest_RefundsAllocationDays()
    {
        // Arrange - request Id 2 is already Approved = true, EndDate - StartDate = 2 days
        var handler = CreateHandler();
        var command = new ChangeLeaveRequestApprovalCommand { Id = 2, Approved = false };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedAllocation = await _mockLeaveAllocationRepo.Object.GetUserAllocations("employee-1", 1);
        updatedAllocation.NumberOfDays.ShouldBe(12); // 10 + 2

        var updatedRequest = await _mockLeaveRequestRepo.Object.GetByIdAsync(2);
        updatedRequest.Approved.ShouldBe(false);
    }

    [Fact]
    public async Task Approve_AlreadyApprovedRequest_DoesNotDeductDaysAgain()
    {
        // Arrange - request Id 2 is already Approved = true; approving it again shouldn't double-deduct
        var handler = CreateHandler();
        var command = new ChangeLeaveRequestApprovalCommand { Id = 2, Approved = true };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedAllocation = await _mockLeaveAllocationRepo.Object.GetUserAllocations("employee-1", 1);
        updatedAllocation.NumberOfDays.ShouldBe(10); // unchanged
    }

    [Fact]
    public async Task Handle_RequestNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new ChangeLeaveRequestApprovalCommand { Id = 999, Approved = true };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AllocationNotFound_ThrowsNotFoundException()
    {
        // Arrange - request for an employee/leave type with no allocation record
        var mockLeaveRequestRepo = MockLeaveRequestRepository.GetMockLeaveRequestRepository();
        var noAllocationRequest = new HR.LeaveManagement.Domain.LeaveRequest
        {
            Id = 3,
            StartDate = new DateTime(2026, 11, 1),
            EndDate = new DateTime(2026, 11, 3),
            LeaveTypeId = 2, // no allocation exists for this leave type
            RequestingEmployeeId = "employee-1"
        };
        mockLeaveRequestRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(noAllocationRequest);

        var handler = new ChangeLeaveRequestApprovalCommandHandler(
            mockLeaveRequestRepo.Object,
            _mockLeaveTypeRepo.Object,
            _mockLeaveAllocationRepo.Object,
            _mockUserService.Object,
            _mapper,
            _mockEmailSender.Object);

        var command = new ChangeLeaveRequestApprovalCommand { Id = 3, Approved = true };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}