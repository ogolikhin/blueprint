import {IMainBreadcrumbService} from "./mainbreadcrumb.svc";
import {IBreadcrumbLink} from "../../../shared/widgets/bp-breadcrumb/breadcrumb-link";
import {IStatefulArtifact} from "../../../managers/artifact-manager/artifact";

export class MainBreadcrumbServiceMock implements IMainBreadcrumbService {

    public breadcrumbLinks: IBreadcrumbLink[];

    constructor() {
        this.breadcrumbLinks = [];
    }

    public reloadBreadcrumbs(artifact: IStatefulArtifact) {
        this.breadcrumbLinks = [];
    }
}
