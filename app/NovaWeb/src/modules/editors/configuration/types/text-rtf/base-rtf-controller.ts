import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IValidationService} from "../../../../managers/artifact-manager/validation/validation.svc";
import {Helper} from "../../../../shared/utils/helper";
import {IDialogSettings, IDialogService} from "../../../../shared/widgets/bp-dialog/bp-dialog";
import {
    ArtifactPickerDialogController,
    IArtifactPickerOptions
} from "../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog";
import {Models, Enums} from "../../../../main/models";
import {ISelectionManager} from "../../../../managers/selection-manager/selection-manager";
import {IArtifactService} from "../../../../managers/artifact-manager/artifact/artifact.svc";
import {IArtifactRelationships} from "../../../../managers/artifact-manager/relationships/relationships";
import {IStatefulArtifact} from "../../../../managers/artifact-manager/artifact/artifact";
import {IStatefulSubArtifact} from "../../../../managers/artifact-manager/sub-artifact/sub-artifact";
import {IMessageService} from "../../../../core/messages/message.svc";
import {IRelationship, LinkType, TraceDirection} from "../../../../main/models/relationshipModels";

export interface IBPFieldBaseRTFController {
    editorBody: HTMLElement;
    observer: MutationObserver;
    handleClick(event: Event): void;
    handleLinks(nodeList: Node[] | NodeList, remove: boolean): void;
    handleMutation(mutation: MutationRecord): void;
}

interface ITinyMceMenu {
    icon?: string;
    text: string;
    onclick: Function;
}

export class BPFieldBaseRTFController implements IBPFieldBaseRTFController {
    static $inject: [string] = [
        "$q",
        "$scope",
        "$window",
        "navigationService",
        "validationService",
        "messageService",
        "localization",
        "dialogService",
        "selectionManager",
        "artifactService",
        "artifactRelationships"
    ];

    public editorBody: HTMLElement;
    public observer: MutationObserver;

    protected currentArtifact: IStatefulArtifact;
    protected currentSubArtifact: IStatefulSubArtifact;

    protected isDirty: boolean;
    protected contentBuffer: string;
    protected mceEditor: TinyMceEditor;
    protected onChange: AngularFormly.IExpressionFunction;
    protected allowedFonts: string[];
    protected isSingleLine: boolean = false;
    protected execCommandEvents: string[] = [
        "mceInsertContent",
        "mceToggleFormat",
        "RemoveFormat",
        "InsertUnorderedList",
        "InsertOrderedList",
        "Outdent",
        "Indent"
    ];
    protected fontSizes: string[] = ["8", "9", "10", "11", "12", "14", "16", "18", "20"];

    constructor(protected $q: ng.IQService,
                protected $scope: AngularFormly.ITemplateScope,
                protected $window: ng.IWindowService,
                public navigationService: INavigationService,
                protected validationService: IValidationService,
                protected messageService: IMessageService,
                protected localization: ILocalizationService,
                protected dialogService: IDialogService,
                protected selectionManager: ISelectionManager,
                protected artifactService: IArtifactService,
                protected artifactRelationships: IArtifactRelationships) {
        this.currentArtifact = selectionManager.getArtifact();
        this.currentSubArtifact = selectionManager.getSubArtifact();

        // The following is to pre-request the relationships in order to calculate if the user can manage them
        // doing it now will avoid for the user to wait when he click on the Inline Traces TinyMCE menu.
        // See canManageTraces below for additional Info.
        if (this.currentArtifact && this.currentArtifact.supportRelationships()) {
            let relationships: IRelationship[];
            this.currentArtifact.relationships.get().then((rel: IRelationship[]) => {
                relationships = rel;
            });
        }

        this.isDirty = false;
        this.contentBuffer = undefined;

        // the onChange event has to be called from the custom validator (!) as otherwise it will fire before the actual validation takes place
        this.onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        //we override the default onChange as we need to deal with changes differently when using tinymce
        $scope.to.onChange = undefined;

        $scope["$on"]("$destroy", () => {
            $scope.to.onChange = this.onChange;

            this.removeObserver();
            if (this.editorBody) {
                this.handleLinks(this.editorBody.querySelectorAll("a"), true);
            }
        });
    }

