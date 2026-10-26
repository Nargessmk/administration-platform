using System.Text;
using System.Text.RegularExpressions;

namespace Cloudland.APM.Utils;

public sealed class TransliterationService : ITransliterationService
{
    #region Persian → Finglish map

    private static readonly Dictionary<char, string> PersianToFinglish = new()
    {
        ['ا'] = "a", ['ب'] = "b", ['پ'] = "p", ['ت'] = "t", ['ث'] = "s", ['ج'] = "j",
        ['چ'] = "ch", ['ح'] = "h", ['خ'] = "kh", ['د'] = "d", ['ذ'] = "z", ['ر'] = "r",
        ['ز'] = "z", ['ژ'] = "zh", ['س'] = "s", ['ش'] = "sh", ['ص'] = "s", ['ض'] = "z",
        ['ط'] = "t", ['ظ'] = "z", ['ع'] = "a", ['غ'] = "gh", ['ف'] = "f", ['ق'] = "gh",
        ['ک'] = "k", ['گ'] = "g", ['ل'] = "l", ['م'] = "m", ['ن'] = "n", ['و'] = "v",
        ['ه'] = "h", ['ی'] = "y", ['ء'] = "a", ['أ'] = "a", ['إ'] = "e", ['آ'] = "a",
        ['ؤ'] = "v", ['ئ'] = "y", ['ي'] = "y", ['ة'] = "h",
        ['۰'] = "0", ['۱'] = "1", ['۲'] = "2", ['۳'] = "3", ['۴'] = "4",
        ['۵'] = "5", ['۶'] = "6", ['۷'] = "7", ['۸'] = "8", ['۹'] = "9"
    };

    #endregion

    // Persian Unicode block checker
    private static readonly Regex PersianRegex = new(@"[\u0600-\u06FF]", RegexOptions.Compiled);

    // Token splitter for SQL expressions
    private static readonly Regex TokenSplitter = new(@"(\s+|>=|<=|<>|=|<|>|\(|\)|\+|-|\*|/|,|\bAND\b|\bOR\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    #region Finglish → Persian map (طول بلندتر اول)

    private static readonly (string Finglish, string Persian)[] FinglishToPersianRaw =
    [
        ("kh", "خ"), ("sh", "ش"), ("ch", "چ"), ("gh", "غ"), ("zh", "ژ"),
        ("aa", "ا"), ("oo", "و"), ("ee", "ی"),
        ("a", "ا"), ("b", "ب"), ("p", "پ"), ("t", "ت"), ("s", "س"),
        ("j", "ج"), ("h", "ح"), ("d", "د"), ("z", "ز"), ("r", "ر"),
        ("f", "ف"), ("k", "ک"), ("g", "گ"), ("l", "ل"), ("m", "م"),
        ("n", "ن"), ("v", "و"), ("o", "و"), ("u", "و"), ("y", "ی"),
        ("0", "۰"), ("1", "۱"), ("2", "۲"), ("3", "۳"), ("4", "۴"),
        ("5", "۵"), ("6", "۶"), ("7", "۷"), ("8", "۸"), ("9", "۹")
    ];

    // مرتب‌سازی یک‌بار برای همیشه → سریع‌ترین جستجوی تطابق طولانی
    private static readonly (string Finglish, string Persian)[] F2P =
        FinglishToPersianRaw.OrderByDescending(p => p.Finglish.Length).ToArray();

    #endregion

    #region Public  API

    public bool IsPersian(string text)
    {
        return !string.IsNullOrWhiteSpace(text) && PersianRegex.IsMatch(text);
    }

    public string ToFinglish(string persian, bool spaceToUnderscore = true)
    {
        if (string.IsNullOrWhiteSpace(persian)) return string.Empty;
        if (!IsPersian(persian)) return persian; // متن لاتین را تغییر نده

        var sb = new StringBuilder(persian.Length * 2);
        foreach (var ch in persian)
        {
            if (ch == ' ')
            {
                sb.Append(spaceToUnderscore ? '_' : ' ');
                continue;
            }

            sb.Append(PersianToFinglish.TryGetValue(ch, out var map) ? map : ch);
        }

        return sb.ToString();
    }

    public string ToPersian(string finglish, bool underscoreToSpace = true)
    {
        if (string.IsNullOrWhiteSpace(finglish)) return string.Empty;
        if (IsPersian(finglish)) return finglish; // نیازی به تبدیل نیست

        var sb = new StringBuilder(finglish.Length);
        for (var i = 0; i < finglish.Length;)
        {
            // آندرلاین → فاصله
            if (underscoreToSpace && finglish[i] == '_')
            {
                sb.Append(' ');
                i++;
                continue;
            }

            var matched = false;
            foreach (var (key, val) in F2P)
            {
                if (i + key.Length > finglish.Length ||
                    !finglish.AsSpan(i, key.Length).Equals(key.AsSpan(), StringComparison.OrdinalIgnoreCase))
                    continue;
                sb.Append(val);
                i += key.Length;
                matched = true;
                break;
            }

            if (matched) continue;
            sb.Append(finglish[i]);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    ///     فقط شناسه‌های کاملاً فارسی داخل عبارت SQL را به فینگیلیش برمی‌گرداند؛
    ///     عملگرها، اعداد و شناسه‌های لاتین دست‌نخورده می‌مانند.
    ///     برای جلوگیری از خطای «non‑boolean type near ...».
    /// </summary>
    public string NormalizeSqlExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression)) return string.Empty;

        var tokens = TokenSplitter.Split(expression);
        var sb = new StringBuilder(expression.Length * 2);

        foreach (var token in tokens)
        {
            if (string.IsNullOrEmpty(token)) continue;

            // اگر توکن دارای حداقل یک نویسه فارسی و فاقد نویسه لاتین است → تبدیل شود
            if (PersianRegex.IsMatch(token) && !token.Any(c => c is >= 'A' and <= 'z'))
                sb.Append(ToFinglish(token, false));
            else
                sb.Append(token);
        }

        return sb.ToString();
    }

    #endregion
}