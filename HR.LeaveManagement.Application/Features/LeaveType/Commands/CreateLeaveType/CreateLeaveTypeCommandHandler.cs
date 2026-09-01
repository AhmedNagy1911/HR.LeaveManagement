using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveType.Commands.CreateLeaveType;

public class CreateLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository, Mapper mapper)
    : IRequestHandler<CreateLeaveTypeCommand, int>
{
    private readonly ILeaveTypeRepository _leavetyperepository = leaveTypeRepository;
    private readonly Mapper _mapper = mapper;

    public async Task<int> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        // Validate incoming data



        // convert to domain entity object
        var leaveTypeToCreate = _mapper.Map<Domain.LeaveType>(request);

        // add to database
        await _leavetyperepository.CreateAsync(leaveTypeToCreate);

        // retun record id
        return leaveTypeToCreate.Id;
    }
}
