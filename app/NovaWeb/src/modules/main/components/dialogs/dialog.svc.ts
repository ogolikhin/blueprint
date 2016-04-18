import "angular";
import {ILocalizationService} from "../../../core/localization";

export enum DialogTypeEnum {
    General,
    Alert,
    Confirm
}


export interface IDialogOptions {
    header?: string;
    message?: string;
    cancelButton?: string;
    okButton?: string;
    template?: string;
    controller?: any;
}

export interface IDialogService {
    open(params: IDialogOptions): ng.IPromise<any>;
    alert(message: string): ng.IPromise<any>;
    confirm(message: string): ng.IPromise<any>;
}

export class DialogService implements IDialogService {

    private dialogType: DialogTypeEnum;

    public static $inject = ["localization", "$uibModal"];

    private params: IDialogOptions = {};

    private defaultParams: IDialogOptions = {
        cancelButton: this.localization.get("App_Button_Cancel","Cancel"),
        okButton: this.localization.get("App_Button_Ok","Ok"),
        template: require("./dialog.html"),
        controller: BaseDialogController
    };

    public get type(): DialogTypeEnum {
        return this.dialogType;
    }

    constructor(private localization: ILocalizationService, private $uibModal: ng.ui.bootstrap.IModalService) {
    }

    private initialize(params: IDialogOptions) {
        this.params = angular.extend({}, this.defaultParams, params);
    }

    private openInternal = () => {
        var instance = this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
            template: this.params.template,
            controller: this.params.controller,
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


    public open(params: IDialogOptions): ng.IPromise<any> {
        this.dialogType = DialogTypeEnum.General;
        this.initialize(params);
        return this.openInternal().result;
    }

    public alert(message: string) {
        this.dialogType = DialogTypeEnum.Alert;
        this.initialize({
            header: this.localization.get("App_DialogTitle_Alert"),
            message : message,
            cancelButton: null,
        });
        return this.openInternal().result;
    }

    public confirm(message: string) {
        this.dialogType = DialogTypeEnum.Confirm;
        this.initialize({
            header: this.localization.get("App_DialogTitle_Confirmation"),
            message: message,
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



