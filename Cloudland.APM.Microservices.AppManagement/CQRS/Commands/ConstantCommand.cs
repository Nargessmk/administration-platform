using Cloudland.APM.DTO;
using Cloudland.APM.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.APM.Microservices.AppManagement.CQRS.Commands;

public sealed record AddConstantCommand(ConstantDTO constant) : AddCommand<ConstantDTO?>(constant);

public sealed record UpdateConstantCommand(ConstantDTO constant) : UpdateCommand<ConstantDTO>(constant);

public sealed record DeleteConstantCommand(Guid Id) : DeleteCommand<Guid>(Id);

public sealed record DeleteAllChildsConstantCommandByParentId(Guid ParentId) : DeleteCommand<Guid>(ParentId);

public sealed record ReindexConstantCommand : ReindexCommand<ConstantDTO>;

public sealed record StoreJsonDataCommand(Guid NodeId, string? NodeType, string DataObject)
    : StoreJsonDataCommand<Guid>(NodeId, NodeType, DataObject);