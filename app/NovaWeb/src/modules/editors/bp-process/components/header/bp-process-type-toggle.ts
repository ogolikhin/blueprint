import {ICommunicationManager} from "../../";

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
}

export class BpProcessTypeToggleController implements ng.IComponentController {
    public static $inject: [string] = [
        "communicationManager"
    ];

    public options: IProcessTypeToggleOption[];
    public currentProcessType: number;

    constructor(
        private communicationManager: ICommunicationManager
    ) {
        this.options = [
            { id: 1, iconClass: "fonticon2-user-user" },
            { id: 2, iconClass: "fonticon2-user-system" }
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
    }
}