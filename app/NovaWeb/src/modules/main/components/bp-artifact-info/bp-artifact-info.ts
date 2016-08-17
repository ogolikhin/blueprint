import { Models, Enums, IProjectManager, IWindowManager, IMainWindow, ResizeCause } from "../..";
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

    static $inject: [string] = ["projectManager", "dialogService", "localization", "$element", "stateManager", "windowManager"];
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
        private stateManager: IStateManager,
        private windowManager: IWindowManager
    ) {
    }

    public $onInit() {
        this._subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this)
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

    //private onArtifactChanged(item: ItemState) {
    //    this._isArtifactChanged = item.isChanged;
    //}

    private onWidthResized(mainWindow: IMainWindow) {
        if (mainWindow.causeOfChange === ResizeCause.browserResize || mainWindow.causeOfChange === ResizeCause.sidebarToggle) {
            let sidebarWrapper: Element;
            const sidebarSize: number = 270; // MUST match $sidebar-size in styles/modules/_variables.scss
            let sidebarsWidth: number = 20 * 2; // main content area padding
            sidebarWrapper = document.querySelector(".bp-sidebar-wrapper");
            if (sidebarWrapper) {
                for (let c = 0; c < sidebarWrapper.classList.length; c++) {
                    if (sidebarWrapper.classList[c].indexOf("-panel-visible") !== -1) {
                        sidebarsWidth += sidebarSize;
                    }
                }
            }
            if (this.$element.length) {
                let container: HTMLElement = this.$element[0];
                let toolbar: Element = container.querySelector(".page-top-toolbar");
                let heading: Element = container.querySelector(".artifact-heading");
                if (heading && toolbar) {
                    angular.element(heading).css("max-width", (document.body.clientWidth - sidebarsWidth) < 2 * toolbar.clientWidth ?
                        "100%" : "calc(100% - " + toolbar.clientWidth + "px)");
                }
            }
        }
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

    public get artifactHeadingMinWidth() {
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
        let state = this.stateManager.getState(this._artifact);
        if (state) {
            return state.isReadOnly;
        }
        return false;
    }

    public get isChanged(): boolean {
        let state = this.stateManager.getState(this._artifact);
        if (state) {
            return state.isChanged;
        }
        return this._isArtifactChanged;
    }

    public get isLocked(): boolean {
        let state = this.stateManager.getState(this._artifact);
        if (state) {
            return state.lockedBy === Enums.LockedByEnum.OtherUser;
        }
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