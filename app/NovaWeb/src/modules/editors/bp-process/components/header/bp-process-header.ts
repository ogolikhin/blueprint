import {IArtifact, IItemType, ItemTypePredefined, IEditorContext} from "../../../../main/models/models";
import {Helper} from "../../../../shared";

export class BpProcessHeader implements ng.IComponentOptions {
    public template: string = require("./bp-process-header.html");
    public controller: Function = BpProcessHeaderController;
    public controllerAs: string = "$ctrl";
    public bindings: any = {
        context: "<"
    };
    public transclude: boolean = true;
}

interface IArtifactHeader {
    name: string;
    iconClass: string;
    typeDescription: string;
}

export class BpProcessHeaderController implements ng.IComponentController, IArtifactHeader {
    static $inject: [string] = [
        "$element"
    ];

    private _artifact: IArtifact;
    private _type: IItemType;

    constructor(
        private $element: ng.IAugmentedJQuery
    ) {
    }

    public $onInit(): void {
    }

    public $onChanges(changesObj: any): void {
        if (changesObj.context) {
            let editorContext = changesObj.context.currentValue as IEditorContext;
            this._artifact = editorContext.artifact;
            this._type = editorContext.type;
        }
    }

    public $onDestroy(): void {
        delete this._artifact;
        delete this._type;
    }

    public $postLink(): void {
    }

    public get name(): string {
        if (!this._artifact) {
            return null;
        }

        return this._artifact.name;
    }

    public get iconClass(): string {
        if (!this._artifact) {
            return null;
        }

        return `icon-${Helper.toDashCase(ItemTypePredefined[this._artifact.predefinedType] || "process")}`;
    }

    private get type(): string {
        if (this._type) {
            return this._type.name || ItemTypePredefined[this._type.predefinedType] || "";
        }

        if (this._artifact) {
            return ItemTypePredefined[this._artifact.predefinedType] || "";
        }

        return null;
    }

    public get typeDescription(): string {
        if (!this._artifact) {
            return null;
        }

        return `${this.type} - ${(this._artifact.prefix || "")}${this._artifact.id}`;
    }

    public get headingMinWidth() {
        let style = {};

        if (this.$element.length) {
            let container: HTMLElement = this.$element[0];
            let toolbar: Element = container.querySelector(".page-top-toolbar");
            let heading: Element = container.querySelector(".artifact-heading");
            let iconWidth: number = heading && heading.querySelector(".icon") ? heading.querySelector(".icon").scrollWidth : 0;
            let nameWidth: number = heading && heading.querySelector(".name") ? heading.querySelector(".name").scrollWidth : 0;
            let typeWidth: number = heading && heading.querySelector(".type-id") ? heading.querySelector(".type-id").scrollWidth : 0;
            let indicatorsWidth: number = heading && heading.querySelector(".indicators") ? heading.querySelector(".indicators").scrollWidth : 0;
            let headingWidth: number = iconWidth + (
                typeWidth > nameWidth + indicatorsWidth ? typeWidth : nameWidth + indicatorsWidth
                ) + 20 + 5; // heading's margins + wiggle room
            
            if (heading && toolbar) {
                style = {
                    "min-width": (headingWidth > toolbar.clientWidth ? toolbar.clientWidth : headingWidth) + "px"
                };
            }
        }

        return style;
    }
}