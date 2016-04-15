import "angular";
import {ILocalizationService} from "../../../core/localization";

export enum DialogTypeEnum {
    General,
    Alert,
    Confirm
}

export interface IDialogParams {
    template?: string;
    controller?: any;
    header?: string;
    message?: string;
    cancelButton?: string;
    okButton?: string;
}

export interface IDialogService {
    open(params: IDialogParams): ng.IPromise<any>;
    alert(message: string): ng.IPromise<any>;
    confirm(message: string): ng.IPromise<any>;
}

export class DialogService implements IDialogService {


    private dialogType: DialogTypeEnum;

    public static $inject = ["localization", "$uibModal"];

    public params: IDialogParams = {};

    private defaultParams: IDialogParams = {
        template: require("./dialog.html"),
        controller: BaseDialogController,
        cancelButton: this.localization.get("App_Button_Cancel") || "Cancel",
        okButton: this.localization.get("App_Button_Ok") || "Ok"
    };

    public get type(): DialogTypeEnum {
        return this.dialogType;
    }


    constructor(private localization: ILocalizationService, private $uibModal: ng.ui.bootstrap.IModalService) {
    }

    private initialize(params: IDialogParams) {
        this.params = angular.extend({}, this.defaultParams);
        angular.extend(this.params, params);
    }

    private openInternal = () => {
        /* tslint:disable*/
        var instance = this.$uibModal.open(<ng.ui.bootstrap.IModalSettings>{
            template: this.params.template,
            controller: this.params.controller,
            controllerAs: "ctrl",
            windowClass: "nova-messaging",
            //            backdrop :true,
            //            windowTopClass: windowTopClass ,
            resolve: {
                params: () => {
                    return this.params;
                }
            }
        });
        /* tslint:enable*/
        return instance;
    };


    public open(params: IDialogParams): ng.IPromise<any> {
        this.dialogType = DialogTypeEnum.General;
        this.initialize(params);
        return this.openInternal().result;
    }

    public alert(message: string) {
        this.dialogType = DialogTypeEnum.Alert;
        this.initialize({
            header: "Attention",
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

    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private params: IDialogParams) {
        this.$instance = $uibModalInstance;
    }

    public okay = () => {
        this.$instance.close(true);
    };

    public cancel = () => {
        this.$instance.dismiss("cancel");
    };
}



