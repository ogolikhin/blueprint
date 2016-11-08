import {IArtifactAttachment} from "../../../../managers/artifact-manager";
import {Models} from "../../../../main";
import {ISelectionManager} from "../../../../managers";
import {FiletypeParser} from "../../../../shared/utils/filetypeParser";
import {ILocalizationService} from "../../../../core/localization/localizationService";

export class BPArtifactAttachmentItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-attachment-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPArtifactAttachmentItemController;
    public bindings: any = {
        attachmentInfo: "=",
        deleteItem: "&",
        canChangeAttachments: "="
    };
}

interface IBPArtifactAttachmentItemController {
    attachmentInfo: IArtifactAttachment;
    deleteItem: Function;
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
    public deleteItem: Function;
    public canChangeAttachments: boolean;

    constructor(private $log: ng.ILogService,
                private localization: ILocalizationService,
                private selectionManager: ISelectionManager,
                private $window: ng.IWindowService) {
    }

    public $onInit() {
        this.fileIconClass = FiletypeParser.getFiletypeClass(this.attachmentInfo.fileName);
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
