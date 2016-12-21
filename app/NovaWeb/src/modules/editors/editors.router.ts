import {ItemStateController} from "./item-state.controller";
import {IArtifactManager} from "./../managers";

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
            .state("main.jobs", <ng.ui.IState>{
                url: "/jobs",
                template: "<unpublished></unpublished>",
                resolve: {
                    saved: ["artifactManager", (am: IArtifactManager) => { return am.autosave(); }]
                }
            })
            .state("main.item", <ng.ui.IState>{
                url: "/{id:int}?{version:int}&{path:navpath}",
                template: "<div ui-view class='artifact-state'></div>",
                reloadOnSearch: false,
                controller: ItemStateController,
                params: {
                    // prevents array format from being split into path=1&path=2&...
                    path: {array : false}
                },
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
