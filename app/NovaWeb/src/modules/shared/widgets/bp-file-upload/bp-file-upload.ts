export class BPFileUpload implements ng.IDirective {
    public restrict = "A";
    public scope = {
        "onFileUpload": "&bpFileUpload"
    };
    public link: Function = ($scope: any, $element: any, $attrs: ng.IAttributes): void => {
        $element.on("change", () => {
            const files = $element[0].files;

            if (files && files.length > 0) {
                $scope.onFileUpload({ files: files });
            }
        });
    };

    constructor() {}

    public static factory() {
        const directive = () => new BPFileUpload();
        directive["$inject"] = [];
        return directive;
    }
}
