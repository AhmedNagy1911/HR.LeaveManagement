using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Identity;
using HR.LeaveManagement.Application.Contracts.Persistence;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveRequest.Queries.GetLeaveRequestList;

public class GetLeaveRequestListQueryHandler(ILeaveRequestRepository leaveRequestRepository,
    IMapper mapper, IUserService userService) : IRequestHandler<GetLeaveRequestListQuery, List<LeaveRequestListDto>>
{
    private readonly ILeaveRequestRepository _leaveRequestRepository = leaveRequestRepository;
    private readonly IMapper _mapper = mapper;
    private readonly IUserService _userService = userService;

    public async Task<List<LeaveRequestListDto>> Handle(GetLeaveRequestListQuery request, CancellationToken cancellationToken)
    {

        var leaveRequests = new List<Domain.LeaveRequest>();
        var requests = new List<LeaveRequestListDto>();

        // Check if it is logged in employee
        if (request.IsLoggedInUser)
        {
            var userId = _userService.UserId;
            leaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetails(userId);

            var employee = await _userService.GetEmployee(userId);
            requests = _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);
            foreach (var req in requests)
            {
                req.Employee = employee;
            }
        }
        else
        {
            leaveRequests = await _leaveRequestRepository.GetLeaveRequestsWithDetails();
            requests = _mapper.Map<List<LeaveRequestListDto>>(leaveRequests);
            foreach (var req in requests)
            {
                req.Employee = await _userService.GetEmployee(req.RequestingEmployeeId);
            }
        }

        return requests;
    }
}