using System;
using System.IO;

namespace LicenseLibrary.Helpers
{
    public class LicenseConstants
    {
        public const string PermutationShortId = "eb771";

        // Path Components
        private const string CompanyName = "Blueprint Software Systems";
        private const string LicenseFolderName = "Licenses";

        // Paths
        private static readonly string BasePath = Path.Combine(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)), "ProgramData");
        public static readonly string CompanyFolderFullPath = Path.Combine(BasePath, CompanyName);
        public static readonly string LicenseFolderFullPath = Path.Combine(CompanyFolderFullPath, LicenseFolderName);
    }
}
