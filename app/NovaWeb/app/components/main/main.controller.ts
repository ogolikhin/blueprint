module nova {
    class MainController {
        public user = {
            email: "user@user.org",
            displayName: "User",
            login: "user"
        };
        constructor() {
            
        }
    }
    angular.module("nova")
        .controller("MainController", MainController);
}