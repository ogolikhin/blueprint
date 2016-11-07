import {IStatefulArtifact, StatefulArtifact} from "../../managers/artifact-manager/artifact";
import {IStatefulSubArtifact, StatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact";
import {IArtifact} from "../../main/models/models";
import {IState} from "../../managers/artifact-manager/state";
import {IDiagram, IDiagramElement} from "./impl/models";
import {IUseCase} from "./impl/usecase/models";
import {ItemTypePredefined} from "./../../main/models/enums";
import {IItem} from "./../../main/models/models";
import {Diagrams, Shapes, ShapeProps} from "./impl/utils/constants";
import {StatefulDiagramArtifact} from "./diagram-artifact";
import {UsecaseToDiagram} from "./impl/usecase/usecase-to-diagram";

export class StatefulUseCaseArtifact extends StatefulDiagramArtifact {

    private diagram: IDiagram;

    public getDiagramModel() {
        return this.diagram;
    }

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = "/svc/bpartifactstore/usecase/" + id;
        return this.services.artifactService.getArtifactModel<IUseCase>(url, id, versionId);
    }

    protected initializeSubArtifacts(artifact: IDiagram| IUseCase) {
        try {
            this.diagram = new UsecaseToDiagram().convert(<IUseCase>artifact);
            this.diagram.isCompatible = true;
            super.initializeSubArtifacts(this.diagram);
        } catch (error) {
            this.diagram = {id: this.artifact.id, isCompatible: false, diagramType: Diagrams.USECASE} as IDiagram;
        }        
    }

}
