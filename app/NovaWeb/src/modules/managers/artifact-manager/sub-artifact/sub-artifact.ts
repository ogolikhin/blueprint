import { Models } from "../../../main/models";
// import { ArtifactState} from "../state";
// import { IArtifactManager } from "../";
import { ArtifactAttachments, IArtifactAttachments } from "../attachments";
import { IDocumentRefs, DocumentRefs } from "../docrefs";
import { CustomProperties } from "../properties";
import { ChangeSetCollector } from "../changeset";
import {
    ChangeTypeEnum,
    IChangeCollector,
    IChangeSet,
    IStatefulArtifact,
    // IArtifactStates,
    IArtifactProperties,
    IState,
    IStatefulArtifactServices,
    IIStatefulArtifact,
    IIStatefulSubArtifact,
    IStatefulSubArtifact,
    IArtifactAttachmentsResultSet
} from "../../models";

export class StatefulSubArtifact implements IStatefulSubArtifact, IIStatefulSubArtifact {
    private artifact: IIStatefulArtifact;
    private subArtifact: Models.ISubArtifact;
    private changesets: IChangeCollector;
    private services: IStatefulArtifactServices;

    public attachments: IArtifactAttachments;
    public docRefs: IDocumentRefs;
    public customProperties: IArtifactProperties;

    constructor(artifact: IIStatefulArtifact, subArtifact: Models.ISubArtifact, services: IStatefulArtifactServices) {
        this.artifact = artifact;
        this.subArtifact = subArtifact;
        this.services = services;
        this.changesets = new ChangeSetCollector();

        this.artifact = artifact;
        this.changesets = new ChangeSetCollector();
        this.services = services;
        this.customProperties = new CustomProperties(this).initialize(artifact);
        this.attachments = new ArtifactAttachments(this);
        this.docRefs = new DocumentRefs(this);

        // this.artifact.artifactState.observable
        //     .filter((it: IState) => !!it.lock)
        //     .distinctUntilChanged()
        //     .subscribeOnNext(this.onLockChanged, this);
    }

    //TODO.
    //Needs implementation of other object like
    //attachments, traces and etc.

    public get artifactState() {
        return this.artifact.artifactState;
    }

    //TODO: implement system property getters and setters
    public get id(): number {
        return this.subArtifact.id;
    }

    public get name(): string {
        return this.subArtifact.name;
    }

    public set name(value: string) {
        this.set("name", value);
    }

    public get description(): string {
        return this.subArtifact.description;
    }

    public set description(value: string) {
        this.set("description", value);
    }

    public get itemTypeId(): number {
        return this.subArtifact.itemTypeId;
    }

    public set itemTypeId(value: number) {
        this.set("itemTypeId", value);
    }

    public get itemTypeVersionId(): number {
        return this.subArtifact.itemTypeVersionId;
    }
    public get predefinedType(): Models.ItemTypePredefined {
        return this.subArtifact.predefinedType;
    }

    public get version() {
        return this.subArtifact.version;
    }

    public get prefix(): string {
        return this.subArtifact.prefix;
    }

    public get parentId(): number {
        return this.subArtifact.parentId;
    }

    private set(name: string, value: any) {
        if (name in this) {
           const oldValue = this[name];
           const changeset = {
               type: ChangeTypeEnum.Update,
               key: name,
               value: value
           } as IChangeSet;
           this.changesets.add(changeset, oldValue);
           this.lock();
        }
    }

    public discard(): ng.IPromise<IStatefulArtifact>   {
        const deferred = this.services.getDeferred<IStatefulArtifact>();

        // this.changesets.reset().forEach((it: IChangeSet) => {
        //     this[it.key as string].value = it.value;
        // });

        // this.customProperties.discard();
        // deferred.resolve(this);
        return deferred.promise;
    }

    public lock(): ng.IPromise<IState> {
        return this.artifact.lock();
    }

    public getAttachmentsDocRefs(): ng.IPromise<IArtifactAttachmentsResultSet> {
        return this.services.attachmentService.getArtifactAttachments(this.parentId, this.id, true)
            .then( (result: IArtifactAttachmentsResultSet) => {

                // initialize attachments
                this.attachments.initialize(result.attachments);

                // TODO: initialize doc refs here
                // this.docRefs.initialize(result.documentReferences);
                console.log("initalizing doc refs with latest data");
                
                return result;
            });
    }

    public getServices(): IStatefulArtifactServices {
        return this.services;
    }
}
