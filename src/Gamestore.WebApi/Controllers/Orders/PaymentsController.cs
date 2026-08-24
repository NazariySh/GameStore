using Gamestore.BLL.DTOs.Payments;
using Gamestore.BLL.Interfaces.Payments;
using Gamestore.WebApi.Constants;
using Gamestore.WebApi.Extensions;
using Gamestore.WebApi.Models.Payments.PaymentMethods;
using Gamestore.WebApi.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamestore.WebApi.Controllers.Orders;

[Route("api/orders")]
public class PaymentsController : BaseApiController
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(PaymentMethodsResponseModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMethods(CancellationToken cancellationToken)
    {
        var paymentMethods = await _paymentService.GetPaymentMethodsAsync(cancellationToken);
        return Ok(new PaymentMethodsResponseModel
        {
            PaymentMethods = paymentMethods,
        });
    }

    [Authorize(Policy = CustomPolicyNames.CanBuyGames)]
    [HttpPost("payment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status402PaymentRequired)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ProcessPayment(PaymentRequest request, CancellationToken cancellationToken)
    {
        var response = await _paymentService.ProcessPaymentAsync(request, User.GetId(), cancellationToken);
        return response.MapToActionResult();
    }
}