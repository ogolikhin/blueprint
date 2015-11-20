'use strict';

var config = require('./config/environment');

module.exports = function (app) {

  // API



  app.route(['/demo/client/api/resources', '/nova-ng/api/resources', '/api/resources' ])
    .get(function (req, res) {

  var path = req.param('path');

      console.log("===============================================================");
      console.log(path);
      console.log("===============================================================");      
      
      if (path === 'Features') {
        res.json(
          [        
            {
              "text": "1-IceBox",
              "type": "folder",
              "icon": "folder.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },          
            {
              "text": "2-BackLog",
              "type": "folder",
              "icon": "folder.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "3-Archive",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "Feature Details",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "User Stories",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            }, 
            {
              "text": "Internal ER System",
              "type": "folder",
              "icon": "folder.png",              
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            }, 
            {
              "text": "Processes",
              "type": "folder",
              "icon": "folder.png",              
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },             
            {
              "text": "HTML5 Product",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },             
          ]
        );
      }
      else if (path === 'Root/Features') {
        res.json(
          [        
            {
              "text": "1-IceBox",
              "type": "folder",
              "icon": "folder.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },          
            {
              "text": "2-BackLog",
              "type": "folder",
              "icon": "folder.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "3-Archive",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "Feature Details",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "User Stories",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            }, 
            {
              "text": "Internal ER System",
              "type": "folder",
              "icon": "folder.png",              
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            }, 
            {
              "text": "Processes",
              "type": "folder",
              "icon": "folder.png",              
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },             
            {
              "text": "HTML5 Product",
              "type": "folder",
              "icon": "folder.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },             
          ]
        );
        
      }


      else if (path === 'Features/1-IceBox') {
        res.json(
          [        
            {
              "text": "Impact Analysis",
              "type": "folder",
              "icon": "Requirement.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },          
            {
              "text": "Full Text Search Enhancement",
              "type": "folder",
              "icon": "Requirement.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
            {
              "text": "Shortcuts",
              "type": "folder",
              "icon": "Requirement.png",              
              "children": true,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Features",
            },  
          ]
        );
        
      }



      else { //if (path === undefined) {
        res.json(
          [
            {
              "text": "Discussions",
              "type": "folder",
              "children": false,
              "icon": "Comment.png",
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },
            {
              "text": "Activity Center",
              "type": "folder",
              "icon": "ActivityCenter16.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },
            {
              "text": "Unpublished Changes",
              "type": "folder",
              "icon": "locked.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },
            {
              "text": "Features",
              "type": "folder",
              "icon": "ProjectExplorerRoot.png",
              "children": true,
              "state": {
                "opened": true
              },
              "url": "/api/resources?path=Root",
            },
            {
              "text": "Collections",
              "type": "folder",
              "icon": "CollectionsFolder16.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },          
            {
              "text": "Baselines and Reviews",
              "type": "folder",
              "icon": "RootBaselineAndReview16.png",
              "children": false,
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },             
            {
              "text": "Blueprint - Impact Analysis",
              "type": "folder",
              "children": false,
              "icon": "blueprint.png",
              "link": "http://localhost:58796/web/#/impactAnalysis/4",
              "resourceType": "web.app",              
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },
            {
              "text": "Blueprint - Rapid Review",
              "type": "folder",
              "children": false,
              "icon": "blueprint.png",
              "link": "http://localhost:58796/Web/#/RapidReview/17",
              "resourceType": "web.app",              
              "state": {
                "opened": false
              },
              "url": "/api/resources?path=Root",
            },  
            {
              "text": "Blueprint - Graph Editor",
              "type": "folder",
              "children": false,
              "icon": "blueprint.png",
              "link": "/mxgraph/javascript/examples/grapheditor/www/index.html",
              "resourceType": "web.app",              
              "state": {
                "opened": false
              },              
              "url": "/api/resources?path=Root"
            }          
              

          ]
        );
      }

      res.end();
    }
  );


  app.route('/')
    .get(function (req, res) {
      res.sendFile(
        app.get('appPath') + '/index.html',
        { root: config.root }
      );
    });

};
