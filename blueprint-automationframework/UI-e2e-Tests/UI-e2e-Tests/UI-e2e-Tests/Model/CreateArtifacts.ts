/** 
 * This class file will create an artifact at blue print. it will use nodejs 'sync-request' 
 * package to make the request to blueprint
 */

//var mockData = require('../CustomConfig/MockData.json');
let mockData = require("../CustomConfig/MockData.json");
let request = require('sync-request');
let postCreateArtifactUrl = mockData.serverArtifactsInfo.postCreateArtifactUrl;
let postPublishArtifactUrl = mockData.serverArtifactsInfo.postPublishArtifactUrl;

class Artifact {
    public  blueprintAuthorizationToken: any;
    public  projectID: any;
    public artifactName: string;
    public artifactParentId: string;
    public artifactTypeId: string;
    public authorizationTokenbase64: string;
    public artifactProperties: string;
    public getAuthenticationApiUrl: string;

    
    public createArtifact() {

        
        this.artifactName = mockData.serverArtifactsInfo.artifactName;
        this.artifactParentId = mockData.serverArtifactsInfo.artifactParentId;
        this.artifactTypeId = mockData.serverArtifactsInfo.artifactTypeId;
        this.authorizationTokenbase64 = mockData.serverArtifactsInfo.authorizationTokenbase64;
        this.artifactProperties = mockData.serverArtifactsInfo.artifactProperties;
        this.getAuthenticationApiUrl = mockData.serverArtifactsInfo.getAuthenticationApiUrl;

        let artifactId;

        // preparing get request data 
        let options = {

            'headers': {
                'Authorization': 'Basic ' + this.authorizationTokenbase64
            }
        };
        // get request to get the blueprint authentication token
        let res = request('GET', this.getAuthenticationApiUrl, options);
        let tempToken = res.body.toString();
        let objFromGetRequest = JSON.parse(tempToken); // parsing response to Json object
        console.log("This is the token receive from get request " + objFromGetRequest);

        this.blueprintAuthorizationToken = 'Blueprinttoken ' + objFromGetRequest;
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
        console.log("Response: " + temArtifactId);
        artifactId = objArtifact.Artifact.Id;
        this.projectID = objArtifact.Artifact.ProjectId;
        console.log("Artifact ID IS " + artifactId);
        console.log("Project ID IS " + this.projectID);
        return artifactId;
    }
    public publishArtifact() {

        let optionsForPostRequest = {
            body: JSON.stringify([{
                Id: this.createArtifact(),
                ProjectId: this.projectID
            }]),
            method: 'POST',
            headers: {
                'Authorization': this.blueprintAuthorizationToken,
                'Content-Type': 'application/json'
            }
            
        };

        // post request to publish an artifact
        var resFromPost = request('POST', postPublishArtifactUrl, optionsForPostRequest);
        var tempResFromPost = resFromPost.body.toString();
        console.log("Response: " + tempResFromPost);

    }

}
export = Artifact;


