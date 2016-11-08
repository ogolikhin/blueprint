import * as SearchModels from "./models/model";
import { ILocalizationService } from "../../../core/localization/localizationService";
import { IQuickSearchService } from "./quickSearchService";

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
    results: SearchModels.ISearchItem[];
    private page: number;
    private totalItems: number;

    static $inject = [
        "$rootScope",
        "quickSearchService",
        "$log",
        "$uibModalInstance",
        "localization",
        "$q"
    ];

    private stateChangeStartListener: Function;

    constructor(private $rootScope: ng.IRootScopeService,
                private quickSearchService: IQuickSearchService,
                private $log: ng.ILogService,
                private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                private localization: ILocalizationService,
                private $q: ng.IQService) {
        this.searchTerm = _.clone(this.quickSearchService.searchTerm);
        this.isLoading = true;
        
    }

    search(term: string, isNewSearch: boolean = true) {
        if (this.form && this.form.$invalid) {
            this.$log.warn("invalid search");
            return null;
        }
        this.isLoading = true;
        
        if (isNewSearch) {
            this.page = 1;
        }
        const metadataPromise = this.metadataSearch(isNewSearch);

        this.quickSearchService.searchTerm = _.clone(this.searchTerm);

        metadataPromise.then((result) => {                           
            this.updateMetadataInfo(result);
            this.quickSearchService.search(term, this.page).then((results: SearchModels.ISearchResult) => {
                //assign the results and display
                //if results are greater than one
                this.results = results.items;
                this.isLoading = false;
            });
        });
    }    
    
    clearSearch() {
        this.searchTerm = "";
        this.quickSearchService.searchTerm = "";
        this.form.$setPristine();
        this.results = [];
        this.totalItems = 0;
        this.page = 1;
    }

    get showHide(): boolean {
        return !!this.searchTerm || this.form.$dirty;
    }

    hasError(): boolean {
        return this.form.$submitted &&
            this.form.$invalid;
    }

    $onInit() {
        this.page = 1;
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

    private metadataSearch(isNewSearch: boolean): ng.IPromise<SearchModels.ISearchMetadata> {
        //const isSearchDifferent: boolean = !_.isEqual(this.quickSearchService.searchTerm, this.searchTerm);
        if (isNewSearch) {
            return this.quickSearchService.metadata(this.searchTerm);
        }      

        const deferred = this.$q.defer();
        deferred.resolve();
        return deferred.promise;
    }

    private updateMetadataInfo(result: SearchModels.ISearchMetadata) {
        if (result) {
            this.totalItems = result.totalCount;
            if (result.totalCount === 0) {
                this.results = [];
                this.isLoading = false;
                this.page = 1;
                return;
            }
        } 
    }
    
    get getResultsFoundText() {
        return _.replace(this.localization.get("Search_Results_ResultsFound"), "{0}", this.totalItems.toString());        
    } 
}
