var spsave = require('./spsave/lib/spsave'),
	spfolder = require('./spsave/lib/spfolder'),
    gutil = require('gulp-util'),
    PluginError = gutil.PluginError,
    path = require("path"),
	through = require("through2");

var PLUGIN_NAME = 'gulp-spsave';

function gulpspsave(options) {
    if (!options) {
        throw new PluginError(PLUGIN_NAME, 'Missing options');
    }
    
    return through.obj(function (file, enc, cb) {
        if (file.isNull()) {
            return cb();
        }

        if (file.isStream()) {
            this.emit('error', new PluginError(PLUGIN_NAME, 'Streaming not supported'));
            return cb();
        }

        if (file.isBuffer()) {
			options.newFolder = path.dirname(path.relative('.', file.path));			
			options.folder = options.siteFolder + '/' + options.newFolder;			
			
			options.fileName = path.basename(file.path);			
            options.fileContent = file.contents;
			options.folders = [];
			
			spfolder(options, function(err, data){
				if(err){
					console.log(err);
					this.emit('error', new PluginError('gulp-spfolder', err.message));
				}				
				spsave(options, function(err, data){
					if(err){
						console.log(err);
						this.emit('error', new PluginError(PLUGIN_NAME, err.message));
					}
					
					return cb();
				}); 

			}); 			
			
           
        }
    });
}

module.exports = gulpspsave;