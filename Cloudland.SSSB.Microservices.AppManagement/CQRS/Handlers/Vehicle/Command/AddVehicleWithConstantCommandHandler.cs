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
using Cloudland.SSSB.Microservices.AppManagement.Extensions;
using Cloudland.SSSB.Models.View;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Vehicle.Command;

public sealed class AddVehicleWithConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Models.Vehicle> repository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<APM.Models.Constant> constantRepository,
    EntityFrameworkCore.Repository.Interfaces.IRepository<VehicleView> vehicleViewRepository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Models.Vehicle, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<AddVehicleWithConstantCommand, ResultDTO<VehicleDTO>?>
{
    public async Task<ResultDTO<VehicleDTO>?> Handle(AddVehicleWithConstantCommand request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new Exception("HTTP context is not available");
        }

        var newConstant = new APM.Models.Constant
        {
            Id = Guid.NewGuid(),
            ParentId = request.ParentId,
            CreateAt = DateTime.Now,
            LastModifiedAt = DateTime.Now
        };
        if (_httpContextAccessor.HttpContext.User.Claims.Count() > 10)
        {
            var userClaim = _httpContextAccessor.HttpContext.User.Claims.ToList()[10].Value;
            var userId = Guid.Parse(userClaim);
            newConstant.UserCreated = userId;
            newConstant.LastModifiedUser = userId;
        }
        newConstant.CustomerId = Guid.NewGuid();
        newConstant.AllowDelete = true;
        newConstant.AllowUpdate = true;
        newConstant.Disabled = false;
        newConstant.hidden = false;
        newConstant.title = $"{request.Vehicle.LicensePlate +'-'+request.Vehicle.TypeTitle}";
        newConstant.desc = request.Vehicle.desc;
        newConstant.Icon = "error_outline";
        newConstant.NodeTypeId = Guid.Parse("f0f05e21-c819-44a4-836d-1a669889034e");
        newConstant.ConstantValue = "Vehicle";
        var addedConstant = await constantRepository.AddAsync(newConstant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        var entity = _mapper.Map<Models.Vehicle>(request.Vehicle);
        entity.Id = addedConstant.Id;
        entity.CreateAt = DateTime.Now;
        entity.LastModifiedAt = DateTime.Now;
        entity.UserCreated = newConstant.UserCreated;
        entity.LastModifiedUser = newConstant.LastModifiedUser;
        entity.CustomerId = Guid.NewGuid();
        entity.AllowDelete = true;
        entity.AllowUpdate = true;
        entity.Disabled = false;
        entity.hidden = false;

        var added = await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch from view using repository
        try
        {
            var viewQuery = vehicleViewRepository.SingleResultQuery().AndFilter(v => v.Id == entity.Id);
            var viewRow = await vehicleViewRepository.SingleOrDefaultAsync(viewQuery, cancellationToken);
            if (viewRow != null)
            {
                await _elasticClient.IndexAsync(viewRow, i => i
                    .Index(ElasticsearchIndexNames.Vehicle)
                    .Id(viewRow.Id)
                , cancellationToken);
            }
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return ResultDTO<VehicleDTO>.Success(_mapper.Map<VehicleDTO>(added), "وسیله نقلیه با موفقیت ایجاد شد");
    }
}


