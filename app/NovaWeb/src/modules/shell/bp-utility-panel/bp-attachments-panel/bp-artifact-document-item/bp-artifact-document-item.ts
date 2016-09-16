import { ILocalizationService, IMessageService } from "../../../../core";
import {
    IArtifactDocRef, 
    IArtifactAttachmentsService, 
    IArtifactAttachmentsResultSet
} from "../../../../managers/artifact-manager";

export class BPArtifactDocumentItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-document-item.html");
    public controller: Function = BPArtifactDocumentItemController;
    public bindings: any = {
        docRefInfo: "="
    };
}

interface IBPArtifactAttachmentItemController {
    docRefInfo: IArtifactDocRef;
}

export class BPArtifactDocumentItemController implements IBPArtifactAttachmentItemController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "artifactAttachments",
        "messageService",
        "$window"
    ];

    public fileIconClass: string;
    public docRefInfo: IArtifactDocRef;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private artifactAttachments: IArtifactAttachmentsService,
        private messageService: IMessageService,
        private $window: ng.IWindowService) {
    }

    public $onInit(o) {
        this.fileIconClass = "ext-document"; //FiletypeParser.getFiletypeClass(null);
    }

    public deleteItem() {
        alert("deleting attachment");
    }
    
    public downloadItem(): ng.IPromise<any> {
        return this.artifactAttachments.getArtifactAttachments(this.docRefInfo.artifactId)
            .then( (attachmentResultSet: IArtifactAttachmentsResultSet) => {

                if (attachmentResultSet.attachments.length) {
                    this.$window.open(
                        "/svc/components/RapidReview/artifacts/" + attachmentResultSet.artifactId
                        + "/files/" + attachmentResultSet.attachments[0].attachmentId + "?includeDraft=true",
                        "_blank");
                } else {
                    this.messageService.addError(this.localization.get("App_UP_Attachments_Download_No_Attachment"));
                }
            });
    }
}
