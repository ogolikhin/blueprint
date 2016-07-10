module Storyteller {

    export class ChangeStateCommand implements ICommand {

        constructor(private $rootScope: ng.IRootScopeService,
            private $location: ng.ILocationService,
            private dialogService: Shell.IDialogService,
            private processModelService: IProcessModelService,
            private $window: ng.IWindowService,
            private $q: ng.IQService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService) {
        }

        public execute = (data: ICommandData) => {
            this.$rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName);
            let processId = data.processId;
            let url = data.url;
            let defer = this.$q.defer<boolean>();
            if (this.processModelService.isChanged) {
                data.event.preventDefault();
                var params: Shell.IDialogParams = {
                    message: this.getLabel("ST_Confirm_Save_Before_Continue"),
                    publishButton: this.getLabel("ST_Confirm_Save_PublishButton_Label"),
                    saveButton: this.getLabel("ST_Confirm_Save_SaveButton_Label"),
                    discardButton: this.getLabel("ST_Confirm_Save_DiscardButton_Label"),
                    discardDisabled: (this.processModelService.processModel == null || !this.processModelService.processModel.status.hasEverBeenPublished)
                };
                this.dialogService.action(params)
                    .then(() => {
                        if (params.buttonType === Shell.ButtonType.Save) {
                            this.processModelService.save().then(() => {
                                defer.resolve(true);
                            });
                        } else if (params.buttonType === Shell.ButtonType.Publish) {
                            this.artifactVersionControlService.publish(this.processModelService.processModel,
                                this.processModelService.isChanged).then(() => {
                                    this.processModelService.processModel.status.hasEverBeenPublished = true;
                                    this.removeLock(defer, data.model);
                                }, () => {
                                    this.removeLock(defer, data.model);
                                });
                        } else if (params.buttonType === Shell.ButtonType.Discard) {
                            this.artifactVersionControlService.discardArtifactChanges(
                                [
                                    {
                                        artifactId: this.processModelService.processModel.id,
                                        status: this.processModelService.processModel.status
                                    }]).then(() => {
                                        this.removeLock(defer, data.model);
                                    }, () => {
                                        this.removeLock(defer, data.model);
                                    }).then(() => {
                                        this.$rootScope.$broadcast("processChangesDiscarded");
                                    });;
                        }
                    });
            } else {
                defer.resolve(true);
            }

            return defer.promise;
        }

        private removeLock(defer: ng.IDeferred<boolean>, model: any) {
            if (model) {
                model.isLocked = false;
                model.isLockedByMe = false;
            }
            this.processModelService.isChanged = false;
            defer.resolve(true);
        }

        private getLabel(key: string, defaultValue?: string) {
            return ((<any>this.$rootScope).config.labels[key] || defaultValue);
        }
    }
}