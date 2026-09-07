using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Email;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Email;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Commands.ChangeLeaveRequestApproval;

public class ChangeLeaveRequestApprovalCommandHandler(
     ILeaveRequestRepository leaveRequestRepository,
     ILeaveTypeRepository leaveTypeRepository,
     ILeaveAllocationRepository leaveAllocationRepository,
     IUserService userService,
     IMapper mapper,
     IEmailSender emailSender) : IRequestHandler<ChangeLeaveRequestApprovalCommand, Unit>
{
    private readonly IMapper _mapper = mapper;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly ILeaveRequestRepository _leaveRequestRepository = leaveRequestRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository = leaveTypeRepository;
    private readonly ILeaveAllocationRepository _leaveAllocationRepository = leaveAllocationRepository;
    private readonly IUserService _userService = userService;

    public async Task<Unit> Handle(ChangeLeaveRequestApprovalCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _leaveRequestRepository.GetByIdAsync(request.Id);

        if (leaveRequest is null)
            throw new NotFoundException(nameof(LeaveRequest), request.Id);

        var previousApprovalStatus = leaveRequest.Approved;

        leaveRequest.Approved = request.Approved;
        await _leaveRequestRepository.UpdateAsync(leaveRequest);

        var allocation = await _leaveAllocationRepository.GetUserAllocations(leaveRequest.RequestingEmployeeId, leaveRequest.LeaveTypeId);

        if (allocation is null)
            throw new NotFoundException(nameof(LeaveAllocation), leaveRequest.LeaveTypeId);

        int daysRequested = (leaveRequest.EndDate - leaveRequest.StartDate).Days;

        // بيتحول من غير موافَق عليه لموافَق عليه => نخصم الأيام
        if (request.Approved && previousApprovalStatus != true)
        {
            allocation.NumberOfDays -= daysRequested;
            await _leaveAllocationRepository.UpdateAsync(allocation);
        }
        // كان موافَق عليه وبيترفض دلوقتي => نرجّع الأيام
        else if (!request.Approved && previousApprovalStatus == true)
        {
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
                Body = $"The approval status for your leave request for {leaveRequest.StartDate:D} to {leaveRequest.EndDate:D} has been updated.",
                Subject = "Leave Request Approval Status Updated"
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