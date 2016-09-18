import { Models, Enums } from "../../models";
import { IWindowManager, IMainWindow, ResizeCause } from "../../services";
import { IMessageService, Message, MessageType, ILocalizationService } from "../../../core";
import { Helper, IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController } from "../dialogs/bp-artifact-picker/bp-artifact-picker";
import { ILoadingOverlayService } from "../../../core/loading-overlay";

import { IArtifactManager } from "../../../managers";
import { IStatefulArtifact, IArtifactState } from "../../../managers/models";

export { IArtifactManager }

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;

    public transclude: boolean = true;
}

export class BpArtifactInfoController {

    static $inject: [string] = [
        "$scope", "$element", "artifactManager", "localization", "messageService", "dialogService", "windowManager", "loadingOverlayService"];

    private subscribers: Rx.IDisposable[];
    public isReadonly: boolean;
    public isChanged: boolean;
    public isLocked: boolean;
    public lockMessage: Message;
    public selfLocked: boolean;
    public isLegacy: boolean;
    public artifactName: string;
    public artifactType: string;
    public artifactClass: string;
    public artifactTypeId: number;
    public artifactTypeIcon: number;
    public artifactTypeDescription: string;
    private _artifactId: number;

    constructor(
        public $scope: ng.IScope,
        private $element: ng.IAugmentedJQuery,
        private artifactManager: IArtifactManager,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private dialogService: IDialogService,
        private windowManager: IWindowManager,
        private loadingOverlayService: ILoadingOverlayService
    ) {
        this.initProperties();
    }

    public $onInit() {
        this.subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this),
//            this.artifactManager.selection.artifactObservable.subscribeOnNext(this.onSelectArtifact, this),
             this.artifactManager.selection.getArtifact().artifactState.observable.subscribeOnNext(this.onStateChange, this),
        ];
    }


    public $onDestroy() {
        try {
            this.initProperties();
            this.subscribers = this.subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
        } catch (ex) {
            this.messageService.addError(ex.message);
            throw ex;
        }
    }

    private initProperties() {
        this.artifactName = null;
        this.artifactType = null;
        this.artifactTypeId = null;
        this.artifactTypeIcon = null;
        this.artifactTypeDescription = null;
        this.isLegacy = false;
        this.isReadonly = false;
        this.isChanged = false;
        this.isLocked = false;
        this.selfLocked = false;
        this.isLegacy = false;
        this.artifactClass = null;
        this._artifactId = null;
        if (this.lockMessage) {
            this.messageService.deleteMessageById(this.lockMessage.id);
            this.lockMessage = null;
        }
    }

    private updateProperties(artifact: IStatefulArtifact) {
        this.initProperties();
        if (!artifact) {
            return;
        }
        this.artifactName = artifact.name || "";
        this._artifactId = artifact.id;
        let itemType = artifact.metadata.getItemType(); 
        if (itemType) {
            this.artifactTypeId = itemType.id;
            this.artifactType = itemType.name || Models.ItemTypePredefined[itemType.predefinedType] || "";
            if (itemType.iconImageId && angular.isNumber(itemType.iconImageId)) {
                this.artifactTypeIcon = itemType.iconImageId;
            }
            this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;
        }
        
        this.artifactClass = "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[itemType.predefinedType] || "document"));

        this.isLegacy = itemType.predefinedType === Enums.ItemTypePredefined.Storyboard ||
            itemType.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
            itemType.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
            itemType.predefinedType === Enums.ItemTypePredefined.UseCase ||
            itemType.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
            itemType.predefinedType === Enums.ItemTypePredefined.UIMockup ||
            itemType.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
            itemType.predefinedType === Enums.ItemTypePredefined.Glossary;

        this.isReadonly = artifact.artifactState.readonly;
        this.isChanged = artifact.artifactState.dirty;
        switch (artifact.artifactState.lockedBy) {
            case Enums.LockedByEnum.CurrentUser:
                this.selfLocked = true;
                break;
            case Enums.LockedByEnum.OtherUser:
                 let msg = artifact.artifactState.lockOwner ? "Locked by " + artifact.artifactState.lockOwner : "Locked "; 
                 if (artifact.artifactState.lockDateTime) {
                     msg += " on " + this.localization.current.formatShortDateTime(artifact.artifactState.lockDateTime);
                 }
                this.messageService.addMessage(this.lockMessage = new Message(MessageType.Lock, msg));
                break;
            default:
                break;

        }
        
    }

    private onSelectArtifact = (artifact: IStatefulArtifact) => {
        // so, just need to do an extra check if the component has created
        //this.updateProperties(artifact);
        
    }

    private onStateChange = (state: IArtifactState) => {
        this.updateProperties(this.artifactManager.selection.getArtifact());
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

    
     public saveChanges() {
         let artifact: any;
          this.artifactManager.get(61788).then((it: IStatefulArtifact) =>{
              artifact = it;

         });

        //  let overlayId: number = this.loadingOverlayService.beginLoading();
        //  try {
        //      this.artifactManager.save();
        //  } finally {
        //      this.loadingOverlayService.endLoading(overlayId);
        //  }
     }

    public openPicker() {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Ok"),
            template: require("../dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: "Some header"
        }).then((artifact: any) => {
            
        });
    }
}