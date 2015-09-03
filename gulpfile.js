'use strict';

//var gulp = require('gulp');
var wrench = require('wrench');



/**
 *  This will load all js or coffee files in the gulp directory
 *  in order to load all gulp tasks
 */
var tasksPath = './gulp/tasks/';
wrench.readdirSyncRecursive(tasksPath).filter(function(file) {
    return (/\.(js|coffee)$/i).test(file);
}).map(function(file) {
    require(tasksPath + file);
});
