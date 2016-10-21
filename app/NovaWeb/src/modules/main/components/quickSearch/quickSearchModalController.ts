export class QuickSearchModalController {
    searchTerm: string;
    static $inject = [
        "quickSearchService"
    ];

    constructor(private quickSearchService) {
        this.searchTerm = _.clone(this.quickSearchService.searchTerm);
    }

    search(){

}a
}
