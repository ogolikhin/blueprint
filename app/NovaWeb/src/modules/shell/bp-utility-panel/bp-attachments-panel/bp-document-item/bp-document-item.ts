import {
    IArtifactDocRef,
    IArtifactAttachmentsService,
    IArtifactAttachmentsResultSet
} from "../../../../managers/artifact-manager";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {IMessageService} from "../../../../core/messages/message.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class BPDocumentItem implements ng.IComponentOptions {
    public template: string = require("./bp-document-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPDocumentItemController;
    public bindings: any = {
        docRefInfo: "=",
        deleteItem: "&",
        canChangeAttachments: "="
    };
}

interface IBPAttachmentItemController {
    docRefInfo: IArtifactDocRef;
    deleteItem: Function;
}

export class BPDocumentItemController implements IBPAttachmentItemController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "artifactAttachments",
        "messageService",
        "navigationService",
        "$window"
    ];

    public fileIconClass: string;
    public docRefInfo: IArtifactDocRef;
    public deleteItem: Function;
    public canChangeAttachments: boolean;

    constructor(private $log: ng.ILogService,
                private localization: ILocalizationService,
                private artifactAttachments: IArtifactAttachmentsService,
                private messageService: IMessageService,
                private navigationService: INavigationService,
                private $window: ng.IWindowService) {
    }

    public $onInit() {
        this.fileIconClass = "ext-document"; //FiletypeParser.getFiletypeClass(null);
    }

    public downloadItem(): ng.IPromise<any> {
        let isHistorial = this.isHistoricalVersion(this.docRefInfo);
        return this.artifactAttachments.getArtifactAttachments(this.docRefInfo.artifactId, null, isHistorial ? this.docRefInfo.versionId : null)
            .then((attachmentResultSet: IArtifactAttachmentsResultSet) => {
                if (attachmentResultSet.attachments.length) {
                    const artifactId = attachmentResultSet.artifactId;
                    const attachmentId = attachmentResultSet.attachments[0].attachmentId;
                    let url = `/svc/bpartifactstore/artifacts/${artifactId}/attachments/${attachmentId}`;
                    if (isHistorial) {
                        url += `?versionId=${this.docRefInfo.versionId}`;
                    }
                    this.$window.open(url, "_blank");
                } else {
                    this.messageService.addError(this.localization.get("App_UP_Attachments_Download_No_Attachment"));
                }
            });
    }

    private isHistoricalVersion(docRefInfo: IArtifactDocRef): boolean {
        return _.isFinite(this.docRefInfo.versionId) 
            && _.isFinite(this.docRefInfo.versionsCount)
            && this.docRefInfo.versionId !== this.docRefInfo.versionsCount;
    }

    public navigateToDocumentReference(artifactId: number) {
        this.navigationService.navigateTo({id: artifactId});
    }
}
