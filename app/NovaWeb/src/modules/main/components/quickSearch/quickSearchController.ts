export class QuickSearchController {
    static $inject = ["$log"];
    searchTerm: string;

    constructor(private $log: ng.ILogService) {

    }

    openModal() {
        this.$log.debug("open modal");
    }

    closeModal() {
        this.$log.debug("close modal");
    }

}
