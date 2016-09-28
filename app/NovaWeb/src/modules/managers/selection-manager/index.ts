import * as angular from "angular";
import {
    SelectionManager,
    ISelectionManager,
    ISelection,
    SelectionSource
} from "./selection-manager";


angular.module("bp.managers.selection", [])
    .service("selectionManager", SelectionManager);


export {
    SelectionManager,
    ISelectionManager,
    ISelection,
    SelectionSource
};
