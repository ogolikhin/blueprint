export class QuickSearchModalController {
    searchTerm: string;
    form: ng.IFormController;
    isLoading: boolean;
    results: {};
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
    }

    search(term) {
        if (this.form.$invalid) {
            this.$log.warn("invalid search");
            return null;
        }
        this.isLoading = true;
        this.quickSearchService.searchTerm = _.clone(this.searchTerm);

        this.quickSearchService.search(term).then((results) => {
            //assign the results and display
            //if results are greater than one
            this.results = results.fullTextSearchItems;
            this.isLoading = false;
        });
    }

    hasError() {
        return this.form.$submitted &&
            this.form.$invalid;
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
