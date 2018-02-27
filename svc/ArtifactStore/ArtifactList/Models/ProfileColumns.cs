using System;
using System.Collections.Generic;
using System.Linq;
using ArtifactStore.ArtifactList.Helpers;
using ArtifactStore.Collections.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Models
{
    public class ProfileColumns
    {
        private const int DefaultMaxCapacity = 20;

        private static ProfileColumns _default;

        private readonly int _maxCapacity;
        private readonly List<ProfileColumn> _columns;

        public static ProfileColumns Default =>
            _default ?? (_default = new ProfileColumns(
                new List<ProfileColumn>
                {
                    new ProfileColumn("Artifact ID", PropertyTypePredefined.ID, PropertyPrimitiveType.Text),
                    new ProfileColumn("Artifact Type", PropertyTypePredefined.ArtifactType,
                        PropertyPrimitiveType.Choice),
                    new ProfileColumn("Name", PropertyTypePredefined.Name, PropertyPrimitiveType.Text),
                    new ProfileColumn("Description", PropertyTypePredefined.Description, PropertyPrimitiveType.Text)
                }));

        public IEnumerable<ProfileColumn> Items => _columns.ToList();

        private ProfileColumns(int maxCapacity = DefaultMaxCapacity)
        {
            _maxCapacity = maxCapacity;
            _columns = new List<ProfileColumn>(_maxCapacity);
        }

        public ProfileColumns(IEnumerable<ProfileColumn> columns, int maxCapacity = DefaultMaxCapacity)
            : this(maxCapacity)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            foreach (var column in columns)
            {
                Add(column);
            }
        }

        public bool PropertyTypeIdMatches(int propertyTypeId)
        {
            if (propertyTypeId < 1 || _columns.IsEmpty())
            {
                return false;
            }

            return _columns.Any(column => column.PropertyTypeId == propertyTypeId);
        }

        public bool PredefinedMatches(PropertyTypePredefined predefined)
        {
            return !_columns.IsEmpty() && _columns.Any(column => column.Predefined == predefined);
        }

        public IReadOnlyList<ProfileColumn> GetInvalidColumns(IEnumerable<ProfileColumn> propertyTypes)
        {
            return _columns
                .Where(column =>
                    !propertyTypes.Any(propertyType => column.Equals(propertyType))
                    && !propertyTypes.Any(
                        customPropertyType =>
                            customPropertyType.Predefined == PropertyTypePredefined.CustomGroup
                            && customPropertyType.PropertyTypeId == column.PropertyTypeId))
                .ToList();
        }

        /// <summary>
        /// Item 1 returns valid ProfileColumns.
        /// Item 2 returns wether profile columns were changed in comparison to the corresponding custom properties.
        /// </summary>
        /// <param name="propertyTypes">Original properties in database.</param>
        public Tuple<ProfileColumns, bool> ToValidColumns(IReadOnlyList<PropertyTypeInfo> propertyTypes)
        {
            var changedCustomColumns = _columns
                .Where(column =>
                    propertyTypes.Any(propertyType =>
                        propertyType.Predefined == PropertyTypePredefined.CustomGroup
                        && propertyType.Id == column.PropertyTypeId
                        && (propertyType.PrimitiveType != column.PrimitiveType
                            || propertyType.Name != column.PropertyName)))
                .ToList();

            foreach (var changedCustomColumn in changedCustomColumns)
            {
                var propertyType = propertyTypes.FirstOrDefault(x => changedCustomColumn.PropertyTypeId == x.Id);
                var column = _columns.FirstOrDefault(q => q.PropertyTypeId == changedCustomColumn.PropertyTypeId);

                column.Predefined = propertyType.Predefined;
                column.PropertyName = propertyType.Name;
                column.PrimitiveType = propertyType.PrimitiveType;
            }

            return new Tuple<ProfileColumns, bool>(this, changedCustomColumns.Any());
        }

        private void Add(ProfileColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (NameMatches(column.PropertyName))
            {
                throw ArtifactListExceptionHelper.DuplicateColumnException(column.PropertyName);
            }

            if (_columns.Count >= _maxCapacity)
            {
                throw ArtifactListExceptionHelper.ColumnCapacityExceededException(column.PropertyName, _maxCapacity);
            }

            _columns.Add(column);
        }

        private bool NameMatches(string name)
        {
            return !string.IsNullOrEmpty(name) &&
                   _columns.Any(column => column.NameMatches(name) && column.PropertyName.Length == name.Length);
        }
    }
}
