import * as _ from "lodash";
import {ISelectionManager} from "../selection-manager/selection-manager";
import {IDialogService, IDialogSettings} from "../../shared";
import {IMetaDataService} from "./metadata";
import {IStatefulArtifact, IArtifactService} from "./artifact";
import {IDispose} from "../models";
import {ILoadingOverlayService} from "../../core/loadingOverlay/loadingOverlay.service";

export interface IArtifactManager extends IDispose {
    collectionChangeObservable: Rx.Observable<IStatefulArtifact>;
    selection: ISelectionManager;
    list(): IStatefulArtifact[];
    add(artifact: IStatefulArtifact);
    get(id: number): IStatefulArtifact;
    remove(id: number): IStatefulArtifact;
    removeAll(projectId?: number);
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
        "dialogService",
        "loadingOverlayService"
    ];

    constructor(private $log: ng.ILogService,
                private $q: ng.IQService,
                private selectionService: ISelectionManager,
                private artifactService: IArtifactService,
                private metadataService: IMetaDataService,
                private dialogService: IDialogService,
                private loadingOverlayService: ILoadingOverlayService) {
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

    public autosave(showConfirm: boolean = true): ng.IPromise<any> {
        const artifact = this.selection.getArtifact();
        if (artifact) {
            let autosaveId = this.loadingOverlayService.beginLoading();
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
            }).finally(() => this.loadingOverlayService.endLoading(autosaveId));
        }
        return this.$q.resolve();
    }
}
