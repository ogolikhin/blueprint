import { ILocalizationService } from "../../../../core";
import { Helper, IDialogService } from "../../../../shared";
import { Relationships } from "../../../../main";
import { IArtifactManager } from "../../../../managers";
import {IStatefulArtifact} from "../../../../managers/models";
import { IRelationshipDetailsService } from "../../../";

export class BPArtifactRelationshipItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-relationship-item.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPArtifactRelationshipItemController;
    public bindings: any = {
        artifact: "=",
        selectedTraces: "=",
        selectable: "@",
        deleteItem: "&",
    };
}

export interface IResult {
    found: boolean;
    index: number;
}

interface IBPArtifactRelationshipItemController {
    deleteItem: Function;
}

export class BPArtifactRelationshipItemController implements IBPArtifactRelationshipItemController{
    public static $inject: [string] = [
        "localization",
        "relationshipDetailsService",
        "artifactManager",
        "dialogService"
    ];

    public expanded: boolean = false;
    public relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo;
    public artifact: Relationships.IRelationship;
    public selectedTraces: Relationships.IRelationship[];
    public fromOtherProject: boolean = false;  
    public selectable: boolean = false;
    public deleteItem: Function;

    constructor(
        private localization: ILocalizationService,
        private relationshipDetailsService: IRelationshipDetailsService,
        private artifactManager: IArtifactManager,
        private dialogService: IDialogService
    ) {

    }

    public get isSelected() {
        return this.selectable.toString() === "true" && this.artifact.isSelected;
    }

    public setDirection(direction: Relationships.TraceDirection): void {
        if (this.artifact.hasAccess) {
            this.artifact.traceDirection = direction;
        }
    }

    public toggleFlag() {
        if (this.artifact.hasAccess) {
            this.artifact.suspect = this.artifact.suspect === true ? false : true;
        }
    }

    public expand($event) {
        this.remove($event);
        if (!this.expanded) {
            this.getRelationshipDetails(this.artifact.artifactId)
                .then((relationshipExtendedInfo: any) => {
                    this.relationshipExtendedInfo = relationshipExtendedInfo;
                });
        }
        this.expanded = !this.expanded;
    }

    public selectTrace() {
            if (!this.artifact.isSelected) {
                if (this.selectedTraces) {
                    let res = this.inArray(this.selectedTraces);
                    if (!res.found) {
                        this.selectedTraces.push(this.artifact);
                    }
                }
            } else {
                if (this.selectedTraces) {                
                    let res = this.inArray(this.selectedTraces);
                    if (res.found) {
                        this.selectedTraces.splice(res.index, 1);
                    }                     
                }
            }
            this.artifact.isSelected = !this.artifact.isSelected;
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

    public inArray(array) {
        let found = false,
            index = -1;
        if (array) {
            for (let i = 0; i < array.length; i++) {
                if (array[i].itemId === this.artifact.itemId) {
                    found = true;
                    index = i;
                    break;
                }
            }
        }

        return <IResult>{ "found": found, "index": index };
    }

    public limitChars(str) {
        if (str) {
            let text = Helper.stripHTMLTags(str);
            if (text) {
                if (text.length > 100) {
                    return text.substring(0, 100) + "...";
                }
                return text;
            }
            return "";
        }
        return "";
    }

    private getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
        return this.relationshipDetailsService.getRelationshipDetails(artifactId)
            .then((relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo) => {
                if (relationshipExtendedInfo.pathToProject[0].parentId === 0) {
                    this.artifact.projectId = relationshipExtendedInfo.pathToProject[0].itemId;
                    this.artifact.projectName = relationshipExtendedInfo.pathToProject[0].itemName;
                    if (relationshipExtendedInfo.pathToProject[0].itemId === this.artifact.projectId) {
                        relationshipExtendedInfo.pathToProject.shift();
                    } else {
                        this.fromOtherProject = true;
                    }
                }
                return relationshipExtendedInfo;
            });
    }

    public navigateToArtifact(relationship: Relationships.IRelationship) {
        if (relationship.hasAccess) {
            this.artifactManager.get(relationship.artifactId).then((artifact: IStatefulArtifact) => {
                this.artifactManager.selection.setExplorerArtifact(artifact);
            });
        }
    }
}
