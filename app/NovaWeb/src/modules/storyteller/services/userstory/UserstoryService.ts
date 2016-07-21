module Storyteller {

    export class UserstoryService implements IUserstoryService {
        public static $inject = ["$http", "$q", "$rootScope", "messageService"];

        public lastGeneratedUserStories: IUserStory[];

        constructor(private $http: ng.IHttpService,
                    private $q: ng.IQService,
                    private $rootScope: ng.IRootScopeService,
                    private messageService: Shell.IMessageService) {
        }

        /**
        *  Generates user story artifacts for specific project and process
        */
        public generateUserstories(projectId: number, processId: number, taskId?: number): ng.IPromise<IUserStory[]> {

            this.messageService.clearMessages();
            var restPath = `/svc/components/storyteller/projects/${projectId}/processes/${processId}/userstories`;
            if (taskId)
                restPath = restPath + `?taskId=${taskId}`;
            var deferred = this.$q.defer<IUserStory[]>();

            this.$http.post<any>(restPath, "").success((result: IUserStory[]) => {
                this.lastGeneratedUserStories = result;
                this.$rootScope.$broadcast("generateUserStories", taskId);
                deferred.resolve(result);
                const userStoriesGeneratedMessage = (taskId) ? this.$rootScope["config"].labels["ST_User_Story_Generated_Message"] : this.$rootScope["config"].labels["ST_User_Stories_Generated_Message"];
                this.messageService.addMessage(new Shell.Message(Shell.MessageType.Success, userStoriesGeneratedMessage));
            }).error((err: Shell.IHttpError, status: number) => {

                err.statusCode = status;
                deferred.reject(err);
            });

            return deferred.promise;
        }

        /**
        *  Returns user story id by specified user task id.
        */
        public getUserStoryId(userTaskId: number) {
            if (this.lastGeneratedUserStories) {
                for (let userstory of this.lastGeneratedUserStories) {
                    if (userstory.processTaskId === userTaskId) {
                        return userstory.id;
                    }
                }
            }
            return null;
        }
    }

    var app = angular.module("Storyteller");
    app.service("userstoryService", UserstoryService);
}
