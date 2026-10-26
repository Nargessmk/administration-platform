using AutoMapper;
using MD.PersianDateTime.Standard;

namespace Cloudland.APM.Mapper.Profiles.CustomConverter;

public class PersianDateToDateTimeNonNullableConverter : ITypeConverter<string, DateTime>
{
    public DateTime Convert(string source, DateTime destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            return default; // accept null/empty input without throwing

        return PersianDateTime.Parse(source).ToDateTime();
    }
}
