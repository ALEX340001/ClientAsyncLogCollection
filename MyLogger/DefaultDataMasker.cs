using System.IO;
using System.Text.RegularExpressions;

namespace MyCompany.Logging
{
    public class DefaultDataMasker : IDataMasker
    {
        private static readonly Regex TokenPattern = new Regex(@"\b[0-9A-Fa-f]{8,}\b", RegexOptions.Compiled);
        private static readonly Regex IpPattern = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b", RegexOptions.Compiled);
        private static readonly Regex PasswordPattern = new Regex(@"(password|pwd)\s*[=:]\s*\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PathPattern = new Regex(@"[A-Z]:\\(?:[^\\/:*?""<>|\r\n]+\\)*[^\\/:*?""<>|\r\n]*", RegexOptions.Compiled);

        public string Mask(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string masked = input;
            masked = TokenPattern.Replace(masked, m => m.Value.Length > 8 ? m.Value.Substring(0, 4) + "***" : m.Value);
            masked = IpPattern.Replace(masked, m =>
            {
                var parts = m.Value.Split('.');
                return parts.Length == 4 ? $"{parts[0]}.{parts[1]}.***.***" : m.Value;
            });
            masked = PasswordPattern.Replace(masked, "$1=***");
            masked = PathPattern.Replace(masked, m =>
            {
                try
                {
                    string path = m.Value;
                    string root = Path.GetPathRoot(path);
                    return string.IsNullOrEmpty(root) ? "***" : root + "***";
                }
                catch { return "***"; }
            });
            return masked;
        }
    }
}