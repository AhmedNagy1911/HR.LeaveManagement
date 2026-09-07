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
        List<Domain.LeaveRequest> leaveRequests;
        List<LeaveRequestListDto> requests;

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

            // جيب كل الموظفين مرة واحدة بدل استدعاء منفصل لكل Request
            var allEmployees = await _userService.GetEmployees();
            var employeeLookup = allEmployees.ToDictionary(e => e.Id);

            foreach (var req in requests)
            {
                if (employeeLookup.TryGetValue(req.RequestingEmployeeId, out var employee))
                {
                    req.Employee = employee;
                }
                else
                {
                    // fallback نادر لو الموظف مش موجود في نتيجة GetEmployees (مثلاً دوره اتغيّر)
                    req.Employee = await _userService.GetEmployee(req.RequestingEmployeeId);
                }
            }
        }

        return requests;
    }
}