import { IWindowResize, WindowResize} from "./window-resize";
import { IWindowVisibility, WindowVisibility} from "./window-visibility";

angular.module("bp.core.services", [])
    .service("windowResize", WindowResize)
    .service("windowVisibility", WindowVisibility);

export {
    IWindowResize,
    IWindowVisibility
}