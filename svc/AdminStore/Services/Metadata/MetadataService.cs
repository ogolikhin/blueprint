using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdminStore.Repositories.Metadata;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ItemType;

namespace ServiceLibrary.Repositories.Metadata
{
    public class MetadataService
    {
        // private readonly IMetadataRepository _metadataRepository;
        private readonly ISqlItemTypeRepository _sqlItemTypeRepository;

        private const int ItemTypeIconSize = 32;

        public MetadataService()
            : this(
                new SqlItemTypeRepository())
                // new MetadataRepository())
        {
        }

        public MetadataService(ISqlItemTypeRepository sqlItemTypeRepository)
            // IMetadataRepository metadataRepository)
        {
            _sqlItemTypeRepository = sqlItemTypeRepository;
            // _metadataRepository = metadataRepository;
        }

        private static readonly IDictionary<ItemTypePredefined, string> IconFileNames = new Dictionary<ItemTypePredefined, string>
        {
            // { ItemTypePredefined.Actor, "actor.png" },

            // { ItemTypePredefined.BusinessProcess, "business-process.png" },
            // { ItemTypePredefined.BPConnector, "business-process-sub.png" },
            // { ItemTypePredefined.BPShape, "business-process-sub.png"},

            // { ItemTypePredefined.Document, "document.png"},

            // { ItemTypePredefined.DomainDiagram, "domain-diagram.png"},
            // { ItemTypePredefined.DDConnector, "domain-diagram-sub.png"},
            // { ItemTypePredefined.DDShape, "domain-diagram-sub.png"},

            // { ItemTypePredefined.PrimitiveFolder, "folder.png"},

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

        public async Task<byte[]> GetCustomItemTypeIcon(int itemTypeId)
        {
            int revisionId;
            revisionId = int.MaxValue;
            var itemTypeInfo = await _sqlItemTypeRepository.GetItemTypeInfo(itemTypeId, revisionId);

            if (itemTypeInfo == null)
            {
                throw new ResourceNotFoundException("Artifact type not found.");
            }
            return itemTypeInfo.Icon;
        }

        public void GetItemTypeIcon(int? typeId)
        {
            throw new NotImplementedException();
        }
    }
}
