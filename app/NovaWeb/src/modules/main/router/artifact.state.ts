﻿import "angular";
import * as Models from "../models/models";
import {IProjectManager, ISelectionManager, } from "../";
import {SelectionSource} from "../services/selection-manager";
import {ILocalizationService} from "../../core";
import {MessageService} from "../../shell";

export interface IEditorParameters {
    context: Models.IEditorContext;
}

export class ArtifactState implements ng.ui.IState {
    public url = "/{id:any}";
    public template = "<div ui-view></div>";
    public controller = "artifactStateController";
    public reloadOnSearch = false;
}

export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state", "projectManager", "selectionManager", "messageService", "localization"];

    constructor(
        private $rootScope,
        private $state: angular.ui.IStateService,
        private projectManager: IProjectManager,
        private selectionManager: ISelectionManager,
        private messageService: MessageService,
        private localization: ILocalizationService) {
        
        let id = parseInt($state.params["id"], 10);

        // TODO: if project manager can't find artifact, need to load artifact by itself (should be covered in 'go to' user story)
        let artifact = projectManager.getArtifact(id);
        if (artifact) {
            let artifactType = artifact.predefinedType;
            if (selectionManager.selection &&
                selectionManager.selection.artifact &&
                selectionManager.selection.artifact.id !== artifact.id) {

                selectionManager.selection = {
                    source: SelectionSource.Explorer,
                    artifact: artifact
                };
            }
            let context: Models.IEditorContext = {};
            context.artifact = artifact;
            context.type = projectManager.getArtifactType(artifact);      

            this.navigateToSubRoute(artifactType, context);

        } else {
            //TODO: to restore error message when then user story "GO TO Artifact"is comleted
            //messageService.addError(this.localization.get("Artifact_NotFound"));
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

