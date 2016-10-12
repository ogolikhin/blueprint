import * as angular from "angular";
import IRootScopeService = angular.IRootScopeService;
import ICompileService = angular.ICompileService;
export class ComponentTest<TController> {
    public element: ng.IAugmentedJQuery;
    public scope: ng.IScope;
    private rootScope: ng.IScope;
    private compile: ng.ICompileService;

    constructor(private template: string, private registerName: string) {
        angular.mock.inject(($rootScope: IRootScopeService, $compile: ICompileService) => {
            this.rootScope = $rootScope;
            this.compile = $compile;
        });
    }

    public createComponent(attributes: any): TController {
        this.scope = this.rootScope.$new();
        for (let key in attributes) {
            this.scope[key] = attributes[key];
        }
        this.element = this.compile(this.template)(this.scope);
        this.scope.$digest();
        return this.element.controller(this.registerName);
    }

    public createComponentWithMockParent(attributes: any, parentName: string, parentController: any): TController {
        this.scope = this.rootScope.$new();
        // TODO: figure out how to add scope variables to child controller
        // for (var key in attributes) {
        //     this.scope[key] = attributes[key];
        // }
        this.template = "<div>" + this.template + "</div>";
        let el = angular.element(this.template);
        el.data("$" + parentName + "Controller", parentController);

        this.element = this.compile(el)(this.scope);
        this.scope.$digest();

        let controller = this.element.find(this.registerName).isolateScope()["$ctrl"];
        return controller;
    }
}
