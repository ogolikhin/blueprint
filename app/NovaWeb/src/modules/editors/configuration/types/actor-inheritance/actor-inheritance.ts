import "angular";
import {ILocalizationService, IMessageService} from "../../../../core";
import {IDialogSettings, IDialogService} from "../../../../shared";
import {BPFieldBaseController} from "../base-controller";
import {Models} from "../../../../main/models";
import {ISelectionManager} from "../../../../managers";
import {ArtifactPickerDialogController, IArtifactPickerOptions} from "../../../../main/components/bp-artifact-picker";
import {INavigationService} from "../../../../core/navigation/navigation.svc";

export class BPFieldInheritFrom implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldInheritFrom";
    public wrapper: string = "bpFieldLabel";
    public template: string = require("./actor-inheritance.template.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPFieldInheritFromController;
    public defaultOptions: AngularFormly.IFieldConfigurationObject;

    constructor() {
        this.defaultOptions = {};
    }
}

export class BPFieldInheritFromController extends BPFieldBaseController {
    static $inject: [string] = ["$scope", "localization", "$window", "messageService", "dialogService", "selectionManager", "navigationService"];

    constructor(private $scope: AngularFormly.ITemplateScope,
                private localization: ILocalizationService,
                private $window: ng.IWindowService,
                private messageService: IMessageService,
                private dialogService: IDialogService,
                private selectionManager: ISelectionManager,
                private navigationService: INavigationService) {
        super();

        const templateOptions: AngularFormly.ITemplateOptions = $scope["to"];
        let onChange = (templateOptions["onChange"] as AngularFormly.IExpressionFunction); //notify change function. injected on field creation.

        let currentModelVal = <Models.IActorInheritancePropertyValue>$scope.model[$scope.options["key"]];
        if (currentModelVal != null) {
            currentModelVal.isProjectPathVisible = isArtifactactPathFitToControl(currentModelVal.actorPrefix,
                currentModelVal.actorName, currentModelVal.actorId, currentModelVal.pathToProject);
        }

        $scope["deleteBaseActor"] = () => {
            onChange(null, getInheritanceField(), $scope);
            deleteBaseActor();
        };

        function deleteBaseActor() {
            currentModelVal = null;
            $scope.model[$scope.options["key"]] = null;
        }

        function getArtifactPath(artifact: Models.IArtifact): string[] {
            if (!artifact) {
                return [];
            }
            let currentArtifact = artifact.parent;
            let path: string[] = [];
            while (currentArtifact) {
                path.unshift(currentArtifact.name);
                currentArtifact = currentArtifact.parent;
            }
            return path;
        }

        function isArtifactactPathFitToControl(prefix: string, name: string, id: number, artifactPath: string[]): boolean {
            if (!artifactPath || !prefix || !id || !name) {
                return true;
            }
            return artifactPath.length > 0 && (artifactPath.toString().length + prefix.length + id.toString().length + name.length) < 39;
        }

        function setBaseActor() {
            const dialogSettings = <IDialogSettings>{
                okButton: localization.get("App_Button_Open"),
                template: require("../../../../main/components/bp-artifact-picker/bp-artifact-picker-dialog.html"),
                controller: ArtifactPickerDialogController,
                css: "nova-open-project",
                header: localization.get("App_Properties_Actor_InheritancePicker_Title")
            };

            const dialogData: IArtifactPickerOptions = {
                selectableItemTypes: [Models.ItemTypePredefined.Actor],
                showSubArtifacts: false
            };

            dialogService.open(dialogSettings, dialogData).then((items: Models.IItem[]) => {
                if (items.length === 1) {
                    const artifact = items[0];
                    let selected = selectionManager.getArtifact();
                    if (selected) {
                        if (selected.id === artifact.id) {
                            messageService.addError("App_Properties_Actor_SameBaseActor_ErrorMessage"); // , "Actor cannot be set as its own parent")
                            return;
                        }
                    }
                    if (currentModelVal != null) {
                        deleteBaseActor();

                    }
                    const artifactPath = getArtifactPath(artifact);
                    $scope.model[$scope.options["key"]] = {
                        actorName: artifact.name,
                        actorId: artifact.id,
                        actorPrefix: artifact.prefix,
                        hasAccess: true,
                        pathToProject: artifactPath,
                        isProjectPathVisible: isArtifactactPathFitToControl(artifact.prefix, artifact.name, artifact.id, artifactPath)
                    };
                    currentModelVal = $scope.model[$scope.options["key"]];
                    const changedResult = {
                        actorId: artifact.id
                    };
                    onChange(changedResult, getInheritanceField(), $scope);
                }
            });
        }

        function getInheritanceField(): any {
            if (!$scope.fields) {
                return null;
            }
            return $scope.fields[1];
        }

        $scope["navigateToItem"] = (id: number) => {
            this.navigationService.navigateTo(id);
        };

        $scope["selectBaseActor"] = () => {
            setBaseActor();
        };
    }
}
