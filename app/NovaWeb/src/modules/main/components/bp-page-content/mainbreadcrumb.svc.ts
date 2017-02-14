import {Models} from "../../../main/models";
import {IArtifactService} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {IArtifactState} from "../../../managers/artifact-manager/state/state";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {ItemTypePredefined} from "../../models/item-type-predefined";

export interface IMainBreadcrumbService {
    breadcrumbLinks: IBreadcrumbLink[];
    reloadBreadcrumbs(artifact: IStatefulArtifact);
}

export class MainBreadcrumbService implements IMainBreadcrumbService {

    public breadcrumbLinks: IBreadcrumbLink[];

    public static $inject = [
        "artifactService",
        "projectService"
    ];

    constructor(private artifactService: IArtifactService,
                private projectService: IProjectService) {
        this.breadcrumbLinks = [];
    }

    public reloadBreadcrumbs(artifact: IStatefulArtifact) {
        this.breadcrumbLinks = [];
        if (artifact.predefinedType === ItemTypePredefined.Project) {
            this.setProjectBreadCrumb(artifact.id);
        } else {
            this.setArtifactBreadCrumb(artifact.id, artifact.artifactState);
        }
    }

     private setProjectBreadCrumb = (projectId: number): void => {
        this.projectService.getProjectNavigationPath(projectId, false)
            .then((result: string[]) => {
                this.breadcrumbLinks = _.map(result, s => {
                    return {
                        // We do not need to navigate to Instance Folder
                        id: 0,
                        name: s,
                        isEnabled: false
                    };
                });
            });
    };

    private setArtifactBreadCrumb = (artifactId: number, artifactState: IArtifactState): void => {
        if (!artifactState.deleted) {
            this.artifactService.getArtifactNavigationPath(artifactId)
                .then((result: Models.IArtifact[]) => {
                    this.breadcrumbLinks = _.map(result, artifact => {
                        return {
                            id: artifact.id,
                            name: artifact.name,
                            isEnabled: !artifactState.historical
                        };
                    });
                });
        }
    }
}
