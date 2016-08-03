import "angular";
import * as Models from "../../main/models/models";
import {IProjectManager, ProjectManager} from "../../main/services/project-manager";
import {MessageService} from "../";

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

    public static $inject = ["$rootScope", "$state", "projectManager", "messageService"];

    constructor(
        private $rootScope,
        private $state: any,
        private projectManager: IProjectManager,
        private messageService: MessageService) {

        console.log("change state");

        // Need to load artifact type from id instead of param        
        //let artifactType = <Models.ItemTypePredefined>$state.params["artifactType"];
        let id = $state.params["id"];

        let artifact = projectManager.getArtifact(id);
        if (artifact) {
            let artifactType = artifact.predefinedType;
            projectManager.setCurrentArtifact(artifact);
            this.navigateToSubRoute(artifactType);
        } else{
            messageService.addError("Cannot find artifact");
        }
       
    }

    public navigateToSubRoute(artifactType: Models.ItemTypePredefined) {
        switch (artifactType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.$state.go('main.artifact.diagram');
                break;
            case Models.ItemTypePredefined.Glossary:
                this.$state.go('main.artifact.glossary');
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                this.$state.go('main.artifact.general');
                //this.$state.go('main.artifact.details');
                break;
            case Models.ItemTypePredefined.Process:
                this.$state.go('main.artifact.storyteller');
                break;            
            default:
                this.$state.go('main.artifact.details');
        }
    }
   

}


                                                        