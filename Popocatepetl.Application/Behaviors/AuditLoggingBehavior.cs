using MediatR;
using Popocatepetl.Application.Common;
using Popocatepetl.Domain.Entities;
using Popocatepetl.Domain.Interfaces;

namespace Popocatepetl.Application.Behaviors;

public sealed class AuditLoggingBehavior<TRequest, TResponse>(
    IAuditLogRepository auditLogRepository,
    ICurrentUserContext currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IAuditableRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            var response = await next();
            await auditLogRepository.AddAsync(
                AuditLog.Create(currentUser.Email, request.ActionName, DateTime.UtcNow, true, null));
            return response;
        }
        catch (Exception ex)
        {
            await auditLogRepository.AddAsync(
                AuditLog.Create(currentUser.Email, request.ActionName, DateTime.UtcNow, false, ex.Message));
            throw;
        }
    }
}
