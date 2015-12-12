using System;
using System.Linq;
using System.Reflection;

namespace LicenseLibrary.Models
{
    public enum ProductFeature
    {
        None,
        [InishTech(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, "View")]
        Viewer,
        [InishTech(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, "Collaborate")]
        Collaborator,
        [InishTech(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, "Author")]
        Author,
        [InishTech(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, "HP ALM Adapter")]
        HewlettPackardQcIntegration,
        [InishTech(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, "Microsoft TFS Adapter")]
        MicrosoftTfsIntegration,
        [InishTech(InishTechAttribute.BlueprintProductName, InishTechAttribute.BlueprintProductVersion, "Blueprint API")]
        BlueprintOpenApi,
        [InishTech(InishTechAttribute.DataAnalyticsProductName, InishTechAttribute.DataAnalyticsProductVersion, null)]
        DataAnalytics
    }

    public static class ProductFeatureExtensions
    {
        public static string GetProductName(this ProductFeature feature)
        {
            var attribute = GetInishTechAttribute(feature);
            return attribute == null ? null : attribute.ProductName;
        }

        public static string GetProductVersion(this ProductFeature feature)
        {
            var attribute = GetInishTechAttribute(feature);
            return attribute == null ? null : attribute.ProductVersion;
        }

        public static string GetFeatureName(this ProductFeature feature)
        {
            var attribute = GetInishTechAttribute(feature);
            return attribute == null ? null : attribute.FeatureName;
        }

        private static InishTechAttribute GetInishTechAttribute(ProductFeature feature)
        {
            var member = typeof(ProductFeature).GetMember(feature.ToString()).SingleOrDefault();
            return member == null ? null : (InishTechAttribute)member.GetCustomAttribute(typeof(InishTechAttribute));
        }
    }

    internal class InishTechAttribute : Attribute
    {
        internal const string BlueprintProductName = "Blueprint";
        internal const string BlueprintProductVersion = "5.0";
        internal const string DataAnalyticsProductName = "Blueprint Data Analytics";
        internal const string DataAnalyticsProductVersion = "5.0";

        public string ProductName { get; }
        public string ProductVersion { get; }
        public string FeatureName { get; }

        public InishTechAttribute(string productName, string productValue, string featureName)
        {
            ProductName = productName;
            ProductVersion = productValue;
            FeatureName = featureName;
        }
    }

}