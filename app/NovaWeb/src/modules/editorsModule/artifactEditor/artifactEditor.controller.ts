import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {IMessageService} from "../../main/components/messages/message.svc";
import {Enums} from "../../main/models";
import {IMainWindow, IWindowManager} from "../../main/services/window-manager";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {BpBaseEditor} from "../bp-base-editor";
import {IPropertyDescriptor, IPropertyDescriptorBuilder} from "../services";
import {PropertyEditor} from "../configuration/classes/bp-property-editor";

export abstract class BpArtifactEditor extends BpBaseEditor {

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[] = [];
    public artifactPreviouslyReadonly: boolean = false;
    public editor: PropertyEditor;
    public activeTab: number;

    private fieldObserver: MutationObserver;

    constructor(protected $window: ng.IWindowService,
                public messageService: IMessageService,
                public selectionManager: ISelectionManager,
                public windowManager: IWindowManager,
                public localization: ILocalizationService,
                public propertyDescriptorBuilder: IPropertyDescriptorBuilder) {
        super(selectionManager);
        this.activeTab = 0;
    }

    public $onInit() {
        super.$onInit();
        this.editor = new PropertyEditor(this.localization);
        this.subscribers.push(this.windowManager.mainWindow.subscribeOnNext(this.setArtifactEditorLabelsWidth, this));
    }

    protected destroy(): void {
        if (this.editor) {
            this.editor.destroy();
        }

        this.editor = undefined;
        this.fields = undefined;
        this.model = undefined;

        super.destroy();
    }

    public clearFields() {
        this.fields = [];
    }

    public hasFields(): boolean {
        return (this.fields || []).length > 0;
    }

    private shouldRenewFields(): boolean {
        //Renew fields only if readonly status has changed
        const readonlyStatusChanged = this.artifact.artifactState.readonly !== this.artifactPreviouslyReadonly;
        this.artifactPreviouslyReadonly = this.artifact.artifactState.readonly;

        return readonlyStatusChanged || !this.hasFields();
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        this.fields.push(field);
    }

    protected onArtifactReady() {
        if (this.isDestroyed) {
            return;
        }

        this.propertyDescriptorBuilder.createArtifactPropertyDescriptors(this.artifact)
            .then(propertyContexts => {
                if (this.isDestroyed) {
                    return;
                }

                this.displayContent(propertyContexts);
            });
    }

    private onBeforeFieldCreatedCallback(context: IPropertyDescriptor) {
        if (context.isRichText && context.isMultipleAllowed) {
            context.allowAddImages = true;
        }
    }

    private displayContent(propertyContexts: IPropertyDescriptor[]) {
        const shouldCreateFields = this.editor.create(this.artifact,
            propertyContexts,
            this.shouldRenewFields(),
            this.onBeforeFieldCreatedCallback);

        if (shouldCreateFields) {
            this.clearFields();
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                _.assign(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                const isReadOnly = this.artifact.artifactState.readonly;
                field.templateOptions["isReadOnly"] = isReadOnly;
                if (isReadOnly) {
                    if (field.type !== "bpDocumentFile" &&
                        field.type !== "bpFieldImage" &&
                        field.type !== "bpFieldInheritFrom") {
                        field.type = "bpFieldReadOnly";
                    }
                }
                this.onFieldUpdate(field);
            });
        } else {
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                field.data["isFresh"] = true;
            });
        }
        this.model = this.editor.getModel();

        const pageBodyWrapper = this.$window.document.querySelector(".page-body-wrapper") as HTMLElement;
        const mutationObserver = window["MutationObserver"];
        if (pageBodyWrapper && !_.isUndefined(mutationObserver)) {
            this.fieldObserver = new MutationObserver(mutations => {
                for (let m in mutations) {
                    let mutation = mutations[m];
                    if (mutation.target.nodeType === 1 && mutation.addedNodes.length) {
                        const element = mutation.target as HTMLElement;
                        if (element.classList.contains("formly-field")) {
                            this.setArtifactEditorLabelsWidth();
                            this.fieldObserver.disconnect();
                            return;
                        }
                    }
                }
            });
            this.fieldObserver.observe(pageBodyWrapper, {
                attributes: false,
                childList: true,
                characterData: false,
                subtree: true
            });
        } else {
            this.setArtifactEditorLabelsWidth();
        }

        super.onArtifactReady();
        this.onFieldUpdateFinished();
    }

    protected onFieldUpdateFinished() {
        //
    }

    public setArtifactEditorLabelsWidth(mainWindow?: IMainWindow) {
        let computedMinWidth: number;

        const pageBodyWrapper = this.$window.document.querySelector(".page-body-wrapper") as HTMLElement;
        if (pageBodyWrapper) {
            computedMinWidth = _.parseInt(this.$window.getComputedStyle(pageBodyWrapper).getPropertyValue("min-width"), 10);
        }
        const minWidth = _.isFinite(computedMinWidth) ? computedMinWidth + 6 : 392;

        const formlyField = this.$window.document.querySelector(".page-body-wrapper .formly-field") as HTMLElement;

        if (formlyField) {
            if (formlyField.offsetWidth < minWidth) {
                pageBodyWrapper.classList.add("single-column-property");
            } else {
                pageBodyWrapper.classList.remove("single-column-property");
            }
        }
    };

    public setActive = (index: number): void => {
        this.activeTab = index;
    };

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: ng.IScope) {
        $scope.$applyAsync(() => {
            try {
                const context = $field.data as IPropertyDescriptor;
                if (!context || !this.editor) {
                    return;
                }
                //here we need to update original model
                const value = this.editor.convertToModelValue($field);
                switch (context.lookup) {
                    case Enums.PropertyLookupEnum.Custom:
                        this.artifact.customProperties.set(context.modelPropertyName as number, value);
                        break;
                    case Enums.PropertyLookupEnum.Special:
                        this.artifact.specialProperties.set(context.modelPropertyName as number, value);
                        break;
                    default:
                        this.artifact[context.modelPropertyName] = value;
                        break;
                }
                context.isFresh = false;
            } catch (err) {
                this.messageService.addError(err);
            }
        });
    };
}
