import {IStatefulArtifactFactory} from "./";
import {IStatefulArtifact, StatefulArtifact, StatefulProcessSubArtifact} from "../artifact";
import {StatefulSubArtifact, IStatefulSubArtifact} from "../sub-artifact";
import {Models} from "../../../main/models";
import {IProcessShape} from "../../../editors/bp-process/models/process-models";

export class StatefulArtifactFactoryMock implements IStatefulArtifactFactory {

    public createStatefulArtifact(artifact: Models.IArtifact): IStatefulArtifact {
        return new StatefulArtifact(artifact, null);
    }

    public createStatefulSubArtifact(artifact: IStatefulArtifact, subArtifact: Models.ISubArtifact): IStatefulSubArtifact {
        return new StatefulSubArtifact(artifact, subArtifact, null);
    }

    public createStatefulProcessSubArtifact(artifact: IStatefulArtifact, subArtifact: IProcessShape): StatefulProcessSubArtifact {
        return new StatefulProcessSubArtifact(artifact, subArtifact, null);
    }
}
