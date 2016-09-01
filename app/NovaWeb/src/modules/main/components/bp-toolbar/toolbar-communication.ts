import { ICommunicationWrapper, CommunicationWrapper } from "../../../editors/bp-process/components/modal-dialogs/communication-wrapper";

export interface IToolbarCommunication {
    registerClickDeleteObserver(observer: any);
    removeClickDeleteObserver(observer: any);
    clickDelete();

    registerEnableDeleteObserver(observer: any);
    removeEnableDeleteObserver(observer: any);
    enableDelete(value: boolean);

    onDestroy();
}

export class ToolbarCommunication implements IToolbarCommunication {
    private setClickDeleteSubject: ICommunicationWrapper; 
    private setEnableDeleteSubject: ICommunicationWrapper;

    constructor() {
        // Create subjects
        this.setClickDeleteSubject = new CommunicationWrapper();
        this.setEnableDeleteSubject = new CommunicationWrapper();
    };

    // 1. Click delete  
    public registerClickDeleteObserver(observer: any): string {
        return this.setClickDeleteSubject.subscribe(observer);
    }

    public removeClickDeleteObserver(handler: string) {
        this.setClickDeleteSubject.disposeObserver(handler);
    }

    public clickDelete() {
        this.setClickDeleteSubject.notify(true);
    }

    // 2. Enable delete  
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
        this.setClickDeleteSubject.dispose();
        this.setEnableDeleteSubject.dispose();
    }
}

