import "angular";
import "angular-ui-router";
import "angular-ui-bootstrap";
import { LocalizationService, ILocalizationService } from "./localization";
import { EventManager, IEventManager, EventSubscriber } from "./event-manager";
import { ConfigValueHelper, IConfigValueHelper } from "./config.value.helper";
import { IDialogSettings, IDialogService, DialogService } from "./services/dialog";
import { BPTreeInlineEditing } from "./widgets/bp-tree-inline-editing/bp-tree-inline-editing";
import { BPTreeDragndrop } from "./widgets/bp-tree-dragndrop/bp-tree-dragndrop";
import { BPTooltip } from "./widgets/bp-tooltip/bp-tooltip";
import { BPInfiniteScroll } from "./widgets/bp-infinite-scroll/bp-infinite-scroll";


let module = angular.module("app.core", ["ui.router", "ui.bootstrap"])
    .service("localization", LocalizationService)
    .service("eventManager", EventManager)
    .service("dialogService", DialogService)
    .service("configValueHelper", ConfigValueHelper)
    .directive("bpTreeInlineEditing", BPTreeInlineEditing.factory())
    .directive("bpTreeDragndrop", BPTreeDragndrop.factory())
    .directive("bpTooltip", BPTooltip.factory())
    .directive("bpInfiniteScroll", BPInfiniteScroll.factory());

export default module.name;
export {
    ILocalizationService,
    IConfigValueHelper,
    IEventManager,
    EventSubscriber,
    IDialogSettings,
    IDialogService,
    DialogService
};