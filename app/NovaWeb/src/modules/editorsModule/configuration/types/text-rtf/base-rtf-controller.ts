import "angular-formly";
import "angular-ui-tinymce";
import "tinymce";
import {IFormlyScope} from "../../formly-config";
import {INavigationService} from "../../../../commonModule/navigation/navigation.service";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";
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
import {IRelationship, LinkType, TraceDirection} from "../../../../main/models/relationshipModels";
import {IPropertyDescriptor} from "../../../services";
import {IMessageService} from "../../../../main/components/messages/message.svc";

enum DragAndDropState {
    None = 0,
    Drag = 1,
    Drop = 2,
    DropExternal = 3
}

export interface IBPFieldBaseRTFController {
    editorBody: HTMLElement;
    observer: MutationObserver;
    handleClick(event: Event): void;
    handleLinks(nodeList: Node[] | NodeList, remove: boolean): void;
    handleMutation(mutation: MutationRecord): void;
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
    protected isLinkPopupOpen: boolean;
    protected isIE11: boolean;
    protected dragAndDropState: DragAndDropState;
    protected hasReceivedFocus: boolean;
    protected contentBuffer: string;
    protected mceEditor: TinyMceEditor;
    protected editorContainer: HTMLElement;
    protected onChange: AngularFormly.IExpressionFunction;
    protected allowedFonts: string[];
    protected isSingleLine: boolean = false;
    protected execCommandEvents: {[key: string]: boolean} = {
        "mceInsertContent": true,
        "mceToggleFormat": true,
        "RemoveFormat": true,
        "InsertUnorderedList": true,
        "InsertOrderedList": true,
        "Outdent": true,
        "Indent": true,
        "FontName": true
    };
    protected linkEvents: {[key: string]: boolean} = {
        "mceLink": true
    };
    protected fontSizes: string[] = ["8", "9", "10", "11", "12", "14", "16", "18", "20"];
    protected embeddedImagesUrls: {[key: string]: boolean};

