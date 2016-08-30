import { ICommunicationWrapper, CommunicationWrapper } from "../../../editors/bp-process/components/modal-dialogs/communication-wrapper";

export interface IToolbarCommunicationManager {
    registerClickDeleteObserver(observer: any);
    removeClickDeleteObserver(observer: any);
    clickDelete();

    registerEnableDeleteObserver(observer: any);
    removeEnableDeleteObserver(observer: any);
    enableDelete(value: boolean);

    onDestroy();
}

export class ToolbarCommunicationManager implements IToolbarCommunicationManager {
    private setClickDeleteSubject: ICommunicationWrapper; 
    private setEnableDeleteSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setClickDeleteSubject = new CommunicationWrapper();
        this.setEnableDeleteSubject = new CommunicationWrapper();
    };

    // 1. Click delete  
    public registerClickDeleteObserver(observer: any) {
        return this.setClickDeleteSubject.subscribe(observer);
    }

    public removeClickDeleteObserver(observer: Rx.IDisposable) {
        this.setClickDeleteSubject.disposeObserver(observer);
    }

    public clickDelete() {
        this.setClickDeleteSubject.notify(true);
    }

    // 2. Enable delete  
    public registerEnableDeleteObserver(observer: any) {
        return this.setEnableDeleteSubject.subscribe(observer);
    }

    public removeEnableDeleteObserver(observer: Rx.IDisposable) {
        this.setEnableDeleteSubject.disposeObserver(observer);
    }

    public enableDelete(value: boolean) {
        this.setEnableDeleteSubject.notify(value);
    }

    public onDestroy() {
        this.setClickDeleteSubject.dispose();
        this.setEnableDeleteSubject.dispose();
    }
}

