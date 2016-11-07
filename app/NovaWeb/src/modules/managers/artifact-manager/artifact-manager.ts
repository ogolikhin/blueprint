import * as _ from "lodash";
import {ISelectionManager} from "../selection-manager/selection-manager";
import {IMetaDataService} from "./metadata";
import {IStatefulArtifact} from "./artifact";
import {IDispose} from "../models";

export interface IArtifactManager extends IDispose {
    collectionChangeObservable: Rx.Observable<IStatefulArtifact>;
    selection: ISelectionManager;
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): IStatefulArtifact;
    remove(id: number): IStatefulArtifact;
    removeAll(projectId?: number);
}

export class ArtifactManager implements IArtifactManager {
    private artifactDictionary: { [id: number]: IStatefulArtifact };
    private collectionChangeSubject: Rx.BehaviorSubject<IStatefulArtifact>;

    public static $inject = [
        "$log",
        "selectionManager",
        "metadataService"
    ];

    constructor(private $log: ng.ILogService,
                private selectionService: ISelectionManager,
                private metadataService: IMetaDataService) {
        this.artifactDictionary = {};
        this.collectionChangeSubject = new Rx.BehaviorSubject<IStatefulArtifact>(null);
    }

    public dispose() {
        this.removeAll();
        this.selection.dispose();
    }

    public get selection(): ISelectionManager {
        return this.selectionService;
    }

    public list(): IStatefulArtifact[] {
        return _.values(this.artifactDictionary) as IStatefulArtifact[];
    }

    public get collectionChangeObservable(): Rx.Observable<IStatefulArtifact> {
        return this.collectionChangeSubject.filter((it: IStatefulArtifact) => it != null).asObservable();
    }

    public get(id: number): IStatefulArtifact {
        return this.artifactDictionary[id];
    }

    public add(artifact: IStatefulArtifact) {
        if (this.artifactDictionary[artifact.id]) {
            this.$log.info(`Overwriting an already added artifact with id: ${artifact.id}`);
        }
        this.artifactDictionary[artifact.id] = artifact;
        this.collectionChangeSubject.onNext(artifact);
    }

    public remove(id: number): IStatefulArtifact {
        let artifact: IStatefulArtifact = this.artifactDictionary[id];

        if (artifact) {
            delete this.artifactDictionary[id];
            this.collectionChangeSubject.onNext(artifact);
        }

        return artifact;
    }

    public removeAll(projectId?: number) {
        for (const artifactKey in this.artifactDictionary) {
            const artifact = this.artifactDictionary[artifactKey];

            if (this.artifactDictionary.hasOwnProperty(artifactKey)) {
                if (!projectId || artifact.projectId === projectId) {
                    artifact.dispose();
                    delete this.artifactDictionary[artifactKey];
                }
            }
        }
        if (projectId) {
            this.metadataService.remove(projectId);
        }
    }
}
