using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ArtifactStore.Models.Review
{
    [Flags]
    public enum ActivityDiagramFlags
    {
        None = 0,
        UseShortText = 0x1,
        ShowActorNames = 0x2,
        ShowDataOperations = 0x4
    }

    [Flags]
    public enum DisplayFlags
    {
        None,
        CollapseNavigationView = 0x01,
        CollapseUtilityFrame = 0x02,

        DisplayUIMockups = 0x10,
        DisplayComments = 0x20,
        DisplayRelationships = 0x40,
        DisplayFiles = 0x80,
        DisplayData = 0x100,
        DisplayActors = 0x200,
        DisplayProperties = 0x400
    }

    public partial class SimulationSettings
    {
        [Key]
        public int ParentId
        {
            get;
            set;
        }

        [Key]
        public int ReviewPackageId
        {
            get;
            set;
        }

        public bool SelectedUseCaseOnly
        {
            get;
            set;
        }

        public int UseCaseLevel
        {
            get;
            set;
        }

        public bool UseAutomaticNavigation
        {
            get;
            set;
        }

        public int AutomaticNavigationInterval
        {
            get;
            set;
        }

        public ActivityDiagramFlags ActivityDiagram
        {
            get;
            set;
        }

        public DisplayFlags Display
        {
            get;
            set;
        }

        public string MainContentTab
        {
            get;
            set;
        }
    }
}