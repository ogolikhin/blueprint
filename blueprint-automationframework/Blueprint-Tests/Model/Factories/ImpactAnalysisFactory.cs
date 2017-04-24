using CustomAttributes;
using Model.Impl;

namespace Model.Factories
{
    public static class ImpactAnalysisFactory
    {
        /// <summary>
        /// Creates a new ImpactAnalysis.
        /// </summary>
        /// <param name="address">The URI address of the Impact Analysis service.</param>
        /// <returns>An ImpactAnalysis object.</returns>
        public static ImpactAnalysis CreateImpactAnalysis(string address)
        {
            return new ImpactAnalysis(address);
        }

        /// <summary>
        /// Creates an ImpactAnalysis object with the settings defined in the TestConfiguration.
        /// </summary>
        /// <returns>An ImpactAnalysis object.</returns>
        /// <exception cref="DataException">If there was an error reading required information from the TestConfiguration.</exception>
        public static ImpactAnalysis GetImpactAnalysisFromTestConfig()
        {
            string address = FactoryCommon.GetServiceAddressFromTestConfig(Categories.ImpactAnalysis);
            return CreateImpactAnalysis(address);
        }
    }
}
