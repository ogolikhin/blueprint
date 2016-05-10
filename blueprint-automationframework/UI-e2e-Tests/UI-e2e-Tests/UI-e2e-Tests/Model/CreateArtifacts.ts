/**
 * This class file will create an artifact at blue print. it will use nodejs 'sync-request' 
 * package to make the request to blueprint
 * Assumption: Project and user need to be predefined.
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * 
 */

var OR = require('../Json/OR.json');
class CreateArtifact {
    public static createArt() {
        var request = require('sync-request');
        var artifactName = OR.mockData.artifactName;
        var artifactParentId = OR.mockData.artifactParentId;
        var artifactTypeId = OR.mockData.artifactTypeId;
        var authorizationTokenbase64 = OR.mockData.authorizationTokenbase64;
        var artifactProperties = OR.mockData.artifactProperties;
        var getAuthenticationApiUrl = OR.mockData.getAuthenticationApiUrl;
        var postCreateArtifactUrl = OR.mockData.postCreateArtifactUrl;
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
        console.log("This is token receive from get request " + objFromgetRequest);

        var blueprintAuthorizationToken = 'Blueprinttoken ' + objFromgetRequest;
        console.log("This blueprint token" + blueprintAuthorizationToken);

        // preparing post request data 
        var optionsForPostRequest = {           
            method: 'POST',
            headers: {
                'Authorization': blueprintAuthorizationToken,
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

        artifactId = objArtifact.Artifact.Id;
        console.log("Artifact ID IS " + artifactId);
        return artifactId;
    }

}
export = CreateArtifact;

