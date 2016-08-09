import {ModalDialogType} from "./base-modal-dialog-controller";
import {Observable} from "./observable";

export interface ICommunicationService {
    registerSetGraphObserver(observer: any);
    removeSetGraphObserver(observer: any);
    setGraph(func: any);

    registerOpenDialogObserver(observer: any);
    removeOpenDialogObserver(observer: any);
    openDialog(id: number, dialogType: ModalDialogType);
}

export class CommunicationService implements ICommunicationService {
    private setGraphObservable: Observable<any>;
    private openDialogObservable: Observable<any>;

    constructor() {

        //this.exceptionHandler = new Shell.ExceptionHandler(messageService, $rootScope);

        // Create observables
        this.setGraphObservable = new Observable<any>();
        this.openDialogObservable = new Observable<any>();
    };

    // 1. Set graph object  
    public registerSetGraphObserver(observer: any) {
        this.setGraphObservable.registerObserver(observer);
    }

    public removeSetGraphObserver(observer: any) {
        this.setGraphObservable.removeObserver(observer);
    }

    public setGraph(func: any) {
        this.setGraphObservable.notifyObservers(func);
    }

    // 2. Open dialog  
    public registerOpenDialogObserver(observer: any) {
        this.openDialogObservable.registerObserver(observer);
    }

    public removeOpenDialogObserver(observer: any) {
        this.openDialogObservable.removeObserver(observer);
    }

    public openDialog(id: number, dialogType: ModalDialogType) {
        this.openDialogObservable.notifyObservers(id, dialogType);
    }
}

