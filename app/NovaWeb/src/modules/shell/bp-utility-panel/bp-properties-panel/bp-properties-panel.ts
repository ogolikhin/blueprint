import {ILocalizationService } from "../../../core";
import { Models, IWindowManager } from "../../../main";
import { ISelectionManager, IStatefulArtifact, IStatefulSubArtifact } from "../../../managers/artifact-manager";
import {IBpAccordionPanelController } from "../../../main/components/bp-accordion/bp-accordion";
import {BPBaseUtilityPanelController } from "../bp-base-utility-panel";
import {IMessageService} from "../../../core";
import {PropertyEditor} from "../../../editors/bp-artifact/bp-property-editor";
import {PropertyContext} from "../../../editors/bp-artifact/bp-property-context";
import {PropertyLookupEnum, LockedByEnum} from "../../../main/models/enums";

export class BPPropertiesPanel implements ng.IComponentOptions {
    public template: string = require("./bp-properties-panel.html");
    public controller: Function = BPPropertiesController;   
    public controllerAs = "$ctrl";
    public require: any = {
        bpAccordionPanel: "^bpAccordionPanel"
    };
} 

export class BPPropertiesController extends BPBaseUtilityPanelController {

    public static $inject: [string] = [
        "$q",
        "selectionManager",
        "messageService",
        "windowManager",
        "localization",
    ];

    public form: angular.IFormController;
    public model = {};
    public fields: AngularFormly.IFieldConfigurationObject[];

    public editor: PropertyEditor;   

    public isLoading: boolean = false;

    public systemFields: AngularFormly.IFieldConfigurationObject[];
    public customFields: AngularFormly.IFieldConfigurationObject[];
    public specificFields: AngularFormly.IFieldConfigurationObject[];
    public richTextFields: AngularFormly.IFieldConfigurationObject[];

    private selectedArtifact: IStatefulArtifact;
    private selectedSubArtifact: Models.ISubArtifact;

    constructor(
        $q: ng.IQService,
        protected selectionManager: ISelectionManager,        
        public messageService: IMessageService,
        public windowManager: IWindowManager,
        public localization: ILocalizationService,
        public bpAccordionPanel: IBpAccordionPanelController) {

        super($q, selectionManager, bpAccordionPanel);
        this.editor = new PropertyEditor(this.localization);
    }    

    public $onDestroy() {
        delete this.systemFields;
        delete this.specificFields;
        delete this.customFields;
        delete this.richTextFields;
        super.$onDestroy();        
    }

    public get isSystemPropertyAvailable(): boolean {
        return this.systemFields && this.systemFields.length > 0;
    }

    public get isSpecificPropertyAvailable(): boolean {
        return this.specificFields && this.specificFields.length > 0;
    }

    public get isCustomPropertyAvailable(): boolean {
        return this.customFields && this.customFields.length > 0;
    }

    public get isRichTextPropertyAvailable(): boolean {
        return this.richTextFields && this.richTextFields.length > 0;
    }

    protected onSelectionChanged(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<any> {
        try {
            this.fields = [];
            this.model = {};
            this.systemFields = [];
            this.customFields = [];
            this.specificFields = [];
            this.richTextFields = [];         
            if (artifact) {
                return this.onLoad(artifact, subArtifact, timeout);
            }
        } catch (ex) {
            this.messageService.addError(ex);
            throw ex;
        }
        return super.onSelectionChanged(artifact, subArtifact, timeout);
    }

    private onLoad(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact, timeout: ng.IPromise<void>): ng.IPromise<void> {
        let deferred = this.$q.defer<any>();
        this.isLoading = true;
        if (subArtifact) {
            subArtifact.load(true, timeout).then(() => {
                this.onUpdate(artifact, subArtifact);
            })
            .finally(() => {
                deferred.resolve();
                this.isLoading = false;
            });
                    
        } else {
            artifact.load(false).then(() => {
                this.onUpdate(artifact, subArtifact);
            })
            .finally(() => {
                deferred.resolve();
                this.isLoading = false;
            });
        }
        return deferred.promise;
    }

    public onUpdate(artifact: IStatefulArtifact, subArtifact: IStatefulSubArtifact) {
        this.selectedArtifact = artifact;
        this.selectedSubArtifact = subArtifact;
        try {
            
            if (!artifact || !this.editor) {
                return;
            }

            if (subArtifact) {
                this.editor.load(subArtifact, subArtifact.metadata.getSubArtifactPropertyTypes());
            } else {
                this.editor.load(artifact, artifact.metadata.getArtifactPropertyTypes());
            }

            // let changedArtifact = this.getChangedArtifact(artifact);           
            // let changedSubArtifact = this.getChangedSubArtifact(subArtifact);            

            
            this.model = this.editor.getModel();
            this.editor.getFields().forEach((field: AngularFormly.IFieldConfigurationObject) => {
                //add property change handler to each field
                angular.extend(field.templateOptions, {
                    onChange: this.onValueChange.bind(this)
                });

                let isReadOnly = this.selectedArtifact.artifactState.readonly || this.selectedArtifact.artifactState.lockedBy === LockedByEnum.OtherUser;
                field.templateOptions["isReadOnly"] = isReadOnly;
                if (isReadOnly) {
                    if (field.key !== "documentFile" &&
                        field.type !== "bpFieldImage" &&
                        field.type !== "bpFieldInheritFrom") {
                        field.type = "bpFieldReadOnly";
                    }
                }                   

                this.onFieldUpdate(field);                                

            });
        } catch (ex) {
            this.messageService.addError(ex);
            throw ex;
        }       
    }

    public onFieldUpdate(field: AngularFormly.IFieldConfigurationObject) {
        let propertyContext = field.data as PropertyContext;
        if (!propertyContext) {
            return;
        }

        if (true === propertyContext.isRichText && PropertyLookupEnum.Special === propertyContext.lookup) {
            this.systemFields.push(field);
            return;
        }

        //re-group fields
        if (true === propertyContext.isRichText) {
            this.richTextFields.push(field);
        } else if (PropertyLookupEnum.System === propertyContext.lookup) {
            this.systemFields.push(field);
        } else if (PropertyLookupEnum.Custom === propertyContext.lookup) {
            this.customFields.push(field);
        } else if (PropertyLookupEnum.Special === propertyContext.lookup) {
            this.specificFields.push(field);
        }

    }

    public onValueChange($value: any, $field: AngularFormly.IFieldConfigurationObject, $scope: AngularFormly.ITemplateScope) {
        //here we need to update original model
        let context = $field.data as PropertyContext;
        if (!context) {
            return;
        }
        //let value = this.editor.convertToModelValue($field, $value);
        // let changeSet: IPropertyChangeSet = {
        //     lookup: context.lookup,
        //     id: context.modelPropertyName,
        //     value: value
        // };
        
        // if (this.selectedSubArtifact) {
        //     this.addSubArtifactChangeset(this.selectedArtifact, this.selectedSubArtifact, changeSet);
        // } else {
        //     this.stateManager.addChange(this.selectedArtifact, changeSet);
        // }
    };

    public get specificPropertiesHeading(): string {
        if (this.selectedArtifact.predefinedType === Models.ItemTypePredefined.Document) {
            return this.localization.get("Nova_Document_File", "File");
        } else if (this.selectedArtifact.predefinedType === Models.ItemTypePredefined.Actor) {
            return this.localization.get("Property_Actor_Section_Name", "Actor Properties");
        } else {
            return this.selectedArtifact.predefinedType + this.localization.get("Nova_Properties", " Properties");
        }
    }
}

