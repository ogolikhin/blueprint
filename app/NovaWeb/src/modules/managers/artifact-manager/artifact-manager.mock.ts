import * as _ from "lodash";
import {IMessageService} from "../../core/messages";
import {ISelectionManager} from "../selection-manager/selection-manager";
import {IMetaDataService} from "./metadata";
import {IStatefulArtifactFactory, IStatefulArtifact} from "./artifact";

import {IArtifactManager} from "./artifact-manager";

export class ArtifactManagerMock implements IArtifactManager {
    public static $inject = [
    ];

    constructor() {
        ;
    }

    public dispose() {
        ;
    }

    public get selection(): ISelectionManager {
        return null;
    }

    public list(): IStatefulArtifact[] {
        return [];
    }

    public get collectionChangeObservable(): Rx.Observable<IStatefulArtifact> {
        return null;
    }

    public get(id: number): IStatefulArtifact {
        return null;
    }

    public add(artifact: IStatefulArtifact) {
        ;
    }

    public remove(id: number): IStatefulArtifact {
        return null;
    }

    public removeAll(projectId?: number) {
        ;
    }
}
