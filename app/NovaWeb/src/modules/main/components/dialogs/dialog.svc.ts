import "angular";
import {ILocalizationService} from "../../../core/localization";

export enum DialogTypeEnum {
    General,
    Alert,
    Confirm
}

export interface IDialogTemplate {
    template: string;
    controller: any;
}

export interface IDialogOptions {
    header?: string;
    message?: string;
    cancelButton?: string;
    okButton?: string;
}

export interface IDialogService {
    open(params: IDialogOptions, layout?: IDialogTemplate): ng.IPromise<any>;
    alert(message: string): ng.IPromise<any>;
    confirm(message: string): ng.IPromise<any>;
}

export class DialogService implements IDialogService {

    private dialogType: DialogTypeEnum;

    public static $inject = ["localization", "$uibModal"];

    private template: string = require("./dialog.html");
    private controller: any = BaseDialogController;

    public params: IDialogOptions = {};

    private defaultParams: IDialogOptions = {
        cancelButton: this.localization.get("App_Button_Cancel") || "Cancel",
        okButton: this.localization.get("App_Button_Ok") || "Ok"
    };

    public get type(): DialogTypeEnum {
        return this.dialogType;
    }

    constructor(private localization: ILocalizationService, private $uibModal: ng.ui.bootstrap.IModalService) {
    }

    private initialize(params: IDialogOptions, dialogTemplate?: IDialogTemplate ) {
        dialogTemplate = dialogTemplate || <IDialogTemplate>{};
        this.template = dialogTemplate.template || require("./dialog.html");
        this.controller = dialogTemplate.controller || BaseDialogController;

        this.params = angular.extend({}, this.defaultParams);
        angular.extend(this.params, params);
    }

    private openInternal = () => {
        var instance = this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
            template: this.template,
            controller: this.controller,
            controllerAs: "ctrl",
            windowClass: "nova-messaging",
            resolve: {
                params: () => {
                    return this.params;
                }
            }
        });
        return instance;
    };


    public open(params: IDialogOptions, dialogController?: IDialogTemplate): ng.IPromise<any> {
        this.dialogType = DialogTypeEnum.General;
        this.initialize(params, dialogController);
        return this.openInternal().result;
    }

    public alert(message: string) {
        this.dialogType = DialogTypeEnum.Alert;
        this.initialize({
            header: this.localization.get("App_DialogTitle_Alert"),
            message : message,
            cancelButton: null,
            okButton: this.localization.get("App_Button_Ok") || "Okay"
        });
        return this.openInternal().result;
    }

    public confirm(message: string) {
        this.dialogType = DialogTypeEnum.Confirm;
        this.initialize({
            header: this.localization.get("App_DialogTitle_Confirmation"),
            message: message,
            cancelButton: this.localization.get("App_Button_Cancel") || "Cancel",
            okButton: this.localization.get("App_Button_Ok") || "Okay"
        });
        return this.openInternal().result;
    }


}

export class BaseDialogController {

    static $inject = ["$uibModalInstance", "params"];

    public hasCloseButton: boolean;

    public $instance: ng.ui.bootstrap.IModalServiceInstance;

    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private params: IDialogOptions) {
        this.$instance = $uibModalInstance;
    }

    public okay = () => {
        this.$instance.close(true);
    };

    public cancel = () => {
        this.$instance.dismiss("cancel");
    };
}



