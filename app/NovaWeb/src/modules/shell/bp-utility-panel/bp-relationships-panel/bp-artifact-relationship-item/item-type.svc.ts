import { ILocalizationService } from "../../../../core";
import {Relationships} from "../../../../main";




export interface IItemTypeService {
    getIconClass(predefined: string): string;
}

export class ItemTypeService implements IItemTypeService {
    public getIconClass(predefined: string): string {
        switch (predefined) {
            case "PRO": //ItemTypePredefined.Project:
                return "fonticon-project";

            case "ST-US(Agile Pack)": //ItemTypePredefined.PrimitiveFolder:
                return "fonticon-folder";

            //case ItemTypePredefined.TextualRequirement:
            //    return "fonticon-bp-textual-req";

            //case ItemTypePredefined.Actor:
            //    return "fonticon-bp-actor";

            //case ItemTypePredefined.Document: // TODO: no icon for Document artifact
            //    return "fonticon-bp-document-req ";

            //case ItemTypePredefined.Glossary:
            //    return "fonticon-bp-glossary";

            //case ItemTypePredefined.Term:
            //    return "demo-icon fonticon-bp-glossary-sub";

            //case ItemTypePredefined.BusinessProcess:
            //    return "fonticon-bp-business-process";
            //case ItemTypePredefined.BPShape:
            //case ItemTypePredefined.BPConnector:
            //    return "demo-icon fonticon-bp-business-process-sub";

            //case ItemTypePredefined.DomainDiagram:
            //    return "fonticon-bp-domain-diagram";
            //case ItemTypePredefined.DDShape:
            //case ItemTypePredefined.DDConnector:
            //    return "demo-icon fonticon-bp-domain-diagram-sub";

            //case ItemTypePredefined.GenericDiagram:
            //    return "fonticon-bp-generic-diagram";
            //case ItemTypePredefined.GDShape:
            //case ItemTypePredefined.GDConnector:
            //    return "demo-icon fonticon-bp-generic-diagram-sub";

            //case ItemTypePredefined.UseCaseDiagram:
            //    return "fonticon-bp-use-case-diagram";
            //case ItemTypePredefined.UCDShape:
            //case ItemTypePredefined.UCDConnector:
            //    return "demo-icon fonticon-bp-use-case-diagram-sub";

            //case ItemTypePredefined.UseCase:
            //    return "fonticon-bp-use-case";

            //case ItemTypePredefined.PreCondition:
            //case ItemTypePredefined.PostCondition:
            //case ItemTypePredefined.Step:
            //    return "demo-icon fonticon-bp-use-case-sub";

            //case ItemTypePredefined.Storyboard:
            //    return "fonticon-bp-storyboard";
            //case ItemTypePredefined.SBShape:
            //case ItemTypePredefined.SBConnector:
            //    return "demo-icon fonticon-bp-storyboard-sub";

            //case ItemTypePredefined.UIMockup:
            //    return "fonticon-bp-ui-mockup";
            //case ItemTypePredefined.UIShape:
            //case ItemTypePredefined.UIConnector:
            //    return "demo-icon fonticon-bp-ui-mockup-sub";

            default:
                return "fonticon-error"; // TODO: Unexpected Type
        }
    }
}

