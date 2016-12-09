if (!window.location.hash && window.location.search.toLowerCase().startsWith("?artifactid")) {
        var params = window.location.search.toLowerCase().slice(1).split("$");
        var id;

        for (var i = 0; i < params.length; i++) {
            var keyValue = params[i].split("=", 2);

            if (keyValue.length === 2 && keyValue[0] && keyValue[1]) {
                switch (keyValue[0]) {
                    case "artifactid":
                        id = parseInt(keyValue[1]);
                        break;
                }
            }
        }

    var newHash = "/main" + (id ? "/" + id: "");
    if (window.history.replaceState) {
        window.history.replaceState({}, "", window.location.toString().replace(window.location.search, ""));
    }
    window.location.hash = newHash;

}
