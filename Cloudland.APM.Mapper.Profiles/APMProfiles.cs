using Cloudland.APM.DTO;
using Cloudland.APM.Mapper.Profiles.CustomConverter;
using Cloudland.APM.Models;

namespace Cloudland.APM.Mapper.Profiles;

public sealed class APMProfiles : BaseProfiles
{
    public APMProfiles()
    {
        // Nullable DateTime conversions
        CreateMap<DateTime?, string?>().ConvertUsing(new DateTimeToPersianDateTimeConverter());
        CreateMap<string?, DateTime?>().ConvertUsing(new PersianDateToDateTimeConverter());
        
        // Non-nullable DateTime conversions
        CreateMap<DateTime, string>().ConvertUsing(new DateTimeToStringConverter());
        CreateMap<string, DateTime>().ConvertUsing(new StringToDateTimeConverter());

         CreateMap<Constant, ConstantDTO>().ReverseMap();
         CreateMap<UserConstantPermission, UserConstantPermissionDTO>().ReverseMap();
         CreateMap<ConstantRecursive, ConstantRecursiveDTO>().ReverseMap();
         CreateMap<ConstantRecursiveWithPermission, ConstantRecursiveWithPermissionDTO>().ReverseMap();
         CreateMap<UserConstantPermission, UserConstantPermissionDTO>().ReverseMap();
         CreateMap<Calendar, CalendarDTO>().ReverseMap();
    }
}