using System;
using System.Globalization;
using ArtifactStore.Models.Review;

namespace ArtifactStore.Helpers
{
    public static class IMeaningOfSignatureValueExtensions
    {
        public static string GetMeaningOfSignatureDisplayValue(this IMeaningOfSignatureValue meaningOfSignature)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", meaningOfSignature.MeaningOfSignatureValue, meaningOfSignature.RoleName);
        }
    }
}
