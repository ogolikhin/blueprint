import * as angular from "angular";
import { IDialogSettings, IDialogData, IDialogService, DialogService, BaseDialogController } from "./bp-dialog";

angular.module("bp.widgets.dialog", [])
    .service("dialogService", DialogService);

export { IDialogSettings, IDialogData, IDialogService, DialogService, BaseDialogController };
