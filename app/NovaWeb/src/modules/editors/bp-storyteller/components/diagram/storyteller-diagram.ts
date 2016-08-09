// References to StorytellerDiagram
import {IProcess} from "../../models/processModels";
import {IProcessService} from "../../services/process/process.svc";
import {IMessageService} from "../../../../core";
import {ProcessViewModel, IProcessViewModel} from "./viewmodel/process-viewmodel";
import {IProcessGraph} from "./presentation/graph/process-graph-interfaces";
import {ProcessGraph} from "./presentation/graph/process-graph";


export class StorytellerDiagram {
    public processModel: IProcess;
    public processViewModel: IProcessViewModel = null;
    private graph: IProcessGraph = null;

    constructor(
        private $rootScope: ng.IRootScopeService,
        private $scope: ng.IScope,
        private $state: ng.ui.IState,
        private $timeout: ng.ITimeoutService,
        private $q: ng.IQService,
        private $log: ng.ILogService,
        private processService: IProcessService,
        private messageService: IMessageService) {

        this.processModel = null;
    }

    public debugInformation: string;
    public createDiagram(processId: string) {
        // retrieve the specified process from the server and 
        // create a new diagram

        //const storytellerParams = this.bpUrlParsingService.getStateParams();   

        this.processService.load(processId//,
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
    private onLoad(process: IProcess, useAutolayout: boolean = false, selectedNodeId: number = undefined) {

        this.processModel = process;

        this.debugInformation = "PROCESS LOADED";
        this.dumpDebugInformation(this.processModel);
        let processViewModel = this.createProcessViewModel(process);
        //if (processViewModel.isReadonly) this.disableStorytellerToolbar();
        this.createGraph(processViewModel, useAutolayout, selectedNodeId);
    }

    private createProcessViewModel(process: IProcess): IProcessViewModel {
        if (this.processViewModel == null) {
            this.processViewModel = new ProcessViewModel(process, this.$rootScope, this.$scope, this.messageService);
        } else {
            this.processViewModel.updateProcessClientModel(process);
        }
        return this.processViewModel;
    }

    private createGraph(processViewModel: IProcessViewModel, useAutolayout: boolean = false, selectedNodeId: number = undefined) {

        // create a new process graph and render it
        try {
            if (this.graph) {
                this.graph.destroy();
                this.graph = null;
            }
            this.graph = new ProcessGraph(
                            this.$rootScope,
                            this.$scope,
                            this.processService,
                            processViewModel,
                            this.messageService,
                            this.$log);

            this.graph.render(useAutolayout, selectedNodeId);

        } catch (err) {
            this.messageService.addError("Cannot create graph: " + err.message);
            this.$log.error("Fatal: cannot render graph for process " + processViewModel.id);
            this.$log.error("Error: " + err.message);
        }
    }

    public destroy() {
        // tear down embedded objects and event handlers
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
}