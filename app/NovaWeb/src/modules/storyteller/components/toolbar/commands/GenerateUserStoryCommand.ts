module Storyteller {
    export class GenerateUserStoryCommand implements IStorytellerCommand {

        constructor(private $rootScope,
            private processModelService: IProcessModelService,
            private userstoryService: IUserstoryService,
            private dialogService: Shell.IDialogService,
            private messageService: Shell.IMessageService,
            private artifactVersionControlService: Shell.IArtifactVersionControlService) {
        }
        /*
        * Checks whether command can be executed or not.
        * In case of null or undefined parameter command is executable to generate user stories for entire process.
        */
        public canExecute(elements: IDiagramNode[]): boolean {
            if (!elements || elements.length === 0)
                return true;
            for (let i = 0; i < elements.length; i++) {
                if (elements[i].canGenerateUserStory() === false)
                    return false;
            }
            return true;
        }

        public execute(elements: IDiagramNode[]): void {
            //Array should be cloned to prevent modification during command's execution
            const clonnedElements = this.cloneArray(elements);
            if (this.processModelService.isUnpublished) {
                
                const params: Shell.IDialogParams = {
                    message: this.$rootScope.config.labels["ST_Confirm_Publish_Before_Generate_User_Story"],
                    confirmButton: this.$rootScope.config.labels["ST_Confirm_Publish_ConfirmButton_Label"]
                };
                this.dialogService.confirm(params)
                    .then(() => this.artifactVersionControlService.publish(this.processModelService.processModel, this.processModelService.isChanged))
                    .then(() => {
                        // use event bus to reset locks 
                        this.$rootScope.publish("Toolbar:ResetLock", null);
                        this.processModelService.processModel.status.hasEverBeenPublished = true;

                        this.generateUserStories(clonnedElements);
                       
                    });
            } else {
                this.generateUserStories(clonnedElements);
            }
        }

        private cloneArray(elements: IDiagramNode[]) {
            if (elements == null) {
                return null;
            }
            return elements.map((e) => e);
        }

        private generateUserStories(elements: IDiagramNode[]) {

            if (!this.processModelService.processModel) {
                return;
            }

            if (this.processModelService.processModel && this.processModelService.processModel.status && this.processModelService.processModel.status.isReadOnly) {
                var message = new Shell.Message(Shell.MessageType.Error, this.$rootScope.config.labels["ST_View_OpenedInReadonly_Message"]);
                this.messageService.addMessage(message);
                return;
            }

            const projectId = this.processModelService.processModel.projectId;
            const processId = this.processModelService.processModel.id;
            if(!elements || elements.length === 0) {
                this.userstoryService.generateUserstories(projectId, processId);
                return;
            }
            if (this.canExecute(elements)) {
                let nodeId: number;
                for (let i = 0; i < elements.length; i++) {
                    nodeId = parseInt(elements[i].getId());
                    this.userstoryService.generateUserstories(projectId, processId, nodeId);
                }
            }
        }
    }
}