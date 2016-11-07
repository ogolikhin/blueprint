import {IUserStory} from "../models/process-models";
import {ILocalizationService} from "../../../core";
import {IMessageService} from "../../../core/messages/message.svc";

export interface IUserStoryService {
    generateUserStories(projectId: number, processId: number, userTaskId?: number): ng.IPromise<IUserStory[]>;
}

export class UserStoryService implements IUserStoryService {
    public static $inject: string[] = [
        "$http",
        "$q",
        "localization",
        "messageService"
    ];

    constructor(
        private $http: ng.IHttpService,
        private $q: ng.IQService,
        private localization: ILocalizationService,
        private messageService: IMessageService
    ) {
    }

    /**
     * Generates user story artifacts for specific project and process
     */
    public generateUserStories(projectId: number, processId: number, userTaskId?: number): ng.IPromise<IUserStory[]> {
        let restPath = `/svc/components/storyteller/projects/${projectId}/processes/${processId}/userstories`;

        if (userTaskId) {
            restPath = `${restPath}?taskId=${userTaskId}`;
        }

        const deferred = this.$q.defer<IUserStory[]>();

        this.$http.post<IUserStory[]>(restPath, "")
            .then(
                (result: ng.IHttpPromiseCallbackArg<IUserStory[]>) => {
                    deferred.resolve(result.data);
                }
            ).catch(
                (error: ng.IHttpPromiseCallbackArg<IUserStory[]>) => {
                    deferred.reject(error.data);
                }
            );

        return deferred.promise;
    }
}
