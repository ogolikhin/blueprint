import "angular";
import {MainState} from "./main.state";
import {ArtifactState} from "./artifact.state";
import {StorytellerState} from "./storyteller.state";
import {ErrorState} from "../error/error.state";
 
export class Routes  {

    public static $inject = ["$stateProvider", "$urlRouterProvider", "$urlMatcherFactoryProvider"];

    constructor(
        $stateProvider: ng.ui.IStateProvider,
        $urlRouterProvider: ng.ui.IUrlRouterProvider,
        $urlMatcherFactoryProvider: any) {
        
        $urlMatcherFactoryProvider.caseInsensitive(true);

        // pass through / to main state 
        $urlRouterProvider.when("", "/main");
  
        // unrecognized routes go to error state 
        $urlRouterProvider.otherwise("/error");

        // register states with the router 
        $stateProvider
            .state("main", new MainState())
            .state("main.artifact", new ArtifactState())
            .state("main.artifact.storyteller", new StorytellerState())
        	.state("error", new ErrorState());
    }

};
