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
    clearSearch() {
        this.quickSearchService.searchTerm = "";
        this.form.$setPristine();
    }
    get showHide() {
        return this.quickSearchService.searchTerm || this.form.$dirty;
    }
    hasError() {
        return  this.form.$submitted &&
                this.form.$invalid &&
                this.form.$error &&
                !this.form.$error.required;
    }
    onKeyPress($event: KeyboardEvent) {
        const enterKeyCode = 13;
        if ($event.keyCode !== enterKeyCode) {
            this.form.$setPristine();
        }
    }
    onKeyDown($event: KeyboardEvent) {
        const backspaceKeyCode = 8;
        const deleteKeyCode = 46;
        if ($event.keyCode === 8 || $event.keyCode === 46) {
            this.form.$setPristine();
        }
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
