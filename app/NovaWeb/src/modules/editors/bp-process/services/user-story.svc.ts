import {IUserStory} from "../models/process-models";
import {ILocalizationService, IMessageService} from "../../../core";

export interface IUserStoryService {
    // getUserStoryId(userTaskId: number): number;
    generateUserStories(projectId: number, processId: number, userTaskId?: number): ng.IPromise<IUserStory[]>;
}

export class UserStoryService implements IUserStoryService {
    public lastGeneratedUserStories: IUserStory[];

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
     *  Returns user story id by specified user task id.
     */
    // public getUserStoryId(userTaskId: number) {
    //     if (this.lastGeneratedUserStories) {
    //         for (const userStory of this.lastGeneratedUserStories) {
    //             if (userStory.processTaskId === userTaskId) {
    //                 return userStory.id;
    //             }
    //         }
    //     }

    //     return null;
    // }

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
                    this.lastGeneratedUserStories = result.data;
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