import "angular-ui-router";
import "angular-ui-bootstrap";
import "./services";
import "./messages";
import "./navigation";
import "./loading-overlay";

angular.module("app.core", [
    "bp.core.services",
    "bp.core.messages",
    "bp.core.navigation",
    "bp.core.loadingOverlay"]);

export {
    IWindowResize,
    IWindowVisibility,
    IUserOrGroupInfo,
    IUsersAndGroupsService,
    UsersAndGroupsService
} from "./services";

