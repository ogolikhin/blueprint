import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "./widgets";

angular.module("app.shared", [
    "ui.router",
    "ui.bootstrap",
    "bp.widjets"]);
    
export {
    IBPTreeController,
    ITreeNode,
    IDialogSettings,
    IDialogService,
    BaseDialogController
} from "./widgets"

export {
    Helper,
} from "./utils/helper";