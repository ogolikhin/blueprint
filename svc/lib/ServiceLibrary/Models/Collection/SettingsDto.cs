using System.Collections.Generic;

namespace ServiceLibrary.Models.Collection
{
    public class SettingsDto
    {
        public IEnumerable<FilterDto> Filters { get; set; }
        public IEnumerable<ColumnDto> Columns { get; set; }
    }
}