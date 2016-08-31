import { Models, Enums } from "../../../main/models";
import { ArtifactState} from "../state";
import { ArtifactProperties } from "../properties";
import { IStatefulArtifact, IProperty, IArtifactManagerService } from "../interfaces";


export class StatefullArtifact implements IStatefulArtifact {
    public manager: IArtifactManagerService;
    public state: ArtifactState;
    public properties: ArtifactProperties; 

    //TODO. 
    //Needs implementation of other object like 
    //attachments, traces and etc.


    constructor(manager: IArtifactManagerService, artifact: Models.IArtifact) {
        
        this.manager = manager;
        this.properties = new ArtifactProperties(this,this.collectProperties(artifact));
        this.properties.add()
        this.state = new ArtifactState(this);
    }

    public get id(): number {
        return this.properties.system("id").value;
    }
    public get name(): number {
        return this.properties.system("name").value;
    }

    private collectProperties(artifact: Models.IArtifact) {
        let properties: IProperty[] = []; 
        for(let key in artifact) {
            if (key === "customPropertyValues") {
                properties.concat(
                    artifact.customPropertyValues.map((it:Models.IPropertyValue) => {
                        return angular.extend({}, it, {
                            propertyLookup: Enums.PropertyLookupEnum.Custom
                        } );
                    })
                )
            } else if (key === "specificPropertyValues") {
                properties.concat(
                    artifact.specificPropertyValues.map((it:Models.IPropertyValue) => {
                        return angular.extend({}, it, {
                            propertyLookup: Enums.PropertyLookupEnum.Special
                        });
                    })
                )
            } else {
                properties.push({
                    propertyLookup: Enums.PropertyLookupEnum.System,
                    propertyTypeId: -1,
                    propertyName: key,
                    propertyTypePredefined: Enums.PropertyTypePredefined[key],
                    value: artifact[key]

                } );
            }

            return properties;
        }
        
    }

 
}
