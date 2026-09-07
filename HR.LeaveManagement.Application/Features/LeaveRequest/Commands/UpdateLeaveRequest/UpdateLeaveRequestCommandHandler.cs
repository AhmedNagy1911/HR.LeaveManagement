using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Logging;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.UpdateLeaveRequest;

public class UpdateLeaveRequestCommandHandler(
     ILeaveRequestRepository leaveRequestRepository, ILeaveTypeRepository leaveTypeRepository,
     IUserService userService, IMapper mapper, IEmailSender emailSender,
     IAppLogger<UpdateLeaveRequestCommandHandler> appLogger) : IRequestHandler<UpdateLeaveRequestCommand, Unit>
{
    private readonly IMapper _mapper = mapper;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IUserService _userService = userService;
    private readonly IAppLogger<UpdateLeaveRequestCommandHandler> _appLogger = appLogger;
    private readonly ILeaveRequestRepository _leaveRequestRepository = leaveRequestRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository = leaveTypeRepository;

    public async Task<Unit> Handle(UpdateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

        if (leaveRequest is null)
            throw new NotFoundException(nameof(LeaveRequest), request.Id);


        var validator = new UpdateLeaveRequestCommandValidator(_leaveTypeRepository, _leaveRequestRepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid Leave Request", validationResult);

        _mapper.Map(request, leaveRequest);

        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        try
        {
            // send confirmation email
            var employee = await _userService.GetEmployee(leaveRequest.RequestingEmployeeId);
            var email = new EmailMessage
            {
                To = employee.Email,
                Body = $"Your leave request for {request.StartDate:D} to {request.EndDate:D} " +
                        $"has been updated successfully.",
                Subject = "Leave Request Updated"
            };

            await _emailSender.SendEmail(email);
        }
        catch (Exception ex)
        {
            _appLogger.LogWarning(ex.Message);
        }

        return Unit.Value;
    }
}