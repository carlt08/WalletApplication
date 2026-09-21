namespace WalletApplication.API.Models;

/// <summary>Thrown when a withdrawal breaks a business rule. Maps to 400 Bad Request.</summary>
public class InvalidWithdrawalException : Exception
{
    public InvalidWithdrawalException(string message) : base(message)
    {
    }
}

/// <summary>Thrown when the requested wallet does not exist. Maps to 404 Not Found.</summary>
public class WalletNotFoundException : Exception
{
    public WalletNotFoundException(Guid walletId)
        : base($"Wallet '{walletId}' was not found.")
    {
    }
}

/// <summary>Thrown when a concurrent withdrawal changed the wallet first. Maps to 409 Conflict.</summary>
public class ConcurrentWithdrawalException : Exception
{
    public ConcurrentWithdrawalException(Guid walletId)
        : base($"Wallet '{walletId}' was modified by another withdrawal. Please retry.")
    {
    }
}
