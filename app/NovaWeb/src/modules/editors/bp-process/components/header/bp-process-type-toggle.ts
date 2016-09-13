import {ICommunicationManager} from "../../";
import {ILocalizationService} from "../../../../core/localization";

export class BpProcessTypeToggle implements ng.IComponentOptions {
    public template: string = require("./bp-process-type-toggle.html");
    public controller: Function = BpProcessTypeToggleController;
    public controllerAs: string = "$ctrl";
    public bindings: any = {
        options: "<"
    };
}

export interface IProcessTypeToggleOption {
    id: number;
    iconClass: string;
    tooltip: string;
}

export class BpProcessTypeToggleController implements ng.IComponentController {
    public static $inject: [string] = [
        "communicationManager",
        "localization"
    ];

    public options: IProcessTypeToggleOption[];
    public currentProcessType: number;
    public isProcessTypeToggleEnabled: boolean;

    constructor(
        private communicationManager: ICommunicationManager,
        private localization: ILocalizationService
    ) {
        this.options = [
            { id: 1, iconClass: "fonticon2-user-user", tooltip: this.localization.get("ST_ProcessType_BusinessProcess_Label") },
            { id: 2, iconClass: "fonticon2-user-system", tooltip: this.localization.get("ST_ProcessType_UserToSystemProcess_Label") }
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