export class AppComponent implements ng.IComponentOptions {
    // Inline template
    //public template: string = "<ui-view></ui-view>";

    // Template will be injected on the build time
    //public template: string = require("./app.view.html");

    // 'External' template should ends with *.view.html to be copied to the dest folder
    public templateUrl: string = "/modules/application/app.view.html"
}
