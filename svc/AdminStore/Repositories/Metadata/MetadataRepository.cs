﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
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

             // not sure
            { ItemTypePredefined.ArtifactCollection, "collection.svg" },

             // not sure
            { ItemTypePredefined.CollectionFolder, "collections.svg" },

            { ItemTypePredefined.Document, "document.svg" },

            { ItemTypePredefined.DomainDiagram, "domain-diagram.svg" },
            { ItemTypePredefined.DDConnector, "subartifact.svg" },
            { ItemTypePredefined.DDShape, "subartifact.svg" },

            { ItemTypePredefined.PrimitiveFolder, "folder.svg" },

             // folder open?
             // { ItemTypePredefined.PrimitiveFolder, "folder-open.svg" },

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

        public Stream GetSvgIcon(ItemTypePredefined predefined, string color = null)
        {
            string iconSvgFileName;
            if (!IconFileNames.TryGetValue(predefined, out iconSvgFileName))
            {
                return null;
            }

            var resourceStream = LoadSvgResourceImage(iconSvgFileName);
            if (string.IsNullOrEmpty(color))
            {
                return resourceStream;
            }
            return AddFillAttribute(resourceStream, color);
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

        private Stream AddFillAttribute(Stream resourceStream, string color)
        {
            string hexColor = string.Format(CultureInfo.CurrentCulture, "#{0}", color);
            using (resourceStream)
            {
                var svgDocument = XDocument.Load(resourceStream);
                var svgElement = svgDocument.Root;

                foreach (var pathElement in svgElement.Descendants("{http://www.w3.org/2000/svg}path"))
                {
                    var fillAttribute = new XAttribute("fill", hexColor);
                    pathElement.Add(fillAttribute);
                }
                var memoryStream = new MemoryStream();
                svgDocument.Save(memoryStream);
                memoryStream.Position = 0;
                return memoryStream;
            }
        }
    }
}