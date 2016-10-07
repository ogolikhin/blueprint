﻿import * as angular from "angular";
import { Models, Enums } from "../../models";
import { IWindowManager, IMainWindow, ResizeCause } from "../../services";
import { IMessageService, Message, MessageType, ILocalizationService } from "../../../core";
import { ArtifactPickerDialogController, IArtifactPickerOptions } from "../bp-artifact-picker";
import { ILoadingOverlayService } from "../../../core/loading-overlay";
import { IArtifactManager, IStatefulArtifact } from "../../../managers/artifact-manager";
import { IProjectManager } from "../../../managers/project-manager";
import { INavigationService } from "../../../core/navigation/navigation.svc";
import { 
    Helper, 
    IDialogSettings, 
    IDialogService, 
    IBPToolbarOption, 
    IBPButtonToolbarOption, 
    IBPDropdownToolbarOption, 
    IBPDropdownMenuItemToolbarOption, 
    IBPToggleToolbarOption
} from "../../../shared";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BpArtifactInfoController;
    public transclude: boolean = true;
    public bindings: any = {
        context: "<"
    };
}

export class BpArtifactInfoController {
    static $inject: [string] = [
        "$scope", 
        "$element",
        "artifactManager", 
        "localization", 
        "messageService", 
        "dialogService", 
        "windowManager", 
        "loadingOverlayService",
        "navigationService",
        "projectManager"
    ];

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
    public toolbarOptions: IBPToolbarOption[];

    constructor(
        public $scope: ng.IScope,
        private $element: ng.IAugmentedJQuery,
        private artifactManager: IArtifactManager,
        private localization: ILocalizationService,
        private messageService: IMessageService,
        private dialogService: IDialogService,
        private windowManager: IWindowManager,
        private loadingOverlayService: ILoadingOverlayService,
        protected navigationService: INavigationService,
        protected projectManager: IProjectManager
    ) {
        this.initProperties();
        this.subscribers = [];
    }

    public $onInit() {
        const windowSub = this.windowManager.mainWindow.subscribeOnNext(this.onWidthResized, this);
        // const stateSub = this.artifactManager.selection.artifactObservable
        //     // cannot always skip 1 and rely on the artifact observable having 2 values (initial and new)
        //     // this is true when navigating to artifact X from artifact X via breadcrumb (loop)
        //     // .skip(1) // skip the first (initial) value

        //     .filter((artifact: IStatefulArtifact) => artifact != null)
        //     .distinctUntilChanged(artifact => artifact.id)
        //     .flatMap((artifact: IStatefulArtifact) => {
        //         this.artifact = artifact;
        //         return artifact.getObservable();
        //     })
        //     .subscribeOnNext(this.onStateChanged);

        this.subscribers.push(windowSub);
    }

    public $onChanges(obj: any) {
        this.artifactManager.get(obj.context.currentValue).then((artifact) => {
            if (artifact) {
                this.artifact = artifact;
                const artifactObserver = artifact.getObservable()
                        .subscribe(this.onArtifactChanged, this.onError);

                this.subscribers.push(artifactObserver);
            }
        });
    }

    public $onDestroy() {
        this.initProperties();
        this.subscribers.forEach(subscriber => { subscriber.dispose(); });
        delete this.subscribers;
    }

    private onArtifactChanged = () => {
        this.updateProperties(this.artifact);
    }

    public onError = (error: any) => {
        if (this.artifact.artifactState.deleted) {
            this.dialogService.alert("Artifact_Lock_DoesNotExist");
        } else if (this.artifact.artifactState.misplaced) {
            //Occurs when refreshing an artifact that's been moved; do nothing
        } else {
            this.messageService.addError(error);
        }
        this.onArtifactChanged();
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
        this.toolbarOptions = null;

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

        this.updateToolbarOptions();

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

        if (artifact.artifactState.misplaced) {
            this.dialogService.alert("Artifact_Lock_DoesNotExist").then(() => {}) ;
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

    private updateToolbarOptions(): void {
        this.toolbarOptions = [
            <IBPButtonToolbarOption>{
                type: "button",
                click: () => console.log("Clicked delete button"),
                icon: "fonticon fonticon2-delete",
                tooltip: "Delete",
                isDisabled: false
            },
            <IBPDropdownToolbarOption> {
                type: "dropdown",
                icon: "fonticon fonticon2-news",
                menuItems: [
                    <IBPDropdownMenuItemToolbarOption>{
                        label: "Test1",
                        click: () => console.log("Test1 clicked"),
                        isDisabled: false
                    },
                    <IBPDropdownMenuItemToolbarOption>{
                        label: "Test2",
                        click: () => console.log("Test2 clicked"),
                        isDisabled: false
                    },
                    <IBPDropdownMenuItemToolbarOption>{
                        label: "Test3",
                        click: () => console.log("Test3 clicked"),
                        isDisabled: true
                    },
                    <IBPDropdownMenuItemToolbarOption>{
                        label: "Test4",
                        click: () => console.log("Test4 clicked"),
                        isDisabled: true
                    }
                ],
                // label: "Test Menu",
                isDisabled: false
            },
            <IBPToggleToolbarOption>{
                type: "toggle",
                toggleOptions: [
                    <IBPButtonToolbarOption>{
                        type: "button",
                        click: () => console.log("Clicked add button"),
                        icon: "fonticon fonticon2-user-user",
                        tooltip: "Add",
                        isDisabled: false
                    },
                    <IBPButtonToolbarOption>{
                        type: "button",
                        click: () => console.log("Clicked remove button"),
                        icon: "fonticon fonticon2-user-system",
                        tooltip: "Remove",
                        isDisabled: false
                    }
                ],
                isDisabled: false
            }
        ];
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
        try {
            this.artifactManager.selection.getArtifact().save().finally(() => {
                this.loadingOverlayService.endLoading(overlayId);
            });
        } catch (err) {
            this.messageService.addError(err);
            this.loadingOverlayService.endLoading(overlayId);
            throw err;
        }
    }

    public openPicker($event: MouseEvent) {
        const dialogSettings: IDialogSettings = {
            okButton: this.localization.get("App_Button_Ok"),
            template: require("../bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: "Some header"
        };

        const dialogData: IArtifactPickerOptions = {
            selectableItemTypes: $event.altKey ? [Models.ItemTypePredefined.Document] : undefined,
            selectionMode: $event.shiftKey ? "multiple" : ($event.ctrlKey || $event.metaKey) ? "checkbox" : "single",
            showSubArtifacts: true
        };

        this.dialogService.open(dialogSettings, dialogData).then((items: Models.IItem[]) => {
            console.log(items);
        });
    }

    public refresh() {
        //loading overlay
        const overlayId = this.loadingOverlayService.beginLoading();
        const currentArtifact = this.artifactManager.selection.getArtifact();
        
        currentArtifact.refresh()
            .catch((error) => {
                //this.dialogService.alert(error.message);
                //this.navigationService.navigateToArtifact(currentArtifact.parentId);
                //this.artifactManager.remove(currentArtifact.id);

                // We're not interested in the error type.
                // sometimes this error is created by artifact.load(), which returns the statefulArtifact instead of an error object.
                this.projectManager.refresh(this.projectManager.getSelectedProject());
            }).finally(() => {
                this.loadingOverlayService.endLoading(overlayId);
            });
      }
}
