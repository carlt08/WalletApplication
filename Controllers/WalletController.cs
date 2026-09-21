using Microsoft.AspNetCore.Mvc;
using WalletApplication.API.Dtos;
using WalletApplication.API.Events;
using WalletApplication.API.Models;
using WalletApplication.API.Services;

namespace WalletApplication.API.Controllers;

[ApiController]
[Route("api/wallets")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly ILogger<WalletController> _logger;

    public WalletController(IWalletService walletService, ILogger<WalletController> logger)
    {
        _walletService = walletService;
        _logger = logger;

        // Subscribe to the internal backend event and log it operationally.
        _walletService.WithdrawalCompleted += OnWithdrawalCompleted;
    }

    [HttpGet("{walletId:guid}/balance")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance(Guid walletId, CancellationToken cancellationToken)
    {
        try
        {
            var balance = await _walletService.GetBalanceAsync(walletId, cancellationToken);
            return Ok(new BalanceResponse(walletId, balance));
        }
        catch (WalletNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{walletId:guid}/withdraw")]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Withdraw(Guid walletId, WithdrawRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var balance = await _walletService.WithdrawAsync(walletId, request.Amount, cancellationToken);
            return Ok(new BalanceResponse(walletId, balance));
        }
        catch (WalletNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidWithdrawalException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ConcurrentWithdrawalException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    private void OnWithdrawalCompleted(object? sender, WithdrawalEvent e)
    {
        _logger.LogInformation(
            "WithdrawalEvent received: Wallet {WalletId}, Amount {Amount}, OccurredAtUtc {OccurredAtUtc}.",
            e.WalletId, e.Amount, e.OccurredAtUtc);
    }
}
