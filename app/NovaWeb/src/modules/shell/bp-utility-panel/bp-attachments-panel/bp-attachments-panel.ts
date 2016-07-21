import { ILocalizationService } from "../../../core";
import { IProjectManager, Models} from "../../../main";
import { IArtifactAttachmentsResultSet, IArtifactAttachments } from "./artifact-attachments.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";

interface IAddOptions {
    value: string;
    label: string;
}

export class BPAttachmentsPanel implements ng.IComponentOptions {
    public template: string = require("./bp-attachments-panel.html");
    public controller: Function = BPAttachmentsPanelController;
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
}

export class BPAttachmentsPanelController extends BPBaseUtilityPanelController {
    public static $inject: [string] = [
        "localization",
        "projectManager",
        "artifactAttachments"
    ];

    public artifactAttachmentsList: IArtifactAttachmentsResultSet;
    public addOptions: IAddOptions[];
    public categoryFilter: number;
    public isLoading: boolean = false;
    
    constructor(
        private localization: ILocalizationService,
        protected projectManager: IProjectManager,
        private artifactAttachments: IArtifactAttachments,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(projectManager, bpAccordionPanel);

        this.addOptions = [
            { value: "attachment", label: this.localization.get("App_UP_Attachments_Add_Attachment") },
            { value: "document", label: this.localization.get("App_UP_Attachments_Add_Ref_Doc") },
        ];
    }

    public $onInit() {
        super.$onInit();
    }

    public $onDestroy() {
        super.$onDestroy();
    }

    protected setArtifactId = (artifact: Models.IArtifact) => {
        this.artifactAttachmentsList = null;

        if (artifact !== null) {
            this.getAttachments(artifact.id)
                .then( (result: IArtifactAttachmentsResultSet) => {
                    this.artifactAttachmentsList = result;
                });
        }
    }

    private getAttachments(artifactId: number, subArtifactId: number = null): ng.IPromise<IArtifactAttachmentsResultSet> {
        this.isLoading = true;
        return this.artifactAttachments.getArtifactAttachments(artifactId)
            .then( (result: IArtifactAttachmentsResultSet) => {
                return result;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
