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

    public search(value: string, emailDiscussions: boolean = false): ng.IPromise<IUserOrGroupInfo[]> {
        var deferred = this.$q.defer<IUserOrGroupInfo[]>();

        this.$http.get<IUserOrGroupInfo[]>("/svc/shared/users/search", { params: { search: value, emailDiscussions: emailDiscussions } })
            .success((result) => {
                deferred.resolve(result);
            }).error((data: IHttpError, status: number) => {
                data.statusCode = status;
                deferred.reject(data);
            });

        return deferred.promise;
    }

}