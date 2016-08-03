import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import { Helper } from "./utils/helper";
import { AppConstants, IAppConstants } from "./constants/app-constants";
import { LocalizationService, ILocalizationService } from "./localization";

import { ConfigValueHelper, IConfigValueHelper } from "./config.value.helper";
import { IDialogSettings, IDialogService, DialogService, BaseDialogController } from "./widgets/bp-dialog/bp-dialog";
import { BPTreeComponent } from "./widgets/bp-tree/bp-tree";
import { BPTreeInlineEditing } from "./widgets/bp-tree-inline-editing/bp-tree-inline-editing";
import { BPTreeDragndrop } from "./widgets/bp-tree-dragndrop/bp-tree-dragndrop";
import { BPTooltip } from "./widgets/bp-tooltip/bp-tooltip";
import { BPInfiniteScroll } from "./widgets/bp-infinite-scroll/bp-infinite-scroll";
import { BPCompileHtml } from "./widgets/bp-compile-html/bp-compile-html";
import { BPAvatar } from "./widgets/bp-avatar/bp-avatar";
import { BPSelect } from "./widgets/bp-select/bp-select";
import { BPItemTypeIconComponent } from "./widgets/bp-item-icon/bp-item-icon";
import { BpSpecialLinkContainer } from "./widgets/bp-special-link-container";

let module = angular.module("app.core", ["ui.router", "ui.bootstrap"])
    .constant("appConstants", new AppConstants())
    .service("localization", LocalizationService)
    .service("dialogService", DialogService)
    .service("configValueHelper", ConfigValueHelper)
    .directive("bpTreeInlineEditing", BPTreeInlineEditing.factory())
    .directive("bpTreeDragndrop", BPTreeDragndrop.factory())
    .directive("bpTooltip", BPTooltip.factory())
    .directive("bpInfiniteScroll", BPInfiniteScroll.factory())
    .directive("bpCompileHtml", BPCompileHtml.factory())
    .directive("body", BpSpecialLinkContainer.factory())
    .component("bpItemTypeIcon", new BPItemTypeIconComponent())
    .component("bpTree", new BPTreeComponent())
    .component("bpAvatar", new BPAvatar())
    .component("bpSelect", new BPSelect());

export default module.name;
export {
    Helper,
    IAppConstants,
    ILocalizationService, 
    IConfigValueHelper,
    ConfigValueHelper,
    IDialogSettings,
    IDialogService,
    DialogService,
    BaseDialogController
};