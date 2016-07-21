module Storyteller {
    export class DeleteCommand implements IStorytellerCommand {
   
        public canDelete: boolean = false; 
        private unsubscribeToolbarEvents = [];

        public constructor(
            private rootScope,
            private scope, 
            private selectionManager: ISelectionManager,
            private dialogService: Shell.IDialogService) {

            this.subscribeToSelectionChangedEvent();
        }
        public canExecute(): boolean {
            this.canDelete = false;

            var elements = this.selectionManager.getSelectedNodes();

            if (elements) {
                this.canDelete = this.canElementBeDeleted(elements);
            }
            return this.canDelete;
        }

        public execute(): void {

            this.rootScope.$broadcast(BaseModalDialogController.dialogOpenEventName);
            if (this.canExecute() === false) {
                return;
            }
            var elements = this.selectionManager.getSelectedNodes();

            if (elements) {
                // force $digest cycle to update button state
                var timer = setTimeout(() => {
                    this.scope.$apply(() => {
                        var diagramNode: IDiagramNode = <IDiagramNode>elements[0];
                        let dialogParameters = diagramNode.getDeleteDialogParameters();
                        this.dialogService.warning(dialogParameters)
                        .then(
                            // okay
                            () => {
                                try {
                                    // use the event bus to notify subscribers that 
                                    // the selected diagram node can be deleted 

                                    this.scope.publish("Toolbar:Delete", diagramNode);
                                }
                                finally {
                                    this.canDelete = false;
                                    clearTimeout(timer);
                                }
                            }, 
                            // cancel
                            () => {
                                this.canDelete = true;
                                clearTimeout(timer);
                            });
                                          
                    });
                }, 200);
            }
        }

        private canElementBeDeleted(elements: Array<IDiagramNode>) {

            // if the element's canBeDeleted property is set to true
            // the element can be deleted otherwise it cannot be deleted
            var canDelete = false;

            if (elements && elements.length > 0) {
                for (var i = 0; i < elements.length; i++) {
                    canDelete = elements[i].canDelete();
                    if (!canDelete) {
                        break;
                    }
                }
            }
            return canDelete;            
        }

        private removeToolbarEventListeners() {

            if (this.unsubscribeToolbarEvents.length > 0) {
                for (var i = 0; i < this.unsubscribeToolbarEvents.length; i++) {
                    this.unsubscribeToolbarEvents[i]();
                    this.unsubscribeToolbarEvents[i] = null;
                }
            }
            this.unsubscribeToolbarEvents = [];
        }

        private subscribeToSelectionChangedEvent() {
            if (this.scope.subscribe) {
                if (this.unsubscribeToolbarEvents.length > 0) {
                    // remove previous event listeners 
                    this.removeToolbarEventListeners();
                }
                this.unsubscribeToolbarEvents.push(
                    this.scope.subscribe("SelectionManager:SelectionChanged", (event, elements) => {
                            var timer = setTimeout(() => {
                                this.scope.$apply(() => {
                                    this.canExecute();
                                    clearTimeout(timer);
                                });
                            }, 100);
                        })
                );
            }
        }

        public destroy() {
  
            this.removeToolbarEventListeners();
        }
    }
}