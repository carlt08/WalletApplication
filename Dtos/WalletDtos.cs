using System.ComponentModel.DataAnnotations;

namespace WalletApplication.API.Dtos;

public record WithdrawRequest([Required] decimal Amount);

public record BalanceResponse(Guid WalletId, decimal Balance);
