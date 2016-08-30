import {ModalDialogType} from "./base-modal-dialog-controller";
import {CommunicationWrapper, ICommunicationWrapper} from "./communication-wrapper";

export interface IModalDialogManager {
    registerSetGraphObserver(observer: any);
    removeSetGraphObserver(observer: any);
    setGraph(func: any);

    registerOpenDialogObserver(observer: any);
    removeOpenDialogObserver(observer: any);
    openDialog(id: number, dialogType: ModalDialogType);

    onDestroy();
}

export class ModalDialogManager implements IModalDialogManager {
    private setGraphSubject: ICommunicationWrapper; 
    private openDialogSubject: ICommunicationWrapper;

    constructor() {

        //this.exceptionHandler = new Shell.ExceptionHandler(messageService, $rootScope);

        // Create observables
        this.setGraphSubject = new CommunicationWrapper();
        this.openDialogSubject = new CommunicationWrapper();
    };

    // 1. Set graph object  
    public registerSetGraphObserver(observer: any) {
        return this.setGraphSubject.subscribe(observer);
    }

    public removeSetGraphObserver(observer: Rx.IDisposable) {
        this.setGraphSubject.disposeObserver(observer);
    }

    public setGraph(func: any) {
        this.setGraphSubject.notify(func);
    }

    // 2. Open dialog  
    public registerOpenDialogObserver(observer: any) {
        return this.openDialogSubject.subscribe(observer);
    }

    public removeOpenDialogObserver(observer: Rx.IDisposable) {
        this.openDialogSubject.disposeObserver(observer);
    }

    public openDialog(id: number, dialogType: ModalDialogType) {
        this.openDialogSubject.notify(arguments);
    }

    public onDestroy() {
        this.setGraphSubject.dispose();
        this.openDialogSubject.dispose();
    }
}

