namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileSettingsParams
    {
        private ProfileColumns _columns;
        private bool _columnsUndefined = true;
        private int? _paginationLimit;
        private bool _paginationLimitUndefined = true;

        internal ProfileColumns Columns
        {
            get { return _columns; }
            set
            {
                _columnsUndefined = _columnsUndefined && _columns == value;
                _columns = value;
            }
        }

        internal bool ColumnsUndefined => _columnsUndefined;

        internal int? PaginationLimit
        {
            get { return _paginationLimit; }
            set
            {
                _paginationLimitUndefined = _paginationLimitUndefined && _paginationLimit == value;
                _paginationLimit = value;
            }
        }

        internal bool PaginationLimitUndefined => _paginationLimitUndefined;
    }
}