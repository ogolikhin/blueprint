export class BPTreeDragndrop implements ng.IDirective {
    public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    //public scope = {};
    public restrict = "A";

    private timeout;

    constructor(
        //list of other dependencies*/
    ) {
        // It's important to add `link` to the prototype or you will end up with state issues.
        // See http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/#comment-2111298002 for more information.
        BPTreeDragndrop.prototype.link = (scope: any, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            console.log("dnd", scope);
        };
    }

    public static Factory() {
        var directive = (
            //list of dependencies
        ) => {
            return new BPTreeDragndrop (
                //list of other dependencies
            );
        };

        directive["$inject"] = [
            //list of other dependencies
        ];

        return directive;
    }
}