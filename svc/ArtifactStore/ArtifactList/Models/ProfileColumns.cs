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

        public IReadOnlyList<ProfileColumn> GetInvalidColumns(IEnumerable<PropertyTypeInfo> propertyTypeInfos)
        {
            return _columns
                .Where(column =>
                    !propertyTypeInfos
                        .Any(info =>
                            info.Name == column.PropertyName &&
                            info.Predefined == column.Predefined &&
                            info.PrimitiveType == column.PrimitiveType &&
                            info.Id == column.PropertyTypeId))
                .ToList();
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
            return !string.IsNullOrEmpty(name) && _columns.Any(column => column.NameMatches(name));
        }
    }
}
