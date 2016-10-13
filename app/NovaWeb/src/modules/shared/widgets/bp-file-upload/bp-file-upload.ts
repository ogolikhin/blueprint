export class BPFileUpload implements ng.IDirective {
    public restrict = "A";
    public scope = {
        "onFileUpload": "&bpFileUpload"
    };
    public link: Function = ($scope: any, $element: any, $attrs: ng.IAttributes): void => {

        const clearSelectedFiles = () => {
            $element[0].value = "";
        };

        $element.on("change", () => {
            const files = $element[0].files;

            if (files && files.length > 0) {
                $scope.onFileUpload({files: files, callback: clearSelectedFiles});
            }
        });
    };

    constructor() {
//fixme: empty constructors can be removed as not needed
    }

    public static factory() {
        const directive = () => new BPFileUpload();
        directive["$inject"] = [];
        return directive;
    }
}
