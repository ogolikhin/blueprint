import {IProjectManager, Models, Enums} from "../..";
import { ILocalizationService, IDialogSettings, IDialogService } from "../../../core";
import { ArtifactPickerController } from "../dialogs/bp-artifact-picker/bp-artifact-picker";

export class BpArtifactInfo implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-info.html");
    public controller: Function = BpArtifactInfoController;
    public controllerAs = "$ctrl";
    public bindings: any = {
        currentArtifact: "<",
    };
    public transclude: boolean = true;
}


export class BpArtifactInfoController  {
    private _subscribers: Rx.IDisposable[];
    static $inject: [string] = ["$scope", "projectManager", "dialogService", "localization"];
    private _artifact: Models.IArtifact;

    public currentArtifact: string;

    constructor(private $scope, private projectManager: IProjectManager, private dialogService: IDialogService, private localization: ILocalizationService) {
        
    }
    //all subscribers need to be created here in order to unsubscribe (dispose) them later on component destroy life circle step
    public $onInit() {
        //use context reference as the last parameter on subscribe...
        this._subscribers = [
            //subscribe for current artifact change (need to distinct artifact)
            this.projectManager.currentArtifact.subscribeOnNext(this.updateInfo, this),
        ];
    }
    
     
    public $onDestroy() {
        //dispose all subscribers
        this._subscribers = this._subscribers.filter((it: Rx.IDisposable) => { it.dispose(); return false; });
    }

    private updateInfo = (artifact: Models.IArtifact) => {
        this._artifact = artifact;
    }

    
    public get artifactName(): string {
        return this._artifact ? this._artifact.name : null;
    }

    public get artifactType(): string {
        return this._artifact ?
            (Models.ItemTypePredefined[this._artifact.predefinedType] || "") : 
            null;
    }
    public get artifactTypeDescription(): string {
        return this._artifact ?
            `${Models.ItemTypePredefined[this._artifact.predefinedType] || ""} - ${(this._artifact.prefix || "")}${this._artifact.id}` :
            null;
    }

    public get isReadonly(): boolean {
        return false;
    }

    public get isChanged(): boolean {
        return false;
    }
    public get isLocked(): boolean {
        return false;
    }

    public get isDiagram(): boolean {
        return this._artifact && (this._artifact.predefinedType === Enums.ItemTypePredefined.Storyboard ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.GenericDiagram ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.BusinessProcess ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.UseCase ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.UseCaseDiagram ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.UIMockup ||
            this._artifact.predefinedType === Enums.ItemTypePredefined.DomainDiagram);

    }

    public openPicker() {
        this.dialogService.open(<IDialogSettings>{
            okButton: this.localization.get("App_Button_Open"),
            template: require("../dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: "Some header"
        }).then((artifact:any) => {
            if (artifact) {
                this.projectManager.setCurrentArtifact(artifact);
            }
        });
    }
}