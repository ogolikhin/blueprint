﻿// References to StorytellerDiagram
import {IMessageService, Message, MessageType} from "../../../../core";
import {IProcess} from "../../models/processModels";
import {IProcessService} from "../../services/process/process.svc";
import {ProcessViewModel, IProcessViewModel} from "./viewmodel/process-viewmodel";
import {IProcessGraph} from "./presentation/graph/process-graph-interfaces";
import {ProcessGraph} from "./presentation/graph/process-graph";
import {IDialogManager, DialogManager} from "../dialogs/dialog-manager";


export class StorytellerDiagram {
    public processModel: IProcess;
    public processViewModel: IProcessViewModel = null;
    private graph: IProcessGraph = null;
    private htmlElement: HTMLElement; 
   
    constructor(
        private $rootScope: ng.IRootScopeService,
        private $scope: ng.IScope,
        private $state: ng.ui.IState,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $log: ng.ILogService,
        private processService: IProcessService,
        private messageService: IMessageService,
        private dialogManager: IDialogManager) {

        this.processModel = null;
    }

    public debugInformation: string;

    public createDiagram(processId: number, htmlElement: HTMLElement) {
        // retrieve the specified process from the server and 
        // create a new diagram

        this.checkParams(processId, htmlElement);
        this.htmlElement = htmlElement;
        let id: string = processId.toString();

        //const storytellerParams = this.bpUrlParsingService.getStateParams();   

        this.processService.load(id //,
            //storytellerParams.versionId,
            //storytellerParams.revisionId,
            //storytellerParams.baselineId,
            //storytellerParams.readOnly
        ).then((process: IProcess) => {
            this.onLoad(process);
            //if (!this.processViewModel.isReadonly) {
            //    this.processViewModel.isWithinShapeLimit(0, true);
            //}
        }).catch((err: any) => {
            // if access to proccess info is forbidden
            if (err && err.statusCode === 403) {
                //handle errors if need to
            }
        });
    }

    private checkParams(processId: number, htmlElement: HTMLElement): void {
        if (!this.validProcessId(processId)) {
            throw new Error("Process id '" + processId + "' is invalid.");
        }
        if (htmlElement) {
            // okay 
        } else {
            throw new Error("There is no html element for the diagram");
        }
    }

    private validProcessId(processId: number): boolean {
        return processId != null && processId > 0;
    }

    private onLoad(process: IProcess, useAutolayout: boolean = false, selectedNodeId: number = undefined) {

        this.resetBeforeLoad();
        
        this.processModel = process;

        this.debugInformation = "PROCESS LOADED";
        this.dumpDebugInformation(this.processModel);
        let processViewModel = this.createProcessViewModel(process);
        // set isSpa flag to true. Note: this flag may no longer be needed.
        processViewModel.isSpa = true;

        //if (processViewModel.isReadonly) this.disableStorytellerToolbar();
        this.createProcessGraph(processViewModel, useAutolayout, selectedNodeId);
    }

    private createProcessViewModel(process: IProcess): IProcessViewModel {
        if (this.processViewModel == null) {
            this.processViewModel = new ProcessViewModel(process, this.$rootScope, this.$scope, this.messageService);
            this.processViewModel.dialogManager = this.dialogManager; 
        } else {
            this.processViewModel.updateProcessGraphModel(process);
        }
        return this.processViewModel;
    }

    private createProcessGraph(processViewModel: IProcessViewModel, useAutolayout: boolean = false, selectedNodeId: number = undefined) {

        try {
            this.graph = new ProcessGraph(
                            this.$rootScope,
                            this.$scope,
                            this.htmlElement,
                            this.processService,
                            processViewModel,
                            this.messageService,
                            this.$log);
        } catch (err) {
            this.handleInitProcessGraphFailed(processViewModel.id, err);
        }

        try {
            this.graph.render(useAutolayout, selectedNodeId);

        } catch (err) {
            this.handleRenderProcessGraphFailed(processViewModel.id, err);
        }
    }

    public openDialog() {
        this.processViewModel.dialogManager.openDialog(1, 0);
    }

    private resetBeforeLoad() {
        if (this.graph != null) {
            this.graph.destroy();
            this.graph = null;
        } 
    }

    public destroy() {
        // tear down persistent objects and event handlers
        if (this.graph != null) {
            this.graph.destroy();
            this.graph = null;
        } 
        if (this.processViewModel != null) {
            this.processViewModel.destroy();
            this.processViewModel = null;
        }
    }

    private dumpDebugInformation(model: IProcess): void {
        if (window.console && console.log) {
            //let output:string[] = [];
            if (model.shapes) {
                for (let s of model.shapes) {
                    console.log(`shape: id: ${s.id}, type: ${s.propertyValues["clientType"].value} at (x: ${s.propertyValues["x"].value}, y: ${s.propertyValues["y"].value})`);
                    //output.push(`shape: id: ${s.id}, type: ${s.propertyValues["clientType"].value} at (x: ${s.propertyValues["x"].value}, y: ${s.propertyValues["y"].value})`);
                }
            }

            if (model.links) {
                for (let l of model.links) {
                    console.log(`link: sourceId: ${l.sourceId}, destinationId: ${l.destinationId}, orderIndex: ${l.orderindex}`);
                    //output.push(`link: sourceId: ${l.sourceId}, destinationId: ${l.destinationId}, orderIndex: ${l.orderindex}`);
                }
            }

            if (model.decisionBranchDestinationLinks) {
                for (let b of model.decisionBranchDestinationLinks) {
                    console.log(`condition destinations: sourceId: ${b.sourceId}, destinationId: ${b.destinationId}, orderIndex: ${b.orderindex}`);
                    //output.push(`condition destinations: sourceId: ${b.sourceId}, destinationId: ${b.destinationId}, orderIndex: ${b.orderindex}`);
                }
            }
            //this.debugInformation = output;//.join("<br>");
        }
    }

    private handleInitProcessGraphFailed(processId: number, err: any) {
        this.messageService.addMessage(new Message(
            MessageType.Error, "There was an error initializing the process graph."));
        this.$log.error("Fatal: cannot initialize process graph for process " + processId);
        this.$log.error("Error: " + err.message);
    }

    private handleRenderProcessGraphFailed(processId: number, err: any) {
        this.messageService.addMessage(new Message(
            MessageType.Error, "There was an error displaying the process graph."));
        this.$log.error("Fatal: cannot render process graph for process " + processId);
        this.$log.error("Error: " + err.message);
    }
}