interface IProjectExplorerController {
    isToggled: boolean;
    toggle(evt: ng.IAngularEvent): void
}

export class ProjectExplorer implements ng.IComponentOptions {
    public template: string;
    public controller: Function;
    public bindings: any; 
    public transclude: boolean = true;

    constructor() {
        this.template = require("./projectexplorer.html");
        this.controller = ProjectExplorerCtrl;
        this.bindings = {
        };
    }
}

class ProjectExplorerCtrl implements IProjectExplorerController {
    static $inject: [string] = ["$scope", "$element"];
    public isToggled: boolean;

    constructor(private $scope, private $element) {
        this.isToggled = false;
    }

    public toggle(evt: ng.IAngularEvent) {
        evt.preventDefault();
        this.isToggled = !this.isToggled;
        if(this.isToggled) {
            this.$element.addClass('show');
            this.$scope.$parent.main.$element.addClass('project-explorer-visible');
        } else {
            this.$element.removeClass('show');
            this.$scope.$parent.main.$element.removeClass('project-explorer-visible');
        }
    }
}