import { IDialogSettings, IDialogService, DialogService, BaseDialogController } from "./bp-dialog";

angular.module("bp.widjets.dialog", [])
    .service("dialogService", DialogService);

export { IDialogSettings, IDialogService, BaseDialogController };