    constructor(protected $q: ng.IQService,
                protected $scope: IFormlyScope,
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
        this.isLinkPopupOpen = false;
        this.hasReceivedFocus = false;
        this.dragAndDropState = DragAndDropState.None;
        this.contentBuffer = undefined;

        this.isIE11 = false;
        if (this.$window.navigator) {
            const ua = this.$window.navigator.userAgent;
            this.isIE11 = !!(ua.match(/Trident/) && ua.match(/rv[ :]11/)) && !ua.match(/edge/i);
        }

        // the onChange event has to be called from the custom validator (!) as otherwise it will fire before the actual validation takes place
        this.onChange = ($scope.to.onChange as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.
        //we override the default onChange as we need to deal with changes differently when using tinymce
        $scope.to.onChange = undefined;

        $scope.$on("$destroy", () => {
            $scope.to.onChange = this.onChange;

            this.destroy();

            // the following is to avoid TFS BUG 4330
            // The bug is caused by IE9-11 not being able to focus on other INPUT elements if the focus was
            // on a destroyed/removed from DOM element before. See also:
            // http://stackoverflow.com/questions/19581464
            // http://stackoverflow.com/questions/8978235
            if (this.isIE11 && !this.isSingleLine && this.hasReceivedFocus) {
                const focusCatcher = this.$window.document.body.querySelector("input[type='text']") as HTMLElement;
                if (focusCatcher) {
                    focusCatcher.focus();
                    focusCatcher.blur();
                }
            }
        });

        $scope.options["expressionProperties"] = {
            "model": () => {
                const context: IPropertyDescriptor = $scope.options["data"];
                if (context.isFresh && this.mceEditor) { // format the data only if fresh
                    this.prepRTF(!this.isSingleLine);
                }
            }
        };
    }

    private destroy = () => {
        if (this.observer) {
            this.observer.disconnect();
        }
        if (this.editorBody) {
            this.handleLinks(this.editorBody.querySelectorAll("a"), true);
        }
        if (this.mceEditor) {
            // https://www.tinymce.com/docs/api/tinymce/tinymce.editor/#destroy
            this.mceEditor.destroy(false);
        }
    };

    private getAppBaseUrl = (): string => {
        const location = this.$window.location as Location;

        let origin: string = location.origin;
        if (!origin) {
            origin = location.protocol + "//" + location.hostname + (location.port ? ":" + location.port : "");
        }

        return origin;
    };

    private initEmbeddedImagesList = (content?: string) => {
        this.embeddedImagesUrls = {};

        const baseUrl = this.getAppBaseUrl();
        const node = this.$window.document.createElement("div");
        node.innerHTML = content || this.contentBuffer;
        const images = node.getElementsByTagName("img");
        _.forEach(images, (image: HTMLImageElement) => {
            this.embeddedImagesUrls[image.src.replace(baseUrl, "")] = true;
        });
    };

    /*
     Strips all images not already in the RT editor on drag&drop and copy/paste.

     We cannot allow to paste images in the RT editor, as the back-end assumes the images are coming from the
     Insert Image menu command and not from other sources.

     Moreover, we cannot even allow to copy and paste and already embedded image from a RT editor into the same
     RT editor as the back-end responds with a "An item with the same key has already been added." error if the
     user tries to copy an artifact with a duplicated embedded image).

     While performing a drag&drop of text+images inside an RT editor doesn't cause any issue (as the dragged image
     just changes position), we need to deal with the following scenario:
     - the user has an RT editor with text and images
     - he selects one or more images
     - he cuts the selection (e.g. Control+X)
     - he pastes the selection (e.g. Control+V). This action SHOULD be allowed
     - he pastes again. This action should NOT be allowed
     */
    private stripNonEmbeddedAndDuplicatedImages = (node: HTMLElement) => {
        /*
         We need to strip images only if:
         - the user pastes something (DragAndDropState.None)
         - the user drops something from an external source (DragAndDropState.DropExternal)
        */
        if (this.dragAndDropState !== DragAndDropState.None &&
            this.dragAndDropState !== DragAndDropState.DropExternal) {
            return;
        }

        const baseUrl = this.getAppBaseUrl();
        const embeddedImagesUrls = _.clone(this.embeddedImagesUrls);

        if (this.mceEditor && this.dragAndDropState === DragAndDropState.None) {
            /*
             If it's not caused by drag&drop:
             - make a copy of the embedded images URLs list (which is populated on RT editor init, and any time
               a new image is added via the Insert Image menu command)
             - get the fresh content of the RT editor
             - remove, from the copied list, all the images currently present in the content (thus leaving the
               images that have been cut).
             */
            const container = this.$window.document.createElement("div");
            container.innerHTML = this.mceEditor.getContent();
            const embeddedImages = container.getElementsByTagName("img");
            _.forEach(embeddedImages, (image: HTMLImageElement) => {
                /*
                 If it's not caused by drag&drop, embeddedImagesUrls contains only the embedded images that are NOT
                 currently present in the content (e.g. the use has cut them), and therefore it will allow pasting
                 only those images.

                 Otherwise (drag&drop), embeddedImagesUrls contains ALL the embedded images, and therefore will allow
                 only dragged embedded images to stay (thus removing any image embedded in content dragged from other
                 external sources).
                 */
                let imgSrc = image.src as string;
                imgSrc = imgSrc.replace(baseUrl, "");
                embeddedImagesUrls[imgSrc] = false;
            });
        }

        const images = node.getElementsByTagName("img");
        _.forEachRight(images, (image: HTMLImageElement | any) => {
            let imgSrc = image.src || image.dataset.tempSrc as string;
            imgSrc = imgSrc.replace(baseUrl, "");
            if (!embeddedImagesUrls[imgSrc]) {
                image.parentNode.removeChild(image);
            }
        });

        if (this.dragAndDropState === DragAndDropState.DropExternal) {
            // very expensive, as it will cycle through all the drag&dropped elements
            const nodesWithBackground = _.filter(node.getElementsByTagName("*"), (child: HTMLElement) => {
                const backgroundImage = this.$window.getComputedStyle(child).getPropertyValue("background-image");
                return backgroundImage !== "none" && backgroundImage !== "";
            });
            _.forEach(nodesWithBackground, (node: HTMLElement) => {
                node.style.backgroundImage = "";
            });
        }
    };

    protected addEmbeddedImageToList = (url: string) => {
        const imageUrl = url.replace(this.getAppBaseUrl(), "");
        this.embeddedImagesUrls[imageUrl] = true;
    };

    protected handleValidation = (content: string) => {
        const $scope = this.$scope;
        $scope.$applyAsync(() => {
            const isValid = this.validationService.textRtfValidation.hasValueIfRequired($scope.to.required, content);
            const formControl = $scope.fc as ng.IFormController;
            if (formControl) {
                formControl.$setValidity("requiredCustom", isValid, formControl);
                $scope.to["isInvalid"] = !isValid;
                $scope.options.validation["show"] = !isValid;
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

        const $scope = this.$scope;
        if (this.contentBuffer !== newContent) {
            this.isDirty = true;
            this.isLinkPopupOpen = false;

            this.handleValidation(newContent);

            $scope.options["data"].isFresh = false;
            if (typeof this.onChange === "function") {
                $scope.model[$scope.options["key"]] = newContent;
                this.onChange(newContent, $scope.options, $scope);
            }
        }
    };

    protected prepRTF = (hasTables: boolean = false) => {
        const $scope = this.$scope;
        this.mceEditor.setContent($scope.model[$scope.options["key"]] || "");
        this.isDirty = false;
        this.isLinkPopupOpen = false;
        this.editorBody = this.mceEditor.getBody() as HTMLElement;
        this.normalizeHtml(this.editorBody, hasTables);
        this.handleLinks(this.editorBody.querySelectorAll("a"));
        this.contentBuffer = this.mceEditor.getContent();
        this.initEmbeddedImagesList();

        this.handleValidation(this.contentBuffer);
        $scope.options["data"].isFresh = false;
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

    protected fontSizeMenu = (editor): TinyMceMenu[] => {
        return this.fontSizes.map((size) => {
            return {
                text: size,
                onclick: () => {
                    editor.formatter.apply(`font${size}`);
                    this.triggerChange();
                }
            } as TinyMceMenu;
        });
    };

    protected linksMenu = (editor): TinyMceMenu[] => {
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
        }] as TinyMceMenu[];
    };

    protected initInstanceCallback = (editor) => {
        this.mceEditor = editor;
        this.mceEditor.undoManager.clear();

        editor.formatter.register("font8", {
            inline: "span",
            styles: {"font-size": "8pt"}
        });
        editor.formatter.register("font9", {
            inline: "span",
            styles: {"font-size": "9pt"}
        });
        editor.formatter.register("font10", {
            inline: "span",
            styles: {"font-size": "10pt"}
        });
        editor.formatter.register("font11", {
            inline: "span",
            styles: {"font-size": "11pt"}
        });
        editor.formatter.register("font12", { // default font
            inline: "span",
            styles: {"font-size": "12pt"}
        });
        editor.formatter.register("font14", {
            inline: "span",
            styles: {"font-size": "14pt"}
        });
        editor.formatter.register("font16", {
            inline: "span",
            styles: {"font-size": "16pt"}
        });
        editor.formatter.register("font18", {
            inline: "span",
            styles: {"font-size": "18pt"}
        });
        editor.formatter.register("font20", {
            inline: "span",
            styles: {"font-size": "20pt"}
        });

        this.prepRTF(!this.isSingleLine);

        if (this.isSingleLine && this.editorBody && this.editorBody.parentElement) {
            this.editorContainer = this.editorBody.parentElement;
        } else if (!this.isSingleLine && editor && editor.editorContainer) {
            this.editorContainer = editor.editorContainer;
        }

        // MutationObserver
        const mutationObserver = this.$window["MutationObserver"];
        if (this.editorBody && !_.isUndefined(mutationObserver)) {
            // create an observer instance
            this.observer = new MutationObserver((mutations) => {
                mutations.forEach(this.handleMutation);
            });

            const observerConfig = {
                attributes: false,
                childList: true,
                characterData: false,
                subtree: true
            };
            this.observer.observe(this.editorBody, observerConfig);
        }

        editor.on("KeyDown", (e: KeyboardEvent) => {
            const KEY_X = 88;
            const KEY_Z = 90;

            if (e && [KEY_X, KEY_Z].indexOf(e.keyCode) !== -1 && (e.ctrlKey || e.metaKey)) {
                this.$scope.$applyAsync(() => {
                    editor.save();
                    this.triggerChange(editor.getContent());
                });
            }
        });

        editor.on("KeyUp", (e: KeyboardEvent) => {
            const KEY_DELETE = 8;
            const KEY_BACKSPACE = 46;

            if (e && [KEY_DELETE, KEY_BACKSPACE].indexOf(e.keyCode) !== -1) {
                if (this.isDirty || this.contentBuffer !== editor.getContent()) {
                    this.triggerChange();
                }
                return;
            }
        });

        editor.on("Change", (e) => {
            if (e && _.isObject(e.lastLevel)) { // tinyMce emits a 2 change events per actual change
                if (this.isDirty || this.contentBuffer !== editor.getContent() || this.hasChangedFormat() || this.isLinkPopupOpen) {
                    this.triggerChange();
                }
            }
        });

        editor.on("ExecCommand", (e) => {
            if (e && this.execCommandEvents[e.command]) {
                this.triggerChange();
            } else if (e && this.linkEvents[e.command]) {
                this.isLinkPopupOpen = true;
            }
        });

        editor.on("Focus", (e) => {
            this.hasReceivedFocus = true;
            if (this.editorContainer && this.editorContainer.parentElement) {
                this.editorContainer.parentElement.classList.remove("tinymce-toolbar-hidden");
            }
        });

        editor.on("Blur", (e) => {
            if (this.editorContainer && this.editorContainer.parentElement) {
                this.editorContainer.parentElement.classList.add("tinymce-toolbar-hidden");
            }
        });

        /*
         The DragEvent sequence is comprised of events being fired between the element being dragged and the drop target.

         The general order is:
         1. elem.dragstart
         2. target.dragenter
         3. elem.drag/target.dragover (each every few hundreds milliseconds)
         4 .target.drop or target.dragleave (depending if the element has been dropped on the target or dragged outside)
         5. elem.dragend

         When dragging and dropping within the editor, the order is:
         1. dragstart
         2. drop
         3. dragend (Firefox seems not to fire this one!)

         When drag starts in the editor but ends outside, the order is:
         1. dragstart
         2. dragend

         When drag starts outside the editor but ends inside, we just have a drop event

         See http://www.developerfusion.com/article/144828/the-html5-drag-and-drop-api/
        */
        let contentBeforeDrag: string;

        editor.on("dragstart", (e: DragEvent) => {
            // TinyMce sets Text/URL data on the event's dataTransfer object to a special "data:text/mce-internal" url.
            // This is to workaround the inability to set custom contentType on IE and Safari. The editor's selected
            // content is encoded into this url so drag and drop between editors will work. Unfortunately, this approach
            // creates issues when the user drags content from the editor into other input/textarea elements, as the
            // dropped content is what TinyMce has encoded. Therefore the need to replace/handle it.

            this.dragAndDropState = DragAndDropState.Drag;

            if (e) {
                if (!this.isIE11) {
                    // In case we are dragging a single image, we attach a sort of "tag" to the dataTransfer object,
                    // in order to be able to recognize it before dropping, as the dragged content is not available before
                    // the drop event and it is writable only on dragstart. See:
                    // - https://www.w3.org/TR/2011/WD-html5-20110113/dnd.html#the-datatransfer-interface
                    // - http://stackoverflow.com/questions/11065803/determine-what-is-being-dragged-from-dragenter-dragover-events/23416174#23416174
                    // - https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Recommended_drag_types
                    const data = e.dataTransfer.getData("text/html");
                    const reImg = /^<img [^>]*>$/gi;
                    if (reImg.test(data)) {
                        e.dataTransfer.setData("tinymce-image", "");
                    }
                } else {
                    // IE11 ONLY allows the type "text" and therefore the only approach is to replace the data being dragged
                    // with a text-only equivalent (i.e. no html tags). This apporach cannot be used with Chrome or Firefox,
                    // as those browsers use multiple and different types to transfer the data.
                    e.dataTransfer.setData("text", editor.selection.getContent({format: "text"}));
                    contentBeforeDrag = editor.getContent();
                }
            }
        });

        editor.on("dragend", (e: DragEvent) => {
            if (this.dragAndDropState === DragAndDropState.Drag) {
                // we are dragging outside of the editor
                this.dragAndDropState = DragAndDropState.None;

                if (this.isIE11) {
                    this.$scope.$applyAsync(() => {
                        editor.setContent(contentBeforeDrag);
                    });
                }
            }
        });

        editor.on("drop", (e: DragEvent) => {
            if (this.dragAndDropState === DragAndDropState.Drag) {
                // we are dropping after drag started from within editor.
                this.dragAndDropState = DragAndDropState.Drop;
            } else {
                // we are dropping after drag started from outside the editor
                this.dragAndDropState = DragAndDropState.DropExternal;
            }

            this.$scope.$applyAsync(() => {
                editor.save();
                this.stripNonEmbeddedAndDuplicatedImages(editor.getBody() as HTMLElement);
                this.triggerChange();
                this.dragAndDropState = DragAndDropState.None;
            });
        });
    };

    protected pastePostProcess = (plugin, args) => { // https://www.tinymce.com/docs/plugins/paste/#paste_postprocess
        this.normalizeHtml(args.node, !this.isSingleLine);
        Helper.removeAttributeFromNode(args.node, "id");
    };

    protected pastePreProcess = (plugin, args) => { // https://www.tinymce.com/docs/plugins/paste/#paste_preprocess
        let pasteContent = args.content as string;

        const node = document.createElement("div");
        node.innerHTML = Helper.replaceImgSrc(pasteContent, true);
        this.stripNonEmbeddedAndDuplicatedImages(node);
        pasteContent = Helper.replaceImgSrc(node.innerHTML, false);

        // remove generic font family
        args.content = pasteContent.replace(/, ?sans-serif([;'"])/gi, "$1")
            .replace(/, ?serif([;'"])/gi, "$1")
            .replace(/, ?monospace([;'"])/gi, "$1");
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
    }

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
        const linkUrl: string = this.getAppBaseUrl() + "/?ArtifactId=" + id.toString();
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
        this.mceEditor.insertContent(inlineTrace + spacer);
    };

    public handleClick = (event: Event) => {
        const navigationService = this.navigationService;
        const target = event.currentTarget as HTMLElement;

        event.stopPropagation();
        event.preventDefault();
        const itemId = Number(target.getAttribute("subartifactid")) || Number(target.getAttribute("artifactid"));
        if (itemId) {
            navigationService.navigateTo({id: itemId})
                .then(() => {
                    if (this.mceEditor) {
                        this.mceEditor.destroy(false);
                    }
                });
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
