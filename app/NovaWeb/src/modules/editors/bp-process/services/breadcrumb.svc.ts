import {INavigationService, INavigationState} from "../../../core/navigation";
import {ItemTypePredefined} from "../../../main/models/enums";

export interface IArtifactReference {
    id: number;
    projectId: number;
    name: string;
    typePrefix: string;
    projectName: string;
    baseItemTypePredefined: ItemTypePredefined;
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

        let url = "/svc/shared/navigation/";
        const navigationState: INavigationState = this.navigationService.getNavigationState();

        if (navigationState.path && navigationState.path.length > 0) {
            url = `${url}${navigationState.path.join("/")}/${navigationState.id}`;
        } else {
            url = `${url}${navigationState.id}`;
        }

        this.$http.get(url)
            .then((result) => {
                deferred.resolve(result.data);
            })
            .catch((error) => {
                deferred.reject(error);
            });

        return deferred.promise;
    }
}
