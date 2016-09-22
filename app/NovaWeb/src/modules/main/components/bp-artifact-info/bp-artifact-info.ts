import { Models, Enums } from "../../models";
import { IWindowManager, IMainWindow, ResizeCause } from "../../services";
import { IMessageService, Message, MessageType, ILocalizationService } from "../../../core";
import { Helper, IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController, IArtifactPickerOptions } from "../dialogs/bp-artifact-picker/bp-artifact-picker";
import { ILoadingOverlayService } from "../../../core/loading-overlay";

import { IArtifactManager, IStatefulArtifact } from "../../../managers/artifact-manager";

export { IArtifactManager }

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;
    public transclude: boolean = true;
    public bindings: any = {
        context: "<"
    };
}

export class BpArtifactInfoController {

    static $inject: [string] = [
        "$scope", "$element", "artifactManager", "localization", "messageService", "dialogService", "windowManager", "loadingOverlayService"];

    private subscribers: Rx.IDisposable[];
    private artifact: IStatefulArtifact;
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
        this.subscribers = [];     
    }

    public $onInit() {
        this.subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this),
            this.artifactManager.selection.artifactObservable.subscribeOnNext(this.onArtifactChanged, this),
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

    private onArtifactChanged = (artifact: IStatefulArtifact) => {
        if (artifact) {
            this.artifact = artifact;
            this.updateProperties(this.artifact);
            this.subscribers.push(
                this.artifact.artifactState.observable().subscribeOnNext(this.onStateChanged)
            );
        }
    }
    private onStateChanged = () => {
        this.updateProperties(this.artifact);
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
         let overlayId: number = this.loadingOverlayService.beginLoading();
         try{
            this.artifactManager.selection.getArtifact().save().finally(() => {
               this.loadingOverlayService.endLoading(overlayId);
            });
        }catch(err){
            this.messageService.addError(err);
            this.loadingOverlayService.endLoading(overlayId);
            throw err;
        }
     }

    public openPicker() {
        const dialogSettings: IDialogSettings = {
            okButton: this.localization.get("App_Button_Ok"),
            template: require("../dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: "Some header"
        };

        const dialogData: IArtifactPickerOptions = {
            selectableItemTypes: [],
            selectionMode: "checkbox",
            showSubArtifacts: true
        };

        this.dialogService.open(dialogSettings, dialogData).then((artifact: any) => {
            
        });
    }
}
