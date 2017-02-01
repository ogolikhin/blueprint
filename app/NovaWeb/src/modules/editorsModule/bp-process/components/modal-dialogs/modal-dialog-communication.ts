import {ModalDialogType} from "./modal-dialog-constants";
import {CommunicationWrapper, ICommunicationWrapper} from "../../services/communication-wrapper";

export interface IModalDialogCommunication {
    registerSetGraphObserver(observer: any);
    removeSetGraphObserver(handler: string);
    setGraph(func: any);

    registerOpenDialogObserver(observer: any);
    removeOpenDialogObserver(handler: string);
    openDialog(id: number, dialogType: ModalDialogType);

    registerModalProcessViewModelObserver(observer: any);
    removeModalProcessViewModelObserver(handler: string);
    setModalProcessViewModel(modalProcessViewModel: any);    

    registerUserStoryLoadedObserver(observer: any);
    removeUserStoryLoadedObserver(handler: string);
    notifyUserStoryLoaded(isLoaded: boolean);

    onDestroy();
}

export class ModalDialogCommunication implements IModalDialogCommunication {
    private setGraphSubject: ICommunicationWrapper;
    private openDialogSubject: ICommunicationWrapper;
    private setModalProcessViewModelSubject: ICommunicationWrapper;
    private userStoryLoadedSubject: ICommunicationWrapper;

    constructor() {

        //this.exceptionHandler = new Shell.ExceptionHandler(messageService, $rootScope);

        // Create observables
        this.setGraphSubject = new CommunicationWrapper();
        this.openDialogSubject = new CommunicationWrapper();
        this.setModalProcessViewModelSubject = new CommunicationWrapper();
        this.userStoryLoadedSubject = new CommunicationWrapper();
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

    // 3. Set ModalProcessViewModel object
    public registerModalProcessViewModelObserver(observer: any) {
        return this.setModalProcessViewModelSubject.subscribe(observer);
    }

    public removeModalProcessViewModelObserver(handler: string) {
        this.setModalProcessViewModelSubject.disposeObserver(handler);
    }

    public setModalProcessViewModel(modalProcessViewModel: any) {
        this.setModalProcessViewModelSubject.notify(modalProcessViewModel);
    }
    
    // 4. User Story Loaded
    public registerUserStoryLoadedObserver(observer: any): string {
        return this.userStoryLoadedSubject.subscribe(observer);
    }

    public removeUserStoryLoadedObserver(handler: string) {
        this.userStoryLoadedSubject.disposeObserver(handler);
    }

    public notifyUserStoryLoaded(isLoaded: boolean) {
        this.userStoryLoadedSubject.notify(isLoaded);
    }

    public onDestroy() {
        this.setGraphSubject.dispose();
        this.openDialogSubject.dispose();
        this.setModalProcessViewModelSubject.dispose();
        this.userStoryLoadedSubject.dispose();
    }
}

