import { ICommunicationWrapper, CommunicationWrapper } from "../modal-dialogs/communication-wrapper";

export interface IToolbarCommunication {
    registerToggleProcessTypeObserver(observer: any);
    removeToggleProcessTypeObserver(observer: any);
    toggleProcessType(processType: number);

    registerEnableProcessTypeToggleObserver(observer: any);
    removeEnableProcessTypeToggleObserver(observer: any);
    enableProcessTypeToggle(value: boolean, processType: number);

    registerClickDeleteObserver(observer: any);
    removeClickDeleteObserver(observer: any);
    clickDelete();

    registerEnableDeleteObserver(observer: any);
    removeEnableDeleteObserver(observer: any);
    enableDelete(value: boolean);

    onDestroy();
}

export class ToolbarCommunication implements IToolbarCommunication {
    private setToggleProcessTypeSubject: ICommunicationWrapper;
    private setEnableProcessTypeToggleSubject: ICommunicationWrapper;
    
    private setClickDeleteSubject: ICommunicationWrapper;
    private setEnableDeleteSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setToggleProcessTypeSubject = new CommunicationWrapper();
        this.setEnableProcessTypeToggleSubject = new CommunicationWrapper();
        this.setClickDeleteSubject = new CommunicationWrapper();
        this.setEnableDeleteSubject = new CommunicationWrapper();
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

    // Enable process type toggle
    public registerEnableProcessTypeToggleObserver(observer: any): string {
        return this.setEnableProcessTypeToggleSubject.subscribe(observer);
    }

    public removeEnableProcessTypeToggleObserver(observer: any) {
        this.setEnableProcessTypeToggleSubject.disposeObserver(observer);
    }

    public enableProcessTypeToggle(value: boolean, processType: number) {
        this.setEnableProcessTypeToggleSubject.notify({ value, processType });
    }

    // Click delete
    public registerClickDeleteObserver(observer: any): string {
        return this.setClickDeleteSubject.subscribe(observer);
    }

    public removeClickDeleteObserver(handler: string) {
        this.setClickDeleteSubject.disposeObserver(handler);
    }

    public clickDelete() {
        this.setClickDeleteSubject.notify(true);
    }

    // Enable delete
    public registerEnableDeleteObserver(observer: any): string {
        return this.setEnableDeleteSubject.subscribe(observer);
    }

    public removeEnableDeleteObserver(handler: string) {
        this.setEnableDeleteSubject.disposeObserver(handler);
    }

    public enableDelete(value: boolean) {
        this.setEnableDeleteSubject.notify(value);
    }

    public onDestroy() {
        this.setToggleProcessTypeSubject.dispose();
        this.setEnableProcessTypeToggleSubject.dispose();
        this.setClickDeleteSubject.dispose();
        this.setEnableDeleteSubject.dispose();
    }
}

