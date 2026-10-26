namespace Cloudland.APM.Utils;

public interface ITransliterationService
{
    bool IsPersian(string text);
    string ToFinglish(string persian, bool spaceToUnderscore = true);
    string ToPersian(string finglish, bool underscoreToSpace = true);
    string NormalizeSqlExpression(string expression);
}