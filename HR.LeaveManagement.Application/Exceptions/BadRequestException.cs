namespace HR.LeaveManagement.Application.Exceptions;

public class BadRequestException(string message) : Exception(message)
{
}
