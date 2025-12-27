using WebApi.DAL;
using WebApi.DAL.Interfaces;
using WebApi.DAL.Models;
using Models.Dto.V1.Requests;

namespace WebApi.BLL.Services;

public class AuditLogOrderService(UnitOfWork unitOfWork, IAuditLogOrderRepository auditLogOrderRepository)
{
    public async Task<bool> BulkInsert(V1AuditLogOrderRequest request, CancellationToken token)
    {
        var now = DateTimeOffset.UtcNow;
        await using var transaction = await unitOfWork.BeginTransactionAsync(token);

        try
        {
            var auditLogDals = request.Orders.Select(o => new AuditLogOrderDal
            {
                OrderId = o.OrderId,
                OrderItemId = o.OrderItemId,
                CustomerId = o.CustomerId,
                OrderStatus = o.OrderStatus,
                CreatedAt = now,
                UpdatedAt = now
            }).ToArray();

            await auditLogOrderRepository.BulkInsert(auditLogDals, token);

            await transaction.CommitAsync(token);
            return true;
        }
        catch
        {
            await transaction.RollbackAsync(token);
            throw;
        }
    }
}