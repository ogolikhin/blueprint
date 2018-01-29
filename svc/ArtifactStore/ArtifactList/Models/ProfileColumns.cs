using System;
using System.Collections.Generic;
using System.Linq;
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
                    new ProfileColumn("Artifact ID", PropertyTypePredefined.ID, PropertyPrimitiveType.Number),
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

        private void Add(ProfileColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            if (NameMatches(column.PropertyName))
            {
                var errorMessage = I18NHelper.FormatInvariant(
                    ErrorMessages.ArtifactList.AddColumnColumnExists, column.PropertyName);
                throw new ArgumentException(errorMessage);
            }

            if (_columns.Count >= _maxCapacity)
            {
                var errorMessage = I18NHelper.FormatInvariant(
                    ErrorMessages.ArtifactList.AddColumnCapacityReached, column.PropertyName, _maxCapacity);
                throw new ApplicationException(errorMessage);
            }

            _columns.Add(column);
        }

        private bool NameMatches(string name)
        {
            return !string.IsNullOrEmpty(name) && _columns.Any(column => column.NameMatches(name));
        }
    }
}
