using WebApi.DAL.Models;

namespace WebApi.DAL.Interfaces;

public interface IAuditLogOrderRepository
{
    Task<AuditLogOrderDal[]> BulkInsert(AuditLogOrderDal[] model, CancellationToken token);
}