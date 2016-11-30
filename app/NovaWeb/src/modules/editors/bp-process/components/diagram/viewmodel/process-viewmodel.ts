import * as Models from "../../../../../main/models/models";
import * as Enums from "../../../../../main/models/enums";
import {IProcessGraphModel, ProcessGraphModel} from "./process-graph-model";
import {ProcessModels, ProcessEnums} from "../../../";
import {ICommunicationManager} from "../../../";
import {IStatefulArtifact} from "../../../../../managers/artifact-manager/";
import {IStatefulProcessSubArtifact, StatefulProcessSubArtifact} from "../../../process-subartifact";
import {IStatefulProcessArtifact, StatefulProcessArtifact} from "../../../process-artifact";
import {ProcessEvents} from "../process-diagram-communication";
import {MessageType, Message} from "../../../../../core/messages/message";
import {IMessageService} from "../../../../../core/messages/message.svc";

export interface IPersonaReferenceContainer {
    userTaskPersonaReferenceList: ProcessModels.IArtifactReference[];
    systemTaskPersonaReferenceList: ProcessModels.IArtifactReference[];
}

export interface IProcessViewModel extends IProcessGraphModel, IPersonaReferenceContainer {
    description: string;
    processType: ProcessEnums.ProcessType;
    isHistorical: boolean;
    isReadonly: boolean;
    isChanged: boolean;
    isUserToSystemProcess: boolean;
    licenseType: Enums.LicenseTypeEnum;
    isSpa: boolean;
    isSMB: boolean;
    shapeLimit: number;
    hasSelection: boolean;
    communicationManager: ICommunicationManager;
    isWithinShapeLimit(additionalShapes: number, isLoading?: boolean): boolean;
    getMessageText(message_id: string);
    showMessage(messageType: MessageType, messageText: string);
    updateProcessGraphModel(process);
    resetJustCreatedShapeIds();
    addJustCreatedShapeId(id: number);
    isShapeJustCreated(id: number): boolean;
    addShape(processShape: ProcessModels.IProcessShape);
    removeShape(shapeId: number);
}

export class ProcessViewModel implements IProcessViewModel {
    private DEFAULT_SHAPE_LIMIT: number = 100;

    private _rootScope: any = null;
    private _scope: ng.IScope = null;
    private _messageService: IMessageService = null;
    private processGraphModel: IProcessGraphModel = null;
    private _licenseType: Enums.LicenseTypeEnum;
    private _isSpa: boolean;
    private _isSMB: boolean;
    private _shapeLimit: number = this.DEFAULT_SHAPE_LIMIT;
    private _justCreatedShapeIds: number[] = [];
    private artifactUpdateHandler: string;
    private personaReferenceUpdatedHandler: string;

    constructor(private process,
                public communicationManager: ICommunicationManager,
                rootScope?: any,
                scope?: any,
                messageService?: IMessageService) {
        this.updateProcessGraphModel(process);
        this._rootScope = rootScope;

        if (scope) {
            this._scope = scope;
            this.getConfigurationSettings();
        }

        if (messageService) {
            this._messageService = messageService;
        }

        if (communicationManager) {
            this.artifactUpdateHandler = communicationManager.processDiagramCommunication
                .register(ProcessEvents.ArtifactUpdate, this.artifactsOnUpdate);
            this.personaReferenceUpdatedHandler = communicationManager.processDiagramCommunication
                .register(ProcessEvents.PersonaReferenceUpdated, this.personaReferenceOnUpdate);
        }
    }

    private artifactsOnUpdate = () => {
        const statefulArtifact = this.getStatefulArtifact();
        statefulArtifact.processOnUpdate();
    }

    private personaReferenceOnUpdate = (eventPayload: any) => {
        if (!eventPayload || !eventPayload.personaReference) {
            return;
        }
        if (eventPayload.isUserTask) {
            if (this.userTaskPersonaReferenceList.filter(o => o.id
                === eventPayload.personaReference.id).length === 0) {
                this.userTaskPersonaReferenceList.push(
                    <ProcessModels.IArtifactReference>eventPayload.personaReference
                );

            }
        }
        if (eventPayload.isSystemTask) {
            if (this.systemTaskPersonaReferenceList.filter(o => o.id
                === eventPayload.personaReference.id).length === 0) {
                this.systemTaskPersonaReferenceList.push(
                    <ProcessModels.IArtifactReference>eventPayload.personaReference
                );
            }
        }   
    }      

