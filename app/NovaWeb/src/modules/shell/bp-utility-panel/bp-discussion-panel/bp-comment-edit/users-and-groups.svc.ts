import { IHttpInterceptorConfig } from "../../../error/http-error-interceptor";
export interface IHttpError {
    message: string;
    statusCode: number; // client side only
    errorCode: number;
}

export interface IUserOrGroupInfo {
    id: string;
    name: string;
    email: string;
    isGroup: boolean;
    guest: boolean;
    isBlocked: boolean;
}

export interface IUsersAndGroupsService {
    search(value: string): ng.IPromise<IUserOrGroupInfo[]>;
    search(value: string, emailDiscussions: boolean): ng.IPromise<IUserOrGroupInfo[]>;
}

export class UsersAndGroupsService implements IUsersAndGroupsService {
    public static $inject = ["$http", "$q"];
    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    private createRequestConfig(value: string, emailDiscussions: boolean = false): ng.IRequestConfig {
        var config = <IHttpInterceptorConfig>{ };
        config.params = { search: value, emailDiscussions: emailDiscussions };
        config.dontRetry = true;
        return config;
    }

    public search(value: string, emailDiscussions: boolean = false): ng.IPromise<IUserOrGroupInfo[]> {
        var deferred = this.$q.defer<IUserOrGroupInfo[]>();
        var sanitizedValue = encodeURI(value);
        this.$http.get<IUserOrGroupInfo[]>("/svc/shared/users/search", this.createRequestConfig(sanitizedValue, emailDiscussions))
            .then((result: ng.IHttpPromiseCallbackArg<IUserOrGroupInfo[]>) => {
                deferred.resolve(result.data);
            }, (result: ng.IHttpPromiseCallbackArg<any>) => {
                const error = {
                    statusCode: result.status,
                    message: (result.data ? result.data.message : "")
                };
                deferred.reject(error);
            });
        return deferred.promise;
    }

}