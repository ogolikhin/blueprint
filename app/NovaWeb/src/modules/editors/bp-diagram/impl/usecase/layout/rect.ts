/**
 * Declares a structure that is used to describe the Size of an object.
 */
export interface ISize {
    height: number;
    width: number;
}

/**
 * Represents an x- and y-coordinate pair in two-dimensional space.
 */
export interface IPosition {
    x: number;
    y: number;
}

/**
 * Describes the width, height, and location of a rectangle.
 */
export interface IRect extends ISize, IPosition {

    /**
     * Expands or shrinks the rectangle by using the specified width and height amounts, in all directions.
     */
    inflate(x: number, y?: number);

    /**
     * Expands the current rectangle exactly enough to contain the specified rectangle.
     */
    union(rect: IRect);

    /**
     * Gets the x-axis value of the right side of the rectangle.
     */
    getRight(): number;

    /**
     * Gets the y-axis value of the bottom of the rectangle.
     */
    getBottom(): number;
}

/**
 * Describes the width, height, and location of a rectangle.
 */
export class Rect implements IRect {
    public x = 0;
    public y = 0;
    public height = 0;
    public width = 0;

    constructor(x: number, y: number, height: number, width: number) {
        this.x = x;
        this.y = y;
        this.height = height;
        this.width = width;
    }

    /**
     * Expands or shrinks the rectangle by using the specified width and height amounts, in all directions.
     */
    public inflate(value: number) {
        this.inflateInternal(value, value);
    }

    private inflateInternal(x: number, y: number) {
        this.x -= x;
        this.y -= y;

        const newWidth = this.width + 2 * x;
        if (newWidth > 0) {
            this.width = newWidth;
        }

        const newHeight = this.height + 2 * y;
        if (newHeight > 0) {
            this.height = newHeight;
        }
    }

    /**
     * Initializes a new instance of the Rect structure that has the specified top-left corner location and the specified width and height.
     */
    public static createRect(position: IPosition, size: ISize): IRect {
        return new Rect(position.x, position.y, size.height, size.width);
    }

    /**
     * Expands the current rectangle exactly enough to contain the specified rectangle.
     */
    public union(rect: IRect) {
        if (rect == null) {
            return;
        }
        const newX = Math.min(this.x, rect.x);
        const newY = Math.min(this.y, rect.y);
        this.width = Math.max(Math.max(this.getRight(), rect.getRight()) - newX, 0);
        this.height = Math.max(Math.max(this.getBottom(), rect.getBottom()) - newY, 0);
        this.x = newX;
        this.y = newY;
    }

    /**
     * Gets the x-axis value of the right side of the rectangle.
     */
    public getRight() {
        return this.x + this.width;
    }

    /**
     * Gets the y-axis value of the bottom of the rectangle.
     */
    public getBottom() {
        return this.y + this.height;
    }

    /**
     * Expands the current rectangle exactly enough to contain the all rectangles in the array.
     */
    public static unionAll(rects: Array<IRect>): IRect {
        const rect: IRect = new Rect(0, 0, 0, 0);
        if (rects != null && rects.length > 0) {
            rects.forEach(r => {
                rect.union(r);
            });
        }
        return rect;
    }
}
