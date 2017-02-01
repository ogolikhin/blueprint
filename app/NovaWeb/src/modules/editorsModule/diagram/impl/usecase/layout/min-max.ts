/**
 * Describes structure wich keeps min and max values
 */
export class MinMax {
    public min = 0;
    public max = 0;

    /**
     * Updates min or/and max value.
     */
    public update(value: number) {
        if (this.min > value) {
            this.min = value;
        }

        if (this.max < value) {
            this.max = value;
        }
    }
}
