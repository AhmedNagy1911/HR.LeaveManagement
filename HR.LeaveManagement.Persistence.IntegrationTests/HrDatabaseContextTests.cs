using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace HR.LeaveManagement.Persistence.IntegrationTests;

public class HrDatabaseContextTests
{
    private HrDatabaseContext _hrDatabaseContext;

    public HrDatabaseContextTests()
    {
        var dbOptions = new DbContextOptionsBuilder<HrDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

        _hrDatabaseContext = new HrDatabaseContext(dbOptions);
    }

    [Fact]
    public async Task Save_SetDateCreatedValue()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            Id = 1,
            DefaultDays = 10,
            Name = "Test Vacation"
        };

        // Act
        await _hrDatabaseContext.LeaveTypes.AddAsync(leaveType, TestContext.Current.CancellationToken);
        await _hrDatabaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        leaveType.DateCreated.ShouldNotBeNull();
    }

    [Fact]
    public async Task Save_SetDateModifiedValue()
    {
        // Arrange
        var leaveType = new LeaveType
        {
            Id = 1,
            DefaultDays = 10,
            Name = "Test Vacation"
        };

        // Act
        await _hrDatabaseContext.LeaveTypes.AddAsync(leaveType, TestContext.Current.CancellationToken);
        await _hrDatabaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        leaveType.DateModified.ShouldNotBeNull();
    }
}