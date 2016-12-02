import {IStatefulArtifact, StatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {IStatefulSubArtifact, StatefulSubArtifact} from "../../managers/artifact-manager/sub-artifact";
import {IArtifact} from "../../main/models/models";

export interface IStatefulGlossaryArtifact extends IStatefulArtifact {
}

export class StatefulGlossaryArtifact extends StatefulArtifact implements IStatefulGlossaryArtifact {

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<IArtifact> {
        const url = "/svc/bpartifactstore/glossary/" + id;
        return this.services.artifactService.getArtifactModel<IArtifact>(url, id, versionId);
    }

    protected initialize(artifact: IArtifact): void {
        const statefulSubartifacts: IStatefulSubArtifact[] = artifact.subArtifacts.map(subArtifact => {
            return new StatefulSubArtifact(this, subArtifact, this.services);
        });
        this.subArtifactCollection.initialise(statefulSubartifacts);

        super.initialize(artifact);
    }
}
