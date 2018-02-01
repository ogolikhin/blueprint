using System;
using System.Collections.Generic;
using System.Linq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.Collections.Models
{
    public class ArtifactListColumns
    {
        private const int DefaultMaxCapacity = 20;
        private static ArtifactListColumns _default;
        private readonly int _maxCapacity;
        private readonly List<ArtifactListColumn> _columns;

        public static ArtifactListColumns Default =>
            _default ?? (_default = new ArtifactListColumns(
                new List<ArtifactListColumn>
                {
                    new ArtifactListColumn("Artifact ID", PropertyTypePredefined.ID, PropertyPrimitiveType.Number),
                    new ArtifactListColumn("Artifact Type", PropertyTypePredefined.ArtifactType,
                        PropertyPrimitiveType.Choice),
                    new ArtifactListColumn("Name", PropertyTypePredefined.Name, PropertyPrimitiveType.Text),
                    new ArtifactListColumn("Description", PropertyTypePredefined.Description, PropertyPrimitiveType.Text)
                }));

        public ArtifactListColumns(IEnumerable<ArtifactListColumn> columns, int maxCapacity = DefaultMaxCapacity)
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

        public IEnumerable<ArtifactListColumn> Items => _columns.ToList();

        private ArtifactListColumns(int maxCapacity = DefaultMaxCapacity)
        {
            _maxCapacity = maxCapacity;
            _columns = new List<ArtifactListColumn>(_maxCapacity);
        }

        private void Add(ArtifactListColumn column)
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
