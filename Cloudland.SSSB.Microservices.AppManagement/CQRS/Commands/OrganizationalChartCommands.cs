using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddOrganizationalChartCommand(OrganizationalChartDTO organizationalChart) : AddCommand<OrganizationalChartDTO>(organizationalChart);
public sealed record AddOrganizationalChartWithConstantCommand(OrganizationalChartDTO organizationalChart, Guid? ParentId) : AddCommand<OrganizationalChartDTO>(organizationalChart);
public sealed record UpdateOrganizationalChartCommand(OrganizationalChartDTO organizationalChart) : UpdateCommand<OrganizationalChartDTO>(organizationalChart);
public sealed record DeleteOrganizationalChartCommand(Guid Id) : DeleteCommand<Guid>(Id);
