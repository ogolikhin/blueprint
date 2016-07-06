import { ILocalizationService } from "../../../../core";
import { Relationships, IProjectManager } from "../../../../main";
import {IArtifactRelationships} from "../artifact-relationships.svc";

export class BPArtifactRelationshipItem implements ng.IComponentOptions {
    public template: string = require("./bp-artifact-relationship-item.html");
    public controller: Function = BPArtifactRelationshipItemController;
    public bindings: any = {
        artifact: "="
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
    public relationshipExtendedInfo: Relationships.RelationshipExtendedInfo;    
    public artifact: Relationships.Relationship;

    constructor(
        private $log: ng.ILogService,
        private localization: ILocalizationService,
        private artifactRelationships: IArtifactRelationships,     
        private projectManager: IProjectManager) {
       
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

    public limitChars(str) {
        if (str) {
            var text = str.replace(/(<\/?[^>]+>)([&#x200b;]+)?/gi, '');
            if (text && text.length > 100) {
                return text.substring(0, 100) + "...";
            } else {
                return '';
            }
        }
        return '';

    }

    private getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.RelationshipExtendedInfo> {
        return this.artifactRelationships.getRelationshipDetails(artifactId)
            .then((relationshipExtendedInfo: Relationships.RelationshipExtendedInfo) => {
                return relationshipExtendedInfo;
            });
    }

    public navigateToArtifact(artifact: Relationships.Relationship) {
        var art = this.projectManager.getArtifact(artifact.artifactId);
        if (art) {
            this.projectManager.setCurrentArtifact(art);
        }
    }
}
