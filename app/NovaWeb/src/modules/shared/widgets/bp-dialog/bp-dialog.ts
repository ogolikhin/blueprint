import * as _ from "lodash";
import {ILocalizationService} from "../../../core/localization/localizationService";
export enum DialogTypeEnum {
    Base,
    Alert,
    Confirm
}
/*fixme: what is the point of this?*/
export interface IDialogData {
}

/*fixme: why is everything optional? some values must be required*/
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
    open(dialogSettings: IDialogSettings, dialogData?: IDialogData): ng.IPromise<any>;
    alert(message: string, header?: string, okButton?: string, cancelButton?: string): ng.IPromise<any>;
    confirm(message: string, header?: string, css?: string): ng.IPromise<any>;
    dialogSettings: IDialogSettings;
}

export class DialogService implements IDialogService {

    public static $inject = ["localization", "$uibModal"];
    public dialogSettings: IDialogSettings = {};
    public dialogData: any;
    private defaultSettings: IDialogSettings;

    constructor(private localization: ILocalizationService, private $uibModal: ng.ui.bootstrap.IModalService) {
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
        const options = _.assign(
            this.dialogSettings,
            optsettings,
            <ng.ui.bootstrap.IModalSettings>{
                windowClass: this.dialogSettings.css,
                resolve: {
                    dialogSettings: () => this.dialogSettings,
                    dialogData: () => this.dialogData
                }
            });

        return this.$uibModal.open(options);
    };

    public get type(): DialogTypeEnum {
        return this.dialogSettings.type;
    }

    public open(dialogSettings?: IDialogSettings, dialogData?: IDialogData): ng.IPromise<any> {
        this.dialogSettings = _.assign(this.defaultSettings, dialogSettings);
        if (dialogData) {
            this.dialogData = dialogData;
        }
        return this.openInternal().result;
    }

    public alert(message: string, header?: string, okButton?: string, cancelButton?: string) {
        const dialogSettings = {
            type: DialogTypeEnum.Alert,
            header: this.localization.get(header) || header || this.localization.get("App_DialogTitle_Alert"),
            message: this.localization.get(message) || message,
            cancelButton: this.localization.get(cancelButton) || cancelButton || null, //Don't show cancel button if not defined
            css: "modal-alert nova-messaging"
        }  as IDialogSettings;
        if (okButton) {
            //We only want to set the okButton if it's specified, otherwise use the initialize default.
            dialogSettings.okButton = this.localization.get(okButton) || okButton;
        }
        this.dialogSettings = _.assign(this.defaultSettings, dialogSettings);
        return this.openInternal(<ng.ui.bootstrap.IModalSettings>{
            keyboard: false
        }).result;
    }

    public confirm(message: string, header?: string, css?: string) {
        const dialogSettings = {
            type: DialogTypeEnum.Confirm,
            header: this.localization.get(header) || header || this.localization.get("App_DialogTitle_Confirmation"),
            css: css,
            message: this.localization.get(message) || message
        } as IDialogSettings;
        this.dialogSettings = _.assign(this.defaultSettings, dialogSettings);

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
        this.$instance = undefined;
        this.dialogSettings = undefined;

    };
}

/*fixme: one class per file*/
export class DialogServiceMock implements IDialogService {
    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public open(dialogSettings: IDialogSettings): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public alert(message: string, header?: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public confirm(message: string, header?: string): ng.IPromise<any> {
        const deferred = this.$q.defer<any>();
        deferred.resolve(true);
        return deferred.promise;
    }

    public dialogSettings: IDialogSettings = {
        type: DialogTypeEnum.Base,
        header: "test",
        message: "test",
        cancelButton: "test",
        okButton: "test",
        template: "test",
        controller: null,
        css: null,
        backdrop: false
    };
}
