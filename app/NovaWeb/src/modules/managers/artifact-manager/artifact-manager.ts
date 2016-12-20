import * as _ from "lodash";
import {ISelectionManager} from "../selection-manager/selection-manager";
import {IDialogService, IDialogSettings} from "../../shared";
import {IMetaDataService} from "./metadata";
import {IStatefulArtifact, IArtifactService} from "./artifact";
import {IDispose} from "../models";
import {Models} from "../../main/models";

export interface IArtifactManager extends IDispose {
    collectionChangeObservable: Rx.Observable<IStatefulArtifact>;
    selection: ISelectionManager;
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): IStatefulArtifact;
    remove(id: number): IStatefulArtifact;
    removeAll(projectId?: number);
    create(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): ng.IPromise<Models.IArtifact>;
    autosave(showConfirm?: boolean): ng.IPromise<any>;

}

export class ArtifactManager implements IArtifactManager {
    private artifactDictionary: { [id: number]: IStatefulArtifact };
    private collectionChangeSubject: Rx.BehaviorSubject<IStatefulArtifact>;

    public static $inject = [
        "$log",
        "$q",
        "selectionManager",
        "artifactService",
        "metadataService",
        "dialogService"
    ];

    constructor(private $log: ng.ILogService,
                private $q: ng.IQService,
                private selectionService: ISelectionManager,
                private artifactService: IArtifactService,
                private metadataService: IMetaDataService,
                private dialogService: IDialogService) {
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

    private clearSelection(projectId?: number): void {
        if (!projectId) {
            this.selection.clearAll();
            return;
        }

        const selectedArtifact = this.selection.getArtifact();
        if (!selectedArtifact) {
            return;
        }

        if (selectedArtifact.projectId === projectId) {
            this.selection.clearAll();
        }
    }

    public removeAll(projectId?: number) {
        this.clearSelection(projectId);

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

    public create(name: string, projectId: number, parentId: number, itemTypeId: number, orderIndex?: number): ng.IPromise<Models.IArtifact> {
        const deferred = this.$q.defer<Models.IArtifact>();

        this.artifactService.create(name, projectId, parentId, itemTypeId, orderIndex)
            .then((artifact: Models.IArtifact) => {
                deferred.resolve(artifact);
            })
            .catch((error) => {
                deferred.reject(error);
            });

        return deferred.promise;
    }

    public autosave(showConfirm: boolean = true): ng.IPromise<any> {
        const artifact = this.selection.getArtifact();
        if (artifact) {
            return artifact.save(true).catch((error) => {
                if (showConfirm) {
                    return this.dialogService.open(<IDialogSettings>{
                        okButton: "App_Button_Proceed",
                        message: "App_Save_Auto_Confirm",
                        header: "App_DialogTitle_Alert",
                        css: "modal-alert nova-messaging"
                    }).then(() => {
                        artifact.discard();
                    });
                } else {
                    return this.$q.reject(error);
                }
            });
        }
        return this.$q.resolve();
    }
}
