export class BpProcessHeader implements ng.IComponentOptions {
    template: string = require("./bp-process-header.html");
    controller: Function = BpProcessHeaderController;
}

interface IArtifactHeader {
    name: string;
    iconClass: string;
    typeDescription: string;
}

export class BpProcessHeaderController implements ng.IComponentController, IArtifactHeader {
    public $onInit(): void {
    }

    public $onChanges(changesObj: any): void {
    }

    public $onDestroy(): void {
    }

    public $postLink(): void {
    }

    public get name(): string {
        return "Placeholder Name";
    }

    public get iconClass(): string {
        return "icon-process";
    }

    public get typeDescription(): string {
        return "Process - PRO1234";
    }
}