    public get isReadonly(): boolean {
        const statefulProcess: StatefulProcessArtifact = <StatefulProcessArtifact>this.process;

        if (statefulProcess && statefulProcess.artifactState) {
            return statefulProcess.artifactState.readonly;
        }

        return null;
    }

    public get isChanged(): boolean {
        const statefulProcess: StatefulProcessArtifact = <StatefulProcessArtifact>this.process;

        if (statefulProcess && statefulProcess.artifactState) {
            return statefulProcess.artifactState.dirty;
        }

        return null;
    }

    public get shapeLimit(): number {
        return this._shapeLimit;
    }

    public set shapeLimit(value: number) {
        this._shapeLimit = value;
    }

    public get hasSelection(): boolean {
        return this.process.hasSelection;
    }

    public set hasSelection(value: boolean) {
        this.process.hasSelection = value;
    }

    public get licenseType(): Enums.LicenseTypeEnum {
        return this._licenseType;
    };

    public set licenseType(value: Enums.LicenseTypeEnum) {
        this._licenseType = value;
    };

    public get isSpa(): boolean {
        return this._isSpa;
    }

    public set isSpa(value: boolean) {
        this._isSpa = value;
    }

    public get isSMB(): boolean {
        return this._isSMB;
    }

    public set isSMB(value: boolean) {
        this._isSMB = value;
    }

    public updateProcessGraphModel(process: ProcessModels.IProcess) {
        this.processGraphModel = new ProcessGraphModel(process);
    }

    public get processType(): ProcessEnums.ProcessType {
        return this.propertyValues["clientType"].value;
    }

    public set processType(value: ProcessEnums.ProcessType) {
        this.propertyValues["clientType"].value = value;
        const statefulArtifact: IStatefulArtifact = this.getStatefulArtifact();
        //checkgin for specialProperties so that unit tests pass.
        //It is additional work to make the specialProperties work with our IProcess model in unit tests
        if (statefulArtifact && statefulArtifact.specialProperties) {
            statefulArtifact.specialProperties.set(Enums.PropertyTypePredefined.ClientType, value);
        }
    }

    public get isUserToSystemProcess(): boolean {
        return this.processType === ProcessEnums.ProcessType.UserToSystemProcess;
    }

    public isWithinShapeLimit(additionalShapes: number = 1, isLoading: boolean = false): boolean {
        let result: boolean = false;
        let eightyPercent: number = Math.floor(this.shapeLimit * .80);
        let shapeCount = this.shapes.length + additionalShapes;

        // okay:  less than eighty percent of the shape limit
        if (shapeCount < eightyPercent) {
            result = true;
        } else if (shapeCount > this.shapeLimit) {
            let message: string;
            let messageType: MessageType = MessageType.Error;

            if (isLoading) {
                message = this.getMessageText("ST_Shape_Limit_Exceeded_Initial_Load");
                // replace {0} placeholder with number of shapes added
                // and {1} with shape limit value
                message = message.replace("{0}", shapeCount.toString());
                message = message.replace("{1}", this.shapeLimit.toString());
            } else {
                message = this.getMessageText("ST_Shape_Limit_Exceeded");
                // replace {0} placeholder with shape limit value
                message = message.replace("{0}", this.shapeLimit.toString());
            }

            // exceeds limit cannot add more shapes
            this.showMessage(messageType, message);
            return false;
        } else if (shapeCount >= eightyPercent &&
            shapeCount <= this.shapeLimit) {
            // if between eighty percent of shape limit and the shape limit
            // show warning
            let message = this.getMessageText("ST_Eighty_Percent_of_Shape_Limit_Reached");
            if (message) {
                // replace {0} placeholder with number of shapes added
                // and {1} with shape limit value
                message = message.replace("{0}", this.shapes.length + additionalShapes);
                message = message.replace("{1}", this.shapeLimit.toString());
                this.showMessage(MessageType.Warning, message);
            }

            return true;
        }

        return result;
    }

    public showMessage(messageType: MessageType, messageText: string) {
        const message = new Message(messageType, messageText);
        if (message && this._messageService) {
            this._messageService.clearMessages();
            this._messageService.addMessage(message);
            // force $digest cycle to show message
            if (this._scope && this._scope.$apply) {
                this._scope.$apply();
            }
        }
    }

    public get id(): number {
        return this.processGraphModel.id;
    }

    public get name(): string {
        return this.processGraphModel.name;
    }

