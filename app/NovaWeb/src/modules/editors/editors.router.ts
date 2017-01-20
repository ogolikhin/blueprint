import {ItemStateController} from "./item-state/item-state.controller";
import {IItemStateService} from "./item-state/item-state.svc";
import {ISelectionManager} from "../managers/selection-manager/selection-manager";

export class ArtifactRoutes {

    public static $inject = [
        "$stateProvider",
        "$urlMatcherFactoryProvider"
    ];

    constructor($stateProvider: ng.ui.IStateProvider,
                $urlMatcherFactoryProvider: any) {

        $urlMatcherFactoryProvider.caseInsensitive(true);

        // register states with the router
        $stateProvider
            .state("main.unpublished", <ng.ui.IState>{
                url: "/unpublished",
                template: "<unpublished></unpublished>",
                resolve: {
                    saved: ["selectionManager", (sm: ISelectionManager) => sm.autosave()]
                }
            })
            .state("main.jobs", <ng.ui.IState>{
                url: "/jobs",
                template: "<jobs></jobs>",
                resolve: {
                    saved: ["selectionManager", (sm: ISelectionManager) => sm.autosave()]
                }
            })
            .state("main.item", <ng.ui.IState>{
                url: "/{id:int}?{version:int}&{path:navpath}",
                template: require("./item-state/item-state.html"),
                controller: ItemStateController,
                controllerAs: "$ctrl",
                params: {
                    // prevents array format from being split into path=1&path=2&...
                    path: {array : false}
                },
                resolve: {
                    itemInfo: ["$stateParams", "itemStateService", "authenticated",
                        ($stateParams: ng.ui.IStateParamsService, itemStateService: IItemStateService, authenticated) => {

                        const id = parseInt($stateParams["id"], 10);
                        return itemStateService.getItemInfoResult(id);
                    }],
                    saved: ["selectionManager", (sm: ISelectionManager) => sm.autosave()]
                }
            });
    }
}
