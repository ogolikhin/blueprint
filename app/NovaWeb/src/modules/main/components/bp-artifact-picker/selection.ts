export class Selection<T> {
    private selectedItems: T[] = [];

    constructor(
        private selectionMode: "single" | "multiple" | "checkbox",
        private onSelectionChanged?: (selectedItems: T[]) => any) {
    }

    public add(item: T) {
        if (this.selectionMode === "single") {
            this.selectedItems = [item];
        } else {
            this.selectedItems.push(item);
        }
        this.raiseSelectionChanged();
    }

    public remove(item: T) {
        const index = this.selectedItems.indexOf(item);
        if (index >= 0) {
            this.selectedItems.splice(index, 1);
        }
        this.raiseSelectionChanged();
    }

    public contains(item: T): boolean {
        return this.selectedItems.indexOf(item) >= 0;
    }

    private raiseSelectionChanged() {
        if (this.onSelectionChanged) {
            this.onSelectionChanged(this.selectedItems);
        }
    }
}
