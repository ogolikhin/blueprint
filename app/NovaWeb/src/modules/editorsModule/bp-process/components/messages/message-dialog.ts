export interface IDialogParams {
    denyButton?: string;
    confirmButton?: string;
    publishButton?: string;
    saveButton?: string;
    discardButton?: string;
    discardDisabled?: boolean;
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
