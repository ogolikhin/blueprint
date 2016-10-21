export class QuickSearchModalController {
    searchTerm: string;
    form: ng.IFormController;
    static $inject = [
        "quickSearchService",
        "$log",
        "$uibModalInstance"
    ];

    constructor(private quickSearchService,
                private $log: ng.ILogService,
                private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        this.searchTerm = _.clone(this.quickSearchService.searchTerm);
    }

    search(term) {
        if (!term) {
            return false;
        }
        this.$log.debug("searching form ", term);
        this.quickSearchService.searchTerm = _.clone(this.searchTerm);
    }

    $onInit() {
        if (this.searchTerm.length) {
            this.search(this.searchTerm);
        }
    }

    closeModal() {
        this.$log.debug("close modal");
        this.$uibModalInstance.dismiss("cancel");
    }
}
