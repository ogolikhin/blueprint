import * as angular from "angular";
import {Models} from "../main/models";
import {IArtifactManager} from "../managers";
import {IStatefulArtifact} from "../managers/artifact-manager";
import {IStatefulArtifactFactory} from "../managers/artifact-manager/artifact";
import {IMessageService} from "../shell";
import {INavigationService} from "../core/navigation";
import {IItemInfoService, IItemInfoResult} from "../core/navigation/item-info.svc";

export class ItemStateController {

    public static $inject = [
        "$state", 
        "artifactManager", 
        "messageService",
        "navigationService",
        "itemInfoService",
        "statefulArtifactFactory"
    ];

    constructor(private $state: angular.ui.IStateService,
                private artifactManager: IArtifactManager,
                private messageService: IMessageService,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService,
                private statefulArtifactFactory: IStatefulArtifactFactory) {

        let id = parseInt($state.params["id"], 10);

        // either gets a loaded artifact or loads if the artifact hasn't been loaded already
        // TODO: decide whether to use ArtifactManager cache or not
        artifactManager.get(id).then((artifact: IStatefulArtifact) => {
            if (!artifact) {
                throw new Error("Go to functionality is not implemented yet!!!");
            }
            artifact.unload();
            this.navigateToSubRoute(artifact.id, artifact.predefinedType);
        }).catch(error => {
            console.log("Artifact is not loaded");
            this.getItemInfo(id);
        });
    }

    public getItemInfo(id: number) {
        this.itemInfoService.get(id).then((result: IItemInfoResult) => {
            if (this.itemInfoService.isSubArtifact(result)) {
                // subartifact
                console.log("about to display subartifact, navigate to artifact: " + result.parentId);
                // this.navigationService.navigateTo(result.parentId);

            } else if (this.itemInfoService.isProject(result)) {
                // project
                console.log("about to display project");

            } else if (this.itemInfoService.isArtifact(result)) {
                // artifact
                console.log("about to display artifact");
                // this.statefulArtifactFactory.createStatefulArtifact
                this.navigateToSubRoute(result.id, result.predefinedType);

            } else {
                throw new Error("Invalid Id");
            }
        });
    }

    public navigateToSubRoute(artifactId: number, artifactType: Models.ItemTypePredefined) {
        const params = { context: artifactId };
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

    public navigateToSubRouteOld(artifactType: Models.ItemTypePredefined, artifact: IStatefulArtifact) {
        const params = {context: artifact.id};
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
