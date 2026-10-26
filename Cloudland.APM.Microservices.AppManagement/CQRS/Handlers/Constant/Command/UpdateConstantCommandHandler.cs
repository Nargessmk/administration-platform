using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using MassTransit;
using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands;
using Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Base;
using Cloudland.APM.Microservices.AppManagement.Extensions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Nest;
using Raven.Client.Documents.Session;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Handlers.Constant.Command;

public sealed class UpdateConstantCommandHandler(
    IUnitOfWork unitOfWork,
    EntityFrameworkCore.Repository.Interfaces.IRepository<Cloudland.APM.Models.Constant> repository,
    IMapper mapper,
    IPublishEndpoint publishEndpoint,
    IElasticClient elasticClient,
    IDistributedCache distributedCache,
    IHttpContextAccessor httpContextAccessor,
    IAsyncDocumentSession dbSession)
    : BaseHandler<Cloudland.APM.Models.Constant, Guid>(unitOfWork, repository, mapper, publishEndpoint, elasticClient,
            distributedCache, httpContextAccessor, dbSession),
        IRequestHandler<UpdateConstantCommand, ConstantDTO>
{
    public async Task<ConstantDTO> Handle(UpdateConstantCommand request, CancellationToken cancellationToken)
    {
        var query = _repository.SingleResultQuery().AndFilter(item => item.Id == request.constant.Id);

        var founded = await _repository.SingleOrDefaultAsync(query, cancellationToken);


        founded.LastModifiedAt = DateTime.Now;
        founded.LastModifiedUser = new Guid(_httpContextAccessor.HttpContext!.User.Claims.ToList()[10].Value);
        founded.title = request.constant.title;
        founded.Icon = request.constant.Icon;
        founded.Color = request.constant.Color;
        founded.ParentId = request.constant.ParentId;
        founded.ConstantValue = request.constant.ConstantValue;
        founded.JsonData = request.constant.JsonData;
        founded.desc = request.constant.desc;
        founded.AllowDelete = request.constant.AllowDelete;
        founded.AllowUpdate = request.constant.AllowUpdate;
        founded.hidden = request.constant.hidden;
        founded.ValueType = request.constant.ValueType;
        founded.NodeTypeId = request.constant.NodeTypeId;
        founded.ConstantType = request.constant.ConstantType;
        founded.ConstantValue = request.constant.ConstantValue;
        founded.JsonData = request.constant.JsonData;
        founded.Disabled = request.constant.Disabled;
        founded.Priority = request.constant.Priority;

        var constant = _repository.Update(founded);

        if (founded.Id == founded.NodeTypeId)
        {
            var relatedConstantsQuery = _repository.MultipleResultQuery()
                .AndFilter(item => item.NodeTypeId == founded.Id && item.Id != founded.Id);
            
            var relatedConstants = await _repository.SearchAsync(relatedConstantsQuery, cancellationToken);
            
            foreach (var relatedConstant in relatedConstants)
            {
                relatedConstant.Color = founded.Color;
                relatedConstant.Icon = founded.Icon;
                relatedConstant.ConstantValue = founded.ConstantValue;
                relatedConstant.AllowDelete = founded.AllowDelete;
                relatedConstant.AllowUpdate = founded.AllowUpdate;
                relatedConstant.Disabled = founded.Disabled;
                relatedConstant.hidden = founded.hidden;
                relatedConstant.LastModifiedAt = DateTime.Now;
                relatedConstant.LastModifiedUser = new Guid(_httpContextAccessor.HttpContext!.User.Claims.ToList()[10].Value);
                _repository.Update(relatedConstant);
            }
        }
        
        
        await _unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);

        // Sync to Elasticsearch
        try
        {
            var constantDto = _mapper.Map<ConstantDTO>(constant);
            await _elasticClient.IndexAsync(constantDto, i => i
                .Index(ElasticsearchIndexNames.Constant)
                .Id(constantDto.Id)
            , cancellationToken);
        }
        catch
        {
            // ignore ES sync errors for now; logging can be added later
        }

        return _mapper.Map<ConstantDTO>(constant);
    }
}