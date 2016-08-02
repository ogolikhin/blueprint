import "angular";
import {ArtifactEditorType} from "../../core";

export class ArtifactState implements ng.ui.IState {
    public url = "/{id:any}";
    public template = "<div ui-view></div>";
    public controller = "artifactStateController";

    public params = { artifactType: null };
    
    public onEnter = () => {
        let enter = "test";
        console.log("change state enter");
    };

    public onExit = () => {
        let ex = "test";
        console.log("change state exit");
    };
}
 
export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state"];

    constructor(
        private $rootScope,
        private $state: any) {

        console.log("change state");
        let id = <ArtifactEditorType>$state.params["id"];

        // Need to load artifact type from id instead of param        
        let artifactType = <ArtifactEditorType>$state.params["artifactType"];

        if (artifactType == ArtifactEditorType.Diagram) {
            this.$state.go('main.artifact.diagram');
        } else if (artifactType == ArtifactEditorType.Glossary) {
            this.$state.go('main.artifact.glossary');
        } else if (artifactType == ArtifactEditorType.Details) {
            this.$state.go('main.artifact.details');
        } else if (artifactType == ArtifactEditorType.General) {
            this.$state.go('main.artifact.details');
        } else if (artifactType == ArtifactEditorType.Storyteller) {
            this.$state.go('main.artifact.storyteller');
        } else {
            this.$state.go('main');
        }
    }
   

}


                                                        