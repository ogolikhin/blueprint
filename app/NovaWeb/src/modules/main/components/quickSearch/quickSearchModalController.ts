import {ILocalizationService} from "../../../core/localization/localizationService";
import {IQuickSearchService, ISearchMetadata, ISearchItem, ISearchResult} from "./quickSearchService";


import {IApplicationError} from "../../../core/error/applicationError";


export interface IQuickSearchModalController {
    searchTerm: string;
    isLoading: boolean;
    searchWithMetadata(term: string, source: string): void;
    search(term: string, source: string): void;
    clearSearch();
    showHide: boolean;
    hasError(): boolean;
    isServiceAvailable: boolean;
    closeModal();
}

export class QuickSearchModalController implements IQuickSearchModalController {
    searchTerm: string;
    form: ng.IFormController;
    isLoading: boolean;

    results: ISearchItem[];
    metadata: ISearchMetadata;

    isServiceAvailable: boolean;

    maxVisiblePageCount: number = 10;
    
    private page: number;

    static $inject = [
        "$rootScope",
        "quickSearchService",
        "$log",
        "$uibModalInstance",
        "localization",
        "$q",
        "$document"
    ];

    private stateChangeStartListener: Function;

    constructor(private $rootScope: ng.IRootScopeService,
                private quickSearchService: IQuickSearchService,
                private $log: ng.ILogService,
                private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance,
                private localization: ILocalizationService,
                private $q: ng.IQService,
                private $document: Document) {
        this.searchTerm = _.clone(this.quickSearchService.searchTerm);
        this.showLoadingIcon();
        this.setisServiceAvailable(true);

    }

    private isFormInvalid(): boolean {
        if (this.form && this.form.$invalid) {
            this.$log.warn("invalid search");
            return true;
        }
        return false;
    }

    searchWithMetadata(term: string, source: string) {
        if (this.isFormInvalid()) {
            return null;
        }
        this.resetData();
        this.showLoadingIcon();
        this.quickSearchService.metadata(this.searchTerm).then((result) => {
            this.updateMetadataInfo(result);
            if (result.totalCount > 0) {
                this.search(this.searchTerm, source);
            } else {
                this.hideLoadingIcon();
            }
        }, (error) => {

            //If sql timeout occured on server side
            if (error && error.data && error.data.errorCode === 7000) {
                this.setisServiceAvailable(false);
            }
            
            this.hideLoadingIcon();            
        }).finally(() => {            
            const modalDialog = this.$document[0].getElementsByClassName("modal-dialog");
            if (modalDialog && modalDialog.length > 0 && modalDialog[0].parentElement) {
                const outerModalDialog: HTMLElement = modalDialog[0].parentElement;
                outerModalDialog.focus();
            }
        });
    }

    search(term: string, source: string) {
        if (this.isFormInvalid()) {
            return null;
        }
        
        source = !source ? "Modal" : source;
        this.setisServiceAvailable(true);
        
        this.showLoadingIcon();
        this.results = [];

        this.quickSearchService.searchTerm = _.clone(this.searchTerm);

        this.quickSearchService.search(term, source, this.page, this.metadata.pageSize).then((results: ISearchResult) => {
            //assign the results and display
            //if results are greater than one
            this.results = results.items;
            this.hideLoadingIcon();
        }, (error) => {
            //If sql timeout occured on server side
            if (error && error.data && error.data.errorCode === 7000) {
                this.setisServiceAvailable(false);
            }
            
            this.hideLoadingIcon();            
        });
    }

    clearSearch() {
        this.searchTerm = "";
        this.quickSearchService.searchTerm = "";
        this.resetData();
    }

    private resetData() {
        this.setisServiceAvailable(true);
        this.hideLoadingIcon();  
        if (this.form) {
            this.form.$setPristine();
        }
        this.results = [];
        this.page = 1;
        this.resetMetadata();
    }

    

    get showHide(): boolean {
        return !!this.searchTerm || this.form.$dirty;
    }

    hasError(): boolean {
        return this.form.$submitted &&
            this.form.$invalid;
    }

    $onInit() {
        this.resetMetadata();
        this.page = 1;
        if (this.searchTerm.length) {
            this.searchWithMetadata(this.searchTerm, "Header");
        }

        this.stateChangeStartListener = this.$rootScope.$on("$stateChangeStart", this.onStateChangeStart);
    }

    onStateChangeStart(e, toState, toParams, fromState, fromParams) {
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

    showPagination(): boolean {
        return this.metadata.totalCount > 0 && this.metadata.totalPages > 1;
    }

    private resetMetadata() {
        this.metadata = {totalCount: 0, pageSize: null, items: [], totalPages: 0};
    }

    private updateMetadataInfo(result: ISearchMetadata) {
        this.setisServiceAvailable(true);

        this.metadata = result;
        this.page = 1;
        if (result.totalCount === 0) {
            this.results = [];
            this.hideLoadingIcon();
        }
    }

    private showLoadingIcon() {
        this.isLoading = true;
    }

    private hideLoadingIcon() {
        this.isLoading = false;
    }

    private setisServiceAvailable(value: boolean) {
        this.isServiceAvailable = value;
    }
    
    get getResultsFoundText() {
        return _.replace(this.localization.get("Search_Results_ResultsFound"), "{0}", this.metadata.totalCount.toString());
    }
}
