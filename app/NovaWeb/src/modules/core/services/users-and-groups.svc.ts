import { IHttpInterceptorConfig } from "../http";
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
    isLoginEnabled: boolean;
}

export interface IUsersAndGroupsService {
    search(
        value?: string,
        emailDiscussions?: boolean,
        limit?: number,
        includeGuests?: boolean
    ): ng.IPromise<IUserOrGroupInfo[]>;
}

export class UsersAndGroupsService implements IUsersAndGroupsService {
    public static $inject = ["$http", "$q"];
    constructor(private $http: ng.IHttpService, private $q: ng.IQService) {
    }

    private createRequestConfig(
        value?: string,
        emailDiscussions?: boolean,
        limit?: number,
        includeGuests?: boolean
    ): ng.IRequestConfig {
        let config = <IHttpInterceptorConfig>{ };
        config.params = {};
        if (value) {
            config.params.search = value;
        }
        if (emailDiscussions) {
            config.params.emailDiscussions = emailDiscussions;
        }
        if (limit) {
            config.params.limit = limit;
        }
        if (includeGuests === false || includeGuests === true) {
            config.params.includeGuests = includeGuests;
        }
        config.dontRetry = true;
        return config;
    }

    public search(
        value?: string,
        emailDiscussions?: boolean, //webservice default = false
        limit?: number,             //webservice default = 5
        includeGuests?: boolean     //webservice default = true
    ): ng.IPromise<IUserOrGroupInfo[]> {
        let deferred = this.$q.defer<IUserOrGroupInfo[]>();
        let sanitizedValue = encodeURI(value).replace(/%20/g, " "); // we revert the encoding of space (%20)
        let requestConfig = this.createRequestConfig(
            sanitizedValue,
            emailDiscussions,
            limit,
            includeGuests
        );
        this.$http.get<IUserOrGroupInfo[]>("/svc/shared/users/search", requestConfig)
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