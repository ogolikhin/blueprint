import "angular-ui-router";
import "angular-ui-bootstrap";
import "./services";
import "./messages";
import "./navigation";
import "./loading-overlay";

angular.module("bp.core", [
    "ui.router",
    "ui.bootstrap"
    ]);

export {
    IWindowResize,
    IWindowVisibility,
    IUserOrGroupInfo,
    IUsersAndGroupsService,
    UsersAndGroupsService
} from "./services";

