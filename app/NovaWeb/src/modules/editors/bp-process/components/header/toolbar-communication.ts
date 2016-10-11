import {ICommunicationWrapper, CommunicationWrapper} from "../../services/communication-wrapper";

export interface IToolbarCommunication {
    registerToggleProcessTypeObserver(observer: any);
    removeToggleProcessTypeObserver(observer: any);
    toggleProcessType(processType: number);

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

    private setClickDeleteSubject: ICommunicationWrapper;
    private setEnableDeleteSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setToggleProcessTypeSubject = new CommunicationWrapper();
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
        this.setClickDeleteSubject.dispose();
        this.setEnableDeleteSubject.dispose();
    }
}
