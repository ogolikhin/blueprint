import { IStatefulArtifactFactory } from "./";
import { IStatefulArtifact, StatefulArtifact, StatefulProcessSubArtifact } from "../artifact";
import { StatefulSubArtifact, IStatefulSubArtifact } from "../sub-artifact";
import { Models } from "../../../main/models";
import { IProcessShape, IProcess } from "../../../editors/bp-process/models/process-models";
import { StatefulProcessArtifact } from "../../../editors/bp-process/process-artifact";

export interface IStatefulArtifactFactoryMock extends IStatefulArtifactFactory {
    populateStatefulProcessWithPorcessModel(statefulArtifact: StatefulProcessArtifact, process: IProcess);
}
export class StatefulArtifactFactoryMock implements IStatefulArtifactFactoryMock {

    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        if (artifact.predefinedType === Models.ItemTypePredefined.Process) {
            return new StatefulProcessArtifact(artifact, null);
        }
        return new StatefulArtifact(artifact, null);
    }

    public createStatefulArtifactFromId(artifactId: number): ng.IPromise<IStatefulArtifact> {
        return null;
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, null);
    }

    public createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact {
        return new StatefulProcessSubArtifact(artifact, subArtifact, null);
    }
    public populateStatefulProcessWithPorcessModel(statefulArtifact: StatefulProcessArtifact, process: IProcess) {

        statefulArtifact.links = process.links;
        statefulArtifact.decisionBranchDestinationLinks = process.decisionBranchDestinationLinks;
        statefulArtifact.propertyValues = process.propertyValues;
        statefulArtifact.requestedVersionInfo = process.requestedVersionInfo;
        statefulArtifact.status = process.status;

        const statefulSubArtifacts = [];
        statefulArtifact.shapes = [];
        for (const shape of process.shapes) {
            const statefulShape = new StatefulProcessSubArtifact(statefulArtifact, shape, null);
            statefulArtifact.shapes.push(statefulShape);
            statefulSubArtifacts.push(statefulShape);
        }
        statefulArtifact.subArtifactCollection.initialise(statefulSubArtifacts);
    }
}
