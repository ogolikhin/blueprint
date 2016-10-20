import * as angular from "angular";
import * as _ from "lodash";
import {IModalDialogModel} from "./models/modal-dialog-model-interface";

export interface IModalScope extends ng.IScope {
    dialogModel: IModalDialogModel;
}

export class BaseModalDialogController<T extends IModalDialogModel> {
    protected resolve: { [key: string]: string | Function | Array<string | Function> | Object };
    protected modalInstance: ng.ui.bootstrap.IModalServiceInstance;
    protected dialogModel: T;

    public static dialogOpenEventName: string = "openDetailsModal";

    public static $inject = [
        "$rootScope",
        "$scope",
        "$uibModalInstance",
        "dialogModel"
    ];

    constructor(
        protected $rootScope: ng.IRootScopeService,
        protected $scope: IModalScope,
        $uibModalInstance?: angular.ui.bootstrap.IModalServiceInstance,
        dialogModel?: T
    ) {
        this.initializeModalInstance($uibModalInstance);
        this.initializeDialogModel(dialogModel);

        this.$scope.dialogModel = this.dialogModel;

        $rootScope.$on("processUnloadingEvent", (() => {
            this.cancel();
        }));
    }

    public ok = () => {
        this.saveData();
        this.$scope.dialogModel = null;
        this.modalInstance.close();
    };

    public cancel = () => {
        this.$scope.dialogModel = null;
        this.modalInstance.dismiss("cancel");
    };

    protected saveData() {
        // do nothing here
    }

    protected updateModelWithNewValues(destination: any, source: any): void {
        _.merge(destination, source);
    }

    // Initializes modal instance depending on the method of creation: Controller/Template vs Component
    // Component uses bound modalInstance property
    // Controller/Template uses the $uibModalInstance injected value
    private initializeModalInstance = ($uibModalInstance?: angular.ui.bootstrap.IModalServiceInstance) => {
        if ($uibModalInstance) {
            this.modalInstance = $uibModalInstance;
        }

        if (!this.modalInstance) {
            throw new Error("Could not initialize modal instance");
        }
    }

    // Initializes dialog model depending on the method of creation: Controller/Template vs Component
    // Component uses bound resolve property to contain dialog model
    // Controller/Template uses the dialogModel injected value
    private initializeDialogModel = (dialogModel?: T) => {
        if (dialogModel) {
            this.dialogModel = dialogModel;
        } else if (this.resolve && <T>this.resolve["dialogModel"]) {
            this.dialogModel = <T>this.resolve["dialogModel"];
        } else {
            throw new Error("Could not initialize dialog model");
        }
    };
}
