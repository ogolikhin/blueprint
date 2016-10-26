import {IUsersAndGroupsService, IUserOrGroupInfo} from "./users-and-groups.svc";
import {HttpStatusCode} from "../../core/http";

export class UserOrGroupInfo implements IUserOrGroupInfo {
    constructor(public name: string,
                public email: string,
                public isGroup: boolean = false,
                public guest: boolean = false,
                public isBlocked: boolean = false,
                public isLoginEnabled: boolean = true) {
    }

    public id: string;
}

export class UsersAndGroupsServiceMock implements IUsersAndGroupsService {
    public static result: IUserOrGroupInfo[];

    public static $inject = ["$q"];

    constructor(private $q: ng.IQService) {
    }

    public search(search: string, emailDiscussions: boolean = false): ng.IPromise<IUserOrGroupInfo[]> {
        const deferred = this.$q.defer<IUserOrGroupInfo[]>();

        if (search === "error") {
            deferred.reject({message: "Server Error", statusCode: HttpStatusCode.ServerError});
        } else if (search === "return@user.com") {
            const user = new UserOrGroupInfo("test name", "a@a.com", true, false, false);
            user.id = "id";
            deferred.resolve([user]);
        } else if (search === "dontreturn@user.com" || search === "dontreturn") {
            deferred.resolve([]);
        } else {
            deferred.resolve(UsersAndGroupsServiceMock.result);
        }
        return deferred.promise;
    }
}
