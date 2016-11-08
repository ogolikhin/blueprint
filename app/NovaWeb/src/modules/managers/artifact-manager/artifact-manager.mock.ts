import {ISelectionManager} from "../selection-manager/selection-manager";
import {IStatefulArtifact} from "./artifact";
import {ArtifactServiceMock} from "./artifact/artifact.svc.mock";
import {Models} from "../../main/models";

import {IArtifactManager} from "./artifact-manager";

export class ArtifactManagerMock implements IArtifactManager {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
        //
    }

    public dispose() {
        //
    }

    public selection = {} as ISelectionManager;

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
        //
    }

    public remove(id: number): IStatefulArtifact {
        return null;
    }

    public removeAll(projectId?: number) {
        //
    }

    public create(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): ng.IPromise<Models.IArtifact> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(ArtifactServiceMock.createNewArtifact(name, projectId, parentId, itemTypeId, orderIndex));
        return deferred.promise;
    }
}
