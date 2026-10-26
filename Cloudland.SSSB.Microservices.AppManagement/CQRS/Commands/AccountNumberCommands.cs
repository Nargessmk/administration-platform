using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddAccountNumberCommand(AccountNumberDTO accountNumber) : AddCommand<AccountNumberDTO>(accountNumber);
public sealed record UpdateAccountNumberCommand(AccountNumberDTO accountNumber) : UpdateCommand<AccountNumberDTO>(accountNumber);
public sealed record DeleteAccountNumberCommand(Guid Id) : DeleteCommand<Guid>(Id);
