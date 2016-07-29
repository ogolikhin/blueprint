import { Models, Enums, IProjectManager} from "../..";
import { ILocalizationService, IStateManager } from "../../../core";
import { Helper, IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController } from "../dialogs/bp-artifact-picker/bp-artifact-picker";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        context: "<",
    };
    public transclude: boolean = true;
}

interface IArtifactInfoContext {
    artifact?: Models.IArtifact;
    type?: Models.IItemType;
}

export class BpArtifactInfoController {
    
    static $inject: [string] = ["projectManager", "dialogService", "localization", "$element", "stateManager"];   
    private _subscribers: Rx.IDisposable[];
    private _artifact: Models.IArtifact;
    private _artifactType: Models.IItemType;
    private _isArtifactChanged: boolean;

    public currentArtifact: string;

    constructor(
        private projectManager: IProjectManager,
        private dialogService: IDialogService,
        private localization: ILocalizationService,
        private $element: ng.IAugmentedJQuery,
        private stateManager: IStateManager) {

    }

    public $onInit() {
        this._subscribers = [
            this.stateManager.isArtifactChangedObservable.subscribeOnNext(this.onArtifactChanged, this),
        ];
    }
    public $onDestroy() {
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
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

    private onArtifactChanged(state: boolean) {
        this._isArtifactChanged = state;
    }

    private onLoad = (context: IArtifactInfoContext) => {
        this._artifact = context ? context.artifact : null;
        this._artifactType = context ? context.type : null;
    };

    public get artifactName(): string {
        return this._artifact ? this._artifact.name : null;
    }

    public get artifactType(): string {
        if (this._artifactType) {
            return this._artifactType.name || Models.ItemTypePredefined[this._artifactType.predefinedType] || "";
        } else if (this._artifact) {
            return Models.ItemTypePredefined[this._artifact.predefinedType] || "";
        }
        return null;
    }

    public get artifactHeadingWidth() {
        let style = {};

        if (this.$element.length) {
            let container = this.$element[0];
            let toolbar = container.querySelector(".page-top-toolbar");
            let heading = container.querySelector(".artifact-heading");
            let iconWidth = heading.querySelector(".icon") ? heading.querySelector(".icon").scrollWidth : 0;
            let nameWidth = heading.querySelector(".name") ? heading.querySelector(".name").scrollWidth : 0;
            let indicatorsWidth = heading.querySelector(".indicators") ? heading.querySelector(".indicators").scrollWidth : 0;
            let headingWidth = iconWidth + nameWidth + indicatorsWidth + 20 + 2; // heading's margins + wiggle room
            if (heading && toolbar) {
                style = {
                    "max-width": "calc(100% - " + toolbar.clientWidth + "px)",
                    "min-width": (headingWidth > toolbar.clientWidth ? toolbar.clientWidth : headingWidth) + "px"
                }
            }
        }

        return style;
    }

    public get artifactClass(): string {
        return this._artifact ?
            "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[this._artifact.predefinedType] || "document")) :
            null;
    }

    public get artifactTypeDescription(): string {
        return this._artifact ?
            `${this.artifactType} - ${(this._artifact.prefix || "")}${this._artifact.id}` :
            null;
    }

    public get isReadonly(): boolean {
        return false;
    }

    public get isChanged(): boolean {
        return this._isArtifactChanged;
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


    public openPicker() {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: "Some header"
        }).then((artifact: any) => {
            
        });
    }
}