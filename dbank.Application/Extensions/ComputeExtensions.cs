using dbank.Application.Models.CashDeposits;
using dbank.Application.Models.Credits;
using dbank.Application.Models.Payments;
using dbank.Domain.Entities;
using dbank.Domain.Exceptions;

namespace dbank.Application.Extensions;

public static class ComputeExtensions
{
    public static decimal ComputeFinalAmount(this CreateCashDepositDto deposit)
    {
        var monthlyInterestRate = deposit.InterestRate / 12m;

        var finalAmount = deposit.DepositAmount;

        for (decimal month = 0; month < deposit.DepositPeriod; month++)
        {
            finalAmount *= (1m + monthlyInterestRate);
        }
        
        return Math.Round(finalAmount);
    }
    
    public static decimal ComputeInitialPayment(this CreateCreditDto credit)
    {
        var initialPayment = credit.CreditAmount * credit.InitialPaymentRate;
        
        return initialPayment;
    }

    public static decimal ComputeMonthlyPayment(this CreateCreditDto credit, decimal initialPayment)
    {
        var residualCreditAmount = credit.CreditAmount - initialPayment;
        var monthlyInterestRate = credit.InterestRate / 12;
        var numberOfMonths = credit.CreditPeriod * 12;

        var interestPowerTerm = 1m;

        for (var i = 0; i < numberOfMonths; i++)
        {
            interestPowerTerm *= (1 + monthlyInterestRate);
        }
        
        var numerator = residualCreditAmount * monthlyInterestRate * interestPowerTerm;
        var denominator = interestPowerTerm - 1;
        
        var monthlyPayment = numerator / denominator;
        
        return Math.Round(monthlyPayment);
    }
    
    public static void ValidationPayment(this CreatePaymentDto? payment, CustomerEntity? sender,
        CustomerEntity? recipient)
    {
        switch (sender, recipient)
        {
            case (null, _):
                throw new EntityNotFoundException($"Sender with id {payment!.CustomerId} not found.");

            case (_, null):
                throw new EntityNotFoundException(
                    $"The recipient with card number {payment!.RecipientCardNumber} not found.");

            case ({ Balance: null }, _):
                throw new EntityNotFoundException($"The sender with id {payment!.CustomerId} doesn't have open balance.");

            case ({ Balance: { Balance: var senderBalance } }, _)
                when senderBalance < payment!.PaymentAmount:
                throw new EntityNotFoundException("There are insufficient funds on balance.");
        }
    }
}
