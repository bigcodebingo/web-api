using Dapper;
using WebApi.DAL.Interfaces;
using WebApi.DAL.Models;

namespace WebApi.DAL.Repositories;

public class AuditLogOrderRepository(UnitOfWork unitOfWork) : IAuditLogOrderRepository
{
    public async Task<AuditLogOrderDal[]> BulkInsert(AuditLogOrderDal[] model, CancellationToken token)
    {
        var sql = @"
            insert into audit_log_order 
            (
                order_id,
                order_item_id,
                customer_id,
                order_status,
                created_at,
                updated_at  
             )
            select 
                order_id,
                order_item_id,
                customer_id,
                order_status,
                created_at,
                updated_at
            from unnest(@AuditLogs)
            returning 
                id,
                order_id,
                order_item_id,
                customer_id,
                order_status,
                created_at,
                updated_at;
        ";

        var conn = await unitOfWork.GetConnection(token);
        
        var res = await conn.QueryAsync<AuditLogOrderDal>(new CommandDefinition(
            sql, new {AuditLogs = model}, cancellationToken: token));
        
        return res.ToArray();
    }
}