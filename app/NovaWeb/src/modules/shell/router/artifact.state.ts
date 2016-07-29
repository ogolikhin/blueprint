import "angular";
import { IProjectManager } from "../../main/services/project-manager";

export class ArtifactState implements ng.ui.IState {
    public url = "{id:any}";
    public template = "<div ui-view>Main.Artifact</div>";
    public controller = "artifactStateController";
    
    public onEnter = () => {
        let enter = "test";
    };

    public onExit = () => {
        let ex = "test";
    };
}
 
export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state", "projectManager"];

    constructor(
        private $rootScope,  
        private $state: any, 
        private projectManager: IProjectManager ) {

            var test = "test";
            setTimeout(() => {
                $state.go('main.artifact.storyteller');
            }, 2000);
            
    }
}


                                                        