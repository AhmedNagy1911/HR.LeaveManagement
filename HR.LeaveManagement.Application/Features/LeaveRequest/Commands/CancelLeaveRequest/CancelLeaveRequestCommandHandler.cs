using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.CancelLeaveRequest;

public class CancelLeaveRequestCommandHandler(ILeaveRequestRepository leaveRequestRepository,
ILeaveAllocationRepository leaveAllocationRepository,
IUserService userService,
IEmailSender emailSender) : IRequestHandler<CancelLeaveRequestCommand, Unit>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository = leaveRequestRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository = leaveAllocationRepository;
    private readonly IUserService _userService = userService;
    private readonly IEmailSender _emailSender = emailSender;

    public async Task<Unit> Handle(CancelLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

        if (leaveRequest is null)
            throw new NotFoundException(nameof(leaveRequest), request.Id);

        leaveRequest.Cancelled = true;
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        // if already approved, re-evaluate the employee's allocations for the leave type
        if (leaveRequest.Approved == true)
        {
            int daysRequested = (leaveRequest.EndDate - leaveRequest.StartDate).Days;
            var allocation = await _leaveAllocationRepository.GetUserAllocations(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);

            if (allocation is null)
                throw new NotFoundException(nameof(LeaveAllocation), leaveRequest.LeaveTypeId);

            allocation.NumberOfDays += daysRequested;
            await _leaveAllocationRepository.UpdateAsync(allocation);
        }

        // send confirmation email
        try
        {
            var employee = await _userService.GetEmployee(leaveRequest.RequestingEmployeeId);
            var email = new EmailMessage
            {
                To = employee.Email,
                Body = $"Your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D} has been cancelled successfully.",
                Subject = "Leave Request Cancelled"
            };

            await _emailSender.SendEmail(email);
        }
        catch (Exception)
        {
            // log error
        }

        return Unit.Value;
    }
}