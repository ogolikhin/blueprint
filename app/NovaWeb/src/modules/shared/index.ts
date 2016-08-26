import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "./widgets";
import "./filters";

angular.module("app.shared", [
    "ui.router",
    "ui.bootstrap",
    "bp.widjets",
    "bp.filters"]);
    
export {
    IBPTreeController,
    ITreeNode,
    IDialogSettings,
    IDialogService,
    IDialogData,
    BaseDialogController
} from "./widgets"

export {
    Helper,
} from "./utils/helper";
