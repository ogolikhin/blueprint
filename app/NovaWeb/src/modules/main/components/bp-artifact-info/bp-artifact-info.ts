import { Models, Enums } from "../../models";
import { IWindowManager, IMainWindow, ResizeCause } from "../../services";
import { IMessageService, Message, MessageType, ILocalizationService } from "../../../core";
import { Helper, IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController } from "../dialogs/bp-artifact-picker/bp-artifact-picker";
import { IArtifactService } from "../../services";
import { ICommunicationManager } from "../../../editors/bp-process";
import { ILoadingOverlayService } from "../../../core/loading-overlay";

import { IArtifactManager } from "../../../managers";
import { IStatefulArtifact } from "../../../managers/models";



export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;

    public transclude: boolean = true;
}

export class BpArtifactInfoController {

    static $inject: [string] = ["$scope", "artifactManager", "localization", "messageService",
        "dialogService", "$element", "windowManager", "artifactService", "communicationManager", "loadingOverlayService"];
    private _subscribers: Rx.IDisposable[];
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
        private artifactManager: IArtifactManager,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private dialogService: IDialogService,
        private $element: ng.IAugmentedJQuery,
        private windowManager: IWindowManager,
        private artifactService: IArtifactService,
        private communicationManager: ICommunicationManager,
        private loadingOverlayService: ILoadingOverlayService
    ) {
        this.initProperties();
    }

    public $onInit() {
        this._subscribers = [
            this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this),
            this.artifactManager.selection.artifactObservable.subscribeOnNext(this.onSelectArtifact, this),
            // this.stateManager.stateChange.subscribeOnNext(this.onStateChange, this),
        ];
    }

    public $onDestroy() {
        this.initProperties();
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
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
        
        this.artifactName = artifact.name || "";
        this._artifactId = artifact.id;
        this.artifactTypeId = artifact.itemTypeId;
        artifact.metadata.getItemType(artifact.id).then((itemType: Models.IItemType) => {
            this.artifactType = itemType.name || Models.ItemTypePredefined[itemType.predefinedType] || "";
            if (itemType.iconImageId && angular.isNumber(itemType.iconImageId)) {
                this.artifactTypeIcon = itemType.iconImageId;
            }
            
            this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;
        });
        this.artifactClass = "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[artifact.predefinedType] || "document"));

        this.isLegacy = artifact.predefinedType === Enums.ItemTypePredefined.Storyboard ||
            artifact.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
            artifact.predefinedType === Enums.ItemTypePredefined.UseCase ||
            artifact.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.UIMockup ||
            artifact.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
            artifact.predefinedType === Enums.ItemTypePredefined.Glossary;

        this.isReadonly = artifact.artifactState.readonly;
        this.isChanged = artifact.artifactState.dirty;
        switch (artifact.artifactState.locked) {
            case Enums.LockedByEnum.CurrentUser:
                this.selfLocked = true;
                break;
            case Enums.LockedByEnum.OtherUser:
                let name = "";
                let msg = "";
                // let date = this.localization.current.toDate(state.originItem.lockedDateTime);
                // if (state.lock && state.lock.info) {
                //     name = state.lock.info.lockOwnerLogin;
                // }
                // name =  name || state.originItem.lockedByUser.displayName || "";
                // let msg = name ? "Locked by " + name : "Locked "; 
                // if (date) {
                //     msg += " on " + this.localization.current.formatShortDateTime(date);
                // }
                this.messageService.addMessage(this.lockMessage = new Message(MessageType.Lock, msg));
                break;
            default:
                break;

        }
        
    }

    private onSelectArtifact = (artifact: IStatefulArtifact) => {
        // so, just need to do an extra check if the component has created
        this.initProperties();
        
    }

    // private onStateChange(state: ItemState) {
    //     this.initProperties();
    //     if (!state) {
    //         return;
    //     }
    //     let artifact = state.getArtifact(); 

    //     this.artifactName = artifact.name || "";
    //     this._artifactId = artifact.id;

    //     if (state.itemType) {
    //         this.artifactType = state.itemType.name || Models.ItemTypePredefined[state.itemType.predefinedType] || "";
    //     } else {
    //         this.artifactType = Models.ItemTypePredefined[artifact.predefinedType] || "";
    //     }

    //     this.artifactTypeDescription = `${this.artifactType} - ${(artifact.prefix || "")}${artifact.id}`;

    //     this.artifactClass = "icon-" + (Helper.toDashCase(Models.ItemTypePredefined[artifact.predefinedType] || "document"));

    //     this.isLegacy = artifact.predefinedType === Enums.ItemTypePredefined.Storyboard ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.UseCase ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.UIMockup ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.DomainDiagram ||
    //         artifact.predefinedType === Enums.ItemTypePredefined.Glossary;

    //     this.isReadonly = state.isReadonly;
    //     this.isChanged = state.isChanged;
    //     switch (state.lockedBy) {
    //         case Enums.LockedByEnum.CurrentUser:
    //             this.selfLocked = true;
    //             break;
    //         case Enums.LockedByEnum.OtherUser:
    //             let name = "";
    //             let date = this.localization.current.toDate(state.originItem.lockedDateTime);
    //             if (state.lock && state.lock.info) {
    //                 name = state.lock.info.lockOwnerLogin;
    //             }
    //             name =  name || state.originItem.lockedByUser.displayName || "";
    //             let msg = name ? "Locked by " + name : "Locked "; 
    //             if (date) {
    //                 msg += " on " + this.localization.current.formatShortDateTime(date);
    //             }
    //             this.messageService.addMessage(this.lockMessage = new Message(MessageType.Lock, msg));
    //             break;
    //         default:
    //             break;

    //     }
    // }


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

    //TODO: move the save logic to a more appropriate place
    // public saveChanges() {
    //     let overlayId: number = this.loadingOverlayService.beginLoading();
    //     try {
    //         let state: ItemState = this.stateManager.getState(this._artifactId);
    //         let artifactDelta: Models.IArtifact = state.generateArtifactDelta();
    //         this.artifactService.updateArtifact(artifactDelta)
    //             .then((artifact: Models.IArtifact) => {
    //                     let oldArtifact = state.getArtifact();
    //                     if (artifact.version) {
    //                         state.updateArtifactVersion(artifact.version);
    //                     }
    //                     if (artifact.lastSavedOn) {
    //                         state.updateArtifactSavedTime(artifact.lastSavedOn);
    //                     }
    //                     this.messageService.addMessage(new Message(MessageType.Info, this.localization.get("App_Save_Artifact_Error_200")));
    //                     state.finishSave();
    //                     this.isChanged = false;
    //                     this.projectManager.updateArtifactName(state.getArtifact());
    //                 }, (error) => {
    //                     let message: string;
    //                     if (error) {
    //                         if (error.statusCode === 400) {
    //                             if (error.errorCode === 114) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_400_114");
    //                             } else {
    //                                 message = this.localization.get("App_Save_Artifact_Error_400") + error.message;
    //                             }
    //                         } else if (error.statusCode === 404) {
    //                             message = this.localization.get("App_Save_Artifact_Error_404");
    //                         } else if (error.statusCode === 409) {
    //                             if (error.errorCode === 116) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_116");
    //                             } else if (error.errorCode === 117) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_117");
    //                             } else if (error.errorCode === 111) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_111");
    //                             } else if (error.errorCode === 115) {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409_115");
    //                             } else {
    //                                 message = this.localization.get("App_Save_Artifact_Error_409");
    //                             }

    //                         } else {
    //                             message = this.localization.get("App_Save_Artifact_Error_Other") + error.statusCode;
    //                         }
    //                     }
    //                     this.messageService.addError(message);
    //                 }
    //             ).finally(() => this.loadingOverlayService.endLoading(overlayId));
    //     } catch (Error) {
    //         this.messageService.addError(this.localization.get(Error));
    //         this.loadingOverlayService.endLoading(overlayId);
    //     }
    // }

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