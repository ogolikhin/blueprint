/** 
 * This class file will create an artifact at blue print. it will use nodejs 'sync-request' 
 * package to make the request to blueprint
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * 
 */

let OR = require('../Locator/StorytellerLocator.json');
let request = require('sync-request');
let postCreateArtifactUrl = OR.mockData.postCreateArtifactUrl;
let postPublishArtifactUrl = OR.mockData.postPublishArtifactUrl;

class CreateArtifact {
    public  blueprintAuthorizationToken: any;
    public  projectID: any;
    public artifactName: string;
    public artifactParentId: string;
    public artifactTypeId: string;
    public authorizationTokenbase64: string;
    public artifactProperties: string;
    public getAuthenticationApiUrl: string;

    
    public  createArt() {

        
        this.artifactName = OR.mockData.artifactName;
        this. artifactParentId = OR.mockData.artifactParentId;
        this. artifactTypeId = OR.mockData.artifactTypeId;
        this. authorizationTokenbase64 = OR.mockData.authorizationTokenbase64;
        this. artifactProperties = OR.mockData.artifactProperties;
        this. getAuthenticationApiUrl = OR.mockData.getAuthenticationApiUrl;

        let artifactId;

        // preparing get request data 
        let options = {

            'headers': {
                'Authorization': 'Basic ' + this.authorizationTokenbase64
            }
        };
        // get request to get the blueprint authentication token
        let res = request('GET', this.getAuthenticationApiUrl, options);
        let temToken = res.body.toString();
        let objFromgetRequest = JSON.parse(temToken); // parsing response to Json object
        console.log("This is the token receive from get request " + objFromgetRequest);

        this.blueprintAuthorizationToken = 'Blueprinttoken ' + objFromgetRequest;
        console.log("This is blueprint token" + this.blueprintAuthorizationToken);

        // preparing post request data 
        let optionsForPostRequest = {
            method: 'POST',
            headers: {
                'Authorization': this.blueprintAuthorizationToken,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Name: this.artifactName,
                ParentId: this.artifactParentId,
                ArtifactTypeId: this.artifactTypeId,
                Properties: []
            })
        };
        // post request to create an artifact
        let resFromPost = request('POST', postCreateArtifactUrl, optionsForPostRequest);
        let temArtifactId = resFromPost.body.toString();

        let objArtifact = JSON.parse(temArtifactId);// parsing response to Json object
        artifactId = objArtifact.Artifact.Id;
        this.projectID = objArtifact.Artifact.ProjectId;
        console.log("Artifact ID IS " + artifactId);
        console.log("Project ID IS " + this.projectID);
        return artifactId;
    }
    public  ArtifactPublish() {

        let optionsForPostRequest = {
            method: 'POST',
            headers: {
                'Authorization': this.blueprintAuthorizationToken,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify([{
                Id: this.createArt(),
                ProjectId: this.projectID
            }])
        };

        // post request to publish an artifact
        var resFromPost = request('POST', postPublishArtifactUrl, optionsForPostRequest);
        var temTesFromPost = resFromPost.body.toString();

    }

}
export = CreateArtifact;


