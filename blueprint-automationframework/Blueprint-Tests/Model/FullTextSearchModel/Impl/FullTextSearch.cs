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

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(ArtifactStore), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: Delete anything created by this class.
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
