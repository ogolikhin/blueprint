 /**
 * This class file  utility class to wait until all element presence
 * Assumption: This apply on arrayList finder
 * Author : Mohammed Ali Akbar
 * Created date: May10,2016
 * last modified by:
 * Last modified on:
 */ 
class ArrayListPresenceOfAll {
    public static presenceOfAll(elementArrayFinder) {
        return () => {
            return elementArrayFinder.count((count) => {
                return count > 0;
            });
        };
    }
}
export = ArrayListPresenceOfAll;