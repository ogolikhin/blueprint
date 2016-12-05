import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {IProjectService} from "../../../managers/project-manager/project-service";
import {IArtifactManager, ISelection, IArtifactService} from "../../../managers/artifact-manager";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact/artifact";
import {ItemTypePredefined} from "../../../main/models/enums";
import {Models} from "../../../main/models";
import {HttpStatusCode} from "../../../core/http/http-status-code";
import {ApplicationError} from "../../../core/error/applicationError";

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
        if (artifact.predefinedType === ItemTypePredefined.Project) {
            this.setProjectBreadCrumb(artifact.id);
        } else {
            this.setArtifactBreadCrumb(artifact.id, artifact.artifactState.historical);
        }
    }

     private setProjectBreadCrumb = (projectId: number): void => {
        this.projectService.getProjectNavigationPath(projectId, false)
            .then((result: string[]) => {
                this.breadcrumbLinks = [];
                _.each(result, s => {
                    const breadcrumbLink: IBreadcrumbLink = {
                        // We do not need to navigate to Instance Folder
                        id: 0,
                        name: s,
                        isEnabled: false
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                });
            }, (reason: ApplicationError) => {
                if (reason.statusCode === HttpStatusCode.NotFound) {
                    this.breadcrumbLinks = [];
                }
            });
    }

    private setArtifactBreadCrumb = (artifactId: number, isHistorical: boolean): void => {
        this.artifactService.getArtifactNavigationPath(artifactId)
            .then((result: Models.IArtifact[]) => {
                this.breadcrumbLinks = [];
                _.each(result, artifact => {
                    const breadcrumbLink: IBreadcrumbLink = {
                        id: artifact.id,
                        name: artifact.name,
                        isEnabled: !isHistorical && artifact.id !== artifact.projectId
                    };
                    this.breadcrumbLinks.push(breadcrumbLink);
                });
            }, (reason: ApplicationError) => {
                if (reason.statusCode === HttpStatusCode.NotFound) {
                    this.breadcrumbLinks = [];
                }
            });
    }
}
