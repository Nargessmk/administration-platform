using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MediatR;
using Cloudland.SSSB.DTO;
using Microsoft.AspNetCore.Http;
using Cloudland.SSSB.Microservices.AppManagement.Extensions;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Location.Command;

public sealed class SetLocationAdminCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> repository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<SetLocationAdminCommand, ResultDTO<bool>>
{
    public async Task<ResultDTO<bool>> Handle(SetLocationAdminCommand request, CancellationToken cancellationToken)
    {
        var query = repository.SingleResultQuery().AndFilter(x => x.Id == request.LocationId);
        var location = await repository.FirstOrDefaultAsync(query, cancellationToken);
        
        if (location == null)
            return ResultDTO<bool>.Failure("محل رزرو یافت نشد", false);

        location.AdminId = request.AdminId;
        location.LastModifiedAt = DateTime.Now;
        
        // Extract User from Context if available
        if (httpContextAccessor.HttpContext?.User?.Claims?.Any() == true)
        {
            var userClaim = httpContextAccessor.HttpContext.User.Claims.ToList().ElementAtOrDefault(10)?.Value;
            if (Guid.TryParse(userClaim, out var userId))
            {
                location.LastModifiedUser = userId;
            }
        }

        repository.Update(location);
        await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        return ResultDTO<bool>.Success(true, "ادمین رستوران با موفقیت منتسب شد");
    }
}
