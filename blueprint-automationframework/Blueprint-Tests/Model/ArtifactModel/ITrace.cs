using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Model.ArtifactModel
{
    public interface ITrace
    {
        int ProjectId { get; set; }

        int ArtifactId { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
        TraceDirection Direction { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
        TraceTypes TraceType { get; set; }

        bool IsSuspect { get; set; }
    }
}
