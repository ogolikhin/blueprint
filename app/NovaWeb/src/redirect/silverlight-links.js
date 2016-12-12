var searchString = window.location.search.toLowerCase();
if (!window.location.hash && searchString.indexOf("?artifactid") === 0) {
    var params = searchString.slice(1).split("$");
    var id;

    for (var i = 0; i < params.length; i++) {
        var keyValue = params[i].split("=", 2);

        if (keyValue[0] === "artifactid" && keyValue[1]) {
            id = parseInt(keyValue[1]);
        }
    }

    var newHash = "/main" + (id ? "/" + id: "");
    if (window.history.replaceState) {
        window.history.replaceState({}, "", window.location.toString().replace(window.location.search, ""));
    }
    window.location.hash = newHash;
}