    public get description(): string {
        return this.processGraphModel.propertyValues["description"].value;
    }

    public get typePrefix(): string {
        return this.processGraphModel.typePrefix;
    }

    public get projectId(): number {
        return this.processGraphModel.projectId;
    }

    public get baseItemTypePredefined(): Enums.ItemTypePredefined {
        return this.processGraphModel.baseItemTypePredefined;
    }

    public get shapes(): ProcessModels.IProcessShape[] {
        return this.processGraphModel.shapes;
    }

    public set shapes(newValue: ProcessModels.IProcessShape[]) {
        this.processGraphModel.shapes = newValue;
    }

    public get links(): ProcessModels.IProcessLinkModel[] {
        return <ProcessModels.IProcessLinkModel[]>this.processGraphModel.links;
    }

    public set links(newValue: ProcessModels.IProcessLinkModel[]) {
        this.processGraphModel.links = newValue;
    }

    public get userTaskPersonaReferenceList(): ProcessModels.IArtifactReference[] {
        return <ProcessModels.IArtifactReference[]>this.process.userTaskPersonaReferenceList;
    }

    public set userTaskPersonaReferenceList(newValue: ProcessModels.IArtifactReference[]) {
        this.process.userTaskPersonaReferenceList = newValue;
    }

    public get systemTaskPersonaReferenceList(): ProcessModels.IArtifactReference[] {
        return <ProcessModels.IArtifactReference[]>this.process.systemTaskPersonaReferenceList;
    }

    public set systemTaskPersonaReferenceList(newValue: ProcessModels.IArtifactReference[]) {
        this.process.systemTaskPersonaReferenceList = newValue;
    }

    public get propertyValues(): ProcessModels.IHashMapOfPropertyValues {
        return this.processGraphModel.propertyValues;
    }

    public set propertyValues(newValue: ProcessModels.IHashMapOfPropertyValues) {
        this.processGraphModel.propertyValues = newValue;
    }

    public get decisionBranchDestinationLinks(): ProcessModels.IProcessLink[] {
        return this.processGraphModel.decisionBranchDestinationLinks;
    }

    public set decisionBranchDestinationLinks(newValue: ProcessModels.IProcessLink[]) {
        this.processGraphModel.decisionBranchDestinationLinks = newValue;
    }

    protected addToSubArtifactCollection(statefulSubArtifact: IStatefulProcessSubArtifact) {

        const statefulArtifact: IStatefulArtifact = this.getStatefulArtifact();
        if (statefulArtifact) {
            statefulArtifact.subArtifactCollection.add(statefulSubArtifact);
        }
    }

    public addShape(processShape: ProcessModels.IProcessShape) {
        let services;
        if (this.process.getServices) {
            services = this.process.getServices();
        }

        const statefulShape = new StatefulProcessSubArtifact(this.process, processShape, services);
        this.shapes.push(statefulShape);
        this.addToSubArtifactCollection(statefulShape);
    }

    public removeShape(shapeId: number) {
        this.shapes = this.shapes.filter(shape => {
            return shape.id !== shapeId;
        });
        this.removeStatefulShape(shapeId);
    }

    protected removeStatefulShape(shapeId: number) {
        this.shapes = this.shapes.filter(shape => {
            return shape.id !== shapeId;
        });
        // cast process as an IStatefulArtifact

        const statefulArtifact: IStatefulArtifact = this.getStatefulArtifact();
        if (statefulArtifact) {
            statefulArtifact.subArtifactCollection.remove(shapeId);
        }
    }

    public updateTree() {
        this.processGraphModel.updateTree();
    }

    public updateTreeAndFlows() {
        this.processGraphModel.updateTreeAndFlows();
    }

    public getTree(): Models.IHashMap<ProcessModels.TreeShapeRef> {
        return this.processGraphModel.getTree();
    }

    public getLinkIndex(sourceId: number, destinationId: number): number {
        return this.processGraphModel.getLinkIndex(sourceId, destinationId);
    }

    public getNextOrderIndex(id: number): number {
        return this.processGraphModel.getNextOrderIndex(id);
    }

    public getShapeById(id: number): ProcessModels.IProcessShape {
        return this.processGraphModel.getShapeById(id);
    }

    public getShapeTypeById(id: number): ProcessEnums.ProcessShapeType {
        return this.processGraphModel.getShapeTypeById(id);
    }

