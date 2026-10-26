using AutoMapper;
using MD.PersianDateTime.Standard;

namespace Cloudland.APM.Mapper.Profiles.CustomConverter;

public class DateTimeToPersianDateTimeNonNullableConverter : ITypeConverter<DateTime, string>
{
    public string Convert(DateTime source, string destination, ResolutionContext context)
    {
        return new PersianDateTime(source).ToString("yyyy/MM/dd HH:mm:ss");
    }
}
