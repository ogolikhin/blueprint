export class HttpError implements IHttpError {
    constructor(public message: string,
        public statusCode?: number,
        public errorCode?: number) {
    }
}

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

export class UserOrGroupInfo implements IUserOrGroupInfo {
    constructor(public name: string,
        public email: string,
        public isGroup: boolean = false,
        public guest: boolean = false,
        public isBlocked: boolean = false) {
    }

    public id: string;
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