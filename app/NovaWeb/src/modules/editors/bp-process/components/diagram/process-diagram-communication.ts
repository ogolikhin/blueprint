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
    public registerModelUpdateObserver(observer: any) {
        return this.setModelUpdateSubject.subscribe(observer);
    }

    public removeModelUpdateObserver(observer: Rx.IDisposable) {
        this.setModelUpdateSubject.disposeObserver(observer);
    }

    public modelUpdate(selectedNodeId: number) {
        this.setModelUpdateSubject.notify(selectedNodeId);
    }

    public onDestroy() {
        this.setModelUpdateSubject.dispose();
    }
}

