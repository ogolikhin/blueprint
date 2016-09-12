import {
    SelectionManager,
    ISelectionManager,
    ISelection,
    SelectionSource
} from "./selection-manager";


angular.module("bp.managers.selection", [])
    .service("selectionManager2", SelectionManager);


export {
    SelectionManager,
    ISelectionManager,
    ISelection,
    SelectionSource
};
