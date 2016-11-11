import {ItemTypePredefined} from "../../../main/models/enums";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {INavigationState} from "../../../core/navigation/navigation-state";

export interface IPathItem {
    id: number;
    name?: string;
    version?: number;
    accessible?: boolean;
}

export interface IBreadcrumbService {
    getReferences(): ng.IPromise<IPathItem[]>;
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

    public getReferences(): ng.IPromise<IPathItem[]> {
        const deferred = this.$q.defer();
        const navigationState: INavigationState = this.navigationService.getNavigationState();

        if (!navigationState.path || navigationState.path.length === 0) {
            deferred.reject();
        } else {
            let url = `svc/bpartifactstore/process/breadcrumb`;
            
            let pathItems: IPathItem[] = [];
            navigationState.path.forEach(item => pathItems.push({ id: item.id, version: item.version }));
            pathItems.push({ id: navigationState.id, version: navigationState.version });

            this.$http.post(url, pathItems)
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
