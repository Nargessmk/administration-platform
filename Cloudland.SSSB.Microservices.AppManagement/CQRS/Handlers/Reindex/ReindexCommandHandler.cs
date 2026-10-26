using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;
using Cloudland.SSSB.Microservices.AppManagement.Services;
using MediatR;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Handlers.Reindex;

public class ReindexByViewCommandHandler : IRequestHandler<ReindexByViewCommand, ReindexResult>
{
    private readonly IReindexService _reindexService;

    public ReindexByViewCommandHandler(IReindexService reindexService)
    {
        _reindexService = reindexService;
    }

    public async Task<ReindexResult> Handle(ReindexByViewCommand request, CancellationToken cancellationToken)
    {
        return await _reindexService.ReindexByViewNameAsync(request.ViewName, cancellationToken);
    }
}

public class ReindexAllCommandHandler : IRequestHandler<ReindexAllCommand, ReindexResult>
{
    private readonly IReindexService _reindexService;

    public ReindexAllCommandHandler(IReindexService reindexService)
    {
        _reindexService = reindexService;
    }

    public async Task<ReindexResult> Handle(ReindexAllCommand request, CancellationToken cancellationToken)
    {
        return await _reindexService.ReindexAllAsync(cancellationToken);
    }
}

public class GetAvailableViewsCommandHandler : IRequestHandler<GetAvailableViewsCommand, List<string>>
{
    private readonly IReindexService _reindexService;

    public GetAvailableViewsCommandHandler(IReindexService reindexService)
    {
        _reindexService = reindexService;
    }

    public async Task<List<string>> Handle(GetAvailableViewsCommand request, CancellationToken cancellationToken)
    {
        return await _reindexService.GetAvailableViewNamesAsync();
    }
}
