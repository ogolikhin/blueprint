export class PaginationConfig {
    static $inject = [
        "$templateCache"
    ];

    constructor($templateCache: ng.ITemplateCacheService) {
        $templateCache.put("uib/template/pagination/pagination.html", require("./pagination.html"));
    }
}