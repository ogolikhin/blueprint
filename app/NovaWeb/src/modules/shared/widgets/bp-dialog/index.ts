import * as angular from "angular";
import {IDialogSettings, IDialogService, DialogService, BaseDialogController} from "./bp-dialog";

angular.module("bp.widgets.dialog", [])
    .service("dialogService", DialogService);

export {IDialogSettings, IDialogService, DialogService, BaseDialogController};
