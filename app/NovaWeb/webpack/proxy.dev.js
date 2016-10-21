var backend = process.env.npm_config_backend || process.env.npm_package_config_backend || "http://localhost:9801";
console.log({backend: backend});

var proxy ={
	'/svc/*':{
		  target: backend,
		  changeOrigin: true
	  },
	  '/shared/*':{
		  target:backend,
		  changeOrigin: true
	  }
 };


module.exports = proxy;


