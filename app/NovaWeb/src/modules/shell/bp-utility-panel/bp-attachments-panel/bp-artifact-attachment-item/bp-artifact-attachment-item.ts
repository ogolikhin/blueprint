import { ILocalizationService } from "../../../../core";
import { IArtifactAttachment } from "../../../../shell";
import { IProjectManager, Models} from "../../../../main";
import { FiletypeParser } from "../../../../core/utils/filetypeParser";

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
        "projectManager",
        "$window"
    ];

    public fileIconClass: string;
    public attachmentInfo: IArtifactAttachment;
    
    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private projectManager: IProjectManager,
        private $window: ng.IWindowService) {
    }

    public $onInit(o) {
        this.fileIconClass = FiletypeParser.getFiletypeClass(this.attachmentInfo.fileName);
    }

    public deleteAttachment(): void {
        alert("deleting attachment");
    }
    
    public downloadAttachment(): void {
        let artifact: Models.IArtifact = this.projectManager.currentArtifact.getValue();
        this.$window.open(
                `/svc/components/RapidReview/artifacts/${artifact.id}/files/${this.attachmentInfo.attachmentId}`,
                "_blank");
    }
}
