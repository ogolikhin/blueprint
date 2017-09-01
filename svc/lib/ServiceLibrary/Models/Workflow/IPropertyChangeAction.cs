using System.Threading.Tasks;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow
{
    public interface IPropertyChangeAction
    {
        int InstancePropertyTypeId { get; set; }
        string PropertyValue { get; set; }
        PropertyLite PropertyLiteValue { get; }
        MessageActionType ActionType { get; }
        bool ValidateAction(IExecutionParameters executionParameters);
    }
}
