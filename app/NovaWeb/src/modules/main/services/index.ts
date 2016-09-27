import * as angular from "angular";
import { IWindowManager, WindowManager, IMainWindow, ResizeCause } from "./window-manager";

angular.module("bp.main.services", [])
    .service("windowManager", WindowManager);

export {
    IWindowManager, WindowManager, IMainWindow, ResizeCause
}