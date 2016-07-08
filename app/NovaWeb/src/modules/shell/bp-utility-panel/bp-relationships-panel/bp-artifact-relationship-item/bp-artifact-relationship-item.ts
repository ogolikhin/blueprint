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
    public relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo;    
    public artifact: Relationships.IRelationship;

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
            if (text) {
                if (text.length > 100) {
                    return text.substring(0, 100) + "...";
                } else {
                    return text;
                }
            } else {
                return '';
            }
        }
        return '';

    }

    private getRelationshipDetails(artifactId: number): ng.IPromise<Relationships.IRelationshipExtendedInfo> {
        return this.artifactRelationships.getRelationshipDetails(artifactId)
            .then((relationshipExtendedInfo: Relationships.IRelationshipExtendedInfo) => {
                relationshipExtendedInfo.pathToProject.shift();
                return relationshipExtendedInfo;
            });
    }

    public navigateToArtifact(artifact: Relationships.IRelationship) {
        var art = this.projectManager.getArtifact(artifact.artifactId);
        if (art) {
            this.projectManager.setCurrentArtifact(art);
        }
    }
}
