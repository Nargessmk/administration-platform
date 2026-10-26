using AutoMapper;

namespace Cloudland.APM.Mapper.Profiles;

public abstract class BaseProfiles : Profile
{
    protected BaseProfiles()
    {
        AllowNullCollections = true;
        AllowNullDestinationValues = true;
        CreateMap<string, string>().ConvertUsing(s => (string.IsNullOrWhiteSpace(s) ? null : s) ?? string.Empty);
        CreateMap<byte[], byte[]>().ConvertUsing(b => (b != null && b.Length == 0 ? null : b) ?? Array.Empty<byte>());
        CreateMap<bool?, bool?>().ConvertUsing(b => b ?? true);

        CreateMap<string, byte[]>().ConvertUsing(s => Convert.FromBase64String(s));
        CreateMap<byte[], string>().ConvertUsing(bytes => Convert.ToBase64String(bytes));
    }
}