using System;
using Common;
using Model.Impl;

namespace Model.FullTextSearchModel.Impl
{
    public class FullTextSearch : IFullTextSearch
    {
        #region Members inherited from IFullTextSearch

        public FullTextSearchResult Search(User user, FullTextSearchCriteria searchCriteria, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public FullTextSearchMetaDataResult SearchMetaData(User user, FullTextSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        #endregion Members inherited from IFullTextSearch
    }
}
