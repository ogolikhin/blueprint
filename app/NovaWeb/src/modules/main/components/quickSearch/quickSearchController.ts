import ModalSettings = angular.ui.bootstrap.IModalSettings;

export class QuickSearchController {
    static $inject = ["$log", "$uibModal"];
    searchTerm: string;
    animationsEnabled: boolean;
    modalSize: string;
    modalInstance;

    constructor(private $log: ng.ILogService,
                private $uibModal: ng.ui.bootstrap.IModalService) {
        this.animationsEnabled = true;
        this.modalSize = "xxl";

    }

    openModal() {
        this.$log.debug("open modal");

        const settings = <ModalSettings>{
            animation: this.animationsEnabled,
            ariaLabelledBy: "modal-title",
            ariaDescribedBy: "modal-body",
            template: require("./quickSearchResults.html"),
            controller: () => {
//this is the logic for the inner controller
            },
            controllerAs: "$ctrl",
            size: this.modalSize,
            resolve: {}
        };

        this.modalInstance = this.$uibModal.open(settings);

        this.modalInstance.result = this.modalClosed();
    }


    modalClosed() {
        this.$log.debug("modal is closed now, resolve");
    }

    closeModal() {
        this.$log.debug("close modal");
    }

}
