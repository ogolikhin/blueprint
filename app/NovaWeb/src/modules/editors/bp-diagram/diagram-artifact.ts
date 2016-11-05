import {IStatefulArtifact, StatefulArtifact} from "../../managers/artifact-manager/artifact";
import {IStatefulSubArtifact, StatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact";
import {IArtifact} from "../../main/models/models";
import {IState} from "../../managers/artifact-manager/state";
import {IDiagram, IDiagramElement} from "./impl/models";
import {ItemTypePredefined} from "./../../main/models/enums";
import {IItem} from "./../../main/models/models";
import {Diagrams, Shapes, ShapeProps} from "./impl/utils/constants";

export interface IStatefulDiagramArtifact extends IStatefulArtifact {
    getDiagramModel(): IDiagram;
}

export class StatefulDiagramArtifact extends StatefulArtifact implements IStatefulDiagramArtifact {

    public getDiagramModel() {
        return this.artifact as IDiagram;
    }

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IDiagram> {
        const url = "/svc/bpartifactstore/diagram/" + id;
        return this.services.artifactService.getArtifactModel<IDiagram>(url, id, versionId);
    }

    protected initialize(artifact: IDiagram): IState {
        if (artifact.libraryVersion === 0 && artifact.shapes && artifact.shapes.length > 0) {
            artifact.isCompatible = false;
        } else {
            artifact.isCompatible = true;
        }
        this.initializeSubArtifacts(artifact);
        return super.initialize(artifact);
    }

    protected initializeSubArtifacts(artifact: IDiagram) {
        const statefulSubartifacts = [];
        if (artifact.shapes) {
            artifact.shapes.forEach((shape) => {
                this.initPrefixAndType(artifact.diagramType, shape, shape);
                statefulSubartifacts.push(new StatefulSubArtifact(this, shape, this.services));
            });
        }
        if (artifact.connections) {
            artifact.connections.forEach((connection) => {
                this.initPrefixAndType(artifact.diagramType, connection, connection);
                statefulSubartifacts.push(new StatefulSubArtifact(this, connection, this.services));
            });
        }

        this.subArtifactCollection.initialise(statefulSubartifacts);
    }

    private initPrefixAndType(diagramType: string, item: IItem, element: IDiagramElement) {
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
                item.prefix = element.isShape ? "UCDS" : "UCDC";
                item.predefinedType = element.isShape ? ItemTypePredefined.UCDShape : ItemTypePredefined.UCDConnector;
                break;
            default:
                break;
        }
    }
}
