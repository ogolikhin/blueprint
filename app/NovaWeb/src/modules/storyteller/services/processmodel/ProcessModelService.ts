module Storyteller {

    /**
     * Process Model Service:
     * 
     * get process model from the Web API and encapsulate the data model
     * in a service that can be injected into application components
     * 
     */
    export class ProcessModelService implements IProcessModelService {
        public static $inject = ["$http", "$q", "$rootScope", "messageService", "processModelProcessor", "busyIndicatorService", "loginService"];
        public exceptionHandler: Shell.ExceptionHandler;
        constructor(private $http: ng.IHttpService,
            private $q: ng.IQService,
            private $rootScope: ng.IRootScopeService,
            private messageService: Shell.IMessageService,
            private processModelProcessor: IProcessModelProcessor,
            private busyIndicatorService: Shell.IBusyIndicatorService,
            private loginService: Shell.LoginService) {
            
            this.isChanged = false;
            this.exceptionHandler = new Shell.ExceptionHandler(messageService, $rootScope);
        }

        private processPathQueryParameterSection: string = "?versionId=";
        private changedProcessShapes: number[];

        public processModel: IProcess;
        public isChanged: boolean;

        public set isUnpublished(value: boolean) {
            if (!this.processModel) {
                return;
            }
            this.processModel.status.isUnpublished = value;
        }

        public get isUnpublished(): boolean {
            if (!this.processModel) {
                return false;
            }
            return this.isChanged || this.processModel.status.isUnpublished;
        }

        public get licenseType(): Shell.LicenseTypeEnum {
            if (!this.loginService.userInfo) {
                return null;
            }
            return this.loginService.userInfo.licenseType;
        }
        /**
        *  loads a process model from the server 
        */
        public load(processId: string, versionId?: number, revisionId?: number, baselineId?: number, readOnly?: boolean): ng.IPromise<IProcess> {
            var deferred = this.$q.defer<IProcess>();
            this.messageService.clearMessages();
            this.isChanged = false;

            let queryParamData = {
                versionId: isNaN(versionId) ? null : versionId,
                revisionId: isNaN(revisionId) ? null : revisionId,
                baselineId: isNaN(baselineId) ? null : baselineId,
                readOnly: !readOnly ? null : true //Do not send ?readOnly=false query parameter for normal calls
            };

            //Create parameters
            var requestConfig = {
                params: queryParamData
            };

            var restPath = this.processRestPath(processId);

            this.$http.get<IProcess>(restPath, requestConfig).success((result: IProcess) => {

                this.processModel = result;

                // TODO: remove once the information comes with the model
                this.processModel["versionId"] = queryParamData.versionId;
                this.processModel["revisionId"] = queryParamData.revisionId;
                this.processModel["baselineId"] = queryParamData.baselineId;

                deferred.resolve(this.processModel);

            }).error((err: Shell.IHttpError, status: number) => {
                err.statusCode = status;
                deferred.reject(err);
            });
            return deferred.promise;
        }

        public save(): ng.IPromise<IProcess> {

            this.messageService.clearMessages();

            var restPath = this.updateRestPath(this.processModel.id);
            var deferred = this.$q.defer<IProcess>();

            if (this.isModelReadOnly()) {
                var message = new Shell.Message(Shell.MessageType.Error, this.$rootScope["config"].labels["ST_View_OpenedInReadonly_Message"]);
                this.messageService.addMessage(message);
                deferred.reject();
                return deferred.promise;
            }

            var procModel: IProcess = this.processModelProcessor.processModelBeforeSave(this.processModel);

            this.$http.patch<IProcessUpdateResult>(restPath, procModel).success((result: IProcessUpdateResult) => {

                this.isChanged = false;
                const processSavedGeneratedMessage = this.$rootScope["config"].labels["ST_Process_Saved_Message"];
                this.messageService.addMessage(new Shell.Message(Shell.MessageType.Success, processSavedGeneratedMessage));

                if (result.messages) {
                    result.messages.forEach((r) => {
                        var message = new Shell.Message(Shell.MessageType.Warning, r.message);
                        this.messageService.addMessage(message);
                    });
                }
                this.processModel = null;
                this.processModel = result.result;

                deferred.resolve(result.result);

            }).error((err: Shell.IHttpError, status: number) => {
                this.exceptionHandler.outputClientSideMessages(err, status);
                err.statusCode = status;
                deferred.reject(err);
            });

            return deferred.promise;
        }

        public getNextNode(node: ISystemTaskShape): IProcessShape {
            if (this.processModel) {
                for (let link of this.processModel.links) {
                    if (link.sourceId === node.id) {
                        for (let shape of this.processModel.shapes) {
                            if (shape.id === link.destinationId) {
                                return shape;
                            }
                        }
                        return undefined;
                    }
                }
            }
            return undefined;
        }

        public setNextNode(node: ISystemTaskShape, value: IProcessShape) {
            if (this.processModel && value) {
                let i: number;
                for (i = 0; i < this.processModel.links.length; i++) {
                    if (this.processModel.links[i].sourceId === node.id) {
                        break;
                    }
                }
                this.processModel.links[i] = new ProcessLinkModel(node.id, value.id);
            }
        }

        public getNextNodes(userTask: IUserTaskShape): IProcessShape[] {
            if (this.processModel) {
                return this.processModel.shapes.filter(shape => {
                    if (shape.id !== userTask.id && shape.baseItemTypePredefined !== ItemTypePredefined.None) {
                        switch (shape.propertyValues["clientType"].value) {
                            case ProcessShapeType.UserTask:
                            case ProcessShapeType.UserDecision:
                            case ProcessShapeType.End:
                                return true;
                        }
                    }
                    return false;
                });
            }
            return [];
        }

        public dispose() {
            this.processModel = null;
        }

        private isModelReadOnly(): boolean {
            return this.processModel && this.processModel.status && this.processModel.status.isReadOnly;
        }

        /**
        * Returns all processes in specified project
        */
        public getProcesses(projectId: number): ng.IPromise<IArtifactReference[]> {
            this.messageService.clearMessages();
            const restPath = `/svc/components/storyteller/projects/${projectId}/processes`;
            var deferred = this.$q.defer<IArtifactReference[]>();
            this.$http.get<IArtifactReference[]>(restPath).success((processes: IArtifactReference[]) => {
                deferred.resolve(processes);
            }).error((err: Shell.IHttpError, status: number) => {

                err.statusCode = status;
                deferred.reject(err);
            });

            return deferred.promise;
        }

        public isUserToSystemProcess(): boolean {
            return this.processModel != null && this.processModel.propertyValues["clientType"].value === ProcessType.UserToSystemProcess;
        }

        public updateProcessType(systemTaskVisibilityEnabled: boolean) {
            if (systemTaskVisibilityEnabled) {
                this.processModel.propertyValues["clientType"].value = ProcessType.UserToSystemProcess;
            }
            else {
                this.processModel.propertyValues["clientType"].value = ProcessType.BusinessProcess;
            }

            this.isChanged = true;
        }

        /**
        * Return a relative url to request process model from the web API
        * svc/components/storyteller/processes/{*id}
        *
        * Note: versionId is not supported at this time 
        */
        private processRestPath(processId: string): string {
            return  "/svc/components/storyteller/processes/" + processId;
        }

        private updateRestPath(processId: number): string {
            return "/svc/components/storyteller/processes/" + processId;
        }




    }

    var app = angular.module("Storyteller");
    app.service("processModelService", ProcessModelService);
}
