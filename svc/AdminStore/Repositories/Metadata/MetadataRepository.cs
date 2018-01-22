using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;

namespace AdminStore.Repositories.Metadata
{
    public class MetadataRepository : IMetadataRepository
    {

        private static readonly IDictionary<ItemTypePredefined, string> IconFileNames = new Dictionary<ItemTypePredefined, string>
        {
            { ItemTypePredefined.Actor, "actor.svg" },

            { ItemTypePredefined.Baseline, "baseline.svg" },

            { ItemTypePredefined.BusinessProcess, "business-process.svg" },
            { ItemTypePredefined.BPConnector, "subartifact.svg" },
            { ItemTypePredefined.BPShape, "subartifact.svg" },

            { ItemTypePredefined.ArtifactCollection, "collection.svg" },

            { ItemTypePredefined.CollectionFolder, "collections.svg" },

            { ItemTypePredefined.Document, "document.svg" },

            { ItemTypePredefined.DomainDiagram, "domain-diagram.svg" },
            { ItemTypePredefined.DDConnector, "subartifact.svg" },
            { ItemTypePredefined.DDShape, "subartifact.svg" },

            { ItemTypePredefined.PrimitiveFolder, "folder.svg" },

            { ItemTypePredefined.GenericDiagram, "generic-diagram.svg" },
            { ItemTypePredefined.GDConnector, "subartifact.svg" },
            { ItemTypePredefined.GDShape, "subartifact.svg" },

            { ItemTypePredefined.Glossary, "glossary.svg" },
            { ItemTypePredefined.Term, "subartifact.svg" },

            { ItemTypePredefined.Project, "project.svg" },
            { ItemTypePredefined.PROShape, "subartifact.svg" },

            { ItemTypePredefined.ArtifactReviewPackage, "review.svg" },

            { ItemTypePredefined.Storyboard, "storyboard.svg" },
            { ItemTypePredefined.SBConnector, "subartifact.svg" },
            { ItemTypePredefined.SBShape, "subartifact.svg" },

            // storyteller.svg  ??
            { ItemTypePredefined.Process, "storyteller.svg" },

            { ItemTypePredefined.TextualRequirement, "textual.svg" },

            { ItemTypePredefined.UIMockup, "ui-mockup.svg" },
            { ItemTypePredefined.UIConnector, "subartifact.svg" },
            { ItemTypePredefined.UIShape, "subartifact.svg" },

            { ItemTypePredefined.UseCaseDiagram, "use-case-diagram.svg" },
            { ItemTypePredefined.UCDConnector, "subartifact.svg" },
            { ItemTypePredefined.UCDShape, "subartifact.svg" },

            { ItemTypePredefined.UseCase, "use-case.svg" },
            { ItemTypePredefined.PreCondition, "subartifact.svg" },
            { ItemTypePredefined.PostCondition, "subartifact.svg" },
            { ItemTypePredefined.Step, "subartifact.svg" },

            // user story

        };

        public byte[] GetSvgIconContent(ItemTypePredefined predefined, string color = null)
        {
            string iconSvgFileName;
            if (!IconFileNames.TryGetValue(predefined, out iconSvgFileName))
            {
                return null;
            }

            var resourceStream = LoadSvgResourceImage(iconSvgFileName);
            using (resourceStream)
            {
                var svgDocument = XDocument.Load(resourceStream);
                if (string.IsNullOrEmpty(color))
                {
                    return Encoding.UTF8.GetBytes(svgDocument.ToString());
                }
                return Encoding.UTF8.GetBytes(AddFillAttribute(svgDocument, color).ToString());
            }
        }

        private Stream LoadSvgResourceImage(string iconSvgFileName)
        {
            if (string.IsNullOrWhiteSpace(iconSvgFileName))
            {
                throw new ArgumentNullException(nameof(iconSvgFileName));
            }

            var assembly = typeof(MetadataRepository).Assembly;
            var resourceName = string.Format(CultureInfo.CurrentCulture, "{0}.Assets.Icons.ItemTypes.{1}",
                assembly.GetName().Name, iconSvgFileName);
            return assembly.GetManifestResourceStream(resourceName);
        }

        private XDocument AddFillAttribute(XDocument svgDocument, string color)
        {
            string hexColor = string.Format(CultureInfo.CurrentCulture, "#{0}", color);
            Regex hexColorRegex = new Regex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.IgnoreCase);
            if (!hexColorRegex.IsMatch(hexColor))
            {
                return svgDocument;
            }
            var svgElement = svgDocument.Root;

            foreach (var pathElement in svgElement.Descendants("{http://www.w3.org/2000/svg}path"))
            {
                var fillAttribute = new XAttribute("fill", hexColor);
                pathElement.Add(fillAttribute);
            }

            return svgDocument;
        }
    }
}