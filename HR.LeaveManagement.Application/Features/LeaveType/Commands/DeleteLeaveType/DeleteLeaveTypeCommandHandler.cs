using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveType.Commands.DeleteLeaveType;

public class DeleteLeaveTypeCommandHandler(ILeaveTypeRepository leaveTypeRepository) : IRequestHandler<DeleteLeaveTypeCommand, Unit>
{
    private readonly ILeaveTypeRepository _leavetyperepository = leaveTypeRepository;

    public async Task<Unit> Handle(DeleteLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        // retrieve domain entity object
        var leaveTypeToDelete = await _leavetyperepository.GetByIdAsync(request.Id);

        // verify that record exists
        if( leaveTypeToDelete is null)
            throw new NotFoundException(nameof(LeaveType), request.Id);

        // remove from database
        await _leavetyperepository.DeleteAsync(leaveTypeToDelete);

        // return record id
        return Unit.Value;
    }
}
