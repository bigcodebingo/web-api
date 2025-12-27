using Microsoft.AspNetCore.Mvc;
using Models.Dto.V1.Requests;
using Models.Dto.V1.Responses;
using WebApi.BLL.Services;
using WebApi.Validators;

[Route("api/v1/audit")]
public class AuditLogController(AuditLogOrderService auditLogOrderService, ValidatorFactory validatorFactory): ControllerBase
{
    [HttpPost("log-order")]
    public async Task<ActionResult<V1AuditLogOrderResponse>> V1LogOrder([FromBody] V1AuditLogOrderRequest request, CancellationToken token)
    {
        var validationResult = await validatorFactory.GetValidator<V1AuditLogOrderRequest>().ValidateAsync(request, token);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.ToDictionary());
        }
        
        var success = await auditLogOrderService.BulkInsert(request, token);

        return Ok(new V1AuditLogOrderResponse
        {
            Success = success
        });
    }
}