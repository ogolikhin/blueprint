module Storyteller {
    export class PublishProcessCommand implements IStorytellerCommand {
   
        public constructor(
            private rootScope,
            private scope,
            private processModelService: IProcessModelService) {
        }

        public execute(): void {

            if (this.canExecute() === false) {
                return;
            }
            // force $digest cycle to update button state
            var timer = setTimeout(() => {
                this.scope.$apply(() => {

                    try {
                        this.scope.publish("Toolbar:PublishProcess", null);
                    }
                    finally {
                        clearTimeout(timer);
                    }
                });
            }, 200);
            
        }

        public canExecute(): boolean {
            if (this.processModelService.processModel != null) {
                return this.processModelService.processModel.status.isLockedByMe;
            }
            return false;
        }
    }
}