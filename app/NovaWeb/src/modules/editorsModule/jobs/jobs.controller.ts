import moment = require("moment");
import {IPaginationData} from "../../main/components/pagination/model";
import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {IBPAction} from "../../shared/widgets/bp-toolbar/actions/bp-action";
import {ILoadingOverlayService} from "../../commonModule/loadingOverlay/loadingOverlay.service";
import {INavigationService} from "../../commonModule/navigation/navigation.service";
import {IJobsService} from "./jobs.service";
import {IJobInfo, IJobResult, JobStatus, JobType} from "./model/models";
import {JobAction} from "./jobAction";
import {IMessageService} from "../../main/components/messages/message.svc";
import {IDownloadService} from "../../commonModule/download/download.service";

export class JobsController {
    public jobs: IJobInfo[];
    public toolbarActions: IBPAction[];
    public isLoading: boolean;
    public paginationData: IPaginationData;

    public static $inject: [string] = [
        "$log",
        "$window",
        "localization",
        "messageService",
        "jobsService",
        "loadingOverlayService",
        "navigationService",
        "downloadService"
    ];

    constructor(
        private $log: ng.ILogService,
        private $window: ng.IWindowService,
        public localization: ILocalizationService,
        public messageService: IMessageService,
        private jobsService: IJobsService,
        private loadingOverlayService: ILoadingOverlayService,
        private navigationService: INavigationService,
        private downloadService: IDownloadService
    ) {
        this.toolbarActions = [];

        this.paginationData = {
            page: 1,
            pageSize: 10,
            total: 0,
            maxVisiblePageCount: 10
        };
    }

    public $onInit() {
        this.loadPage(1);
    };

    public loadNextPage() {
        this.loadPage(this.paginationData.page + 1);
    }

    private getJobAction(job: IJobInfo): JobAction {
        let jobAction = JobAction.None;

        switch (job.status) {
            case JobStatus.Completed:
                switch (job.jobType) {
                    case JobType.GenerateProcessTests:
                    case JobType.ProjectExport:
                        jobAction = JobAction.Download;
                        break;
                    default:
                        jobAction = JobAction.None;
                        break;
                }
                break;
            case JobStatus.Cancelling:
            case JobStatus.Running:
            case JobStatus.Scheduled:
            case JobStatus.Suspending:
                jobAction = JobAction.Refresh;
                break;
            default:
                jobAction = JobAction.None;
                break;
        }

        return jobAction;
    }

    public canDownload(job: IJobInfo): boolean {
        return this.getJobAction(job) === JobAction.Download;
    }

    public canRefresh(job: IJobInfo): boolean {
        return this.getJobAction(job) === JobAction.Refresh;
    }

    public refreshJob(job: IJobInfo): void {
        if (!this.canRefresh(job)) {
            return;
        }

        this.jobsService.getJob(job.jobId)
            .then((result: IJobInfo) => {
                // refresh job does not return user display name in the stored procedure, so api returns this property as null.
                // Returned value from server needs to be undefined for _.merge() to not overwrite the previous value.
                result.userDisplayName = undefined;
                _.merge(job, result);
            });
    }

    public downloadItem(job: IJobInfo): void {
        if (!this.canDownload(job)) {
            return;
        }

        const url = `/svc/adminstore/jobs/${job.jobId}/result/file`;

        this.downloadService.downloadFile(url);
    }

    public loadPage(page: number) {
        this.isLoading = true;
        this.paginationData.page = page;
        this.jobs = [];
        this.jobsService.getJobs(page, this.paginationData.pageSize)
            .then((result: IJobResult) => {
                this.jobs = result.jobInfos;
                // When user's navigating through pages, if last page returns 0 results, the total count is not returned. In that case when we're
                // not on the first page, and server returns 0 total jobs, we don't update it to keep pagination control for user to go back.
                if (result.totalJobCount || this.isFirstPage()) {
                    this.paginationData.total = result.totalJobCount;
                }
            })
            .finally(() => {
                this.isLoading = false;
            });
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
            default:
                this.$log.error(`Unknown job status: '${statusId}'`);
                return "Unknown Status";
        }
    }

    private isValidStatus(statusId: JobStatus): boolean {
        return statusId === JobStatus.Scheduled ||
               statusId === JobStatus.Completed ||
               statusId === JobStatus.Running;
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
            case JobType.GenerateProcessTests:
                return this.localization.get("Jobs_Type_GenerateProcessTests");

            default:
                this.$log.error(`Unknown job type: '${typeId}'`);
                return "Unknown";
        }
    }

    public isJobRunning(status: JobStatus): boolean {
        if (status === JobStatus.Running) {
            return true;
        }
        return false;
    }

    public isJobsEmpty(): boolean {
        return this.jobs.length === 0;
    }

    public canShowPagination(): boolean {
        return !this.isLoading && (!this.isFirstPage() || this.containsMoreThanOnePage());
    }

    private isFirstPage(): boolean {
        return this.paginationData.page === 1;
    }

    private containsMoreThanOnePage(): boolean {
        return this.paginationData.total > this.paginationData.pageSize;
    }

    public $onDestroy() {
        // not implemented
    }
}
