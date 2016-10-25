// import * as angular from "angular";
// import { ISelectionManager } from "../managers";
// import { IStatefulArtifact } from "../managers/artifact-manager";
// import { IMessageService, Message, MessageType } from "../shell";
// import { IAppicationError, HttpStatusCode } from "../core";
// import { INavigationService } from "../core/navigation";

// export class ItemEnterController {

//     public static $inject = [
//         "selectionManager",
//         "navigationService",
//         "messageService"
//     ];
//     private errorObserver: Rx.IDisposable;

//     constructor(private seletionManager: ISelectionManager,
//                 private navigationService: INavigationService,
//                 private messageService: IMessageService) {

//         const artifact = this.seletionManager.getArtifact();
//         this.errorObserver = artifact.errorObservable().subscribeOnNext(this.onArtifactError);           
//     }
    

//     protected onArtifactError = (error: IAppicationError) => {
//         if (error.statusCode === HttpStatusCode.Forbidden || 
//             error.statusCode === HttpStatusCode.ServerError ||
//             error.statusCode === HttpStatusCode.Unauthorized
//             ) {
//             this.navigationService.navigateToMain();
//         }
//     }
    
// }
// export class ItemExitController {

//     public static $inject = [
//         "selectionManager",
//         "navigationService",
//         "messageService"
//     ];
//     private errorObserver: Rx.IDisposable;

//     constructor(private seletionManager: ISelectionManager,
//                 private navigationService: INavigationService,
//                 private messageService: IMessageService) {

//         const artifact = this.seletionManager.getArtifact();
//         artifact.getObservable();
//     }
    
// }
