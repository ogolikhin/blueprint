/**
 * This class file will create an artifact at blue print. it will use nodejs 'sync-request' 
 * package to make the request to blueprint
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * 
 */

var OR = require('../Json/OR.json');
var request = require('sync-request');
var postCreateArtifactUrl = OR.mockData.postCreateArtifactUrl;
var postPublishArtifactUrl = OR.mockData.postPublishArtifactUrl;

class CreateArtifact {
    public static blueprintAuthorizationToken: any;
    public static projectID : any;
    public static createArt() {
        
        var artifactName = OR.mockData.artifactName;
        var artifactParentId = OR.mockData.artifactParentId;
        var artifactTypeId = OR.mockData.artifactTypeId;
        var authorizationTokenbase64 = OR.mockData.authorizationTokenbase64;
        var artifactProperties = OR.mockData.artifactProperties;
        var getAuthenticationApiUrl = OR.mockData.getAuthenticationApiUrl;
        
        var artifactId;

         // preparing get request data 
       var options = {
            
            'headers': {
                'Authorization': 'Basic ' + authorizationTokenbase64
            }
        };
        // get request to get the blueprint authentication token
        var res = request('GET', getAuthenticationApiUrl, options);
        var temToken = res.body.toString();
        var objFromgetRequest = JSON.parse(temToken); // parsing response to Json object
        console.log("This is the token receive from get request " + objFromgetRequest);

        this.blueprintAuthorizationToken = 'Blueprinttoken ' + objFromgetRequest;
        console.log("This is blueprint token" + this.blueprintAuthorizationToken);

        // preparing post request data 
        var optionsForPostRequest = {           
            method: 'POST',
            headers: {
                'Authorization': this.blueprintAuthorizationToken,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Name: artifactName,
                ParentId: artifactParentId,
                ArtifactTypeId: artifactTypeId,
                Properties: []
            })
        };
        // post request to create an artifact
        var resFromPost = request('POST', postCreateArtifactUrl, optionsForPostRequest);
        var temArtifactId = resFromPost.body.toString();
      
        var objArtifact = JSON.parse(temArtifactId);// parsing response to Json object
        console.log("&&&&&&&&&&&&&&&&&&&&&" + temArtifactId);
        artifactId = objArtifact.Artifact.Id;
        this.projectID = objArtifact.Artifact.ProjectId;
        console.log("Artifact ID IS " + artifactId);
        console.log("Artifact ID IS " + this.projectID);
        return artifactId;
    }
    public static ArtifactPublish() {
        // preparing post request data 
       // var artifactID = this.createArt();
        var optionsForPostRequest = {
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
        console.log("YYYYYYYYYYYYYYYYYYY");
        // post request to publish an artifact
        var resFromPost = request('POST', postPublishArtifactUrl, optionsForPostRequest);
        console.log("YYYYYYYYYYYYYYYYYYY");
        var temTesFromPost = resFromPost.body.toString();
        console.log("YYYYYYYYYYYYYYYYYYY" + temTesFromPost);
    }

}
export = CreateArtifact;

