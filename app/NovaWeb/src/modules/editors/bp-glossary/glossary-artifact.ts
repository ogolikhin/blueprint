import {IStatefulArtifact, StatefulArtifact} from "../../managers/artifact-manager/artifact";
import {IStatefulSubArtifact, StatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact";
import {IArtifact} from "../../main/models/models";
import {IState} from "../../managers/artifact-manager/state";

export interface IStatefulGlossaryArtifact extends IStatefulArtifact {
}

export class StatefulGlossaryArtifact extends StatefulArtifact implements IStatefulGlossaryArtifact {

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = "/svc/bpartifactstore/glossary/" + id;
        return this.services.artifactService.getArtifactModel<IArtifact>(url, id, versionId);
    }

    public initialize(artifact: IArtifact): IState {
        const statefulSubartifacts: IStatefulSubArtifact[] = artifact.subArtifacts.map(subArtifact => {
            return new StatefulSubArtifact(this, subArtifact, this.services);
        });
        this.subArtifactCollection.initialise(statefulSubartifacts);

        return super.initialize(artifact);
    }
}
