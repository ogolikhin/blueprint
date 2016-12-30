using Model.JobModel.Enums;
using System;

namespace Model
{
    public interface IAlmTarget
    {
        #region properties

        int Id { get; set; }
        string Name { get; set; }
        int BlueprintProjectId { get; set; }
        AlmType AlmType { get; set; }
        Uri Url { get; set; }
        string Domain { get; set; }
        string Project { get; set; }

        #endregion properties
    }
}
