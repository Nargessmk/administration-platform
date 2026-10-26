using System.Net.Mail;

namespace Cloudland.APM.DTO.NodeObjects;

public abstract class ValidatableNodeObjectDTO<T> : BaseDTO<T>
{
    public abstract List<ValidationRule> ValidationRules { get; set; }

    public virtual bool IsValid(out List<string> validationErrors)
    {
        return Validate(null, out validationErrors);
    }

    public virtual bool IsValid(ValidatableNodeObjectDTO<T> originalDto, out List<string> validationErrors)
    {
        return Validate(originalDto, out validationErrors);
    }

    private bool Validate(ValidatableNodeObjectDTO<T>? original, out List<string> validationErrors)
    {
        validationErrors = [];

        foreach (var rule in ValidationRules)
        {
            var property = GetType().GetProperty(rule.Name);
            if (property == null) continue;

            var currentValue = property.GetValue(this);

            if (original != null && (rule.IsReadonly || rule.Disabled))
            {
                var originalValue = property.GetValue(original);
                if (!Equals(currentValue, originalValue))
                {
                    validationErrors.Add($"فیلد '{rule.Title}' قابل ویرایش نیست.");
                    continue;
                }
            }

            if (rule.IsRequired)
            {
                var isMissing = property.PropertyType == typeof(string)
                    ? string.IsNullOrWhiteSpace(currentValue as string)
                    : property.PropertyType == typeof(long) || property.PropertyType == typeof(int)
                        ? Convert.ToInt64(currentValue) == 0
                        : currentValue == null;

                if (isMissing)
                {
                    validationErrors.Add($"فیلد '{rule.Title}' الزامی است.");
                    continue;
                }
            }

            if (currentValue == null || string.IsNullOrWhiteSpace(currentValue.ToString())) continue;

            switch (rule.InputType.ToLowerInvariant())
            {
                case "number":
                    if (!long.TryParse(currentValue.ToString(), out _))
                        validationErrors.Add($"مقدار وارد شده برای '{rule.Title}' باید یک عدد معتبر باشد.");
                    break;

                case "email":
                    if (currentValue is string emailString)
                        try
                        {
                            _ = new MailAddress(emailString);
                        }
                        catch (FormatException)
                        {
                            validationErrors.Add($"فرمت ایمیل برای فیلد '{rule.Title}' نامعتبر است.");
                        }

                    break;
            }
        }

        return validationErrors.Count == 0;
    }
}