    private removeObserver = () => {
        if (this.observer) {
            this.observer.disconnect();
        }
        if (this.mceEditor) {
            this.mceEditor.destroy(false);
        }
    };

    private getAppBaseUrl = (): string => {
        const location = this.$window.location as Location;

        let origin: string = location.origin;
        if (!origin) {
            origin = location.protocol + "//" + location.hostname + (location.port ? ":" + location.port : "");
        }

        return origin + "/";
    };

    protected handleValidation = (content: string) => {
        const $scope = this.$scope;
        $scope["$applyAsync"](() => {
            const isValid = this.validationService.textRtfValidation.hasValueIfRequired($scope.to.required, content, content);
            const formControl = $scope.fc as ng.IFormController;
            if (formControl) {
                formControl.$setValidity("requiredCustom", isValid, formControl);
                $scope.to["isInvalid"] = !isValid;
                $scope.showError = !isValid;
            }
        });
    };

    protected fontFormats(): string {
        let fontFormats = "";
        if (_.isArray(this.allowedFonts) && this.allowedFonts.length) {
            this.allowedFonts.forEach(function (font) {
                fontFormats += `${font}=` + (font.indexOf(" ") !== -1 ? `"${font}";` : `${font};`);
            });
        }
        return fontFormats;
    }

    protected triggerChange = (newContent?: string) => {
        if (_.isUndefined(newContent) && this.mceEditor) {
            newContent = this.mceEditor.getContent();
        }
        newContent = newContent || "";
        this.isDirty = true;

        this.handleValidation(newContent);

        const $scope = this.$scope;
        if (typeof this.onChange === "function") {
            this.onChange(newContent, $scope.options, $scope);
        }
    };

    protected prepRTF = (hasTables: boolean = false) => {
        this.isDirty = false;
        this.editorBody = this.mceEditor.getBody() as HTMLElement;
        this.normalizeHtml(this.editorBody, hasTables);
        this.handleLinks(this.editorBody.querySelectorAll("a"));
        this.contentBuffer = this.mceEditor.getContent();

        this.handleValidation(this.contentBuffer);
        this.$scope.options["data"].isFresh = false;
    };

    protected normalizeHtml(body: Node, hasTables: boolean = false) {
        Helper.autoLinkURLText(body);
        if (hasTables) {
            Helper.addTableBorders(body);
        }
        if (_.isArray(this.allowedFonts) && this.allowedFonts.length) {
            Helper.setFontFamilyOrOpenSans(body, this.allowedFonts);
        }
    };

    protected fontSizeMenu = (editor): ITinyMceMenu[] => {
        return this.fontSizes.map((size) => {
            return {
                text: size,
                onclick: () => {
                    editor.formatter.apply(`font${size}`);
                    this.triggerChange();
                }
            } as ITinyMceMenu;
        });
    };

    protected linksMenu = (editor): ITinyMceMenu[] => {
        return [{
            icon: "link",
            text: " Links",
            onclick: () => {
                editor.editorCommands.execCommand("mceLink");
            }
        }, {
            icon: "inlinetrace",
            text: " Inline traces",
            onclick: () => {
                if (this.canManageTraces()) {
                    this.openArtifactPicker();
                } else {
                    this.messageService.addError("Property_RTF_InlineTrace_Error_Permissions");
                }
            }
        }] as ITinyMceMenu[];
    };

