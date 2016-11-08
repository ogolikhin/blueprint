import {ICommunicationWrapper, CommunicationWrapper} from "../../services/communication-wrapper";

export enum ProcessEvents {
    DeleteShape,
    ModelUpdate,
    NavigateToAssociatedArtifact,
    ArtifactUpdate,
    UserStoriesGenerated
}

export interface IProcessDiagramCommunication {
    registerModelUpdateObserver(observer: any);
    removeModelUpdateObserver(observer: any);
    modelUpdate(selectedNodeId: number);

    register(event: ProcessEvents, observer: any): string;
    unregister(event: ProcessEvents, observerHandler: string);
    action(event: ProcessEvents, eventPayload?: any);

    onDestroy();
}

export class ProcessDiagramCommunication implements IProcessDiagramCommunication {
    private setModelUpdateSubject: ICommunicationWrapper;
    private setNavigateToAssociatedArtifactSubject: ICommunicationWrapper;
    private setClickDeleteSubject: ICommunicationWrapper;
    private setArtifactUpdateSubject: ICommunicationWrapper;
    private setUserStoriesGeneratedSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setModelUpdateSubject = new CommunicationWrapper();
        this.setNavigateToAssociatedArtifactSubject = new CommunicationWrapper();
        this.setClickDeleteSubject = new CommunicationWrapper();
        this.setArtifactUpdateSubject = new CommunicationWrapper();
        this.setUserStoriesGeneratedSubject = new CommunicationWrapper();
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

    //Generic handlers
    public register(event: ProcessEvents, observer: any): string {
        let result;
        switch (event) {
            case ProcessEvents.DeleteShape: {
                    result = this.setClickDeleteSubject.subscribe(observer);
                }
                break;
            case ProcessEvents.ModelUpdate: {
                    result = this.registerModelUpdateObserver(observer);
                }
                break;
            case ProcessEvents.NavigateToAssociatedArtifact: {
                    result = this.setNavigateToAssociatedArtifactSubject.subscribe(observer);
                }
                break;
            case ProcessEvents.ArtifactUpdate: {
                    result = this.setArtifactUpdateSubject.subscribe(observer);
                }
                break;
            case ProcessEvents.UserStoriesGenerated: {
                    result = this.setUserStoriesGeneratedSubject.subscribe(observer);
                }
                break;
            default:
                result = undefined;
                break;
        }

        return result;
    }

    public unregister(event: ProcessEvents, observer: string) {
        switch (event) {
            case ProcessEvents.DeleteShape: {
                    this.setClickDeleteSubject.disposeObserver(observer);
                }
                break;

            case ProcessEvents.ModelUpdate: {
                    this.removeModelUpdateObserver(observer);
                }
                break;

            case ProcessEvents.NavigateToAssociatedArtifact: {
                    this.setNavigateToAssociatedArtifactSubject.disposeObserver(observer);
                }
                break;

            case ProcessEvents.ArtifactUpdate: {
                   this.setArtifactUpdateSubject.disposeObserver(observer);
                }
                break;
            
            case ProcessEvents.UserStoriesGenerated: {
                    this.setUserStoriesGeneratedSubject.disposeObserver(observer);
                }
                break;
        }
    }

    public action(event: ProcessEvents, eventPayload?: any) {
        switch (event) {
            case ProcessEvents.DeleteShape: {
                    this.setClickDeleteSubject.notify(true);
                }
                break;

            case ProcessEvents.ModelUpdate: {
                    this.modelUpdate(eventPayload);
                }
                break;

            case ProcessEvents.NavigateToAssociatedArtifact: {
                    this.setNavigateToAssociatedArtifactSubject.notify({
                        id: eventPayload.id,
                        version: eventPayload.version,
                        enableTracking: eventPayload.enableTracking
                    });
                }
                break;

            case ProcessEvents.ArtifactUpdate: {
                    this.setArtifactUpdateSubject.notify(eventPayload);
                }
                break;

            case ProcessEvents.UserStoriesGenerated: {
                    this.setUserStoriesGeneratedSubject.notify(eventPayload);
                }
                break;
        }
    }

    public onDestroy() {
        this.setModelUpdateSubject.dispose();
        this.setNavigateToAssociatedArtifactSubject.dispose();
        this.setClickDeleteSubject.dispose();
        this.setArtifactUpdateSubject.dispose();
        this.setUserStoriesGeneratedSubject.dispose();
    }
}
