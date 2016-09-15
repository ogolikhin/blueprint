import {IModalDialogModel} from "./models/modal-dialog-model-interface";

export interface IModalScope extends ng.IScope {
    dialogModel: IModalDialogModel;
}

export enum ModalDialogType {
    UserSystemTaskDetailsDialogType,
    PreviewDialogType,
    UserSystemDecisionDetailsDialogType
}

export class BaseModalDialogController<T extends IModalDialogModel> {

    public static dialogOpenEventName: string = "openDetailsModal";

    public static $inject = [
        "$rootScope",
        "$scope",
        "$uibModalInstance",
        "dialogModel"
    ];

    constructor(protected $rootScope: ng.IRootScopeService,
        protected $scope: IModalScope,
        protected $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
        protected dialogModel: T) {
        this.$scope.dialogModel = dialogModel;

        $rootScope.$on("processUnloadingEvent", (() => {
            this.cancel();
        }));
    }

    public ok = () => {
        this.saveData();
        this.$scope.dialogModel = null;
        this.$uibModalInstance.close();
    };

    protected saveData() {
        // do nothing here
    }

    protected updateModelWithNewValues(destination: any, source: any) {
        angular.merge(destination, source);
    }

    public cancel = () => {
        this.$scope.dialogModel = null;
        this.$uibModalInstance.dismiss("cancel");
    };

    // public openPropertyFloatWindow = (artifactId: any) => {
    //     //pass the whole $event as referencing DOM nodes in Angular expressions is disallowed!   
    //     artifactId = parseInt(artifactId.target.innerHTML, 10);
    //     let dialogModel = <IDialogModel>(this.$scope.dialogModel);

    //     //only show utility panel when the historical version is set to false and when it is not SMB
    //     let isSMBVal = this.$rootScope["config"].settings.StorytellerIsSMB;

    //     if (dialogModel && !dialogModel.isHistoricalVersion && isSMBVal ==="false") {
    //         this.$scope.dialogModel.propertiesMw.openModalDialog(artifactId);
    //     }
    // }; 
}
