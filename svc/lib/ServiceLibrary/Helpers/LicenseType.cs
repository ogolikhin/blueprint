using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ServiceLibrary.Helpers
{
    public enum LicenseType
    {
        None,
        Viewer,
        Collaborator,
        Author
    }

    /// <summary>
    /// This enum is used to save the ActionType in LicenseActivity Table
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum LicenseActionType
    {
        Login = 1,
        LogOut = 2,
        Timeout = 3
    }

    /// <summary>
    /// This enum is used to save the TransactionType in the LicenseActivity Table
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum LicenseTransactionType
    {
        Acquire = 1,
        Release = 2,
        Deny = 3
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum LicenseConsumerType
    {
        Client = 1,
        Analytics = 2,
        RestApi = 3
    }

    public static class LicenseTypeExt
    {
        public const int UnlimitedLicense = Int32.MaxValue;

        private const string INISHTECH_VIEWER_LICENSE_NAME = "View";
        private const string INISHTECH_COLLABORATOR_LICENSE_NAME = "Collaborate";
        private const string INISHTECH_AUTHOR_LICENSE_NAME = "Author";

        private static readonly IDictionary<LicenseType, LicenseType[]> minLicenseToApplicable = new Dictionary<LicenseType, LicenseType[]>
        {
            { LicenseType.Author, new[] {LicenseType.Author}},
            { LicenseType.Collaborator, new[] {LicenseType.Author, LicenseType.Collaborator}},
            { LicenseType.Viewer, new[] { LicenseType.Author, LicenseType.Collaborator, LicenseType.Viewer }},
            { LicenseType.None, new[] { LicenseType.None }},
        };

        public static IDictionary<LicenseType, string> BlueprintLicenseToInishTechLicense = new Dictionary<LicenseType, string>
        {
            { LicenseType.Author, INISHTECH_AUTHOR_LICENSE_NAME},
            { LicenseType.Collaborator, INISHTECH_COLLABORATOR_LICENSE_NAME},
            { LicenseType.Viewer, INISHTECH_VIEWER_LICENSE_NAME},
            { LicenseType.None, LicenseType.None.ToLicenseName()},
        };

        private static readonly IDictionary<LicenseType, List<LicenseType>> limitingLicenseToApplicable = new Dictionary<LicenseType, List<LicenseType>>
        {
            { LicenseType.Author, new List<LicenseType> {LicenseType.Author, LicenseType.Collaborator, LicenseType.Viewer}},
            { LicenseType.Collaborator, new List<LicenseType> {LicenseType.Collaborator, LicenseType.Viewer}},
            { LicenseType.Viewer, new List<LicenseType> { LicenseType.Viewer }},
            { LicenseType.None, new List<LicenseType> { LicenseType.None }},
        };

        public static LicenseType[] GetMinApplicableLicenses(LicenseType minLicenseType)
        {
            return minLicenseToApplicable[minLicenseType];
        }

        public static IList<LicenseType> ValidLicenses
        {
            get
            {
                return (from LicenseType l in Enum.GetValues(typeof(LicenseType))
                    where l != LicenseType.None
                    select l).ToList();
            }
        }

        public static List<LicenseType> GetLimitingApplicableLicenses(LicenseType limitingLicenseType)
        {
            return limitingLicenseToApplicable[limitingLicenseType];
        }

        public static LicenseType ToLicenseType(this int? licenseTypeId)
        {
            var licenseType = LicenseType.None;

            if (licenseTypeId.HasValue && Enum.IsDefined(typeof(LicenseType), licenseTypeId.Value))
            {
                licenseType = (LicenseType)Enum.ToObject(typeof(LicenseType), licenseTypeId.Value);
            }

            return licenseType;
        }

        public static LicenseType ToLicenseType(this int licenseTypeId)
        {
            Debug.Assert(Enum.IsDefined(typeof(LicenseType), licenseTypeId));

            return (LicenseType)Enum.ToObject(typeof(LicenseType), licenseTypeId);
        }

        public static string ToLicenseName(this int? licenseTypeId)
        {
            LicenseType licenseType = licenseTypeId.ToLicenseType();

            return Enum.GetName(typeof(LicenseType), licenseType);
        }
        public static string ToLicenseName(this LicenseType licenseType)
        {
            return Enum.GetName(typeof(LicenseType), licenseType);
        }

        public static IEnumerable<LicenseType> GetLicenseTypeListToDisplay()
        {
            return Enum.GetValues(typeof(LicenseType))
                .Cast<LicenseType>()
                .Where(licenseType => (licenseType != LicenseType.None) && (licenseType != LicenseType.Viewer))
                .ToList();
        }

        public static LicenseType ToLicenseType(this string license)
        {
            var temp = license.Trim();
            Debug.Assert(Enum.IsDefined(typeof(LicenseType), temp));

            return (LicenseType)Enum.Parse(typeof(LicenseType), temp, true);
        }

        public static LicenseActionType ToLicenseActionType(this int actionTypeId)
        {
            Debug.Assert(Enum.IsDefined(typeof(LicenseActionType), actionTypeId));

            return (LicenseActionType)Enum.ToObject(typeof(LicenseActionType), actionTypeId);
        }

        public static LicenseTransactionType ToLicenseTransactionType(this int transTypeId)
        {
            Debug.Assert(Enum.IsDefined(typeof(LicenseTransactionType), transTypeId));

            return (LicenseTransactionType)Enum.ToObject(typeof(LicenseTransactionType), transTypeId);
        }
    }
}
