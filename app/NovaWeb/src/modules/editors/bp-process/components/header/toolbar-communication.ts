import {ICommunicationWrapper, CommunicationWrapper} from "../../services/communication-wrapper";

export interface IToolbarCommunication {
    registerToggleProcessTypeObserver(observer: any);
    removeToggleProcessTypeObserver(observer: any);
    toggleProcessType(processType: number);  

    onDestroy();
}

export class ToolbarCommunication implements IToolbarCommunication {
    private setToggleProcessTypeSubject: ICommunicationWrapper;

    

    constructor() {
        // Create subjects
        this.setToggleProcessTypeSubject = new CommunicationWrapper();
        
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

    public onDestroy() {
        this.setToggleProcessTypeSubject.dispose();        
    }
}
