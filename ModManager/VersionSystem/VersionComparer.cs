using System.Linq;
using System.Text.RegularExpressions;

namespace ModManager.VersionSystem
{
    public static class VersionComparer
    {
        private static readonly Regex digits = new("\\d+");

        public static bool IsVersionHigher(string? version1, string? version2)
        {
            if (string.IsNullOrEmpty(version1)) { return false; }
            if (string.IsNullOrEmpty(version2)) { return true; }

            var version1Parts = digits.Matches(version1).Select(m => m.Value).ToList();
            var version2Parts = digits.Matches(version2).Select(m => m.Value).ToList();

            for (var i = 0; i < version1Parts.Count(); i++)
            {
                if (i == version2Parts.Count() && i < version1Parts.Count())
                {
                    return true;
                }
                if (int.TryParse(version1Parts[i], out var result1) &&
                    int.TryParse(version2Parts[i], out var result2))
                {
                    if (result1 > result2)
                    {
                        return true;
                    }
                    else if (result1 < result2)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        
        public static bool IsSameVersion(string? version1, string? version2)
        {
            if (version1 == null || version2 == null)
                return false;
            
            version1 = version1.Trim();
            version2 = version2.Trim();
            
            return version1 == version2;
        }
    }
}
