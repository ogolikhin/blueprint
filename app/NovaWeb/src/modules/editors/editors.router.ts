import "angular";
import {ItemStateController} from "./item-state.controller";
<<<<<<< HEAD
import {IArtifactManager} from "./../managers";
=======
import {ISelectionManager} from "../managers/selection-manager/selection-manager";
>>>>>>> c0e08d5d9bc7f5945202d9a68ed9729ac26f3286

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
                    saved: ["artifactManager", (am: IArtifactManager) => { return am.autosave(); }]   
                }
            })
            .state("main.item", <ng.ui.IState>{
                url: "/{id:int}?{version:int}&{path:string}",
                template: "<div ui-view class='artifact-state'></div>",
                reloadOnSearch: false,
                controller: ItemStateController,
                resolve: {
                    saved: ["artifactManager", (am: IArtifactManager) => { return am.autosave(); }]      
                }          
            })

            .state("main.item.process", {
                template: require("./bp-process/process.state.html")
            })
            .state("main.item.details", {
                template: require("./bp-artifact/details.state.html")
            })
            .state("main.item.general", {
                template: require("./bp-artifact/general.state.html")
            })
            .state("main.item.glossary", {
                template: require("./bp-glossary/glossary.state.html")
            })
            .state("main.item.collection", {
                template: require("./bp-collection/collection.state.html")
            })
            .state("main.item.diagram", {
                template: require("./bp-diagram/diagram.state.html")
            });
    }
}
