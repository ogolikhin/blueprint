import * as angular from "angular";
import {ItemStateController} from "./item-state.controller";
import {BaseEditorStateController} from "./base-editor-state.controller";

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
                controller: BaseEditorStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.details", {
                template: require("./bp-artifact/details.state.html"),
                controller: BaseEditorStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.general", {
                template: require("./bp-artifact/general.state.html"),
                controller: BaseEditorStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.glossary", {
                template: require("./bp-glossary/glossary.state.html"),
                controller: BaseEditorStateController,
                controllerAs: "$content",
                params: { context: null }
            })
            .state("main.artifact.diagram", {
                template: require("./bp-diagram/diagram.state.html"),
                controller: BaseEditorStateController,
                controllerAs: "$content",
                params: { context: null }
            });
    }
}
