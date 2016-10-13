// this file is only being used by karma
require("phantomjs-polyfill");

function requireAll(r: any): any {
    r.keys().forEach(r);
}

requireAll((<any>require).context("./", true, /spec.ts$/));

// load all code except specs - required for code coverage (https://github.com/deepsweet/istanbul-instrumenter-loader#testindexjs)
requireAll((<any>require).context("./modules", true, /^((?!spec\.)(?!storyteller).)*ts$/));