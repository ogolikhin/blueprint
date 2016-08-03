// References to StorytellerDiagram
import {ProcessModels, IProcessService} from "../../"
import {IMessageService} from "../../../../core"
import {StorytellerViewModel, IStorytellerViewModel} from "./viewmodel/storyteller-view-model";

export class StorytellerDiagram {
    public processModel: ProcessModels.IProcess;
    public storytellerViewModel: IStorytellerViewModel = null;

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

    public debugInformation:string;
    public createDiagram(processId: string) {
        // retrieve the specified process from the server and 
        // create a new diagram

        //const storytellerParams = this.bpUrlParsingService.getStateParams();   

        this.processService.load(processId//,
            //storytellerParams.versionId,
            //storytellerParams.revisionId,
            //storytellerParams.baselineId,
            //storytellerParams.readOnly
        ).then((process: ProcessModels.IProcess) => {
            this.onLoad(process);
            //if (!this.storytellerViewModel.isReadonly) {
            //    this.storytellerViewModel.isWithinShapeLimit(0, true);
            //}
        }).catch((err: any) => {
            // if access to proccess info is forbidden
            if (err && err.statusCode === 403) {
                //handle errors if need to
            }
        });
    }
    private onLoad(process: ProcessModels.IProcess, useAutolayout: boolean = false, selectedNodeId: number = undefined) {
        this.processModel = process;

        this.debugInformation = "PROCESS LOADED";
        this.dumpDebugInformation(this.processModel);
        let storytellerViewModel = this.createProcessViewModel(process);
        //if (storytellerViewModel.isReadonly) this.disableStorytellerToolbar();
        //this.createGraph(storytellerViewModel, useAutolayout, selectedNodeId);
    }

    private createProcessViewModel(process: ProcessModels.IProcess): IStorytellerViewModel {
        if (this.storytellerViewModel == null) {
            this.storytellerViewModel = new StorytellerViewModel(process, this.$rootScope, this.$scope, this.messageService);
        } else {
            this.storytellerViewModel.updateProcessClientModel(process);
        }
        return this.storytellerViewModel;
    }
    private dumpDebugInformation(model: ProcessModels.IProcess): void {
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