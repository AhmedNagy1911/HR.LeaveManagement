using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveType.Queries.GetLeaveTypeDetails;

public class GetLeaveTypeDetailsQueryHandler(ILeaveTypeRepository leaveTypeRepository, IMapper mapper) 
    : IRequestHandler<GetLeaveTypeDetailsQuery, LeaveTypeDetailDto>
{
    private readonly ILeaveTypeRepository _leaveTypeRepository = leaveTypeRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<LeaveTypeDetailDto> Handle(GetLeaveTypeDetailsQuery request, CancellationToken cancellationToken)
    {
        var leaveType = await _leaveTypeRepository.GetByIdAsync(request.Id);

        if (leaveType is null)
            throw new NotFoundException(nameof(LeaveType), request.Id);

        return _mapper.Map<LeaveTypeDetailDto>(leaveType);
    }
}