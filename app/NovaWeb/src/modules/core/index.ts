import "./configuration";
import "./error";
import "./file-upload";
import "./http";
import "./loading-overlay";
import "./localization";
import "./messages";
import "./navigation";
import "./services";
import "./local-storage";

angular.module("bp.core", [
    "ui.router",
    "ui.bootstrap",

    "bp.core.configuration",
    "bp.core.fileUpload",
    "bp.core.loadingOverlay",
    "bp.core.localization",
    "bp.core.messages",
    "bp.core.navigation",
    "bp.core.services",
    "bp.core.localStorage"
]);

export {
    IWindowResize,
    IWindowVisibility,
    IUserOrGroupInfo,
    IUsersAndGroupsService,
    UsersAndGroupsService
} from "./services";

