using com.inversoft.error;

namespace Cloudland.APM.Utils.ErrorTransform;

public interface ITransformErrorService
{
    List<Dictionary<string, string>>? TransformFieldErrors(IDictionary<string, List<Error>>? fieldErrors);
    string TranslateEmailError(Error error);
    string TranslateUsernameError(Error error);
    string TranslatePasswordError(Error error);
    string ExtractValue(string message);
}