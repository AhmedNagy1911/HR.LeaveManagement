using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest;
using HR.LeaveManagement.Application.MappingProfiles;
using HR.LeaveManagement.Application.Models.Identity;
using HR.LeaveManagement.Application.UnitTests.Mocks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace HR.LeaveManagement.Application.UnitTests.Features.LeaveRequests.Commands;

public class UpdateLeaveRequestCommandHandlerTests
{
    private readonly Mock<ILeaveRequestRepository> _mockLeaveRequestRepo;
    private readonly Mock<ILeaveTypeRepository> _mockLeaveTypeRepo;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<IAppLogger<UpdateLeaveRequestCommandHandler>> _mockAppLogger;
    private readonly IMapper _mapper;

    public UpdateLeaveRequestCommandHandlerTests()
    {
        _mockLeaveRequestRepo = MockLeaveRequestRepository.GetMockLeaveRequestRepository();
        _mockLeaveTypeRepo = MockLeaveTypeRepository.GetMockLeaveTypeRepository();

        _mockUserService = new Mock<IUserService>();
        _mockUserService.Setup(u => u.GetEmployee(It.IsAny<string>()))
            .ReturnsAsync(new Employee { Id = "employee-1", Email = "employee1@test.com", Firstname = "Test", Lastname = "Employee" });

        _mockEmailSender = new Mock<IEmailSender>();
        _mockEmailSender.Setup(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()))
            .ReturnsAsync(true);

        _mockAppLogger = new Mock<IAppLogger<UpdateLeaveRequestCommandHandler>>();

        var mapperConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<LeaveRequestProfile>();
        }, NullLoggerFactory.Instance);

        _mapper = mapperConfig.CreateMapper();
    }

    private UpdateLeaveRequestCommandHandler CreateHandler() =>
        new(_mockLeaveRequestRepo.Object,
            _mockLeaveTypeRepo.Object,
            _mockUserService.Object,
            _mapper,
            _mockEmailSender.Object,
            _mockAppLogger.Object);

    [Fact]
    public async Task Handle_ValidRequest_UpdatesLeaveRequestAndSendsEmail()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new UpdateLeaveRequestCommand
        {
            Id = 1,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 3),
            LeaveTypeId = 1,
            RequestComments = "Updated comment",
            Cancelled = false
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(MediatR.Unit.Value);

        var updatedRequest = await _mockLeaveRequestRepo.Object.GetByIdAsync(1);
        updatedRequest.RequestComments.ShouldBe("Updated comment");

        _mockEmailSender.Verify(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()), Times.Once);
    }

    [Fact]
    public async Task Handle_RequestNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new UpdateLeaveRequestCommand
        {
            Id = 999,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 3),
            LeaveTypeId = 1
        };

        // Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidDateRange_ThrowsBadRequestException()
    {
        // Arrange - EndDate before StartDate should fail BaseLeaveRequestValidator
        var handler = CreateHandler();
        var command = new UpdateLeaveRequestCommand
        {
            Id = 1,
            StartDate = new DateTime(2026, 10, 5),
            EndDate = new DateTime(2026, 10, 1),
            LeaveTypeId = 1
        };

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentLeaveType_ThrowsBadRequestException()
    {
        // Arrange
        var handler = CreateHandler();
        var command = new UpdateLeaveRequestCommand
        {
            Id = 1,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 3),
            LeaveTypeId = 999 // doesn't exist
        };

        // Act & Assert
        await Should.ThrowAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_EmailSendingFails_StillCompletesSuccessfully()
    {
        // Arrange - email failures shouldn't break the update (caught & logged)
        _mockEmailSender.Setup(e => e.SendEmail(It.IsAny<Application.Models.Email.EmailMessage>()))
            .ThrowsAsync(new Exception("SMTP failure"));

        var handler = CreateHandler();
        var command = new UpdateLeaveRequestCommand
        {
            Id = 1,
            StartDate = new DateTime(2026, 10, 1),
            EndDate = new DateTime(2026, 10, 3),
            LeaveTypeId = 1,
            RequestComments = "Should still succeed"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(MediatR.Unit.Value);
        _mockAppLogger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
    }
}