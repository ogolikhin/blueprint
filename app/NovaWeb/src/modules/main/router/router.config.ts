import {ArtifactState} from "./artifact.state";
import * as EditorStates from "./editor.states";

export class Routes {

    public static $inject = ["$stateProvider", "$urlRouterProvider", "$urlMatcherFactoryProvider"];

    constructor(
        $stateProvider: ng.ui.IStateProvider,
        $urlRouterProvider: ng.ui.IUrlRouterProvider,
        $urlMatcherFactoryProvider: any) {

        $urlMatcherFactoryProvider.caseInsensitive(true);

        // register states with the router 
        $stateProvider
            .state("main.artifact", new ArtifactState())
            .state("main.artifact.storyteller", new EditorStates.StorytellerState())
            .state("main.artifact.details", new EditorStates.ArtifactDetailsState())
            .state("main.artifact.general", new EditorStates.GeneralState())
            .state("main.artifact.glossary", new EditorStates.GlossaryState())
            .state("main.artifact.diagram", new EditorStates.DiagramState());
    }

};
