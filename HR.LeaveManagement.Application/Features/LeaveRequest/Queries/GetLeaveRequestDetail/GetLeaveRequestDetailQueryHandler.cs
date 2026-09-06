using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestDetail;

public class GetLeaveRequestDetailQueryHandler(ILeaveRequestRepository leaveRequestRepository,
    IMapper mapper, IUserService userService) : IRequestHandler<GetLeaveRequestDetailQuery, LeaveRequestDetailsDto>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository = leaveRequestRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IUserService _userService = userService;

    public async Task<LeaveRequestDetailsDto> Handle(GetLeaveRequestDetailQuery request, CancellationToken cancellationToken)
    {
        var leaveRequest = _mapper.Map<LeaveRequestDetailsDto>(await _leaveRequestRepository.GetLeaveRequestWithDetails(request.Id));

        if (leaveRequest == null)
            throw new NotFoundException(nameof(LeaveRequest), request.Id);

        // Add Employee details as needed
        leaveRequest.Employee = await _userService.GetEmployee(leaveRequest.RequestingEmployeeId);

        return leaveRequest;
    }
}