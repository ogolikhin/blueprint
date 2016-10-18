import * as angular from "angular";
import {ItemStateController} from "./item-state.controller";
import {DetailsStateController} from "./bp-artifact/details.state";
import {GeneralStateController} from "./bp-artifact/general.state";
import {ProcessStateController} from "./bp-process/process.state";
import {GlossaryStateController} from "./bp-glossary/glossary.state";
import {DiagramStateController} from "./bp-diagram/diagram.state";

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
            .state("main.artifact", {
                url: "/{id:any}?{path:string}",
                template: "<div ui-view class='artifact-state'></div>",
                reloadOnSearch: false,
                controller: ItemStateController
            })
            .state("main.artifact.process", {
                template: require("./bp-process/process.state.html"),
                controller: ProcessStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.details", {
                template: require("./bp-artifact/details.state.html"),
                controller: DetailsStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.general", {
                template: require("./bp-artifact/general.state.html"),
                controller: GeneralStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.glossary", {
                template: require("./bp-glossary/glossary.state.html"),
                controller: GlossaryStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.diagram", {
                template: require("./bp-diagram/diagram.state.html"),
                controller: DiagramStateController,
                controllerAs: "$content",
                params: { context: null }
            });
    }
}
