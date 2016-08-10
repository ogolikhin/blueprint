(function () {
    $(function () {
        $("#input_apiKey").attr("placeholder", "Session-Token");
        $("#input_apiKey").off("change");
        $("#input_apiKey").on("change", function () {
            var token = this.value;
            if (token && token.trim() !== "") {
                var apiKeyAuth = new SwaggerClient.ApiKeyAuthorization("Session-Token", token, "header");
                window.swaggerUi.api.clientAuthorizations.add("apiKey", apiKeyAuth);
                console.log("added token " + token);
            } else {
                window.swaggerUi.api.clientAuthorizations.remove("apiKey");
                console.log("removed token");
            }
        });
    });
})();
