using System;

namespace Model.StorytellerModel
{
    /// <summary>
    /// The Result Returned from UploadFile() 
    /// </summary>
    public class UploadResult
    {
        public string Guid { get; set; }
        public Uri UriToFile { get; set; }
    }
}