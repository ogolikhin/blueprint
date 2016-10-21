import ModalSettings = angular.ui.bootstrap.IModalSettings;

export class QuickSearchController {
    static $inject = ["$log", "$uibModal", "quickSearchService"];
    animationsEnabled: boolean;
    modalSize: string;
    modalInstance: ng.ui.bootstrap.IModalServiceInstance;
    form;
    constructor(private $log: ng.ILogService,
                private $uibModal: ng.ui.bootstrap.IModalService,
                private quickSearchService) {
        this.animationsEnabled = false;
        this.modalSize = "full-screen";
    }
    openModal() {
        if (this.form.$invalid) {
            this.$log.warn("invalid search");
            return false;
        }
        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            windowClass: "quick-search__modal",
            template: require("./quickSearchResults.html"),
            controller: "quickSearchModalController",
            controllerAs: "$ctrl",
            size: this.modalSize
        };

        this.modalInstance = this.$uibModal.open(settings);
    }

}
