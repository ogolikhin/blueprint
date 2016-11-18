import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";
import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";

export class MainBreadcrumbServiceMock implements IMainBreadcrumbService {

    public breadcrumbLinks: IBreadcrumbLink[];

    constructor() {
        this.breadcrumbLinks = [];
    }

    public reloadBreadcrumbs(artifact: IStatefulArtifact) {
        const breadcrumb: IBreadcrumbLink = { id: artifact.id, isEnabled: true, name: artifact.name};
        this.breadcrumbLinks = [breadcrumb];
    }
}
