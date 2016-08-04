import "angular";
import * as Models from "../models/models";
import {IProjectManager, ProjectManager} from "../";
import {ILocalizationService} from "../../core";
import {MessageService} from "../../shell";

export interface IEditorParameters {
    context: Models.IEditorContext;
}

export class ArtifactState implements ng.ui.IState {
    public url = "/{id:any}";
    public template = "<div ui-view></div>";
    public controller = "artifactStateController";
    public params = { context: null };
}

export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state", "projectManager", "messageService", "localization"];

    constructor(
        private $rootScope,
        private $state: angular.ui.IStateService,
        private projectManager: IProjectManager,
        private messageService: MessageService,
        private localization: ILocalizationService) {
        
        let id = $state.params["id"];

        let artifact = projectManager.getArtifact(id);
        if (artifact) {
            let artifactType = artifact.predefinedType;           
            
            this.navigateToSubRoute(artifactType, $state.params["context"]);
        } else {
            messageService.addError(this.localization.get("Artifact_NotFound"));
        }

    }

    public navigateToSubRoute(artifactType: Models.ItemTypePredefined, context: Models.IEditorContext) {
        let parameters: IEditorParameters = { context: context };
        switch (artifactType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.$state.go("main.artifact.diagram", parameters);
                break;
            case Models.ItemTypePredefined.Glossary:
                this.$state.go("main.artifact.glossary", parameters);
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                this.$state.go("main.artifact.general", parameters);
                break;
            case Models.ItemTypePredefined.Process:
                this.$state.go("main.artifact.storyteller", parameters);
                break;
            default:
                this.$state.go("main.artifact.details", parameters);
        }
    }
}

