import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactAttachments } from "../attachments";
import { CustomProperties } from "../properties";
import { IStatefulArtifact, IArtifactState, IArtifactPropertyValues, IArtifactManager, IState } from "../interfaces";


export class StatefullArtifact implements IStatefulArtifact {
    private artifact: Models.IArtifact;
    public manager: IArtifactManager;
    public state: IArtifactState;
    public attachments: ArtifactAttachments;
    public customProperties: IArtifactPropertyValues; 

    //TODO. 
    //Needs implementation of other object like 
    //attachments, traces and etc.

    //TODO: implement system property getters and setters    
    // id: number;
    // name?: string;
    // description?: string;
    // prefix?: string;
    // parentId?: number;
    // itemTypeId?: number;
    // itemTypeVersionId?: number;
    // version?: number;
    // predefinedType?: Enums.ItemTypePredefined;
    // projectId?: number;
    // orderIndex?: number;

    // createdOn?: Date; 
    // lastEditedOn?: Date;
    // createdBy?: Models.IUserGroup;
    // lastEditedBy?: Models.IUserGroup;
    // permissions?: Enums.RolePermissions;
    // readOnlyReuseSettings?: Enums.ReuseSettings;

    
    // lockedByUser?: IUserGroup;
    // lockedDateTime?: Date;

    public get id(): number {
        return this.artifact.id;
    }

    public get name(): string {
        return this.artifact.name;
    }

    public set name(value: string) {
        this.set("name", value);
    }

    public get description(): string {
        return this.artifact.description;
    }

    public set description(value: string) {
        this.set("description", value);
    }

    public get itemTypeId(): number {
        return this.artifact.itemTypeId;
    }

    public set itemTypeId(value: number) {
        this.set("itemTypeId", value);
    }

    public get itemTypeVersionId(): number {
        return this.artifact.itemTypeVersionId;
    }
    public get predefinedType(): Models.ItemTypePredefined {
        return this.artifact.predefinedType;
    }

    public get permissions(): Enums.RolePermissions {
        return this.artifact.permissions;
    }

    public get version() {
        return this.artifact.version;
    }

    public get projectId() {
        return this.artifact.projectId;
    }

    public get prefix(): string {
        return this.artifact.prefix;
    }

    public get parentId(): number {
        return this.artifact.parentId;
    }

    public get createdOn(): Date {
        return this.artifact.createdOn;
    }

    public get lastEditedOn(): Date {
        return this.artifact.lastEditedOn;
    }

    public get createdBy(): Models.IUserGroup {
        return this.artifact.createdBy;
    }

    public get lastEditedBy(): Models.IUserGroup {
        return this.artifact.lastEditedBy;
    }
    

    constructor(manager: IArtifactManager, artifact: Models.IArtifact) {
        this.manager = manager;
        this.artifact = artifact;
        this.state = new ArtifactState(this).initialize(artifact);

        this.attachments = new ArtifactAttachments(this);
        this.customProperties = new CustomProperties(this).initialize(artifact);

        this.state.observable.filter((it: IState) => !!it.lock).distinctUntilChanged().subscribeOnNext(this.onLockChanged, this);
    }



     public set(name: string, value: any) {
        if (name in this) {
            this[name] = value;
        }
        this.lock(); 
    }



    public loadArtifact(timeout?: ng.IPromise<any>)  {
        
        const config: ng.IRequestConfig = {
            url: `/svc/bpartifactstore/artifacts/${this.id}`,
            method: "GET",
            timeout: timeout
        };
        this.manager.request<Models.IArtifact>(config).then((artifact: Models.IArtifact) => {
            this.artifact = artifact;
            this.state.initialize(artifact);
            this.customProperties.initialize(artifact);
        });
    }

    private loadSubArtifact(subArtifactId: number, timeout?: ng.IPromise<any>) {
        const config: ng.IRequestConfig = {
            url:  `/svc/bpartifactstore/artifacts/${this.id}/subartifacts/${subArtifactId}`,
            method: "GET",
            timeout: timeout
        };
        this.manager.request<Models.ISubArtifact>(config).then((artifact: Models.ISubArtifact) => {

        });
    }


    public lock(): ng.IPromise<IState> {
        let deferred = this.manager.$q.defer<IState>();

        const config: ng.IRequestConfig = {
            url: `/svc/shared/artifacts/lock`,
            method: "post",
            data: angular.toJson([this.id])
        };
        this.manager.request<Models.ILockResult>(config).then((lock: Models.ILockResult) => {
            this.state.set({lock: lock} as IState);
            deferred.resolve(this.state.get());
        }).catch((err) => {
            deferred.reject(err);
        });
        return deferred.promise;
    }


     private onLockChanged(state: IState) {
        if (!state.lock) {
            return;
        }
        if (state.lock.result === Enums.LockResultEnum.Success) {
            if (state.lock.info.versionId !== this.version) {
                this.loadArtifact();
            }
        } else if (state.lock.result === Enums.LockResultEnum.AlreadyLocked) {
//            this.messageService.addMessage(new Message(3, "Artifact_Lock_" + Enums.LockResultEnum[lock.result]));
            this.loadArtifact();
        } else if (state.lock.result === Enums.LockResultEnum.DoesNotExist) {
//            this.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
        } else {
//            this.messageService.addError("Artifact_Lock_" + Enums.LockResultEnum[lock.result]);
        }

    }

}
