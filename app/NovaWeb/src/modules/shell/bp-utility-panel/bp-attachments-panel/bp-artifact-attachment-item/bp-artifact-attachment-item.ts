import { ILocalizationService } from "../../../../core";
import { IArtifactAttachment } from "../../../../managers/artifact-manager";
import { Models } from "../../../../main";
import { ISelectionManager } from "../../../../managers";
import { FiletypeParser } from "../../../../shared/utils/filetypeParser";

export class BPArtifactAttachmentItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-attachment-item.html");
    public controller: Function = BPArtifactAttachmentItemController;
    public bindings: any = {
        attachmentInfo: "="
    };
}

interface IBPArtifactAttachmentItemController {
    attachmentInfo: IArtifactAttachment;
}

export class BPArtifactAttachmentItemController implements IBPArtifactAttachmentItemController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "selectionManager",
        "$window"
    ];

    public fileIconClass: string;
    public attachmentInfo: IArtifactAttachment;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private selectionManager: ISelectionManager,
        private $window: ng.IWindowService) {
    }

    public $onInit(o) {
        this.fileIconClass = FiletypeParser.getFiletypeClass(this.attachmentInfo.fileName);
    }

    public deleteItem(): void {
        alert("deleting attachment");
    }
    
    public downloadItem(): void {
        const artifact: Models.IArtifact = this.selectionManager.getArtifact();
        let url: string = "";

        if (this.attachmentInfo.guid) {
            url = `/svc/bpfilestore/file/${this.attachmentInfo.guid}`;
        } else {
            url = `/svc/components/RapidReview/artifacts/${artifact.id}/files/${this.attachmentInfo.attachmentId}?includeDraft=true`;
        }

        this.$window.open(url, "_blank");
    }
}
