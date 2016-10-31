import ModalSettings = angular.ui.bootstrap.IModalSettings;
import {ILocalizationService} from "../../../core/";

export class QuickSearchController {
    static $inject = ["$log", "$uibModal", "quickSearchService", "localization"];
    animationsEnabled: boolean;
    modalSize: string;
    modalInstance: ng.ui.bootstrap.IModalServiceInstance;
    form: ng.IFormController;

    constructor(private $log: ng.ILogService,
                private $uibModal: ng.ui.bootstrap.IModalService,
                private quickSearchService, 
                private localization: ILocalizationService) {
        this.animationsEnabled = false;
        this.modalSize = "full-screen";
    }

    onBlur() {
        this.form.$submitted = false;
    }

    hasError() {
        return  this.form.$submitted &&
                this.form.$invalid;
    }
    openModal() {
        if (this.form.$invalid) {
            this.$log.warn("invalid search");
            return null;
        }
        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            windowClass: "quick-search__modal",
            template: require("./quickSearchResults.html"),
            controller: "quickSearchModalController",
            controllerAs: "$ctrl",
            size: this.modalSize
        };

        return this.modalInstance = this.$uibModal.open(settings);
    }

}
