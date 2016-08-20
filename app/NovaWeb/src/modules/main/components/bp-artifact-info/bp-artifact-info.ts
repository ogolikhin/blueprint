import { Models, Enums } from "../../models";

import { IProjectManager, IWindowManager, IMainWindow, ResizeCause } from "../../services";
import { ILocalizationService, IStateManager, ItemState } from "../../../core";
import { Helper, IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController } from "../dialogs/bp-artifact-picker/bp-artifact-picker";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;

    public transclude: boolean = true;
}

export class BpArtifactInfoController {

    static $inject: [string] = ["projectManager", "dialogService", "localization", "$element", "stateManager", "windowManager"];
    private _subscribers: Rx.IDisposable[];
    public isReadonly: boolean;
    public isChanged: boolean;
    public isLocked: boolean;
    public lockTooltip: string;
    public selfLocked: boolean;;
    public isLegacy: boolean;
    public artifactName: string;
    public artifactType: string;
    public artifactClass: string;
    public artifactTypeDescription: string;


    constructor(
        private projectManager: IProjectManager,
        private dialogService: IDialogService,
        private localization: ILocalizationService,
        private $element: ng.IAugmentedJQuery,
        private stateManager: IStateManager,
        private windowManager: IWindowManager
    ) {
        this.initProperties();
    }

    public $onInit() {
        this._subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this),
            this.stateManager.stateChange.subscribeOnNext(this.onStateChange, this),
        ];
    }

    public $onDestroy() {
        this.initProperties();
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private initProperties() {
        this.artifactName = null;
        this.artifactType = null;
        this.artifactTypeDescription = null;
        this.isLegacy = false;
        this.isReadonly = false;
        this.isChanged = false;
        this.isLocked = false;
        this.selfLocked = false;
        this.isLegacy = false;
        this.artifactClass = null;
    }

    private onStateChange(state: ItemState) {
        this.initProperties();

        let artifact = state.getArtifact(); 

        this.artifactName = artifact.name || "";

        if (state.itemType) {
            this.artifactType = state.itemType.name || Models.ItemTypePredefined[state.itemType.predefinedType] || "";
        } else {
            this.artifactType = Models.ItemTypePredefined[artifact.predefinedType] || "";
        }

        this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;

        this.artifactClass = "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[artifact.predefinedType] || "document"));

        this.isLegacy = artifact.predefinedType === Enums.ItemTypePredefined.Storyboard ||
            artifact.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
            artifact.predefinedType === Enums.ItemTypePredefined.UseCase ||
            artifact.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.UIMockup ||
            artifact.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.Glossary;

        this.isReadonly = state.isReadonly;
        this.isChanged = state.isChanged;
        switch (state.lockedBy) {
            case Enums.LockedByEnum.CurrentUser:
                this.isLocked = true;
                this.selfLocked = true;
                this.lockTooltip = "Locked";
                break;
            case Enums.LockedByEnum.OtherUser:
                this.isLocked = true;
                let date = this.localization.current.toDate(state.originItem.lockedDateTime);
                if (date) {
                    this.lockTooltip = `Locked by user ${state.originItem.lockedByUser.displayName} on ${this.localization.current.formatDate(date)}`;
                }
                break;
            default:
                break;

        }
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