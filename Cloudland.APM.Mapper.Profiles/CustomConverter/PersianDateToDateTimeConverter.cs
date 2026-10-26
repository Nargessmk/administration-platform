using AutoMapper;
using MD.PersianDateTime.Standard;

namespace Cloudland.APM.Mapper.Profiles.CustomConverter;

public class PersianDateToDateTimeConverter : ITypeConverter<string?, DateTime?>
{
    public DateTime? Convert(string? source, DateTime? destination, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source)) return null;
        return PersianDateTime.Parse(source).ToDateTime();
    }
}