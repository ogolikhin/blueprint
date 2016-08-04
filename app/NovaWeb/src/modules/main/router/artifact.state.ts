import "angular";
import * as Models from "../models/models";
import {IProjectManager, ProjectManager} from "../";
import {MessageService} from "../../shell";

export class ArtifactState implements ng.ui.IState {
    public url = "/{id:any}";
    public template = "<div ui-view></div>";
    public controller = "artifactStateController";
    public params = { context: null };
}

export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state", "projectManager", "messageService"];

    constructor(
        private $rootScope,
        private $state: any,
        private projectManager: IProjectManager,
        private messageService: MessageService) {
        
        let id = $state.params["id"];

        let artifact = projectManager.getArtifact(id);
        if (artifact) {
            let artifactType = artifact.predefinedType;            
            this.navigateToSubRoute(artifactType, $state.params["context"]);
        } else {
            messageService.addError($rootScope.config.labels["Artifact_NotFound"]);
        }

    }

    public navigateToSubRoute(artifactType: Models.ItemTypePredefined, context: any) {
        switch (artifactType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.$state.go('main.artifact.diagram', { context: context });
                break;
            case Models.ItemTypePredefined.Glossary:
                this.$state.go('main.artifact.glossary', { context: context });
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                this.$state.go('main.artifact.general', { context: context });
                break;
            case Models.ItemTypePredefined.Process:
                this.$state.go('main.artifact.storyteller', { context: context });
                break;
            default:
                this.$state.go('main.artifact.details', { context: context });
        }
    }
}

