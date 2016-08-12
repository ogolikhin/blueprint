import "angular";
import { ILocalizationService } from "../../../core";

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
}

export interface IDialogService {
    open(params: IDialogSettings): ng.IPromise<any>;
    alert(message: string, header?: string): ng.IPromise<any>;
    confirm(message: string, header?: string): ng.IPromise<any>;
    params: IDialogSettings;
}

export class DialogService implements IDialogService {

    public static $inject = ["localization", "$uibModal"];

    constructor(private localization: ILocalizationService, private $uibModal: ng.ui.bootstrap.IModalService) { }

    public params: IDialogSettings = {};

    private defaultParams: IDialogSettings = {
        type: DialogTypeEnum.Base,
        cancelButton: this.localization.get("App_Button_Cancel", "Cancel"),
        okButton: this.localization.get("App_Button_Ok", "Ok"),
        template: require("./bp-dialog.html"),
        controller: BaseDialogController
    };

    private initialize(params: IDialogSettings) {
        params = this.params = angular.extend({}, this.defaultParams, params);
    }

    private openInternal = (optsettings?: ng.ui.bootstrap.IModalSettings) => {
        var settings = <ng.ui.bootstrap.IModalSettings>{
            template: this.params.template,
            controller: this.params.controller,
            controllerAs: "$ctrl",
            windowClass: this.params.css || "nova-messaging",
            backdrop: this.params.backdrop || false,
            resolve: {
                params: () => {
                    return this.params;
                }
            }
        };
        var instance = this.$uibModal.open(angular.merge({}, settings, optsettings));
        return instance;
    };

    public get type(): DialogTypeEnum {
        return this.params.type;
    }

    public open(params?: IDialogSettings): ng.IPromise<any> {
        this.initialize(params || this.params);
        return this.openInternal().result;
    }

    public alert(message: string, header?: string) {
        this.initialize({
            type: DialogTypeEnum.Alert,
            header: header || this.localization.get("App_DialogTitle_Alert"),
            message : message,
            cancelButton: null,
        });
        return this.openInternal(<ng.ui.bootstrap.IModalSettings>{
            keyboard: false
        }).result;
    }

    public confirm(message: string, header?: string) {
        this.initialize({
            type: DialogTypeEnum.Confirm,
            header: header || this.localization.get("App_DialogTitle_Confirmation"),
            message: message,
        });
        return this.openInternal().result;
    }
}

export interface IDialogController {
    returnValue: any;
    ok: Function;
    cancel: Function;
}

export class BaseDialogController implements IDialogController {

    public hasCloseButton: boolean;

    public get returnValue(): any {
        return true;
    }

    public $instance: ng.ui.bootstrap.IModalServiceInstance;

    static $inject = ["$uibModalInstance", "params"];
    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance, private params: IDialogSettings) {
        this.$instance = $uibModalInstance;
    }

    public ok = () => {
        this.$instance.close(this.returnValue);
    };

    public cancel = () => {
        this.$instance.close(false);
    };
}

export class DialogServiceMock implements IDialogService {
    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) { }

    public open(params: IDialogSettings): ng.IPromise<any> {
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
    public params: IDialogSettings = {
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