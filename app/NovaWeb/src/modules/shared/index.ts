﻿import * as angular from "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "./widgets";
import "./filters";

angular.module("app.shared", [
    "ui.router",
    "ui.bootstrap",
    "bp.widgets",
    "bp.filters"]);

export {
    IDialogSettings,
    IDialogService,
    IDialogData,
    IRowSliderControllerApi,
    BaseDialogController,
    IBPAction,
    BPButtonAction,
    BPDropdownItemAction,
    BPDropdownAction,
    BPToggleItemAction,
    BPToggleAction,
    BPButtonGroupAction
} from "./widgets"

export {
    Helper,
} from "./utils/helper";
