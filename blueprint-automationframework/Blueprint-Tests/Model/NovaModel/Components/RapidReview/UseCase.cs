using System.Collections.Generic;
using Model.ArtifactModel.Enums;

// This file taken from:  blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/RapidReview/Models/UseCase.cs
namespace Model.NovaModel.Components.RapidReview
{
    public class UseCase
    {
        public int Id { get; set; }

        public Step PreCondition { get; set; }

        private List<Step> _steps;
        public List<Step> Steps
        {
            get { return _steps ?? (_steps = new List<Step>()); }
        }

        public Step PostCondition { get; set; }
    }

    public class Step : UseCaseElement
    {
        public string Description { get; set; }

        public StepOfType StepOf { get; set; }

        private List<Flow> _flows;
        public List<Flow> Flows
        {
            get { return _flows ?? (_flows = new List<Flow>()); }
        }

        public ItemIndicatorFlags? IndicatorFlags { get; set; }
    }

    public class Flow : UseCaseElement
    {
        public bool IsExternal { get; set; }

        private List<Step> _steps;
        public List<Step> Steps
        {
            get { return _steps ?? (_steps = new List<Step>()); }
        }

        public string ReturnToStepName { get; set; }
    }

    public class UseCaseElement
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double? OrderIndex { get; set; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum StepOfType : byte
    {
        System = 0,
        Actor = 1
    }
}
