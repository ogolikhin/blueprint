import {ICommunicationManager} from "../../";
import {ILocalizationService} from "../../../../core/localization";
import {ProcessType} from "../../models/enums";

export class BpProcessTypeToggle implements ng.IComponentOptions {
    public template: string = require("./bp-process-type-toggle.html");
    public controller: Function = BpProcessTypeToggleController;
    public controllerAs: string = "$ctrl";
}

export interface IProcessTypeToggleOption {
    id: ProcessType;
    iconClass: string;
    tooltip: string;
}

export class BpProcessTypeToggleController implements ng.IComponentController {
    public static $inject: [string] = [
        "communicationManager",
        "localization"
    ];

    public options: IProcessTypeToggleOption[];
    public currentProcessType: ProcessType;
    public isProcessTypeToggleEnabled: boolean;

    constructor(
        private communicationManager: ICommunicationManager,
        private localization: ILocalizationService
    ) {
        this.options = [
            { 
                id: ProcessType.BusinessProcess, 
                iconClass: "fonticon2-user-user", 
                tooltip: this.localization.get("ST_ProcessType_BusinessProcess_Label")
            },
            { 
                id: ProcessType.UserToSystemProcess, 
                iconClass: "fonticon2-user-system", 
                tooltip: this.localization.get("ST_ProcessType_UserToSystemProcess_Label") 
            }
        ];
    }

    public $onInit() {
        this.communicationManager.toolbarCommunicationManager.registerEnableProcessTypeToggleObserver(this.onEnableProcessTypeToggle);
    }

    public $onDestroy() {
        this.communicationManager.toolbarCommunicationManager.removeEnableProcessTypeToggleObserver(this.onEnableProcessTypeToggle);
    }

    public processTypeChanged() {
        this.communicationManager.toolbarCommunicationManager.toggleProcessType(this.currentProcessType);
    }

    private onEnableProcessTypeToggle = (status: any) => {
        for (let i = 0; i < this.options.length; i++) {
            let option = this.options[i];
            
            if (option.id === status.processType) {
                this.currentProcessType = option.id;
            }
        }

        this.isProcessTypeToggleEnabled = status.value;
    }
}