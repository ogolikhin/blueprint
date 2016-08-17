import { IDialogSettings, IDialogData, IDialogService, DialogService, BaseDialogController } from "./bp-dialog";

angular.module("bp.widjets.dialog", [])
    .service("dialogService", DialogService);

export { IDialogSettings, IDialogData, IDialogService, BaseDialogController };
