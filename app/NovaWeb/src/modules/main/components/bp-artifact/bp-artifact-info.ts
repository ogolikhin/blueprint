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

    private artifactInfoWidthObserver;
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

        window.addEventListener("resize", this.windowResizeHandler);

        this.artifactInfoWidthObserver = new MutationObserver((mutations) => {
            mutations.forEach((mutation) => {
                if (mutation.attributeName === "class") {
                    this.setArtifactHeadingMaxWidth(mutation);
                    this.setArtifactEditorLabelsWidth();
                }
            });
        });
        let wrapper: Node = document.querySelector(".bp-sidebar-wrapper");
        try {
            this.artifactInfoWidthObserver.observe(wrapper, { attributes: true });
        } catch (ex) {
            //this.messageService.addError(ex.message);
        }
    }

    public $onDestroy() {
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });

        window.removeEventListener("resize", this.windowResizeHandler);

        try {
            this.artifactInfoWidthObserver.disconnect();
        } catch (ex) {
            //this.messageService.addError(ex.message);
        }

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
        this.setArtifactHeadingMaxWidth();
        this.setArtifactEditorLabelsWidth();
    };

    private windowResizeTick: boolean = false;
    private windowResizeHandler = () => {
        if (!this.windowResizeTick) {
            // resize events can fire at a high rate. We throttle the event using requestAnimationFrame
            // ref: https://developer.mozilla.org/en-US/docs/Web/API/window/requestAnimationFrame
            window.requestAnimationFrame(() => {
                this.setArtifactHeadingMaxWidth();
                this.setArtifactEditorLabelsWidth();
                this.windowResizeTick = false;
            });
        }
        this.windowResizeTick = true;
    };

    private setArtifactHeadingMaxWidth(mutationRecord?: MutationRecord) {
        let sidebarWrapper: Element;
        const sidebarSize: number = 270; // MUST match $sidebar-size in styles/modules/_variables.scss
        let sidebarsWidth: number = 20 * 2; // main content area padding
        if (mutationRecord && mutationRecord.target  && mutationRecord.target.nodeType === 1) {
            sidebarWrapper = <Element> mutationRecord.target;
        } else {
            sidebarWrapper = document.querySelector(".bp-sidebar-wrapper");
        }
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

    private setArtifactEditorLabelsWidth() {
        let artifactOverview: Element = document.querySelector(".artifact-overview");
        if (artifactOverview) {
            const propertyWidth: number = 392; // MUST match $property-width in styles/partials/_properties.scss
            let actualWidth: number = artifactOverview.querySelector(".formly") ? artifactOverview.querySelector(".formly").clientWidth : propertyWidth;
            if (actualWidth < propertyWidth) {
                artifactOverview.classList.add("single-column");
            } else {
                artifactOverview.classList.remove("single-column");
            }
        }
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