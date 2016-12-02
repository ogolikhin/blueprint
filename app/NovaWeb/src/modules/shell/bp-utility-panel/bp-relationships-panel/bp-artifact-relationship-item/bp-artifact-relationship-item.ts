import {Helper, IDialogService} from "../../../../shared";
import {Relationships} from "../../../../main";
import {INavigationService} from "../../../../core/navigation/navigation.svc";
import {ILocalizationService} from "../../../../core/localization/localizationService";
import {IArtifactManager} from "../../../../managers/artifact-manager/artifact-manager";
import {IRelationshipDetailsService} from "./relationship-details.svc";

export class BPArtifactRelationshipItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-relationship-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPArtifactRelationshipItemController;
    public bindings: any = {
        relationship: "=",
        selectedTraces: "=",
        selectable: "<",
        setItemDirection: "&",
        toggleItemFlag: "&",
        deleteItem: "&",
        isItemReadOnly: "<"
    };
}

interface IBPArtifactRelationshipItemController {
    setItemDirection: Function;
    toggleItemFlag: Function;
    deleteItem: Function;
}

export class BPArtifactRelationshipItemController implements IBPArtifactRelationshipItemController {
    public static $inject: [string] = [
        "localization",
        "relationshipDetailsService",
        "artifactManager",
        "dialogService",
        "navigationService"
    ];

    public expanded: boolean = false;
    public relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo;
    public relationship: Relationships.IRelationship;
    public isItemReadOnly: boolean;
    public selectedTraces: Relationships.IRelationship[];
    public fromOtherProject: boolean = false;
    public selectable: boolean;
    public showActionsPanel: boolean;
    public setItemDirection: Function;
    public toggleItemFlag: Function;
    public deleteItem: Function;
    public itemVersionId: number;
    public traceDescription: string;

    constructor(private localization: ILocalizationService,
                private relationshipDetailsService: IRelationshipDetailsService,
                private artifactManager: IArtifactManager,
                private dialogService: IDialogService,
                private navigationService: INavigationService) {
    }

    public $onInit() {
        if (this.relationship) {
            this.showActionsPanel = this.relationship.hasAccess && this.selectable;
        }
    }

    public get isSelected() {
        return this.selectable && this.relationship.isSelected;
    }

    public setDirection(direction: Relationships.TraceDirection) {
        if (this.relationship.hasAccess) {
            this.relationship.traceDirection = direction;
            this.setItemDirection();
        }
    }

    public expand($event) {
        this.remove($event);
        if (!this.expanded) {
            this.getRelationshipDetails(this.relationship.artifactId)
                .then(relationshipExtendedInfo => {
                    if (relationshipExtendedInfo.pathToProject.length > 0 && relationshipExtendedInfo.pathToProject[0].parentId == null) {
                        relationshipExtendedInfo.pathToProject.shift(); // do not show project in the path.
                    }
                    this.traceDescription = relationshipExtendedInfo.description ?
                        this.limitChars(relationshipExtendedInfo.description) : this.localization.get("Property_Not_Available");
                    this.relationshipExtendedInfo = relationshipExtendedInfo;
                });
        }
        this.expanded = !this.expanded;
    }

    public selectTrace() {
        if (!this.relationship.isSelected) {
            if (this.selectedTraces) {
                const found = _.find(this.selectedTraces, {itemId: this.relationship.itemId});
                if (!found) {
                    this.selectedTraces.push(this.relationship);
                }
            }
        } else {
            if (this.selectedTraces) {
                const foundIndex = _.findIndex(this.selectedTraces, {itemId: this.relationship.itemId});
                if (foundIndex > -1) {
                    this.selectedTraces.splice(foundIndex, 1);
                }
            }
        }
        this.relationship.isSelected = !this.relationship.isSelected;
    }

    public remove($event) {
        if ($event.stopPropagation) {
            $event.stopPropagation();
        }
        if ($event.preventDefault) {
            $event.preventDefault();
        }
        $event.cancelBubble = true;
        $event.returnValue = false;
    }

    public limitChars(str) {
        if (str) {
            return Helper.limitChars(Helper.stripHTMLTags(str));
        }

        return "";
    }

    private getRelationshipDetails(artifactId: number, versionId?: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
        return this.relationshipDetailsService.getRelationshipDetails(artifactId, versionId)
            .then((relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo) => {
                if (relationshipExtendedInfo.pathToProject[0].parentId === 0) {
                    this.relationship.projectId = relationshipExtendedInfo.pathToProject[0].itemId;
                    this.relationship.projectName = relationshipExtendedInfo.pathToProject[0].itemName;
                }
                return relationshipExtendedInfo;
            });
    }

    public navigateToArtifact(id: number) {
        if (this.relationship.hasAccess) {
            this.navigationService.navigateTo({id: id});
        }
    }
}
