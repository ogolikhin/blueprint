import * as Models from "../../../../../main/models/models";
import * as Enums from "../../../../../main/models/enums";
import {IMessageService, Message, MessageType} from "../../../../../core/";
import {IProcessGraphModel, ProcessGraphModel} from "./process-graph-model";
import {ProcessModels, ProcessEnums} from "../../../";
import {ICommunicationManager} from "../../../";
import { IStatefulArtifact } from "../../../../../managers/artifact-manager/";
import { StatefulProcessSubArtifact } from "../../../process-subartifact";

export interface IProcessViewModel extends IProcessGraphModel {
    description: string;
    processType: ProcessEnums.ProcessType;
    isLocked: boolean;
    isLockedByMe: boolean;
    isHistorical: boolean;
    isReadonly: boolean;
    isChanged: boolean;
    isUnpublished: boolean;
    isUserToSystemProcess: boolean;
    showLock: boolean;
    showLockOpen: boolean;
    licenseType: Enums.LicenseTypeEnum;
    isSpa: boolean;
    isSMB: boolean;
    shapeLimit: number;
    communicationManager: ICommunicationManager;
    isWithinShapeLimit(additionalShapes: number, isLoading?: boolean): boolean;
    getMessageText(message_id: string);
    showMessage(messageType: MessageType, messageText: string);
    updateProcessGraphModel(process);
    resetLock();
    resetJustCreatedShapeIds();
    addJustCreatedShapeId(id: number);
    isShapeJustCreated(id: number): boolean;
    addShape(processShape: ProcessModels.IProcessShape);
    removeShape(shapeId: number);
}

export class ProcessViewModel implements IProcessViewModel {

    private DEFAULT_SHAPE_LIMIT: number = 100;
    private _rootScope: any = null;
    private _scope: any = null;
    private _messageService: IMessageService = null;
    private processGraphModel: IProcessGraphModel = null;
    private _isChanged: boolean = false;
    private _showLock: boolean;
    private _showLockOpen: boolean;
    private _isReadonly: boolean = false;
    private _licenseType: Enums.LicenseTypeEnum;
    private _isSpa: boolean;
    private _isSMB: boolean;
    private _shapeLimit: number = this.DEFAULT_SHAPE_LIMIT;
    private _communicationManager: ICommunicationManager;
    private _justCreatedShapeIds: number[] = [];
  
    constructor(private process, rootScope?: any, scope?: any, messageService?: IMessageService) {

        this.updateProcessGraphModel(process);
        this._rootScope = rootScope;
        if (scope) {
            this._scope = scope;
            this.getConfigurationSettings(); 
        }
        
        if (messageService) {
            this._messageService = messageService;
        }
    }
    
    public get isReadonly(): boolean {
        return this._isReadonly;
    }

    public set isReadonly(value) {
        this._isReadonly = value;
    }
    
    public get showLock(): boolean {
        return this._showLock;
    }

    public set showLock(value: boolean) {
        this._showLock = value;
    }

    public get showLockOpen(): boolean {
        return this._showLockOpen;
    }

    public set showLockOpen(value: boolean) {
        this._showLockOpen = value;
    }

    public get isLocked(): boolean {
        return this.status.isLocked;
    }

    public set isLocked(value) {

        this.status.isLocked = value;
        this.showLock = this.status.isLocked && !this.status.isLockedByMe;
        this.showLockOpen = this.status.isLocked && this.status.isLockedByMe;
    }

    public get isLockedByMe(): boolean {
        return this.status.isLockedByMe;
    }

    public set isLockedByMe(value) {

        this.status.isLockedByMe = value;

        this.showLock = this.status.isLocked && !this.status.isLockedByMe;
        this.showLockOpen = this.status.isLocked && this.status.isLockedByMe;
    }

    public get isChanged(): boolean {
        return this._isChanged;
    }

    public set isChanged(value: boolean) {
        this._isChanged = value;
    }

    public get shapeLimit(): number {
        return this._shapeLimit;
    }

    public set shapeLimit(value: number) {
        this._shapeLimit = value;
    }

    public get communicationManager(): ICommunicationManager {
        return this._communicationManager;
    }

    public set communicationManager(value: ICommunicationManager) {
        this._communicationManager = value;
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

    public get isUnpublished(): boolean {
        return this.isChanged || this.status.isUnpublished;
    }

    public set isUnpublished(value: boolean) {
        this.status.isUnpublished = value;
    }

    public updateProcessGraphModel(process: ProcessModels.IProcess) {
        this.processGraphModel = new ProcessGraphModel(process);

        this.showLock = this.status.isLocked && !this.status.isLockedByMe;
        this.showLockOpen = this.status.isLocked && this.status.isLockedByMe;

        this.isChanged = false;
        this.isReadonly = process.status.isReadOnly;
    }

    public get processType(): ProcessEnums.ProcessType {
        return this.propertyValues["clientType"].value;
    }

    public set processType(value: ProcessEnums.ProcessType) {
        this.propertyValues["clientType"].value = value;
    }

    public get isUserToSystemProcess(): boolean {
        return this.processType === ProcessEnums.ProcessType.UserToSystemProcess;
    }

    public resetLock() {
        this.isLocked = false;
        this.isLockedByMe = false;
    }

    public isWithinShapeLimit(additionalShapes: number = 1, isLoading: boolean = false): boolean {
        let result: boolean = false;
        let eightyPercent: number = Math.floor(this.shapeLimit * .80);
        let shapeCount = this.shapes.length + additionalShapes;
        if (shapeCount < eightyPercent) {
            // okay:  less than eighty percent of the shape limit 
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

        var message = new Message(messageType, messageText);

        if (message && this._messageService) {
            //this._messageService.clearMessages();
            this._messageService.addMessage(message);
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

    public get status(): ProcessModels.IItemStatus {
        return this.processGraphModel.status;
    }

    protected addStatefulShape(processShape: ProcessModels.IProcessShape) {

        let statefulShape = new StatefulProcessSubArtifact(this.process,
            processShape, this.process.getServices());

        let statefulArtifact: IStatefulArtifact = this.process; 

        statefulArtifact.subArtifactCollection.add(statefulShape);
    }
     
    public addShape(processShape: ProcessModels.IProcessShape) {

        this.shapes.push(processShape);

        this.addStatefulShape(processShape);
    }

    public removeShape(shapeId: number) {
        this.shapes = this.shapes.filter(shape => { return shape.id !== shapeId; });
        this.removeStatefulShape(shapeId);
    }
    protected removeStatefulShape(shapeId: number) {
        this.shapes = this.shapes.filter(shape => { return shape.id !== shapeId; });
        // cast process as an IStatefulArtifact 
        let statefulArtifact: IStatefulArtifact = this.process;
        statefulArtifact.subArtifactCollection.remove(shapeId);
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
        // TODO: provide proper implementation once version information is available in the model
        return this.processGraphModel["process"]["versionId"] != null ||
            this.processGraphModel["process"]["revisionId"] != null ||
            this.processGraphModel["process"]["baselineId"] != null;
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
            let shapeLimitVal = this._rootScope.config.settings.ProcessShapeLimit;
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

    }
}