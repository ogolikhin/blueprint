import {Models} from "../../main/models";
import {ItemTypePredefined} from "../../main/models/item-type-predefined";
import {IStatefulArtifact, StatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {IStatefulArtifactServices} from "../../managers/artifact-manager/services";
import {ProcessType} from "./models/enums";
import {IProcess, IProcessLink, IProcessShape} from "./models/process-models";
import {IHashMapOfPropertyValues} from "./models/process-models";
import {IArtifactReference} from "./models/process-models";
import {StatefulProcessSubArtifact} from "./process-subartifact";
import {ProcessModelProcessor} from "./services/process-model-processor";

export interface INovaProcess extends Models.IArtifact {
     process: IProcess;
 }

export interface IStatefulProcessArtifact extends IStatefulArtifact {
    processOnUpdate();
}

export class StatefulProcessArtifact extends StatefulArtifact implements IStatefulProcessArtifact, IProcess {
    private loadProcessPromise: ng.IPromise<IStatefulArtifact>;
    private artifactPropertyTypes: {} = null;

    public shapes: IProcessShape[];
    public links: IProcessLink[];
    public decisionBranchDestinationLinks: IProcessLink[];
    public propertyValues: IHashMapOfPropertyValues;

    public userTaskPersonaReferenceList: IArtifactReference[];
    public systemTaskPersonaReferenceList: IArtifactReference[];

    constructor(artifact: Models.IArtifact, protected services: IStatefulArtifactServices) {
        super(artifact, services);
    }

    public processOnUpdate() {
        this.artifactState.dirty = true;
        this.lock();
    }

    public get baseItemTypePredefined(): ItemTypePredefined {
        return this.predefinedType;
    }

    public get typePrefix(): string {
        return this.prefix;
    }

    public getServices(): IStatefulArtifactServices {
        return this.services;
    }

    protected runPostGetObservable() {
        this.loadProcessPromise = null;
    }

    protected displaySuccessPublishMessage() {
        this.services.messageService.addInfo("Publish_Success_Message");
        this.services.messageService.addInfo("ST_ProcessType_RegenerateUS_Message");
    }

    protected isFullArtifactLoadedOrLoading(): boolean {
        return super.isFullArtifactLoadedOrLoading() || !!this.loadProcessPromise;
    }

    protected getArtifactModel(id: number, versionId: number): ng.IPromise<Models.IArtifact> {
         const url = "/svc/bpartifactstore/process/" + id;
         return this.services.artifactService.getArtifactModel<INovaProcess>(url, id, versionId);
    }

    protected updateArtifact(changes: Models.IArtifact, autoSave: boolean): ng.IPromise<Models.IArtifact> {
        const url = `/svc/bpartifactstore/processupdate/${changes.id}`;
        const processor = new ProcessModelProcessor();
        (<INovaProcess>changes).process = processor.processModelBeforeSave(this);
        return this.services.artifactService.updateArtifact(url, changes);
    }

    protected initialize(artifact: Models.IArtifact): void {
        super.initialize(artifact);
        this.onLoad((<INovaProcess>artifact).process);
    }

    private onLoad(newProcess: IProcess) {
        this.initializeSubArtifacts(newProcess);

        const currentProcess = <IProcess>this;
        // TODO: updating name seems to cause an infinite loading of process, something with base class's 'set' logic.
        //currentProcess.name = newProcess.name;
        currentProcess.links = newProcess.links;
        currentProcess.decisionBranchDestinationLinks = newProcess.decisionBranchDestinationLinks;
        currentProcess.propertyValues = newProcess.propertyValues;
        currentProcess.userTaskPersonaReferenceList = newProcess.userTaskPersonaReferenceList;
        currentProcess.systemTaskPersonaReferenceList = newProcess.systemTaskPersonaReferenceList;
    }

    private initializeSubArtifacts(newProcess: IProcess) {
        const statefulSubArtifacts = [];
        this.shapes = [];

        for (const shape of newProcess.shapes) {
            const statefulShape = new StatefulProcessSubArtifact(this, shape, this.services);
            this.shapes.push(statefulShape);
            statefulSubArtifacts.push(statefulShape);
        }

        this.subArtifactCollection.initialise(statefulSubArtifacts);
    }

    protected getArtifactToSave(changes: Models.IArtifact) {
         const novaProcess: INovaProcess = changes as INovaProcess;
         if (novaProcess) {
             novaProcess.process = new ProcessModelProcessor().processModelBeforeSave(<IProcess>this);
         }
         return super.getArtifactToSave(changes);
    }

    public get clientType(): ProcessType {
        if (!this.propertyValues) {
            return ProcessType.None;
        }
        return this.propertyValues["clientType"].value as ProcessType;
    }

    public set clientType(value: ProcessType) {
        this.propertyValues["clientType"].value = value;
    }
}
