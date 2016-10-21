export class QuickSearchModalController {
    searchTerm: string;
    form: ng.IFormController;
    isLoading: boolean;
    hasResults: boolean;
    results: any[];
    static $inject = [
        "quickSearchService",
        "$log",
        "$uibModalInstance"
    ];

    constructor(private quickSearchService,
                private $log: ng.ILogService,
                private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        this.searchTerm = _.clone(this.quickSearchService.searchTerm);
        this.isLoading = true;
        this.hasResults = false;
    }

    search(term) {
        if (!term) {
            return false;
        }
        this.isLoading = true;
        this.$log.debug("searching form ", term);
        this.quickSearchService.searchTerm = _.clone(this.searchTerm);

        this.quickSearchService.search(term).then((results) => {
//assign the results and display
            //if results are greater than one
            if (results.FullTextSearchItems.length > 1) {
                this.hasResults = true;
            }
            this.isLoading = false;
        });
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
