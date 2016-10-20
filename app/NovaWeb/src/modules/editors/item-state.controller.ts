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

        artifactManager.get(id).then((artifact: IStatefulArtifact) => {
            artifact.unload();
            this.navigateToSubRoute(artifact);

        }).catch(error => {
            this.getItemInfo(id);
        });
    }

    public getItemInfo(id: number) {
        this.itemInfoService.get(id).then((result: IItemInfoResult) => {

            if (this.itemInfoService.isSubArtifact(result)) {
                // subartifact
                this.navigationService.navigateTo(result.id, true);

            } else if (this.itemInfoService.isProject(result)) {
                // project
                throw new Error("TODO: implement project navigation in a US");

            } else if (this.itemInfoService.isArtifact(result)) {
                // artifact
                const artifact: Models.IArtifact = {
                    id: result.id,
                    projectId: result.projectId,
                    name: result.name,
                    parentId: result.parentId,
                    predefinedType: result.predefinedType,
                    prefix: result.prefix,
                    version: result.version,
                    orderIndex: result.orderIndex,
                    lockedByUser: result.lockedByUser,
                    lockedDateTime: result.lockedDateTime,
                    permissions: result.permissions
                };
                const statefulArtifact = this.statefulArtifactFactory.createStatefulArtifact(artifact);
                this.artifactManager.addAsOrphan(statefulArtifact);
                this.navigateToSubRoute(statefulArtifact);

            } else {
                throw new Error("Invalid Id");
            }
        });
    }

    public navigateToSubRoute(artifact: IStatefulArtifact) {
        const params = { context: artifact.id };

        switch (artifact.predefinedType) {
            case Models.ItemTypePredefined.GenericDiagram:
            case Models.ItemTypePredefined.BusinessProcess:
            case Models.ItemTypePredefined.DomainDiagram:
            case Models.ItemTypePredefined.Storyboard:
            case Models.ItemTypePredefined.UseCaseDiagram:
            case Models.ItemTypePredefined.UseCase:
            case Models.ItemTypePredefined.UIMockup:
                this.$state.go("main.item.diagram", params);
                break;
            case Models.ItemTypePredefined.Glossary:
                this.$state.go("main.item.glossary", params);
                break;
            case Models.ItemTypePredefined.Project:
            case Models.ItemTypePredefined.CollectionFolder:
                this.$state.go("main.item.general", params);
                break;
            case Models.ItemTypePredefined.ArtifactCollection:            
                this.$state.go("main.item.collection", params);
                break;
            case Models.ItemTypePredefined.Process:
                this.$state.go("main.item.process", params);
                break;
            default:
                this.$state.go("main.item.details", params);
        }
    }
}
