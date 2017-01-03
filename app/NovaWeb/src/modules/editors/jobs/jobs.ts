import moment = require("moment");
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
import {IJobsService, IJobInfo, JobStatus, JobType} from "./jobs.svc";
import {JobAction} from "./jobAction";

export class JobsComponent implements ng.IComponentOptions {
    public template: string = require("./jobs.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = JobsController;
}


export class JobsController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "messageService",
        "jobsService",
        "loadingOverlayService",
        "navigationService",
        "projectManager"
    ];

    public jobs: IJobInfo[];
    public toolbarActions: IBPAction[];
    public isLoading: boolean;
    public page: number;
    public pageLength: number;
 
    constructor(private $log: ng.ILogService,
                public localization: ILocalizationService,
                public messageService: IMessageService,
                private jobsService: IJobsService,
                private loadingOverlayService: ILoadingOverlayService,
                private navigationService: INavigationService,
                private projectManager: IProjectManager) {
        this.toolbarActions = [];
        this.page = 1;
        this.pageLength = 10;
    }

    public $onInit() {
        this.loadPage(1);
    };

    private loadNextPage() {
        this.loadPage(this.page + 1);
    }
    
    private getJobAction(status: JobStatus): JobAction {
        let jobAction = JobAction.Error;
        switch (status) {
            case JobStatus.Completed:
                jobAction = JobAction.Completed;
                break;
            case JobStatus.Cancelling:
            case JobStatus.Running:
            case JobStatus.Scheduled:
            case JobStatus.Suspending:
                jobAction = JobAction.Running;
                break;
            default:
                jobAction = JobAction.Error;
                break;            
        }
        return jobAction;
    }

    public canDownload(status: JobStatus): boolean {
        return this.getJobAction(status) === JobAction.Completed;
    }
    
    public canRefresh(status: JobStatus): boolean {
        return this.getJobAction(status) === JobAction.Running;
    }

    public isJobError(status: JobStatus): boolean {
        return this.getJobAction(status) === JobAction.Error;
    }

    public refreshJob(jobId: number) {
        this.jobsService.getJob(jobId).then((result: IJobInfo) => {
            const index = _.indexOf(this.jobs, _.find(this.jobs, {jobId: result.jobId}));
            result.userDisplayName = undefined;
            _.merge(this.jobs[index], result);
        });
    }

    private loadPage(page: number) {
        this.isLoading = true;
        this.page = page;
        this.jobs = [];
        this.jobsService.getJobs(page)
        .then((result: IJobInfo[]) => {
            this.jobs = result;
        })
        .finally(() => {
            this.isLoading = false;
        });
    } 
    
    private getDate(date: Date): string {
        if (!!date) {
            return moment(date).format("MMMM DD, YYYY");
        }
        return "--";
    }

    private getStatus(statusId: JobStatus): string {
        switch (statusId) {
            case JobStatus.Scheduled:
                return this.localization.get("Jobs_Status_Scheduled"); 
            case JobStatus.Terminated: 
                return this.localization.get("Jobs_Status_Terminated"); 
            case JobStatus.Running: 
                return this.localization.get("Jobs_Status_Running"); 
            case JobStatus.Completed:  
                return this.localization.get("Jobs_Status_Completed"); 
            case JobStatus.Failed:  
                return this.localization.get("Jobs_Status_Failed"); 
            case JobStatus.Cancelling:  
                return this.localization.get("Jobs_Status_Cancelling"); 
            case JobStatus.Suspending:  
                return this.localization.get("Jobs_Status_Suspending"); 
            case JobStatus.Suspended: 
                return this.localization.get("Jobs_Status_Suspended"); 
        }
        this.$log.error(`Unknown Job Status, (${statusId})`);
        return "Unknown Status";
    } 

    private getType(typeId: JobType): string {
        switch (typeId) {
            case JobType.None:
                return this.localization.get("Jobs_Type_None"); 
            case JobType.System:
                return this.localization.get("Jobs_Type_System"); 
            case JobType.DocGen:
                return this.localization.get("Jobs_Type_DocGen"); 
            case JobType.TfsExport:
                return this.localization.get("Jobs_Type_TfsExport"); 
            case JobType.QcExport:
                return this.localization.get("Jobs_Type_QcExport"); 
            case JobType.HpAlmRestExport:
                return this.localization.get("Jobs_Type_HpAlmRestExport"); 
            case JobType.TfsChangeSummary:
                return this.localization.get("Jobs_Type_TfsChangeSummary"); 
            case JobType.QcChangeSummary:
                return this.localization.get("Jobs_Type_QcChangeSummary"); 
            case JobType.HpAlmRestChangeSummary:
                return this.localization.get("Jobs_Type_HpAlmRestChangeSummary"); 
            case JobType.TfsExportTests:
                return this.localization.get("Jobs_Type_TfsExportTests"); 
            case JobType.QcExportTests:
                return this.localization.get("Jobs_Type_QcExportTests"); 
            case JobType.HpAlmRetExportTests:
                return this.localization.get("Jobs_Type_HpAlmRetExportTests"); 
            case JobType.ExcelImport:
                return this.localization.get("Jobs_Type_ExcelImport"); 
            case JobType.ProjectImport:
                return this.localization.get("Jobs_Type_ProjectImport"); 
            case JobType.ProjectExport:
                return this.localization.get("Jobs_Type_ProjectExport"); 
            case JobType.GenerateTests:           
                return this.localization.get("Jobs_Type_GenerateTests"); 
        }

        this.$log.error(`Unknown Job Type, (${typeId})`);
        return "Unknown";
    }

    public isJobsEmpty(): boolean {
        return this.jobs.length === 0;
    }

    public $onDestroy() {
        // not implemented
    }
}
