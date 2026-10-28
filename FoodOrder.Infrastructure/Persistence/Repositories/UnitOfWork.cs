using FoodOrder.Application.Exceptions;
using FoodOrder.Application.Ports;
using Microsoft.EntityFrameworkCore;

namespace FoodOrder.Infrastructure.Persistence.Repositories;

public class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("A mentés sikertelen, mert egy másik folyamat időközben módosította az adatokat. Frissítsd az oldalt!");
        }
    }
}