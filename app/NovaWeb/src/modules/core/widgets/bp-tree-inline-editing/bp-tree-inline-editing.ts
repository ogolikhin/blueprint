export class BPTreeInlineEditing implements ng.IDirective {
    public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    public scope = {};

    constructor(/*list of dependencies*/) {
        // It's important to add `link` to the prototype or you will end up with state issues.
        // See http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/#comment-2111298002 for more information.
        BPTreeInlineEditing.prototype.link = (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            /*handle all your linking requirements here*/
            console.log("ing", scope);
        };
    }

    public static Factory() {
        var directive = (/*list of dependencies*/) => {
            return new BPTreeInlineEditing(/*list of dependencies*/);
        };

        directive["$inject"] = [/*list of dependencies*/];

        return directive;
    }
}

/*
 references:
 http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
 http://stackoverflow.com/questions/23535994/implementing-angularjs-directives-as-classes-in-typescript
 http://devartisans.com/articles/angularjs-directives-typescript
*/