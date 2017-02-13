import * as _ from "lodash";
import {ILocalizationService} from "../../../commonModule/localization/localization.service";
export enum DialogTypeEnum {
    Base,
    Alert,
    Confirm
}

export interface IDialogSettings {
    type?: DialogTypeEnum;
    header?: string;
    message?: string;
    cancelButton?: string;
    okButton?: string;
    template?: string;
    controller?: any;
    css?: string;
    backdrop?: boolean;
    controllerAs?: string;
    bindToController?: boolean;
    windowClass?: string;
}

export interface IDialogService {
    open(dialogSettings: IDialogSettings, dialogData?): ng.IPromise<any>;
    alert(message: string, header?: string, okButton?: string, cancelButton?: string): ng.IPromise<any>;
    confirm(message: string, header?: string, css?: string): ng.IPromise<any>;
    dialogSettings: IDialogSettings;
}

export class DialogService implements IDialogService {
    public dialogSettings: IDialogSettings = {};
    public dialogData: any;
    private defaultSettings: IDialogSettings;

    public static $inject = ["localization",
        "$uibModal",
        "$timeout",
        "$document"];

    constructor(private localization: ILocalizationService,
                private $uibModal: ng.ui.bootstrap.IModalService,
                private $timeout: ng.ITimeoutService,
                private $document: ng.IDocumentService) {
        this.defaultSettings = {
            type: DialogTypeEnum.Base,
            cancelButton: this.localization.get("App_Button_Cancel", "Cancel"),
            okButton: this.localization.get("App_Button_Ok", "Ok"),
            template: require("./bp-dialog.html"),
            controller: BaseDialogController,
            controllerAs: "$ctrl",
            bindToController: true,
            windowClass: "nova-messaging",
            backdrop: false
        };

    }

    private openInternal = (optsettings?: ng.ui.bootstrap.IModalSettings) => {
        this.dialogSettings.cancelButton = this.localization.get(this.dialogSettings.cancelButton) || null;
        this.dialogSettings.okButton = this.localization.get(this.dialogSettings.okButton) || null;
        this.dialogSettings.header = this.localization.get(this.dialogSettings.header) || null;
        this.dialogSettings.message = this.localization.get(this.dialogSettings.message) || null;
        const options = _.assign({},
            this.dialogSettings,
            optsettings,
            <ng.ui.bootstrap.IModalSettings>{
                windowClass: this.dialogSettings.css,
                resolve: {
                    dialogSettings: () => this.dialogSettings,
                    dialogData: () => this.dialogData
                }
            });

        const instance = this.$uibModal.open(options);
        instance.opened.then(() => {
            this.$timeout(() => {
                const modal = this.$document[0].getElementsByClassName("modal").item(0) as HTMLElement;
                modal.focus();
            });
        });
        return instance;
    };

    public get type(): DialogTypeEnum {
        return this.dialogSettings.type;
    }

    public open(dialogSettings?: IDialogSettings, dialogData?): ng.IPromise<any> {
        this.dialogSettings = _.assign({}, this.defaultSettings, dialogSettings);
        this.dialogData = dialogData ? dialogData : undefined;
        return this.openInternal().result;
    }

    public alert(message: string, header?: string, okButton?: string, cancelButton?: string) {
        const dialogSettings = <IDialogSettings>{
            type: DialogTypeEnum.Alert,
            header: this.localization.get(header || "App_DialogTitle_Alert"),
            message: message,
            cancelButton: this.localization.get(cancelButton || null), //Don't show cancel button if not defined
            css: "modal-alert nova-messaging"
        }  as IDialogSettings;
        if (okButton) {
            //We only want to set the okButton if it's specified, otherwise use the initialize default.
            dialogSettings.okButton = this.localization.get(okButton);
        }
        this.dialogSettings = _.assign({}, this.defaultSettings, dialogSettings);
        return this.openInternal(<ng.ui.bootstrap.IModalSettings>{
            keyboard: false
        }).result;
    }

    public confirm(message: string, header?: string, css?: string) {
        const dialogSettings = {
            type: DialogTypeEnum.Confirm,
            header: this.localization.get(header || "App_DialogTitle_Confirmation"),
            css: css || "nova-messaging",
            message: message
        } as IDialogSettings;
        this.dialogSettings = _.assign({}, this.defaultSettings, dialogSettings);

        return this.openInternal().result;
    }
}

export interface IDialogController {
    returnValue: any;
    ok: Function;
    cancel: Function;
}

/*fixme: one class per file*/
export class BaseDialogController implements IDialogController {

    public hasCloseButton: boolean = false;

    public get returnValue(): any {
        return undefined;
    }

    static $inject = ["$uibModalInstance", "dialogSettings"];

    constructor(public $instance: ng.ui.bootstrap.IModalServiceInstance,
                public dialogSettings: IDialogSettings) {
    }

    public ok() {
        this.$instance.close(this.returnValue);
    };

    public cancel() {
        this.$instance.dismiss("cancel");

        /*manual gargabe clean */
        this.$instance = null;
        this.dialogSettings = null;

    };
}
