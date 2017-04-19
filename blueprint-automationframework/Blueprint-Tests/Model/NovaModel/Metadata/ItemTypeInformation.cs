using Newtonsoft.Json;

namespace Model.NovaModel.Metadata
{
    // Taken from blueprint-current/Source/BluePrintSys.RC.Business.Internal/Models/Metadata/ItemTypeInformation.cs
    public class ItemTypeInformation
    {
        /// <summary>
        /// Item Type Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Item Type Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Item type prefix
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// item type predefined
        /// </summary>
        public int Predefined { get; set; }

        /// <summary>
        /// Link to location of icon
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
        public string IconUrl { get; set; }

        /// <summary>
        /// Is the type standard
        /// </summary>
        public bool IsStandard { get; set; }

        /// <summary>
        /// Id for the Standard type Id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? StandardTypeId { get; set; }

        /// <summary>
        /// Project id to which the item type belongs
        /// </summary>
        public int? ProjectId { get; set; }

        /// <summary>
        /// Project name to which the item type belongs
        /// </summary>
        public string ProjectName { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
