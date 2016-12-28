import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {IBPAction} from "../../shared/widgets/bp-toolbar/actions/bp-action";
import {BPButtonGroupAction} from "../../shared/widgets/bp-toolbar/actions/bp-button-group-action";
import {IArtifact, IPublishResultSet} from "../../main/models/models";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {DiscardArtifactsAction} from "../../main/components/bp-artifact-info/actions/discard-artifacts-action";
import {IProjectManager} from "../../managers/project-manager/project-manager";
import {INavigationService} from "../../core/navigation/navigation.svc";
import {ItemTypePredefined} from "../../main/models/enums";

export class JobsComponent implements ng.IComponentOptions {
    public template: string = require("./jobs.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = JobsController;
}


export class JobsController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "messageService",
        //"jobsService",
        "loadingOverlayService",
        "navigationService",
        "projectManager"
    ];
    public jobs: any[];
    public toolbarActions: IBPAction[];
    public isLoading: boolean;
 
    constructor(private $log: ng.ILogService,
                public localization: ILocalizationService,
                public messageService: IMessageService,
                //private jobsService: IJobsService,
                private loadingOverlayService: ILoadingOverlayService,
                private navigationService: INavigationService,
                private projectManager: IProjectManager) {
        this.toolbarActions = [];
        this.jobs = [];
    }

    public $onInit() {
        this.isLoading = false;
    };

    public noJobs(): boolean {
        return this.jobs.length === 0;
    }
     

    public $onDestroy() {
        // not implemented
    }
    
}