    public getShapeType(shape: ProcessModels.IProcessShape): ProcessEnums.ProcessShapeType {
        return this.processGraphModel.getShapeType(shape);
    }

    public getNextShapeIds(id: number): number[] {
        return this.processGraphModel.getNextShapeIds(id);
    }

    public getPrevShapeIds(id: number): number[] {
        return this.processGraphModel.getPrevShapeIds(id);
    }

    public getStartShapeId(): number {
        return this.processGraphModel.getStartShapeId();
    }

    public getPreconditionShapeId(): number {
        return this.processGraphModel.getPreconditionShapeId();
    }

    public getEndShapeId(): number {
        return this.processGraphModel.getEndShapeId();
    }

    public hasMultiplePrevShapesById(id: number): boolean {
        return this.processGraphModel.hasMultiplePrevShapesById(id);
    }

    public getFirstNonSystemShapeId(id: number): number {
        return this.processGraphModel.getFirstNonSystemShapeId(id);
    }

    public getDecisionBranchDestinationLinks(isMatch: (link: ProcessModels.IProcessLink) => boolean): ProcessModels.IProcessLink[] {
        return this.processGraphModel.getDecisionBranchDestinationLinks(isMatch);
    }

    public getConnectedDecisionIds(destinationId: number): number[] {
        return this.processGraphModel.getConnectedDecisionIds(destinationId);
    }

    public getBranchDestinationIds(decisionId: number): number[] {
        return this.processGraphModel.getBranchDestinationIds(decisionId);
    }

    public getBranchDestinationId(decisionId: number, firstShapeInConditionId: number): number {
        return this.processGraphModel.getBranchDestinationId(decisionId, firstShapeInConditionId);
    }

    public isInSameFlow(id: number, otherId: number): boolean {
        return this.processGraphModel.isInSameFlow(id, otherId);
    }

    public isInChildFlow(id: number, otherId: number): boolean {
        return this.processGraphModel.isInChildFlow(id, otherId);
    }

    public isDecision(id: number) {
        return this.processGraphModel.isDecision(id);
    }

    public updateDecisionDestinationId(decisionId: number, orderIndex: number, newDestinationId: number) {
        this.processGraphModel.updateDecisionDestinationId(decisionId, orderIndex, newDestinationId);
    }

    public get isHistorical(): boolean {
        const statefulProcess: StatefulProcessArtifact = <StatefulProcessArtifact>this.process;

        if (statefulProcess && statefulProcess.artifactState && statefulProcess.artifactState.historical !== undefined) {
            return statefulProcess.artifactState.historical;
        }

        return null;
    }

    public resetJustCreatedShapeIds() {
        this._justCreatedShapeIds = [];
    }

    public addJustCreatedShapeId(id: number) {
        this._justCreatedShapeIds.push(id);
    }

    public isShapeJustCreated(id: number): boolean {
        return this._justCreatedShapeIds.filter(newId => id === newId).length > 0;
    }

    private getConfigurationSettings() {
        // get configuration settings from rootscope configuration object
        // and assign to viewmodel properties
        if (this.isRootScopeConfigValid) {
            let shapeLimitVal = this._rootScope.config.settings.StorytellerShapeLimit;
            if ((parseInt(shapeLimitVal, 10) || 0) > 0) {
                this.shapeLimit = Number(shapeLimitVal);
            } else {
                this.shapeLimit = this.DEFAULT_SHAPE_LIMIT;
            }

            let isSMBVal = this._rootScope.config.settings.StorytellerIsSMB;
            if (isSMBVal.toLowerCase() === "true") {
                this.isSMB = true;
            } else {
                this.isSMB = false;
            }
        }
    }

    public getMessageText(message_id: string) {
        // get message text from rootscope settings
        let text = null;
        if (this.isRootScopeConfigValid) {
            text = this._rootScope.config.labels[message_id];
        }

        return text;
    }

    public get isRootScopeConfigValid(): boolean {
        return this._rootScope && this._rootScope.config;
    }

    public destroy() {
        this._scope = null;

        if (this.processGraphModel != null) {
            this.processGraphModel.destroy();
            this.processGraphModel = null;
        }

        if (this.communicationManager) {
            this.communicationManager.processDiagramCommunication
                .unregister(ProcessEvents.ArtifactUpdate, this.artifactUpdateHandler);
        }
    }

    private getStatefulArtifact(): IStatefulProcessArtifact {
        let statefulArtifact: IStatefulProcessArtifact = this.process;
        return statefulArtifact;
    }
}
