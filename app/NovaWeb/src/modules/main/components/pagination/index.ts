
config.$inject = ["$templateCache"];

export function config($templateCache: ng.ITemplateCacheService) {
    $templateCache.put("uib/template/pagination/pagination.html", require("./pagination.html"));
}

angular.module("bp.components.pagination", [])
    .run(config);