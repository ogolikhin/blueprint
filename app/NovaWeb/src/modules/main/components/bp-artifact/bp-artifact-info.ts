import {Models, Enums} from "../..";
import {Helper} from "../../../core/utils/helper";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
    public transclude: boolean = true;
}

export class BpArtifactInfoController  {
    static $inject: string[] = [];
    public _artifact: Models.IArtifact;

    public currentArtifact: string;

    constructor() {}

    public $onInit() {}
    public $onDestroy() {
        delete this._artifact;
    }
    
    public $onChanges(changedObject: any) {
        try {
            let context = changedObject.context ? changedObject.context.currentValue : null;
            this.onLoad(context);
        } catch (ex) {
            //this.messageService.addError(ex.message);
        }
    }


    private onLoad = (artifact: Models.IArtifact) => {
        this._artifact = artifact;
    };
    
    public get artifactName(): string {
        return this._artifact ? this._artifact.name : null;
    }

    public get artifactType(): string {
        return this._artifact ? (Models.ItemTypePredefined[this._artifact.predefinedType] || "") : null;
    }

    public get artifactClass(): string {
        return "icon-" + (this._artifact ? Helper.toDashCase(Models.ItemTypePredefined[this._artifact.predefinedType] || "document") : "document");
    }

    public get artifactTypeDescription(): string {
        return this._artifact ? `${Models.ItemTypePredefined[this._artifact.predefinedType] || ""} - ${(this._artifact.prefix || "")}${this._artifact.id}` : null;
    }

    public get isReadonly(): boolean {
        return false;
    }

    public get isChanged(): boolean {
        return false;
    }
    public get isLocked(): boolean {
        return false;
    }

    public get isLegacy(): boolean {
        return this._artifact && (this._artifact.predefinedType === Enums.ItemTypePredefined.Storyboard ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.UseCase ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.UIMockup ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.Glossary);
    }
}