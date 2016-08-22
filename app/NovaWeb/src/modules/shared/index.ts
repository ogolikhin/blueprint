import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import "./widgets";
import { BpFilesizeFilter } from "./utils/bp-filesize.filter";

angular.module("app.shared", [
    "ui.router",
    "ui.bootstrap",
    "bp.widjets"])

    .filter("BpFilesize", BpFilesizeFilter);
    
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

export { BpFilesizeFilter } from "./utils/bp-filesize.filter";