import { ICommunicationWrapper, CommunicationWrapper } from "../modal-dialogs/communication-wrapper";

export interface IProcessDiagramCommunication {
    registerModelUpdateObserver(observer: any);
    removeModelUpdateObserver(observer: any);
    modelUpdate(selectedNodeId: number);

    onDestroy();
}

export class ProcessDiagramCommunication implements IProcessDiagramCommunication {
    private setModelUpdateSubject: ICommunicationWrapper; 

    constructor() {
        // Create subjects
        this.setModelUpdateSubject = new CommunicationWrapper();
    };

    // 1. Model update  
    public registerModelUpdateObserver(observer: any): string {
        return this.setModelUpdateSubject.subscribe(observer);
    }

    public removeModelUpdateObserver(handler: string) {
        this.setModelUpdateSubject.disposeObserver(handler);
    }

    public modelUpdate(selectedNodeId: number) {
        this.setModelUpdateSubject.notify(selectedNodeId);
    }

    public onDestroy() {
        this.setModelUpdateSubject.dispose();
    }
}

