using ServiceLibrary.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceLibrary.Models
{
    public class EventAction
    {
        public virtual async Task<bool> Execute()
        {
            return await Task.FromResult(true);
        } 
    }

    public interface ISynchronousAction { }
    public interface IASynchronousAction { }

    public class EmailNotificationAction : EventAction, IASynchronousAction
    {
        public IList<string> Emails { get; } = new List<string>();

        public int? PropertyTypeId { get; set; }
        
        public string Message { get; set; }
    }

    public class PropertyChangeAction : EventAction, ISynchronousAction
    {
        public int? PropertyTypeId { get; set; }

        public string PropertyValue { get; set; }
    }

    public class PropertyChangeUserAction : PropertyChangeAction
    {
        // Used for User properties and indicates that PropertyValue contains the group name.
        public bool IsGroup { get; set; }
    }

    public abstract class GenerateAction : EventAction, IASynchronousAction
    {
        public abstract GenerateActionTypes GenerateActionType { get; }
    }

    public class GenerateChildrenAction : GenerateAction
    {
        public override GenerateActionTypes GenerateActionType { get; } = GenerateActionTypes.Children;

        public int? ChildCount { get; set; }

        public int ArtifactTypeId { get; set; }
    }

    public class GenerateUserStoriesAction : GenerateAction
    {
        public override GenerateActionTypes GenerateActionType { get; } = GenerateActionTypes.UserStories;
    }

    public class GenerateTestCasesAction : GenerateAction
    {
        public override GenerateActionTypes GenerateActionType { get; } = GenerateActionTypes.TestCases;
    }
}
