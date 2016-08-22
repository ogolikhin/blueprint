import { ILocalizationService } from "../../../core";
import { ISelectionManager, Models} from "../../../main";
import { IArtifactAttachmentsResultSet, IArtifactAttachments } from "./artifact-attachments.svc";
import { IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import { BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import { Helper } from "../../../shared/utils/helper";

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
        "selectionManager",
        "artifactAttachments"
    ];

    public artifactAttachmentsList: IArtifactAttachmentsResultSet;
    public addOptions: IAddOptions[];
    public categoryFilter: number;
    public isLoading: boolean = false;
    
    constructor(
        private localization: ILocalizationService,
        protected selectionManager: ISelectionManager,
        private artifactAttachments: IArtifactAttachments,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super(selectionManager, bpAccordionPanel);

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

    protected onSelectionChanged = (artifact: Models.IArtifact, subArtifact: Models.ISubArtifact) => {
        this.artifactAttachmentsList = null;

        if (Helper.canUtilityPanelUseSelectedArtifact(artifact)) {
            this.getAttachments(artifact.id, subArtifact ? subArtifact.id : null)
                .then((result: IArtifactAttachmentsResultSet) => {
                    this.artifactAttachmentsList = result;
                });
        } else {
            this.artifactAttachmentsList = null;
        }
    }

    private getAttachments(artifactId: number, subArtifactId: number = null): ng.IPromise<IArtifactAttachmentsResultSet> {
        this.isLoading = true;
        return this.artifactAttachments.getArtifactAttachments(artifactId, subArtifactId)
            .then( (result: IArtifactAttachmentsResultSet) => {
                return result;
            })
            .finally( () => {
                this.isLoading = false;
            });
    }
}
