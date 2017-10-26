using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ServiceLibrary.Models.Reuse;

namespace ServiceLibrary.Repositories.Reuse
{
    public interface IReuseRepository
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="artifactIds"></param>
        /// <returns>Dictionary with Item Id as Key</returns>
        Task<IDictionary<int, SqlItemTypeInfo>> GetStandardTypeIdsForArtifactsIdsAsync(ISet<int> artifactIds);

        /// <summary>
        ///
        /// </summary>
        /// <param name="propertyIds"></param>
        /// <returns>Dictionary with Property Type Id as Key</returns>
        Task<IDictionary<int, SqlPropertyTypeInfo>> GetStandardPropertyTypeIdsForPropertyIdsAsync(ISet<int> propertyIds);

        /// <summary>
        ///
        /// </summary>
        /// <param name="instanceItemTypeIds"></param>
        /// <param name="transaction"></param>
        /// <returns>Dictionary with Type Id as Key</returns>
        Task<IDictionary<int, ItemTypeReuseTemplate>> GetReuseItemTypeTemplatesAsyc(IEnumerable<int> instanceItemTypeIds, IDbTransaction transaction = null);

        /// <summary>
        ///
        /// </summary>
        /// <param name="revisionId"></param>
        /// <param name="transaction"></param>
        /// <returns>Dictionary with Type Id as Key</returns>
        Task<IEnumerable<SqlModifiedItems>> GetModificationsForRevisionIdAsyc(int revisionId, IDbTransaction transaction = null);

        /// <summary>
        ///
        /// </summary>
        /// <param name="itemIds"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<Dictionary<int, bool>> DoItemsContainReadonlyReuse(IEnumerable<int> itemIds, IDbTransaction transaction = null);
    }
}
