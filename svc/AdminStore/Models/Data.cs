using System.Diagnostics.CodeAnalysis;

namespace AdminStore.Models
{
   
    public class Data
    {
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public UserDto[] Users { get; set; }
    }
}