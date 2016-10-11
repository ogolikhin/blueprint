import "angular";
import {Models} from "../models";
import {IArtifactManager} from "../../managers";
import {IStatefulArtifact} from "../../managers/artifact-manager";
import {MessageService} from "../../shell";

export class ArtifactState implements ng.ui.IState {
    public url = "/{id:any}?{path:string}";
    public template = "<div ui-view class='artifact-state'></div>";
    public controller = "artifactStateController";
    public reloadOnSearch = false;
}

export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state", "artifactManager", "messageService"];

    constructor(private $rootScope,
                private $state: angular.ui.IStateService,
                private artifactManager: IArtifactManager,
                private messageService: MessageService) {
        let id = parseInt($state.params["id"], 10);

        // either gets a loaded artifact or loads if the artifact hasn't been loaded already
        artifactManager.get(id).then((artifact: IStatefulArtifact) => {
            if (!artifact) {
                throw new Error("Go to functionality is not implemented yet!!!");
            }
            artifact.unload();
            this.navigateToSubRoute(artifact.predefinedType, artifact);
        });
    }

    public navigateToSubRoute(artifactType: Models.ItemTypePredefined, artifact: IStatefulArtifact) {
        var params = {context: artifact.id};
        switch (artifactType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.$state.go("main.artifact.diagram", params);
                break;
            case Models.ItemTypePredefined.Glossary:
                this.$state.go("main.artifact.glossary", params);
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                this.$state.go("main.artifact.general", params);
                break;
            case Models.ItemTypePredefined.Process:
                this.$state.go("main.artifact.process", params);
                break;
            default:
                this.$state.go("main.artifact.details", params);
        }
    }
}
