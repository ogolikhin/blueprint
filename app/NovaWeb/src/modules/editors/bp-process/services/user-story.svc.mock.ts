import {IUserStoryService} from "./user-story.svc";
import {IUserStory} from "../models/process-models";

export class UserStoryServiceMock implements IUserStoryService {
    public static $inject: string[] = [
        "$q"
    ];

    constructor(
        private $q: ng.IQService
    ) {
    }

    public generateUserStories(): ng.IPromise<IUserStory[]> {
        const deferred: ng.IDeferred<IUserStory[]> = this.$q.defer();
        deferred.resolve();
        return deferred.promise;
    }
}