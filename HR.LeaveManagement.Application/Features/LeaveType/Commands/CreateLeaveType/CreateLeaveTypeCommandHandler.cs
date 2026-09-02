using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveType.Commands.CreateLeaveType;

public class CreateLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository, Mapper mapper)
    : IRequestHandler<CreateLeaveTypeCommand, int>
{
    private readonly ILeaveTypeRepository _leavetyperepository = leaveTypeRepository;
    private readonly Mapper _mapper = mapper;

    public async Task<int> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {

        var validator = new CreateLeaveTypeCommandValidator(_leavetyperepository);
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.Errors.Any())
            throw new BadRequestException("Invalid Leave type", validationResult);

        var leaveTypeToCreate = _mapper.Map<Domain.LeaveType>(request);

        // add to database
        await _leavetyperepository.CreateAsync(leaveTypeToCreate);

        // retun record id
        return leaveTypeToCreate.Id;
    }
}
