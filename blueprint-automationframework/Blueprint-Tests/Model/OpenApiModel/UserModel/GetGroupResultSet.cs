using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.OpenApiModel.UserModel
{
    public class GetGroupResultSet
    {
        public List<GetGroupResult> Groups { get; set; }
    }

    public class GetGroupResult
    {
        #region Serialized properties

        public int? ProjectId { get; set; }

        [JsonProperty("Type")]
        public string UserOrGroupType { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }

        #endregion Serialized properties
    }
}
