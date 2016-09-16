import { IDiagramElement, IShape } from "./../models";
import { Models } from "../../../../main";
import { Diagrams, ShapeProps, Shapes } from "./constants";
import { ShapeExtensions } from "./helpers";
import { ItemTypePredefined } from "./../../../../main/models/enums";
import { IItem } from "./../../../../main/models/models";
import { ISelection, SelectionSource } from "./../../../../main/services/selection-manager";

export class SelectionHelper {

    public getEffectiveSelection(artifact: Models.IArtifact, elements: IDiagramElement[], diagramType: string): ISelection {
        const effectiveSelection: ISelection = {
            artifact: artifact,
            source: SelectionSource.Editor
        };
        if (elements && elements.length > 0) {
            const element = elements[0];
            const item: IItem = { id: element.id, name: element.name };
            effectiveSelection.subArtifact = item;
        
            switch (diagramType) {
                case Diagrams.BUSINESS_PROCESS:
                    item.prefix = element.isShape ? "BPSH" : "BPCT";
                    item.predefinedType = element.isShape ? ItemTypePredefined.BPShape : ItemTypePredefined.BPConnector;
                    break;
                case Diagrams.DOMAIN_DIAGRAM:
                    item.prefix = element.isShape ? "DDSH" : "DDCT";
                    item.predefinedType = element.isShape ? ItemTypePredefined.DDShape : ItemTypePredefined.DDConnector;
                    break;
                case Diagrams.GENERIC_DIAGRAM:
                    item.prefix = element.isShape ? "GDST" : "GDCT";
                    item.predefinedType = element.isShape ? ItemTypePredefined.GDShape : ItemTypePredefined.GDConnector;
                    break;
                case Diagrams.STORYBOARD:
                    item.prefix = element.isShape ? "SBSH" : "SBCT";
                    item.predefinedType = element.isShape ? ItemTypePredefined.SBShape : ItemTypePredefined.SBConnector;
                    break;
                case Diagrams.UIMOCKUP:
                    item.prefix = element.isShape ? "UISH" : "UICT";
                    item.predefinedType = element.isShape ? ItemTypePredefined.UIShape : ItemTypePredefined.UIConnector;
                    break;
                case Diagrams.USECASE:
                    item.prefix = "ST";
                    item.predefinedType = ItemTypePredefined.Step;
                    break;
                case Diagrams.USECASE_DIAGRAM:
                    if (element.type === Shapes.USECASE || element.type === Shapes.ACTOR) {
                        const artifactId = ShapeExtensions.getPropertyByName(element, ShapeProps.ARTIFACT_ID);
                        const versionId = ShapeExtensions.getPropertyByName(element, ShapeProps.VERSION_ID) || 0;
                        effectiveSelection.subArtifact = undefined;
                        if (artifactId != null) {
                            effectiveSelection.artifact = {
                                id: artifactId,
                                predefinedType: element.type === Shapes.USECASE ? ItemTypePredefined.UseCase : ItemTypePredefined.Actor,
                                prefix: this.getArtifactPrefix((<IShape>element).label, artifactId),
                                name: this.getArtifactName((<IShape>element).label, artifactId),
                                version: versionId
                            };
                        } else {
                            effectiveSelection.artifact = null;
                        }
                    } else {
                        item.prefix = element.isShape ? "UCDS" : "UCDC";
                        item.predefinedType = element.isShape ? ItemTypePredefined.UCDShape : ItemTypePredefined.UCDConnector;
                    }
                    break;    
                default:
                    break;
            }
        }
        return effectiveSelection;
    }

    private getArtifactName(label: string, id: number) {
        if (label) {
            const delimeter = ": ";
            const index = label.indexOf(delimeter);
            if (index > 0) {
                return label.substring(index + delimeter.length, label.length);
            }
        }
        return "";
    }

    private getArtifactPrefix(label: string, id: number) {
        if (label) {
            const index = label.indexOf(String(id));
            if (index > 0) {
                return label.substring(0, index);
            }
        }
        return "";
    }
}