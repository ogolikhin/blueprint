import {IStateManager, ItemState} from "../../../../core";
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
    value: string;
    iconClass: string;
    isChecked: boolean;
    isEnabled: boolean;
}

export class ProcessTypeToggleOption implements IProcessTypeToggleOption {
    constructor(
        private _id: number, 
        private _value: string, 
        private _iconClass: string, 
        private _isChecked: boolean, 
        private _isEnabled: boolean,
        private communicationManager?: ICommunicationManager) {
    }

    public get id(): number {
        return this._id;
    }

    public get value(): string {
        return this._value;
    }

    public get iconClass(): string {
        return this._iconClass;
    }

    public get isChecked(): boolean {
        return this._isChecked;
    }

    public set isChecked(value: boolean) {
        this._isChecked = value;

        if (this._isChecked && this.communicationManager) {
            this.communicationManager.toolbarCommunicationManager.toggleProcessType(this.id);
        }
    }

    public get isEnabled(): boolean {
        return this._isEnabled;
    }

    public set isEnabled(value: boolean) {
        this._isEnabled = value;
    }
}

export class BpProcessTypeToggleController implements ng.IComponentController {
    public static $inject: [string] = [
        "communicationManager"
    ];

    public options: IProcessTypeToggleOption[];

    constructor(
        private communicationManager: ICommunicationManager
    ) {
        this.options = [
            new ProcessTypeToggleOption(1, "business", "fonticon2-user-user", false, true, communicationManager),
            new ProcessTypeToggleOption(2, "userToSystem", "fonticon2-user-system", false, true, communicationManager)
        ];
    }

    public $onInit() {
        this.communicationManager.toolbarCommunicationManager.registerEnableProcessTypeToggleObserver(this.onEnableProcessTypeToggle);
    }

    public $onDestroy() {
        this.communicationManager.toolbarCommunicationManager.removeEnableProcessTypeToggleObserver(this.onEnableProcessTypeToggle);
    }

    private onEnableProcessTypeToggle = (status: any) => {
        for (let i = 0; i < this.options.length; i++) {
            let option = this.options[i];
            option.isEnabled = status.value;
            option.isChecked = status.processType === option.id
        }
    }
}