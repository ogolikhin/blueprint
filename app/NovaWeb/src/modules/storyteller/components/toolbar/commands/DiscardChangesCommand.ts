module Storyteller {
    export class DiscardChangesCommand implements IStorytellerCommand {
       public constructor(
            private rootScope,
            private scope,
            private processModelService: IProcessModelService,
            private dialogService: Shell.IDialogService,
            private messageService: Shell.IMessageService
        ) {
        }
        public execute(): void {
            this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName);
            if (this.canExecute() === false) {
                return;
            }
            // force $digest cycle to update button state
            var timer = setTimeout(() => {
                this.scope.$apply(() => {
                    if (this.processModelService.processModel.status.isLockedByMe) {
                        this.dialogService.confirm({
                            message: this.rootScope.config.labels["ST_Discard_Changes_Confirmation"]
                        }).then(
                            () => {
                                try {
                                    this.scope.publish("Toolbar:DiscardChanges", null);
                                }
                                finally {
                                    clearTimeout(timer);
                                }
                            },
                            () => {
                                clearTimeout(timer);
                            }
                            );
                    } else {
                        this.messageService.addError(this.rootScope.config.labels["ST_Discard_No_Changes"]);
                        clearTimeout(timer);
                    }
                });
            }, 200);
        }
        
        public canExecute(): boolean {
            if (this.processModelService.processModel != null && this.processModelService.processModel.status.hasEverBeenPublished) {
                return this.processModelService.processModel.status.isLockedByMe;
            }
            return false;
        }
    }

}