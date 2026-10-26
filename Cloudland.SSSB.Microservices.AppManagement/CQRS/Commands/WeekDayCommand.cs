using Cloudland.SSSB.DTO;
using Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands.Base;

namespace Cloudland.SSSB.Microservices.AppManagement.CQRS.Commands;

public sealed record AddWeekDayCommand(WeekDayDTO WeekDay) : AddCommand<WeekDayDTO>(WeekDay);
public sealed record AddWeekDayWithConstantCommand(WeekDayDTO WeekDay, Guid? ParentId) : AddCommand<WeekDayDTO>(WeekDay);
public sealed record UpdateWeekDayCommand(WeekDayDTO WeekDay) : UpdateCommand<WeekDayDTO>(WeekDay);
public sealed record DeleteWeekDayCommand(Guid Id) : DeleteCommand<Guid>(Id);


