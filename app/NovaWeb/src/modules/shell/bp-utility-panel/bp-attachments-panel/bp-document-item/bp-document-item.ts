import {IDownloadService} from "../../../../commonModule/download";
import {
    IArtifactDocRef,
    IArtifactAttachmentsService,
    IArtifactAttachmentsResultSet
} from "../../../../managers/artifact-manager";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
import {IMessageService} from "../../../../main/components/messages/message.svc";

export class BPDocumentItem implements ng.IComponentOptions {
    public template: string = require("./bp-document-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPDocumentItemController;
    public bindings: any = {
        docRefInfo: "<",
        deleteItem: "&",
        canChangeAttachments: "<"
    };
}

interface IBPAttachmentItemController {
    docRefInfo: IArtifactDocRef;
    deleteItem: Function;
}

export class BPDocumentItemController implements IBPAttachmentItemController {
    public fileIconClass: string;
    public docRefInfo: IArtifactDocRef;
    public deleteItem: Function;
    public canChangeAttachments: boolean;

    public static $inject: [string] = [
        "$log",
        "localization",
        "artifactAttachments",
        "messageService",
        "downloadService"
    ];

    constructor(private $log: ng.ILogService,
                private localization: ILocalizationService,
                private artifactAttachments: IArtifactAttachmentsService,
                private messageService: IMessageService,
                private downloadService: IDownloadService) {
    }

    public $onInit() {
        this.fileIconClass = "ext-document";
    }

    public downloadItem(): ng.IPromise<any> {
        const isHistorical = this.isHistoricalVersion(this.docRefInfo);
        return this.artifactAttachments.getArtifactAttachments(this.docRefInfo.artifactId, null, isHistorical ? this.docRefInfo.versionId : null)
            .then((attachmentResultSet: IArtifactAttachmentsResultSet) => {
                if (attachmentResultSet.attachments.length) {
                    const artifactId = attachmentResultSet.artifactId;
                    const attachmentId = attachmentResultSet.attachments[0].attachmentId;
                    let url = `/svc/bpartifactstore/artifacts/${artifactId}/attachments/${attachmentId}`;
                    if (isHistorical) {
                        url += `?versionId=${this.docRefInfo.versionId}`;
                    }
                    this.downloadService.downloadFile(url);
                } else {
                    this.messageService.addError(this.localization.get("App_UP_Attachments_Download_No_Attachment"));
                }
            });
    }

    private isHistoricalVersion(docRefInfo: IArtifactDocRef): boolean {
        return _.isFinite(this.docRefInfo.versionId)
            && _.isFinite(this.docRefInfo.versionsCount)
            && this.docRefInfo.versionId !== this.docRefInfo.versionsCount
            // not draft version
            && !(this.docRefInfo.versionsCount === 0 && this.docRefInfo.versionId <= 0);
    }
}
