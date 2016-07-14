module Shell {

    export interface IDialogParams {
        denyButton?: string;
        confirmButton?: string;
        publishButton?: string;
        saveButton?: string;
        discardButton?: string;
        discardDisabled?:boolean;
        header?: string;
        message?: string;
        dialogType?: DialogType;
        buttonType?: ButtonType;
        template?: string;
        controllerClassName?: string;
    }

    export enum DialogType {
        Confirmation = 0,
        Warning = 1,
        Error = 2,
        Action = 3
    }

    export enum ButtonType {
        Publish = 0,
        Save = 1,
        Discard = 2,
        Confirmation = 3
    }

    export interface IDialogService {
        open(params: IDialogParams): ng.IPromise<any>;
        confirm(params: IDialogParams): ng.IPromise<any>;
        error(params: IDialogParams): ng.IPromise<any>;
        warning(params: IDialogParams): ng.IPromise<any>;
        action(params: IDialogParams): ng.IPromise<any>;
    }

    export class DialogService implements IDialogService {

        public static $inject = ["$rootScope", "$uibModal"];

        private defaultParams: IDialogParams;

        constructor(private $rootScope: ng.IRootScopeService,
            private $uibModal: angular.ui.bootstrap.IModalService) {
            this.defaultParams = { dialogType: DialogType.Confirmation };
            this.defaultParams.denyButton = this.getLabel("Shell_Default_DenyButton_Label", "Cancel");
            this.defaultParams.confirmButton = this.getLabel("Shell_Default_ConfirmButton_Label", "OK");
            this.defaultParams.publishButton = this.getLabel("Shell_Default_PublishButton_Label", "Publish");
            this.defaultParams.saveButton = this.getLabel("Shell_Default_SaveButton_Label", "Save");
            this.defaultParams.discardButton = this.getLabel("Shell_Default_DiscardButton_Label", "Discard");
            this.defaultParams.template = "DialogTemplate.html";
            this.defaultParams.controllerClassName = "dialogController";
            this.defaultParams.discardDisabled = false;
        }

        public open(params: IDialogParams): ng.IPromise<any> {
            params.dialogType = (params.dialogType || this.defaultParams.dialogType);
            params.denyButton = (params.denyButton || this.defaultParams.denyButton);
            params.confirmButton = (params.confirmButton || this.defaultParams.confirmButton);
            params.publishButton = (params.publishButton || this.defaultParams.publishButton);
            params.saveButton = (params.saveButton || this.defaultParams.saveButton);
            params.discardButton = (params.discardButton || this.defaultParams.discardButton);
            if (params.discardDisabled == null)
                params.discardDisabled = this.defaultParams.discardDisabled;
            params.template = (params.template || this.defaultParams.template);
            params.controllerClassName = (params.controllerClassName || this.defaultParams.controllerClassName);
            this.initDefaultHeader(params);

            return this.openInternal(params, this.getTopWindowClass(params)).result;
        }

        public confirm(params: IDialogParams): ng.IPromise<any> {
            params.dialogType = DialogType.Confirmation;
            this.initDefaultHeader(params);
            return this.open(params);
        }

        public error(params: IDialogParams): ng.IPromise<any> {
            params.dialogType = DialogType.Error;
            this.initDefaultHeader(params);
            return this.open(params);
        }

        public warning(params: IDialogParams): ng.IPromise<any> {
            params.dialogType = DialogType.Warning;
            this.initDefaultHeader(params);
            return this.open(params);
        }

        public action(params: IDialogParams): ng.IPromise<any> {
            params.dialogType = DialogType.Action;
            params.template = "ActionDialogTemplate.html";
            params.controllerClassName = "actionDialogController";
            this.initDefaultHeader(params);
            return this.open(params);
        }

        private initDefaultHeader(params: IDialogParams) {
            switch (params.dialogType) {
                case DialogType.Error:
                    params.header = (params.header || this.getLabel("Shell_Default_Error_Header", "Error"));
                    break;
                case DialogType.Warning:
                    params.header = (params.header || this.getLabel("Shell_Default_Warning_Header", "Warning"));
                    break;
                default:
                    params.header = (params.header || this.getLabel("Shell_Default_Confirm_Header", "Confirmation"));
                    break;
            }
        }

        private getTopWindowClass(params: IDialogParams) {
            switch (params.dialogType) {
                case DialogType.Error:
                    return "modal-confirmation modal-error";
                case DialogType.Warning:
                    return "modal-confirmation modal-warning";             
            }
            return "modal-confirmation";
        }

        private getLabel(key: string, defaultValue?: string) {
            return ((<any>this.$rootScope).config.labels[key] || defaultValue);
        }

        private openInternal = (params: any, windowTopClass: string) => {
            var modalInstance = this.$uibModal.open(<angular.ui.bootstrap.IModalSettings>{
                templateUrl: `/Areas/Web/App/Common/Shell/Dialogs/${params.template}`,
                controller: params.controllerClassName,
                controllerAs: "vm",
                windowTopClass: windowTopClass,
                resolve: {
                    params: () => {
                        return params;
                    }
                }
            });
            return modalInstance;
        }
    }

    var app = angular.module("Shell");
    app.service("dialogService", DialogService);
}