import { ICommunicationWrapper, CommunicationWrapper } from "../modal-dialogs/communication-wrapper";

export interface IProcessDiagramCommunication {
    registerModelUpdateObserver(observer: any);
    removeModelUpdateObserver(observer: any);
    modelUpdate(selectedNodeId: number);

    registerNavigateToAssociatedArtifactObserver(observer: any);
    removeNavigateToAssociatedArtifactObserver(observer: any);
    navigateToAssociatedArtifact(artifactId: number, enableTracking?: boolean);

    onDestroy();
}

export class ProcessDiagramCommunication implements IProcessDiagramCommunication {
    private setModelUpdateSubject: ICommunicationWrapper;
    private setNavigateToAssociatedArtifactSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setModelUpdateSubject = new CommunicationWrapper();
        this.setNavigateToAssociatedArtifactSubject = new CommunicationWrapper();
    };

    // Model update
    public registerModelUpdateObserver(observer: any): string {
        return this.setModelUpdateSubject.subscribe(observer);
    }

    public removeModelUpdateObserver(handler: string) {
        this.setModelUpdateSubject.disposeObserver(handler);
    }

    public modelUpdate(selectedNodeId: number) {
        this.setModelUpdateSubject.notify(selectedNodeId);
    }

    // Navigate to associated artifact
    public registerNavigateToAssociatedArtifactObserver(observer: any): string {
        return this.setNavigateToAssociatedArtifactSubject.subscribe(observer);
    }

    public removeNavigateToAssociatedArtifactObserver(handler: string) {
        this.setNavigateToAssociatedArtifactSubject.disposeObserver(handler);
    }

    public navigateToAssociatedArtifact(id: number, enableTracking?: boolean) {
        this.setNavigateToAssociatedArtifactSubject.notify({ id: id, enableTracking: enableTracking });
    }

    public onDestroy() {
        this.setModelUpdateSubject.dispose();
        this.setNavigateToAssociatedArtifactSubject.dispose();
    }
}
