using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using ServiceLibrary.Models;
using System.Threading.Tasks;

namespace AdminStore.Repositories.Metadata
{
    public class MetadataRepository : IMetadataRepository
    {

        private static readonly IDictionary<ItemTypePredefined, string> IconFileNames = new Dictionary<ItemTypePredefined, string>
        {
             { ItemTypePredefined.Actor, "actor.svg" },

             { ItemTypePredefined.Baseline, "baseline.svg" },

             { ItemTypePredefined.BusinessProcess, "business-process.svg" },

             // not sure
             { ItemTypePredefined.ArtifactCollection, "collection.svg" },

             // not sure
             { ItemTypePredefined.CollectionFolder, "collections.svg" },

             { ItemTypePredefined.Document, "document.svg" },

             { ItemTypePredefined.DomainDiagram, "domain-diagram.svg" },

             { ItemTypePredefined.PrimitiveFolder, "folder.svg" },

             // folder open?
             // { ItemTypePredefined.PrimitiveFolder, "folder-open.svg" },

             // { ItemTypePredefined.GenericDiagram, "generic-diagram.png"},
             // { ItemTypePredefined.GDConnector, "generic-diagram-sub.png"},
             // { ItemTypePredefined.GDShape, "generic-diagram-sub.png"},

             // { ItemTypePredefined.Glossary, "glossary.png"},
             // { ItemTypePredefined.Term, "glossary-sub.png"},

             // { ItemTypePredefined.Process, "process.png"},

             // { ItemTypePredefined.Storyboard, "storyboard.png"},
             // { ItemTypePredefined.SBConnector, "storyboard-sub.png" },
             // { ItemTypePredefined.SBShape, "storyboard-sub.png"},

             // { ItemTypePredefined.TextualRequirement, "textual-requirement.png"},

             // { ItemTypePredefined.UIMockup, "ui-mockup.png"},
             // { ItemTypePredefined.UIConnector, "ui-mockup-sub.png"},
             // { ItemTypePredefined.UIShape, "ui-mockup-sub.png"},

             // { ItemTypePredefined.UseCaseDiagram, "use-case-diagram.png"},
             // { ItemTypePredefined.UCDConnector, "use-case-diagram-sub.png"},
             // { ItemTypePredefined.UCDShape, "use-case-diagram-sub.png"},

             // { ItemTypePredefined.UseCase, "use-case.png"},
             // { ItemTypePredefined.PreCondition, "use-case-sub.png"},
             // { ItemTypePredefined.PostCondition, "use-case-sub.png"},
             // { ItemTypePredefined.Step, "use-case-sub.png"},

             // { ItemTypePredefined.Project, "project.png"},
             // { ItemTypePredefined.PROShape, "storyteller-sub.png"}
        };

        public List<XElement> getSvgXaml(ItemTypePredefined predefined)
        {
            string iconSvgFileName;
            if (!IconFileNames.TryGetValue(predefined, out iconSvgFileName))
            {
                return null;
            }

            var svgIcon = LoadSvgResourceImage(iconSvgFileName);
            return svgIcon;
        }

        private List<XElement> LoadSvgResourceImage(string iconSvgFileName)
        {
            if (string.IsNullOrWhiteSpace(iconSvgFileName))
            {
                throw new ArgumentNullException(nameof(iconSvgFileName));
            }

            var assembly = typeof(MetadataRepository).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Assets.Icons.ItemTypes.{iconSvgFileName}";

            XDocument document = XDocument.Load(@resourceName);
            XElement svgElement = document.Root;

            var data = (from path in svgElement.Descendants("{http://www.w3.org/2000/svg}path")
                select path).ToList();

            return data;
        }
    }
}