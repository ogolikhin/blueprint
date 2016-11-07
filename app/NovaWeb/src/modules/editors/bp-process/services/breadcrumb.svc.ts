import {INavigationService, INavigationState} from "../../../core/navigation";
import {ItemTypePredefined} from "../../../main/models/enums";

export interface IArtifactReference {
    id: number;
    projectId: number;
    name: string;
    typePrefix: string;
    projectName: string;
    baseItemTypePredefined: ItemTypePredefined;
    version?: number;
    link: string;
}

export interface IBreadcrumbService {
    getReferences(): ng.IPromise<IArtifactReference[]>;
}

export class BreadcrumbService implements IBreadcrumbService {
    public static $inject: string[] = [
        "$q",
        "$http",
        "navigationService"
    ];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService,
        private navigationService: INavigationService
    ) {
    }

    public getReferences(): ng.IPromise<IArtifactReference[]> {
        const deferred = this.$q.defer();
        const navigationState: INavigationState = this.navigationService.getNavigationState();

        if (!navigationState.path || navigationState.path.length === 0) {
            deferred.reject();
        } else {
            let url = `/svc/shared/navigation/${navigationState.path.map(item => item.id).join("/")}/${navigationState.id}`;

            if (navigationState.version) {
                url = `${url}?versionId=${navigationState.version}`;
            }

            this.$http.get(url)
                .then((result) => {
                    deferred.resolve(result.data);
                })
                .catch((error) => {
                    deferred.reject(error);
                });
        }

        return deferred.promise;
    }
}
