 /**
 * This class file  utility class to wait until all element presence
 * Assumption: This apply on arrayList finder
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