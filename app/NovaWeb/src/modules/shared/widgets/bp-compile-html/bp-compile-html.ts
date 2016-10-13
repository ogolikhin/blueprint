interface IBPCompileHtmlScope extends ng.IScope {
    bpCompileHtml: string;
}

export class BPCompileHtml implements ng.IDirective {
    public restrict = "A";

    public scope = {
        bpCompileHtml: "="
    };

    public static factory() {
        const directive = ($sce: ng.ISCEService, $compile: ng.ICompileService) => new BPCompileHtml(
            $sce, $compile
        );

        directive["$inject"] = ["$sce", "$compile"];

        return directive;
    }

    constructor(private $sce: ng.ISCEService, private $compile: ng.ICompileService) {
    }

    public link: ng.IDirectiveLinkFn = ($scope: IBPCompileHtmlScope, $element: ng.IAugmentedJQuery) => {
        const template = $scope.bpCompileHtml;

        if (template && template.charAt(0) !== "<") {
            $element.replaceWith(template);
            return;
        }

        const linker: ng.ITemplateLinkingFunction = this.$compile(template);
        const content = linker($scope);
        $element.replaceWith(content);
    };
}
