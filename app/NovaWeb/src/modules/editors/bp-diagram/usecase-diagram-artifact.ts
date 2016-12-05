import {IStatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {StatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact/sub-artifact";
import {IStatefulArtifactServices} from "../../managers/artifact-manager/services";
import {ISubArtifact} from "../../main/models/models";
import {IDiagramElement, IShape} from "./impl/models";
import {Diagrams, Shapes, ShapeProps} from "./impl/utils/constants";
import {StatefulDiagramArtifact} from "./diagram-artifact";

export interface IArtifactProxySubArtifact {
    referencedArtifactId: number;
}

export class StatefulArtifactProxySubArtifact extends StatefulSubArtifact implements IArtifactProxySubArtifact {

    constructor(public referencedArtifactId: number,
                parentArtifact: IStatefulArtifact,
                subArtifact: ISubArtifact,
                services: IStatefulArtifactServices) {
        super(parentArtifact, subArtifact, services);
    }
}

export class StatefulUseCaseDiagramArtifact extends StatefulDiagramArtifact {

    protected createSubArtifact(subArtifactModel: ISubArtifact) {
        const elementType = (<IDiagramElement>subArtifactModel).type;
        if (elementType === Shapes.USECASE || elementType === Shapes.ACTOR) {

            const artifactId = parseInt(this.getPropertyValue(<IShape>subArtifactModel, ShapeProps.ARTIFACT_ID), 10);
            return new StatefulArtifactProxySubArtifact(artifactId, this, subArtifactModel, this.services);
        }
        return new StatefulSubArtifact(this, subArtifactModel, this.services);
    }

    private getPropertyValue(shape: IShape, propertyName: string) {
        const property = _.find(shape.props, property => {
            return property.name === propertyName;
        });
        return property ? property.value : undefined;
    }
}
