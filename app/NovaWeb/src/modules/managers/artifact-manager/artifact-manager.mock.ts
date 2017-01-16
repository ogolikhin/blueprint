import {ISelectionManager} from "../selection-manager/selection-manager";
import {IStatefulArtifact} from "./artifact";
import {IArtifactManager} from "./artifact-manager";

export class ArtifactManagerMock implements IArtifactManager {
    public static $inject = ["$q"];
    private collectionChangeSubject: Rx.BehaviorSubject<IStatefulArtifact>;

    constructor(private $q: ng.IQService, private selectionService: ISelectionManager) {
        this.collectionChangeSubject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
    }

    public dispose() {
        //
    }

    public selection = {
        getArtifact: () => {
            return <IStatefulArtifact>{};
        }
    } as ISelectionManager;

    public list(): IStatefulArtifact[] {
        return [];
    }

    public get collectionChangeObservable(): Rx.Observable<IStatefulArtifact> {
        return this.collectionChangeSubject.filter((it: IStatefulArtifact) => it != null).asObservable();
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

    public autosave(): ng.IPromise<any> {
        return this.$q.resolve();
    }
}
