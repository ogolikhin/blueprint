var SP = require("sharepoint"),
    consts = require("constants"),
    urlparse = require('url').parse,
    util = require("util"),
    httpreq = require("httpreq"),
    httpntml = require("httpntlm"),
    httprequest,
	onPrem,
	log,
	onreadyCallback;

function addFolderToSP(options, callback) {
	
    if(!callback){
		callback = function(){}
	}
    if(!options.isOnPrem){
        options.isOnPrem = isOnPrem;
    }
    if(typeof options.log !== "boolean"){
		log = true;
	}else{
		log = options.log;
	}
	
	onreadyCallback = callback;
    onPrem = options.isOnPrem(options.siteUrl);
    
    if(onPrem){
        httprequest = exhttpntlm;
    } else {
        httprequest = httpreq;
    }  

	if(onPrem){
		spFolderCore(options, {
			username: options.username,
			password: options.password,
			workstation: options.workstation,
			domain: options.domain
		});
	} else{
		var service = new SP.RestService(options.siteUrl);
		service.signin(options.username, options.password, function (err, auth) {
			if (err) {
				logger(err);
				onreadyCallback(err);
				return;
			}
			
			spFolderCore(options, auth);
		});
	}
}

function spFolderCore(options, auth){
    var url = urlparse(options.siteUrl);
    getRequestDigest(url, options, auth, function(digestValue){
        if(options.appWebUrl){
            getAppWebUrl(options, auth, digestValue, function(appWebUrl){
                getRequestDigest(appWebUrl, options, auth, function(newDigest){
                    addFolder(appWebUrl, options, newDigest, auth, 0);
                });                            
            });                        
        }else{
            addFolder(url, options, digestValue, auth, 0);
        }        
    });    
}

function setAuth(options, auth, requestOptions){
    if(onPrem){
        requestOptions.username = auth.username;
        requestOptions.password = auth.password;
        requestOptions.workstation = auth.workstation;
        requestOptions.domain = auth.domain;
    }else{
        requestOptions.cookies = [
            "FedAuth=" + auth.FedAuth,
            "rtFa=" + auth.rtFa
        ];
        requestOptions.secureOptions = consts.SSL_OP_NO_TLSv1_2
    }
}

function getRequestDigest(url, options, auth, onDigest){    
    var opts = {
        headers: {
            "Accept": "application/json;odata=verbose"
        }
    };
	
    setAuth(options, auth, opts);

    httprequest.post(url.href + "/_api/contextinfo", opts, function(err, res){
        if(err){
            logger(err);
			onreadyCallback(err);
			return;
        }else{
            var data = JSON.parse(res.body);
			
			if(data.error){
				logger(data.error);
				onreadyCallback(new Error(JSON.stringify(data.error)));
				return;
			}
			
            onDigest(data.d.GetContextWebInformation.FormDigestValue);
        }        
    });
}

function getAppWebUrl(options, auth, digestValue, onGetAppWeb){
    var dateNow = new Date();
    var dateString = util.format("[%s:%s:%s]", ("0" + dateNow.getHours()).slice(-2), ("0" + dateNow.getMinutes()).slice(-2), ("0" + dateNow.getSeconds()).slice(-2));
    logger(util.format("%s Opening '%s' ...", dateString, options.appWebUrl));
    var openWebUrl = util.format("/_api/site/openWeb(@strUrl)?@strUrl='%s'", options.appWebUrl);
    
    var opts = {
        headers: {
            "Accept": "application/json;odata=verbose",
            "X-RequestDigest" : digestValue
        }
    };
    
    setAuth(options, auth, opts);
    
    var url = urlparse(options.siteUrl);
    httprequest.post(url.href + openWebUrl, opts, function(err, res){
        if(err){
            logger(err);
			onreadyCallback(err);
			return;
        }else{
            var data = JSON.parse(res.body);
			
			if(data.error){
				logger(data.error);
				onreadyCallback(new Error(JSON.stringify(data.error)));
				return;
			}
			
            logger("Web full url: " + data.d.Url);
            onGetAppWeb(urlparse(data.d.Url));
        }        
    });
}


function addFolder(webUrl, options, digestValue, auth, index){
	
	var folders = options.newFolder.split('\\');	
	if (folders.length == 1)
		folders = options.newFolder.split('/');	
	
	if (folders.length > index) {		
		var i =0;
		var parentFolder=options.siteFolder;
		var childFolder='';
		for (;i<index; i++) {
			parentFolder = parentFolder + '/' + folders[i];
		}
		for (; i<index+1; i++) {
			if (childFolder === '')
				childFolder = folders[i];
			else
				childFolder = childFolder + '/' + folders[i] ;
		}
		
		var uploadRestUrl = util.format("/_api/web/GetFolderByServerRelativeUrl('%s')/folders", encodeURIComponent(parentFolder));
		var opts = {
			headers: {
				"Accept": "application/json;odata=verbose",
				"X-RequestDigest" : digestValue,			
				"Content-Type": "application/json;odata=verbose"
			},
			body: "{ '__metadata':{ 'type': 'SP.Folder' }, 'ServerRelativeUrl':'"+encodeURIComponent(childFolder) +"' }",
		};
		
		setAuth(options, auth, opts);
		
		for ( f in options.folders ) {
			if (options.folders[f] === childFolder) {
				return;
			}
		}		
		
		httprequest.post(webUrl.href + uploadRestUrl, opts, function(err, res){
			if(err){
				onreadyCallback(err);
				return;
			}else{
				var data = JSON.parse(res.body);
				
				if(data.error){
					logger(data.error);
					onreadyCallback(new Error(JSON.stringify(data.error)));
					return;
				}
				
				var dateNow = new Date();
				var dateString = util.format("[%s:%s:%s]", ("0" + dateNow.getHours()).slice(-2), ("0" + dateNow.getMinutes()).slice(-2), ("0" + dateNow.getSeconds()).slice(-2));
				logger(util.format("%s folder '%s' successfully created under '%s' folder", dateString, childFolder, parentFolder));
				options.folders.push(childFolder);
				if (folders.length == index + 1)
					onreadyCallback(null, data);
				else {
					addFolder(webUrl, options, digestValue, auth, ++index);
				}
			}        
		});
	}
}

function isOnPrem(url){
    return (urlparse(url)).host.indexOf(".sharepoint.com") === -1;
}

function logger(text){
	if(log){
		console.log(text);
	}
}

function exhttpntlm(method, url, opts, callback){
    opts.url = url;
    return httpntml[method](opts, callback);
}

["get", "post"].forEach(function(method){
    exhttpntlm[method] = exhttpntlm.bind(this, method);
});

module.exports = addFolderToSP;
