using com.inversoft.error;

namespace Cloudland.APM.Utils.ErrorTransform;

public class TransformErrorService : ITransformErrorService
{
    //a service to translate fusion errors from English to Persian

    public List<Dictionary<string, string>>? TransformFieldErrors(IDictionary<string, List<Error>>? fieldErrors)
    {
        if (fieldErrors == null) return null;

        var transformedErrors = new List<Dictionary<string, string>>();

        foreach (var fieldError in fieldErrors)
        {
            var fieldName = fieldError.Key.Replace("user.", "");

            foreach (var error in fieldError.Value)
            {
                var translatedMessage = error.message; // Default to original English message

                // Translate specific field errors to Persian
                switch (fieldName)
                {
                    case "email":
                        translatedMessage = TranslateEmailError(error);
                        break;

                    case "username":
                        translatedMessage = TranslateUsernameError(error);
                        break;

                    case "password":
                        translatedMessage = TranslatePasswordError(error);
                        break;
                }

                transformedErrors.Add(new Dictionary<string, string>
                {
                    [fieldName] = translatedMessage
                });
            }
        }

        return transformedErrors;
    }

    public string TranslateEmailError(Error error)
    {
        return error.code switch
        {
            "[duplicate]user.email" =>
                $"ایمیل {ExtractValue(error.message)} از قبل وجود دارد",
            "[invalid]user.email" =>
                "فرمت ایمیل وارد شده صحیح نیست",
            _ => error.message // Default to English
        };
    }

    public string TranslateUsernameError(Error error)
    {
        return error.code switch
        {
            "[duplicate]user.username" =>
                $"نام کاربری {ExtractValue(error.message)} قبلا ثبت شده است",
            "[invalid]user.username" =>
                "نام کاربری باید بین 3 تا 32 کاراکتر باشد",
            _ => error.message // Default to English
        };
    }

    public string TranslatePasswordError(Error error)
    {
        return error.code switch
        {
            "[invalid]user.password" =>
                "رمز عبور باید حداقل 8 کاراکتر و شامل حروف و اعداد باشد",
            "[weak]user.password" =>
                "رمز عبور بسیار ضعیف است",
            _ => error.message // Default to English
        };
    }

    public string ExtractValue(string message)
    {
        // Extracts values between [] from error messages
        var start = message.IndexOf('[') + 1;
        var end = message.IndexOf(']');
        return start > 0 && end > start
            ? message.Substring(start, end - start)
            : message;
    }
}