import { ILocalizationService } from "../../../../core";
import { IArtifactDocRef, IArtifactAttachments, IArtifactAttachmentsResultSet } from "../../../../shell";
// import { FiletypeParser } from "../../../../core/utils/filetypeParser";

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
        "$window"
    ];

    public fileIconClass: string;
    public docRefInfo: IArtifactDocRef;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private artifactAttachments: IArtifactAttachments,
        private $window: ng.IWindowService) {
    }

    public $onInit(o) {
        this.fileIconClass = "ext-document"; //FiletypeParser.getFiletypeClass(null);
    }

    public deleteAttachment() {
        alert("deleting attachment");
    }
    
    public downloadAttachment() {
        return this.artifactAttachments.getArtifactAttachments(this.docRefInfo.artifactId)
            .then( (attachmentResultSet: IArtifactAttachmentsResultSet) => {

                if (attachmentResultSet.attachments.length) {
                    this.$window.open(
                        `/svc/components/RapidReview/artifacts/${attachmentResultSet.artifactId}/files/${attachmentResultSet.attachments[0].attachmentId}`,
                        "_blank");
                } else {
                    alert("sorry there are no attachments available");
                }
            });
    }
}
