using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain.Common;
using HR.LeaveManagement.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace HR.LeaveManagement.Persistence.Repositories;

public class GenericRepository<T>(HrDatabaseContext context) 
    : IGenericRepository<T> where T : BaseEntity
{
    protected readonly HrDatabaseContext _context = context;

    public async Task CreateAsync(T entity)
    {
        await _context.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _context.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<T>> GetAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>()
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task UpdateAsync(T entity)
    {
        //var originalCreatedDate = await _context.Set<T>().AsNoTracking().Where(q => q.Id == entity.Id).Select(s => s.DateCreated).SingleAsync();
        //entity.DateCreated = originalCreatedDate;
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

}