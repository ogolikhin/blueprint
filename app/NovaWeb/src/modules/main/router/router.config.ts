import {ArtifactState} from "./artifact.state";
import {ArtifactDetailsState} from "./editor-states/details.state";
import {DiagramState} from "./editor-states/diagram.state";
import {GeneralState} from "./editor-states/general.state";
import {GlossaryState} from "./editor-states/glossary.state";
import {ProcessState} from "./editor-states/process.state";

export class Routes {

    public static $inject = ["$stateProvider", "$urlRouterProvider", "$urlMatcherFactoryProvider"];

    constructor($stateProvider: ng.ui.IStateProvider,
                $urlRouterProvider: ng.ui.IUrlRouterProvider,
                $urlMatcherFactoryProvider: any) {

        // $urlMatcherFactoryProvider.caseInsensitive(true);

        // register states with the router 
        // $stateProvider
        //     .state("main.artifact", new ArtifactState())
        //     .state("main.artifact.process", new ProcessState())
        //     .state("main.artifact.details", new ArtifactDetailsState())
        //     .state("main.artifact.general", new GeneralState())
        //     .state("main.artifact.glossary", new GlossaryState())
        //     .state("main.artifact.diagram", new DiagramState());
    }
}
;
