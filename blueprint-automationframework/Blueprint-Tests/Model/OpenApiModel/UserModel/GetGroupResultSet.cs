using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model.OpenApiModel.UserModel
{
    public class GetGroupResultSet
    {
        public List<GetGroupResult> Groups { get; set; }
    }

    // Dev code can be found in:  blueprint-current/Source/BluePrintSys.RC.Api.Business/Models/Group.cs
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
