import {ICommunicationWrapper, CommunicationWrapper} from "../../services/communication-wrapper";

export interface IToolbarCommunication {
    registerToggleProcessTypeObserver(observer: any);
    removeToggleProcessTypeObserver(observer: any);
    toggleProcessType(processType: number);

    registerCopySelectionObserver(observer: any);
    removeCopySelectionObserver(observer: any);
    copySelection();

    onDestroy();
}

export class ToolbarCommunication implements IToolbarCommunication {
    private setToggleProcessTypeSubject: ICommunicationWrapper;
    private setCopySelectionSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setToggleProcessTypeSubject = new CommunicationWrapper();
        this.setCopySelectionSubject = new CommunicationWrapper();
    };

    // Toggle process type
    public registerToggleProcessTypeObserver(observer: any): string {
        return this.setToggleProcessTypeSubject.subscribe(observer);
    }

    public removeToggleProcessTypeObserver(observer: any) {
        this.setToggleProcessTypeSubject.disposeObserver(observer);
    }

    public toggleProcessType(processType: number) {
        this.setToggleProcessTypeSubject.notify(processType);
    }

    // Copy Selection 
    public registerCopySelectionObserver(observer: any): string {
        return this.setCopySelectionSubject.subscribe(observer);
    }

    public removeCopySelectionObserver(observer: any) {
        this.setCopySelectionSubject.disposeObserver(observer);
    }

    public copySelection() {
        this.setCopySelectionSubject.notify(null);
    }

    public onDestroy() {
        this.setToggleProcessTypeSubject.dispose();
        this.setCopySelectionSubject.dispose();
    }
}
