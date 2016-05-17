/**
* This class file  utility class to wait until all element presence
* Assumption: This apply on arrayList finder
* Author : Mohammed Ali Akbar
* Created date: May10,2016
* last modified by:
* Last modified on:
*/
var ArrayListPresenceOfAll = (function () {
    function ArrayListPresenceOfAll() {
    }
    ArrayListPresenceOfAll.presenceOfAll = function (elementArrayFinder) {
        return function () {
            return elementArrayFinder.count(function (count) {
                return count > 0;
            });
        };
    };
    return ArrayListPresenceOfAll;
})();
module.exports = ArrayListPresenceOfAll;
//# sourceMappingURL=ArrayListPresenceOfAll.js.map