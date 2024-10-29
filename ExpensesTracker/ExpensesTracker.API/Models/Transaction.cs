namespace ExpensesTracker.API.Models;

public class Transaction
{

    public required string AccountNumber { get; set; }
    public required string Currency { get; set; }
    public required string ValueDate { get; set; }
    public required string BalanceBefore { get; set; }
    public required string BalanceAfter { get; set; }
    public required string BookDate { get; set; }
    public required string Amount { get; set; }
    public required string Description { get; set; }
}