    protected hasChangedFormat(): boolean {
        const colorRegEx = /(#[a-f0-9]{6})/gi;

        let currentContent = "";
        if (this.mceEditor) {
            currentContent = this.mceEditor.getContent() as string;
        }
        const currentColors = currentContent.match(colorRegEx);
        const bufferColors = (this.contentBuffer || "").match(colorRegEx);

        return !_.isEqual(currentColors, bufferColors);
    };

    private canManageTraces(): boolean {
        // If artifact is locked by other user we still can add/manage traces as long as canEdit=true
        // We query the artifact even when on a subArtifact, as canEdit of the subArtifact is actually its parent artifact
        return this.currentArtifact ? this.currentArtifact.supportRelationships() && this.currentArtifact.relationships.canEdit &&
            (this.currentArtifact.permissions & Enums.RolePermissions.Edit) === Enums.RolePermissions.Edit : false;
    }

    public openArtifactPicker = () => {
        const dialogSettings = <IDialogSettings>{
            okButton: this.localization.get("App_Button_Add"),
            template: require("../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
            controller: ArtifactPickerDialogController,
            css: "nova-open-project",
            header: this.localization.get("Property_RTF_Add_InlineTrace")
        };

        const dialogOption: IArtifactPickerOptions = {
            showSubArtifacts: true
        };

        this.dialogService.open(dialogSettings, dialogOption).then((items: Models.IArtifact[] | Models.ISubArtifactNode[]) => {
            if (items.length !== 1) {
                this.messageService.addError(this.localization.get("Property_RTF_InlineTrace_Error_Invalid_Selection"));
                return;
            }

            const currentItem = this.currentSubArtifact ? this.currentSubArtifact : this.currentArtifact;
            if (currentItem.id === items[0].id) {
                this.messageService.addError(this.localization.get("Property_RTF_InlineTrace_Error_Itself"));
            } else {
                const isSubArtifact: boolean = !items[0].hasOwnProperty("projectId");
                this.$q.when(isSubArtifact ? this.artifactService.getArtifact(items[0].parentId) : items[0] as Models.IArtifact)
                .then((artifact: Models.IArtifact) => {
                    let subArtifact: Models.ISubArtifactNode = isSubArtifact ? items[0] as Models.ISubArtifactNode : undefined;
                    const itemId: number = isSubArtifact ? subArtifact.id : artifact.id;

                    const itemName: string = isSubArtifact ? subArtifact.displayName : artifact.name;
                    const itemPrefix: string = isSubArtifact ? subArtifact.prefix : artifact.prefix;

                    currentItem.relationships.get()
                        .then((relationships: IRelationship[]) => {
                            // get the pre-existing manual traces
                            let manualTraces: IRelationship[] = relationships
                                .filter((relationship: IRelationship) =>
                                relationship.traceType === LinkType.Manual);

                            // if the pre-existing manual traces already include the artifact we want to link
                            // (with either To or TwoWay direction) we don't need to add the manual trace.
                            const isArtifactAlreadyLinkedTo: boolean = manualTraces
                                .some((relationship: IRelationship) => {
                                    return relationship.itemId === itemId &&
                                        (relationship.traceDirection === TraceDirection.To || relationship.traceDirection === TraceDirection.TwoWay);
                                });

                            if (!isArtifactAlreadyLinkedTo) {
                                // if the pre-existing manual traces already include the artifact we want to link
                                // (with From direction) we just update the direction
                                const isArtifactAlreadyLinkedFrom = manualTraces
                                    .some((relationship: IRelationship) => {
                                        return relationship.itemId === itemId && relationship.traceDirection === TraceDirection.From;
                                    });

                                if (isArtifactAlreadyLinkedFrom) {
                                    manualTraces.forEach((relationship: IRelationship) => {
                                        if (relationship.itemId === itemId) {
                                            relationship.traceDirection = TraceDirection.TwoWay;
                                        }
                                    });
                                } else {
                                    const newTrace: IRelationship = {
                                        artifactId: artifact.id,
                                        artifactTypePrefix: artifact.prefix,
                                        artifactName: artifact.name,
                                        itemId: itemId,
                                        itemTypePrefix: isSubArtifact ? subArtifact.prefix : artifact.prefix,
                                        itemName: isSubArtifact ? undefined : artifact.name,
                                        itemLabel: isSubArtifact ? subArtifact.displayName : undefined,
                                        projectId: artifact.projectId,
                                        projectName: artifact.artifactPath && artifact.artifactPath.length ?
                                            artifact.artifactPath[0] : undefined, //TODO: find project name when subartifact
                                        traceDirection: TraceDirection.To,
                                        traceType: LinkType.Manual,
                                        suspect: false,
                                        hasAccess: true,
                                        primitiveItemTypePredefined: undefined, //TODO: put proper value
                                        isSelected: false,
                                        readOnly: false //TODO: put proper value
                                    };

                                    manualTraces = manualTraces.concat([newTrace]);
                                }

                                currentItem.relationships.updateManual(manualTraces);
                            }
                        })
                        .finally(() => {
                            this.insertInlineTrace(itemId, itemName, itemPrefix);
                            this.triggerChange();
                        });
                });
            }
        });
    };

    public insertInlineTrace = (id: number, name: string, prefix: string) => {
        /* tslint:disable:max-line-length */
        // when run locally, the inline trace may not be saved, as the site runs on port 8000, while services are on port 9801
        const linkUrl: string = this.getAppBaseUrl() + "?ArtifactId=" + id.toString();
        const linkText: string = prefix + id.toString() + ": " + name;
        const escapedLinkText: string = _.escape(linkText);
        const spacer: string = "<span>&nbsp;</span>";
        const inlineTrace: string = `<span class="mceNonEditable">` +
            `<a linkassemblyqualifiedname="BluePrintSys.RC.Client.SL.RichText.RichTextArtifactLink, ` +
            `BluePrintSys.RC.Client.SL.RichText, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" ` +
            `text="${escapedLinkText}" canclick="True" isvalid="True" canedit="False" ` +
            `href="${linkUrl}" target="_blank" artifactid="${id.toString()}">` +
            `<span style="text-decoration:underline; color:#0000FF;">${escapedLinkText}</span>` +
            `</a></span>&#65279;`;
        /* tslint:enable:max-line-length */
        this.mceEditor["insertContent"](inlineTrace + spacer);
    };

    public handleClick = (event: Event) => {
        const navigationService = this.navigationService;
        const target = event.currentTarget as HTMLElement;

        event.stopPropagation();
        event.preventDefault();
        const itemId = Number(target.getAttribute("subartifactid")) || Number(target.getAttribute("artifactid"));
        if (itemId) {
            if (this.mceEditor) {
                this.mceEditor.destroy(false);
            }
            navigationService.navigateTo({id: itemId});
        } else {
            window.open(target.getAttribute("href"), "_blank");
        }
    };

    public handleLinks = (nodeList: Node[] | NodeList, remove: boolean = false) => {
        if (nodeList.length === 0) {
            return;
        }
        for (let i = 0; i < nodeList.length; i++) {
            let element = nodeList[i] as HTMLElement;

            if (!remove) {
                // IE doesn't show the pointer cursor over links nested in a element with contenteditable=true
                // We need to remove and add back that attribute on mouseover/out!!
                if (document.body.classList.contains("is-msie")) {
                    element.addEventListener("mouseover", this.disableEditability);
                    element.addEventListener("mouseout", this.enableEditability);
                }
                element.addEventListener("click", this.handleClick);
            } else {
                if (document.body.classList.contains("is-msie")) {
                    element.removeEventListener("mouseover", this.disableEditability);
                    element.removeEventListener("mouseout", this.enableEditability);
                }
                element.removeEventListener("click", this.handleClick);
            }
        }
    };

    public handleMutation = (mutation: MutationRecord) => {
        const addedNodes = mutation.addedNodes;
        const removedNodes = mutation.removedNodes;
        if (addedNodes) {
            for (let i = 0; i < addedNodes.length; i++) {
                let node = addedNodes[i];
                if (node.nodeType === 1) { // ELEMENT_NODE
                    if (node.nodeName.toUpperCase() === "A") {
                        this.handleLinks([node]);
                    } else {
                        let element = node as HTMLElement;
                        this.handleLinks(element.querySelectorAll("a"));
                    }
                }
            }
        }
        if (removedNodes) {
            for (let i = 0; i < removedNodes.length; i++) {
                let node = removedNodes[i];
                if (node.nodeType === 1) { // ELEMENT_NODE
                    if (node.nodeName.toUpperCase() === "A") {
                        this.handleLinks([node], true);
                    } else {
                        let element = node as HTMLElement;
                        this.handleLinks(element.querySelectorAll("a"), true);
                    }
                }
            }
        }
    };

    public disableEditability = (e) => {
        if (this.editorBody) {
            this.editorBody.setAttribute("contentEditable", "false");
        }
    };

    public enableEditability = (e) => {
        if (this.editorBody) {
            this.editorBody.setAttribute("contentEditable", "true");
        }
    };
}
