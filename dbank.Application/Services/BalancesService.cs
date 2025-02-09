using DBank.Application.Abstractions;
using DBank.Application.Models.Balances;
using DBank.Domain;
using DBank.Domain.Entities;
using DBank.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DBank.Application.Services;

public class BalancesService(BankDbContext context) : IBalancesService
{
    public async Task Create(CreateBalanceDto balance)
    {
        var entity = new BalanceEntity
        {
            Balance = balance.Balance,
            CustomerId = balance.CustomerId,
        };
        await context.Balances.AddAsync(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task<BalanceEntity> GetByUser(long customerId)
    {
        var balance = await context.Balances.FirstOrDefaultAsync(b => b.CustomerId == customerId);

        if (balance == null)
        {
            throw new EntityNotFoundException($"User with id {customerId} has no balance entity.");
        }
        
        return balance;
    }
}
