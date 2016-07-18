import { ILocalizationService, Helper } from "../../../../core";
import { Relationships, IProjectManager } from "../../../../main";
import {IArtifactRelationships} from "../artifact-relationships.svc";

export class BPArtifactRelationshipItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-relationship-item.html");
    public controller: Function = BPArtifactRelationshipItemController;
    public bindings: any = {
        artifact: "=",
        selectedTraces: "="
    };
}

export class BPArtifactRelationshipItemController {
    public static $inject: [string] = [
        "$log",
        "localization",
        "artifactRelationships",      
        "projectManager"
    ];

    public expanded: boolean = false;
    public relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo;    
    public artifact: Relationships.IRelationship;
    public selectedTraces: Relationships.IRelationship[];
    public fromOtherProject: boolean = false;
    public _isSelected: boolean = false;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private artifactRelationships: IArtifactRelationships,     
        private projectManager: IProjectManager) {       
       
    }

    public get isSelected() {
        return this._isSelected;
    }

    public expand() {
        if (!this.expanded) {
            this.getRelationshipDetails(this.artifact.artifactId)
                .then((relationshipExtendedInfo: any) => {
                    this.relationshipExtendedInfo = relationshipExtendedInfo;
                });
        }
        this.expanded = !this.expanded;
    }

    public select(art) {
        if (!this._isSelected) {
            if (this.selectedTraces) {
                this.selectedTraces.push(art);
            }
        } else {
            if (this.selectedTraces) {
                this.selectedTraces.pop();
            }
        }
        this._isSelected = !this._isSelected;
    }

    public limitChars(str) {
        if (str) {
            var text = Helper.decodeHtmlText(str);
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
        return this.artifactRelationships.getRelationshipDetails(artifactId)
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

    public navigateToArtifact(artifact: Relationships.IRelationship) {
        var art = this.projectManager.getArtifact(artifact.artifactId);
        if (art && artifact.hasAccess) {
            this.projectManager.setCurrentArtifact(art);
        }
    }
}
