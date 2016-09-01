import {ModalDialogType} from "./base-modal-dialog-controller";
import {CommunicationWrapper, ICommunicationWrapper} from "./communication-wrapper";

export interface IModalDialogCommunication {
    registerSetGraphObserver(observer: any);
    removeSetGraphObserver(observer: any);
    setGraph(func: any);

    registerOpenDialogObserver(observer: any);
    removeOpenDialogObserver(observer: any);
    openDialog(id: number, dialogType: ModalDialogType);

    onDestroy();
}

export class ModalDialogCommunication implements IModalDialogCommunication {
    private setGraphSubject: ICommunicationWrapper; 
    private openDialogSubject: ICommunicationWrapper;

    constructor() {

        //this.exceptionHandler = new Shell.ExceptionHandler(messageService, $rootScope);

        // Create observables
        this.setGraphSubject = new CommunicationWrapper();
        this.openDialogSubject = new CommunicationWrapper();
    };

    // 1. Set graph object  
    public registerSetGraphObserver(observer: any): string {
        return this.setGraphSubject.subscribe(observer);
    }

    public removeSetGraphObserver(handler: string) {
        this.setGraphSubject.disposeObserver(handler);
    }

    public setGraph(func: any) {
        this.setGraphSubject.notify(func);
    }

    // 2. Open dialog  
    public registerOpenDialogObserver(observer: any): string {
        return this.openDialogSubject.subscribe(observer);
    }

    public removeOpenDialogObserver(handler: string) {
        this.openDialogSubject.disposeObserver(handler);
    }

    public openDialog(id: number, dialogType: ModalDialogType) {
        this.openDialogSubject.notify(arguments);
    }

    public onDestroy() {
        this.setGraphSubject.dispose();
        this.openDialogSubject.dispose();
    }
}

