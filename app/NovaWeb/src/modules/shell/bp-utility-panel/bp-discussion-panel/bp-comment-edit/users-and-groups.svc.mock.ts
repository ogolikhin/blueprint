import { IUsersAndGroupsService, IUserOrGroupInfo } from "./users-and-groups.svc";

export class UserOrGroupInfo implements IUserOrGroupInfo {
    constructor(public name: string,
        public email: string,
        public isGroup: boolean = false,
        public guest: boolean = false,
        public isBlocked: boolean = false) {
    }

    public id: string;
}

export class UsersAndGroupsServiceMock implements IUsersAndGroupsService {
    public static result: IUserOrGroupInfo[];

    public static $inject = ["$q"];
    constructor(private $q: ng.IQService) {
    }

    public search(search: string, emailDiscussions: boolean = false): ng.IPromise<IUserOrGroupInfo[]> {
        var deferred = this.$q.defer<IUserOrGroupInfo[]>();

        if (search === "error") {
            deferred.reject({ message: "Server Error", statusCode: 500 });
        } else if (search === "return@user.com") {
            var user = new UserOrGroupInfo("test name", "a@a.com", true, false, false);
            user.id = "id";
            deferred.resolve([user]);
        } else if (search === "dontreturn@user.com" || search === "dontreturn") {
            deferred.resolve([]);
        }
        else {
            deferred.resolve(UsersAndGroupsServiceMock.result);
        }
        return deferred.promise;
    }
}