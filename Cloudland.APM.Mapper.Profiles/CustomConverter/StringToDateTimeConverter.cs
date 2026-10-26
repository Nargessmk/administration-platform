using AutoMapper;

namespace Cloudland.APM.Mapper.Profiles.CustomConverter;

public class StringToDateTimeConverter : ITypeConverter<string, DateTime>
{
    public DateTime Convert(string source, DateTime destination, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source))
            return DateTime.MinValue;
        
        return MD.PersianDateTime.Standard.PersianDateTime.Parse(source).ToDateTime();
    }
}
