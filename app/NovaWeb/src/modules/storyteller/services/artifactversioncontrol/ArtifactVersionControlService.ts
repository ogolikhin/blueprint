module Shell {

    export class DiscardResultsEntry {
        public ErrorCode: number;
        public Message: string;
        public ArtifactId: number;
        public ProjectId: number;
    }

    export class DiscardResultsInfo {
        public DiscardResults: DiscardResultsEntry[];
    }

    export class ArtifactVersionControlService implements IArtifactVersionControlService{
        public static $inject = ["$http", "$q", "$rootScope", "messageService", "busyIndicatorService", "processModelService"];
        constructor(private $http: ng.IHttpService,
            private $q: ng.IQService,
            private $rootScope: ng.IRootScopeService,
            private messageService: IMessageService,
            private busyIndicatorService: IBusyIndicatorService,
            private processModelService: Storyteller.IProcessModelService) {
        }

        public discardArtifactChanges(artifacts: IArtifactVersionControlServiceRequest[]): ng.IPromise<DiscardResultsInfo> {
            var restPath = this.discardRestPath();
            var deferred = this.$q.defer<any>();

            if (artifacts.some(r => { return r.status && r.status.isReadOnly; })) {
                var message = new Message(MessageType.Error, this.$rootScope["config"].labels["ST_View_OpenedInReadonly_Message"]);
                this.messageService.addMessage(message);
                deferred.reject();
                return deferred.promise;
            }

            var artifactIds: number[] = artifacts.map(r => { return r.artifactId;});

            this.$http.post<any>(restPath, angular.toJson(artifactIds)).success((result) => {
                var discardChangesInfo = {DiscardResults: []};
                for (var entry of result.discardResults) {
                    discardChangesInfo.DiscardResults.push({ErrorCode: entry.errorCode, Message: entry.message, ArtifactId: entry.artifactId, ProjectId: entry.projectId});
                }
                deferred.resolve(discardChangesInfo);
            }).error((err: IHttpError, status: number) => {
                err.statusCode = status;
                deferred.reject(err);
            });
            return deferred.promise;
        }

        private discardRestPath(): string {
            var restPath = "/svc/shared/artifacts/discard";
            return restPath;
        }

        public publish(model: IProcess, isChanged: boolean): ng.IPromise<boolean> {
            this.messageService.clearMessages();
            if (isChanged) {
                this.busyIndicatorService.beginGlobalBusyOperation();
                return this.processModelService.save().then((result: IProcess) => {
                    this.$rootScope.$broadcast("processSaved", result);
                    return this.publishProcess(model);
                }).finally(() => {
                    this.busyIndicatorService.endGlobalBusyOperation();
                });
            }
            else {
                return this.publishProcess(model);
            }
        }

        public publishProcess(model: IProcess): ng.IPromise<boolean> {
            this.messageService.clearMessages();
            var restPath = this.publishRestPath();
            var promise = this.publishArtifacts([{artifactId: model.id, status: model.status}]);
            promise.then((result: boolean) => {
                var processSavedGeneratedMessage = this.$rootScope["config"].labels["ST_Process_Successfully_Publised_Message"];
                if (this.isThereAnyUserStory())
                    processSavedGeneratedMessage = processSavedGeneratedMessage + this.$rootScope["config"].labels["ST_Process_Successfully_Publised_Synch_Message"];
                this.messageService.addMessage(new Message(MessageType.Success, processSavedGeneratedMessage));

                this.processModelService.isUnpublished = !result;
            });
            return promise;
        }

        public publishArtifacts(artifacts: IArtifactVersionControlServiceRequest[]): ng.IPromise<boolean> {
            this.messageService.clearMessages();
            var restPath = this.publishRestPath();

            var deferred = this.$q.defer<boolean>();

            if (artifacts.some(r => { return r.status && r.status.isReadOnly; })) {
                var message = new Shell.Message(Shell.MessageType.Error, this.$rootScope["config"].labels["ST_View_OpenedInReadonly_Message"]);
                this.messageService.addMessage(message);
                deferred.reject();
                return deferred.promise;
            }

            var artifactIds: number[] = artifacts.map(r => { return r.artifactId; });

            this.$http.post<boolean>(restPath, angular.toJson(artifactIds)).success((result: boolean) => {

                deferred.resolve(result);

            }).error((err: IHttpError, status: number) => {

                err.statusCode = status;
                deferred.reject(err);
                });

            return deferred.promise;

        }

        private isThereAnyUserStory = (): boolean => {
            for (var i = 0; i < this.processModelService.processModel.shapes.length; i++) {
                var propertyValues = <any>this.processModelService.processModel.shapes[i].propertyValues;
                if (propertyValues.storyLinks && propertyValues.storyLinks.value) {
                    return true;
                }
            }
            return false;
        }

        private publishRestPath(): string {
            return "/svc/shared/artifacts/publish";
        }

        private isModelReadOnly(model: IProcess): boolean {
            return model && model.status && model.status.isReadOnly;
        }

        public lock(artifacts: IProcess[]): ng.IPromise<ILockResultInfo[]> {
            
            var restPath = this.lockRestPath();
            var deferred = this.$q.defer<ILockResultInfo[]>();

            if (artifacts.some(r => { return this.isModelReadOnly(r); })) {
                var message = new Message(MessageType.Error, this.$rootScope["config"].labels["ST_View_OpenedInReadonly_Message"]);
                this.messageService.addMessage(message);
                deferred.reject();
                return deferred.promise;
            }

            var processIds : number[] = artifacts.map(r => { return r.id });

            this.$http.post<ILockResultInfo[]>(restPath, processIds).success((result: ILockResultInfo[]) => {

                deferred.resolve(result);

            }).error((err: IHttpError, status: number) => {

                err.statusCode = status;
                deferred.reject(err);
            });

            return deferred.promise;
        }

        private lockRestPath(): string {
            return "/svc/shared/artifacts/lock";
        }

        
    }
    var app = angular.module("Shell");
    app.service("artifactVersionControlService", ArtifactVersionControlService);
}