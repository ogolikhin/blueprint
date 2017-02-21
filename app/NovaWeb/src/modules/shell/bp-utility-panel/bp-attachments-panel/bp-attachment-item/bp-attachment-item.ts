import {IDownloadService} from "../../../../commonModule/download";
import {IArtifactAttachment} from "../../../../managers/artifact-manager";
import {ISelectionManager} from "../../../../managers";
import {FiletypeParser} from "../../../../shared/utils/filetypeParser";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";

export class BPAttachmentItem implements ng.IComponentOptions {
    public template: string = require("./bp-attachment-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPAttachmentItemController;
    public bindings: any = {
        attachmentInfo: "=",
        deleteItem: "&",
        canChangeAttachments: "="
    };
}

interface IBPAttachmentItemController {
    attachmentInfo: IArtifactAttachment;
    deleteItem: Function;
}

export class BPAttachmentItemController implements IBPAttachmentItemController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "selectionManager",
        "downloadService"
    ];

    public fileIconClass: string;
    public attachmentInfo: IArtifactAttachment;
    public deleteItem: Function;
    public canChangeAttachments: boolean;

    constructor(private $log: ng.ILogService,
                private localization: ILocalizationService,
                private selectionManager: ISelectionManager,
                private downloadService: IDownloadService) {
    }

    public $onInit() {
        this.fileIconClass = FiletypeParser.getFiletypeClass(this.attachmentInfo.fileName);
    }

    public downloadItem(): void {
        const artifact = this.selectionManager.getArtifact();
        let url: string = "";

        if (this.attachmentInfo.guid) {
            url = `/svc/bpfilestore/file/${this.attachmentInfo.guid}`;
        } else {
            url = `/svc/bpartifactstore/artifacts/${artifact.id}/attachments/${this.attachmentInfo.attachmentId}`;
            if (artifact.artifactState.historical) {
                url += `?versionId=${artifact.getEffectiveVersion()}`;
            }
        }

        this.downloadService.downloadFile(url);
    }
}
