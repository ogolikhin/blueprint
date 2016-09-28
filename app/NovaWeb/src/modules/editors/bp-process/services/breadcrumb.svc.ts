import {INavigationState} from "../../../core/navigation/navigation-state";
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
    getReferences(navigationState: INavigationState): ng.IPromise<IArtifactReference[]>;
}

export class BreadcrumbService implements IBreadcrumbService {
    public static $inject: string[] = [
        "$q",
        "$http"
    ];

    constructor(
        private $q: ng.IQService,
        private $http: ng.IHttpService
    ) {
    }

    public getReferences<T>(navigationState: INavigationState): ng.IPromise<T[]> {
        const deferred = this.$q.defer();
        let url = "/svc/shared/navigation/"; 
        
        if (navigationState.path) {
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