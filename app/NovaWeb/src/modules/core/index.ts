import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import {LocalizationService} from "./localization";
import {ConfigValueHelper} from "./config.value.helper";
import {DialogService} from "../services/dialog.svc";
import {BPTreeInlineEditing} from "./widgets/bp-tree-inline-editing/bp-tree-inline-editing";
import {BPTreeDragndrop} from "./widgets/bp-tree-dragndrop/bp-tree-dragndrop";
import {BPTooltip} from "./widgets/bp-tooltip/bp-tooltip";

angular.module("app.core", ["ui.router", "ui.bootstrap"])
    .service("localization", LocalizationService)
    .service("dialogService", DialogService)
    .service("configValueHelper", ConfigValueHelper)
    .directive("bpTreeInlineEditing", BPTreeInlineEditing.factory())
    .directive("bpTreeDragndrop", BPTreeDragndrop.factory())
    .directive("bpTooltip", BPTooltip.factory());

