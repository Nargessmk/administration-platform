using AutoMapper;
using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Base;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Constant.Command;

public sealed class DeleteConstantSmartCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Location> locationRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Personnel> personnelRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalPosition> organizationalPositionRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.OrganizationalChart> organizationalChartRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Contractor> contractorRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Vehicle> vehicleRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Food> foodRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Meal> mealRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.WeekDay> weekDayRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession,
    ILogger<DeleteConstantSmartCommandHandler> logger)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, constantRepository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<DeleteConstantSmartCommand, ResultDTO<int>?>
{
    public async Task<ResultDTO<int>?> Handle(DeleteConstantSmartCommand request, CancellationToken cancellationToken)
    {
        // Load the constant node
        var constantQuery = _repository.SingleResultQuery().AndFilter(item => item.Id == request.Id);
        var constantNode = await _repository.SingleOrDefaultAsync(constantQuery, cancellationToken);
        if (constantNode == null)
        {
            return ResultDTO<int>.Failure("گره ثابت با این شناسه پیدا نشد", 0);
        }

        // Collect the full subtree: all descendants plus the root Id
        var allIdsToDelete = new HashSet<Guid> { request.Id };
        var currentLevel = new List<Guid> { request.Id };
        while (currentLevel.Count > 0)
        {
            var levelIds = currentLevel.ToList();
            var childrenQuery = _repository
                .MultipleResultQuery()
                .AndFilter(item => item.ParentId != null && levelIds.Contains(item.ParentId.Value));
            var children = await _repository.SearchAsync(childrenQuery, cancellationToken);

            var nextLevel = new List<Guid>();
            foreach (var child in children)
            {
                if (allIdsToDelete.Add(child.Id))
                {
                    nextLevel.Add(child.Id);
                }
            }
            currentLevel = nextLevel;
        }

        // Fetch constants for those IDs to know their ConstantValue types
        var constantsQuery = _repository
            .MultipleResultQuery()
            .AndFilter(item => allIdsToDelete.Contains(item.Id));
        var constantsToDelete = await _repository.SearchAsync(constantsQuery, cancellationToken);

        // Group IDs by predefined types
        var locationIds = constantsToDelete.Where(c => c.ConstantValue == "Location").Select(c => c.Id).ToList();
        var personnelIds = constantsToDelete.Where(c => c.ConstantValue == "Personnel").Select(c => c.Id).ToList();
        var orgPositionIds = constantsToDelete.Where(c => c.ConstantValue == "OrganizationalPosition").Select(c => c.Id).ToList();
        var orgChartIds = constantsToDelete.Where(c => c.ConstantValue == "OrganizationalChart").Select(c => c.Id).ToList();
        var contractorIds = constantsToDelete.Where(c => c.ConstantValue == "Contractor").Select(c => c.Id).ToList();
        var vehicleIds = constantsToDelete.Where(c => c.ConstantValue == "Vehicle").Select(c => c.Id).ToList();
        var foodIds = constantsToDelete.Where(c => c.ConstantValue == "Food").Select(c => c.Id).ToList();
        var mealIds = constantsToDelete.Where(c => c.ConstantValue == "Meal").Select(c => c.Id).ToList();
        var calendarIds = constantsToDelete.Where(c => c.ConstantValue == "Calendar").Select(c => c.Id).ToList();
        var weekDayIds = constantsToDelete.Where(c => c.ConstantValue == "WeekDay").Select(c => c.Id).ToList();
        int affectedRelated = 0;

        if (locationIds.Count > 0)
            affectedRelated += await locationRepository.RemoveAsync(x => locationIds.Contains(x.Id), cancellationToken);
        if (personnelIds.Count > 0)
            affectedRelated += await personnelRepository.RemoveAsync(x => personnelIds.Contains(x.Id), cancellationToken);
        if (orgPositionIds.Count > 0)
            affectedRelated += await organizationalPositionRepository.RemoveAsync(x => orgPositionIds.Contains(x.Id), cancellationToken);
        if (orgChartIds.Count > 0)
            affectedRelated += await organizationalChartRepository.RemoveAsync(x => orgChartIds.Contains(x.Id), cancellationToken);
        if (contractorIds.Count > 0)
            affectedRelated += await contractorRepository.RemoveAsync(x => contractorIds.Contains(x.Id), cancellationToken);
        if (vehicleIds.Count > 0)
            affectedRelated += await vehicleRepository.RemoveAsync(x => vehicleIds.Contains(x.Id), cancellationToken);
        if (foodIds.Count > 0)
            affectedRelated += await foodRepository.RemoveAsync(x => foodIds.Contains(x.Id), cancellationToken);
        if (mealIds.Count > 0)
            affectedRelated += await mealRepository.RemoveAsync(x => mealIds.Contains(x.Id), cancellationToken);
        if (weekDayIds.Count > 0)
            affectedRelated += await weekDayRepository.RemoveAsync(x => weekDayIds.Contains(x.Id), cancellationToken);
        
        // Finally delete all constants in the subtree (descendants + root)
        var affectedConstants = await _repository.RemoveAsync(x => allIdsToDelete.Contains(x.Id), cancellationToken);

        if (affectedConstants > 0 || affectedRelated > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        else
        {
            logger.LogWarning($"No rows affected for smart constant delete (cascade). Id={request.Id}");
        }

        var totalAffected = affectedConstants + affectedRelated;
        return ResultDTO<int>.Success(totalAffected, "حذف با موفقیت انجام شد");
    }
}
