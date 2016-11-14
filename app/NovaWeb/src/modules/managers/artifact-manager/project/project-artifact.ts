import {IStatefulArtifact, StatefulArtifact} from "../artifact";
import {IArtifact} from "../../../main/models/models";
import {IStatefulArtifactServices} from "../services/services";

export interface IStatefulProjectArtifact extends IStatefulArtifact {
}

export class StatefulProjectArtifact extends StatefulArtifact implements IStatefulProjectArtifact {
    private isProjectLoaded: boolean;

    constructor(artifact: IArtifact, protected services: IStatefulArtifactServices) {
        super(artifact, services);
        this.isProjectLoaded = false;
    }

    protected isFullArtifactLoadedOrLoading(): boolean {
        return this.isProjectLoaded;
    }

    public getObservable(): Rx.Observable<IStatefulArtifact> {
        if (!this.isFullArtifactLoadedOrLoading()) {
            this.isProjectLoaded = true;
            this.subject.onNext(this);
        }
        return this.subject.filter(it => !!it).asObservable();
    }

    public canBeSaved(): boolean {
        return false;
    }

    public canBePublished(): boolean {
        return false;
    }

    public unload() {
        this.isProjectLoaded = false;
        super.unload();
    }
}

