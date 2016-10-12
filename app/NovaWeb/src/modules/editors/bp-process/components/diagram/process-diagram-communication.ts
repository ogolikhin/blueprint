import { ICommunicationWrapper, CommunicationWrapper } from "../../services/communication-wrapper";

export enum ProcessEvents{
    DeleteShape,
    ModelUpdate,
    NavigateToAssociatedArtifact
}

export interface IProcessDiagramCommunication {
    registerModelUpdateObserver(observer: any);
    removeModelUpdateObserver(observer: any);
    modelUpdate(selectedNodeId: number);

    registerNavigateToAssociatedArtifactObserver(observer: any);
    removeNavigateToAssociatedArtifactObserver(observer: any);
    navigateToAssociatedArtifact(artifactId: number, enableTracking?: boolean);

    register(event: ProcessEvents, observer: any) : string;
    unregister(event: ProcessEvents, observerHandler: string);
    action(event: ProcessEvents, eventPayload?: any);

    onDestroy();
}

export class ProcessDiagramCommunication implements IProcessDiagramCommunication {
    private setModelUpdateSubject: ICommunicationWrapper;
    private setNavigateToAssociatedArtifactSubject: ICommunicationWrapper;

    private setClickDeleteSubject: ICommunicationWrapper;    

    constructor() {
        // Create subjects
        this.setModelUpdateSubject = new CommunicationWrapper();
        this.setNavigateToAssociatedArtifactSubject = new CommunicationWrapper();
        this.setClickDeleteSubject = new CommunicationWrapper();
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

    //Generic handlers
    public register(event: ProcessEvents, observer: any): string{
        switch(event){
            case ProcessEvents.DeleteShape:{
                    return this.setClickDeleteSubject.subscribe(observer);
                }
            case ProcessEvents.ModelUpdate:{
                    return this.registerModelUpdateObserver(observer);
                }
            case ProcessEvents.NavigateToAssociatedArtifact:{
                    return this.registerNavigateToAssociatedArtifactObserver(observer);
                }
        }
    }
    
    public unregister(event: ProcessEvents, observerHandler: string){
        switch(event){
            case ProcessEvents.DeleteShape:{
                    this.setClickDeleteSubject.disposeObserver(observerHandler);
                    break;
                }
            case ProcessEvents.ModelUpdate:{
                    this.removeModelUpdateObserver(observerHandler);
                    break;
                }
            case ProcessEvents.NavigateToAssociatedArtifact:{
                    this.removeNavigateToAssociatedArtifactObserver(observerHandler);
                    break;
                }
        }

    }

    public action(event: ProcessEvents, eventPayload?: any){
        switch(event){
            case ProcessEvents.DeleteShape:{
                    return this.setClickDeleteSubject.notify(true);
                }
            case ProcessEvents.ModelUpdate:{
                    return this.modelUpdate(eventPayload);
                }
            case ProcessEvents.NavigateToAssociatedArtifact:{
                    return this.navigateToAssociatedArtifact(eventPayload.id, eventPayload.enableTracking);
                }
        }

    }


    public onDestroy() {
        this.setModelUpdateSubject.dispose();
        this.setNavigateToAssociatedArtifactSubject.dispose();
        this.setClickDeleteSubject.dispose();
    }
}
