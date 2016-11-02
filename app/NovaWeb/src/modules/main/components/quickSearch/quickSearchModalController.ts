import {ILocalizationService} from "../../../core/";
import * as SearchModels from "./models/model";

export interface IQuickSearchModalController {
    searchTerm: string;
    isLoading: boolean;
    search(term): string;
    clearSearch();
    showHide: boolean;
    hasError(): boolean;
    closeModal();
}

export class QuickSearchModalController {
    searchTerm: string;
    form: ng.IFormController;
    isLoading: boolean;
    results: {};
    static $inject = [
        "$rootScope",
        "quickSearchService",
        "$log",
        "$uibModalInstance",
        "localization"
    ];

    private stateChangeStartListener: Function;

    constructor(private $rootScope: ng.IRootScopeService,
                private quickSearchService,
                private $log: ng.ILogService,
                private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                private localization: ILocalizationService) {
        this.searchTerm = _.clone(this.quickSearchService.searchTerm);
        this.isLoading = true;
    }

    search(term) {
        if (this.form && this.form.$invalid) {
            this.$log.warn("invalid search");
            return null;
        }
        this.isLoading = true;
        this.quickSearchService.searchTerm = _.clone(this.searchTerm);

        this.quickSearchService.search(term).then((results: SearchModels.ISearchResult) => {
            //assign the results and display
            //if results are greater than one
            this.results = results.items;
            this.isLoading = false;
        });
    }    
    
    clearSearch() {
        this.searchTerm = "";
        this.quickSearchService.searchTerm = "";
        this.form.$setPristine();
        this.results = [];
    }

    get showHide(): boolean {
        return !!this.searchTerm || this.form.$dirty;
    }

    hasError(): boolean {
        return this.form.$submitted &&
            this.form.$invalid;
    }

    $onInit() {
        if (this.searchTerm.length) {
            this.search(this.searchTerm);
        }
        
        this.stateChangeStartListener = this.$rootScope.$on("$stateChangeStart", this.onStateChangeStart);
    }

    onStateChangeStart = (e, toState, toParams, fromState, fromParams) => {
        this.$log.debug("state changing from search modal");
        // navigating to same artifact destroys the editor, but does not enter item state to load artifact.
        if (toParams.id === fromParams.id) {
            e.preventDefault();
        }
        this.closeModal();
    }

    closeModal() {
        this.$log.debug("close modal");

        //unregister the listener 
        this.stateChangeStartListener();

        this.$uibModalInstance.dismiss("cancel");
    }
}
