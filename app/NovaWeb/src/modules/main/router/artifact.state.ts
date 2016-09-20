import "angular";
import { Models } from "../models";
import { IArtifactManager, SelectionSource} from "../../managers";
import { IStatefulArtifact } from "../../managers/models";
import { MessageService} from "../../shell";

export class ArtifactState implements ng.ui.IState {
    public url = "/{id:any}?{path:string}";
    public template = "<div ui-view class='artifact-state'></div>";
    public controller = "artifactStateController";
    public reloadOnSearch = false;
}

export class ArtifactStateController {

    public static $inject = ["$rootScope", "$state", "artifactManager", "messageService"];

    constructor(
        private $rootScope,
        private $state: angular.ui.IStateService,
        private artifactManager: IArtifactManager,
        private messageService: MessageService) {
        
        let id = parseInt($state.params["id"], 10);

        // TODO: if project manager can't find artifact, need to load artifact by itself (should be covered in 'go to' user story)
        artifactManager.get(id)
            .then((artifact: IStatefulArtifact) => {
                if (artifact) {
                    artifactManager.selection.setArtifact(artifact);
                    this.navigateToSubRoute(artifact.predefinedType);
                }
            });
    }

    public navigateToSubRoute(artifactType: Models.ItemTypePredefined) {
        let parameters = {};
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
                this.$state.go("main.artifact.process", parameters);
                break;
            default:
                this.$state.go("main.artifact.details", parameters);
        }
    }
}
