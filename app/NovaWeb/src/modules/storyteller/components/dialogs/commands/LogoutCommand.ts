module Storyteller {

    export class LogoutCommand implements ICommand {

        constructor(private $rootScope: ng.IRootScopeService,
            private $location: ng.ILocationService,
            private dialogService: Shell.IDialogService,
            private processModelService: IProcessModelService,
            private $q: ng.IQService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService) {
        }

        public execute = (data: ICommandData) => {
            this.$rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName);
            var model = data.model;
            var event = data.event;
            var defer = this.$q.defer<boolean>();

            const params: Shell.IDialogParams = {
                message: this.getLabel("ST_Confirm_Save_Before_Logout"),
                publishButton: this.getLabel("ST_Confirm_Save_PublishButton_Label"),
                saveButton: this.getLabel("ST_Confirm_Save_SaveButton_Label"),
                discardButton: this.getLabel("ST_Confirm_Save_DiscardButton_Label"),
                discardDisabled: (this.processModelService.processModel == null || !this.processModelService.processModel.status.hasEverBeenPublished),
                saveDisabled: !new SaveProcessCommand(null, null, this.processModelService).canExecute()
            };
            if (model.isLocked && model.isLockedByMe) {
                event.preventDefault();
                this.dialogService.action(params)
                    .then(() => {
                        if (params.buttonType === Shell.ButtonType.Publish) {
                            this.artifactVersionControlService.publish(this.processModelService.processModel,
                                this.processModelService.isChanged).then((result: boolean) => {
                                    this.processModelService.processModel.status.hasEverBeenPublished = true;
                                    this.removeLock(defer, model);
                                }, (error) => {
                                    this.removeLock(defer, model);
                                });
                        } else if (params.buttonType === Shell.ButtonType.Save) {
                            this.processModelService.save().then((result: IProcess) => {
                                this.removeLock(defer, model);
                            }, (error) => {
                                this.removeLock(defer, model);
                            });
                        } else if (params.buttonType === Shell.ButtonType.Discard) {
                            this.artifactVersionControlService.discardArtifactChanges(
                                [
                                    {
                                        artifactId: this.processModelService.processModel.id,
                                        status: this.processModelService.processModel.status
                                    }]).then(() => {
                                        this.removeLock(defer, model);
                                    }, (error) => {
                                        this.removeLock(defer, model);
                                    });
                        }
                    });
            } else {
                defer.resolve(true);
            }
            return defer.promise;
        }

        private removeLock(defer: ng.IDeferred<boolean>, model: any) {
            model.isLocked = false;
            model.isLockedByMe = false;
            this.processModelService.isChanged = false;
            defer.resolve(true);
        }


        private getLabel(key: string, defaultValue?: string) {
            return ((<any>this.$rootScope).config.labels[key] || defaultValue);
        }
    }
}
