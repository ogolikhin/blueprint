/**
* Bootstrapping mechanism for the mxGraph thin client.
* The production version of this file contains all code required to run the mxGraph thin client, as well as global constants to identify the browser and operating system in use
*/
interface MxClient {
    isBrowserSupported(): boolean;
    imageBasePath: string;
}
declare var mxClient: MxClient;

interface MxUtils {
    /**
     * Class: mxUtils
     *
     * A singleton class that provides cross-browser helper methods.
     * This is a global functionality. To access the functions in this
     * class, use the global classname appended by the functionname.
     * You may have to load chrome://global/content/contentAreaUtils.js
     * to disable certain security restrictions in Mozilla for the <open>,
     * <save>, <saveAs> and <copy> function.
     *
     * For example, the following code displays an error message:
     *
     * (code)
     * mxUtils.error('Browser is not supported!', 200, false);
     * (end)
     *
     * Variable: errorResource
     *
     * Specifies the resource key for the title of the error window. If the
     * resource for this key does not exist then the value is used as
     * the title. Default is 'error'.
     */
    errorResource: string;

    /**
     * Variable: closeResource
     *
     * Specifies the resource key for the label of the close button. If the
     * resource for this key does not exist then the value is used as
     * the label. Default is 'close'.
     */
    closeResource: string;

    /**
     * Variable: errorImage
     *
     * Defines the image used for error dialogs.
     */
    errorImage: any;

    /**
     * Function: removeCursors
     *
     * Removes the cursors from the style of the given DOM node and its
     * descendants.
     *
     * Parameters:
     *
     * element - DOM node to remove the cursor style from.
     */
    removeCursors(element: HTMLElement): void;

    /**
     * Function: getCurrentStyle
     *
     * Returns the current style of the specified element.
     *
     * Parameters:
     *
     * element - DOM node whose current style should be returned.
     */
    getCurrentStyle(element: HTMLElement): string;

    /**
     * Function: setPrefixedStyle
     *
     * Adds the given style with the standard name and an optional vendor prefix for the current
     * browser.
     *
     * (code)
     * mxUtils.setPrefixedStyle(node.style, 'transformOrigin', '0% 0%');
     * (end)
     */
    setPrefixedStyle(style:string, name: string, value: any);

    /**
     * Function: hasScrollbars
     *
     * Returns true if the overflow CSS property of the given node is either
     * scroll or auto.
     *
     * Parameters:
     *
     * node - DOM node whose style should be checked for scrollbars.
     */
    hasScrollbars(node: HTMLElement): boolean;

    /**
     * Function: bind
     *
     * Returns a wrapper function that locks the execution scope of the given
     * function to the specified scope. Inside funct, the "this" keyword
     * becomes a reference to that scope.
     */
    bind(scope, funct);

    /**
     * Function: eval
     *
     * Evaluates the given expression using eval and returns the JavaScript
     * object that represents the expression result. Supports evaluation of
     * expressions that define functions and returns the function object for
     * these expressions.
     *
     * Parameters:
     *
     * expr - A string that represents a JavaScript expression.
     */
    eval(expr);

    /**
     * Function: findNode
     *
     * Returns the first node where attr equals value.
     * This implementation does not use XPath.
     */
    findNode(node, attr, value): HTMLElement;

    /**
     * Function: findNodeByAttribute
     *
     * Returns the first node where the given attribute matches the given value.
     *
     * Parameters:
     *
     * node - Root node where the search should start.
     * attr - Name of the attribute to be checked.
     * value - Value of the attribute to match.
     */
    findNodeByAttribute(node, attr, value): HTMLElement;

    /**
     * Function: getFunctionName
     *
     * Returns the name for the given function.
     *
     * Parameters:
     *
     * f - JavaScript object that represents a function.
     */
    getFunctionName(f);

    /**
     * Function: indexOf
     *
     * Returns the index of obj in array or -1 if the array does not contain
     * the given object.
     *
     * Parameters:
     *
     * array - Array to check for the given obj.
     * obj - Object to find in the given array.
     */
    indexOf(array, obj): number;

    /**
     * Function: remove
     *
     * Removes all occurrences of the given object in the given array or
     * object. If there are multiple occurrences of the object, be they
     * associative or as an array entry, all occurrences are removed from
     * the array or deleted from the object. By removing the object from
     * the array, all elements following the removed element are shifted
     * by one step towards the beginning of the array.
     *
     * The length of arrays is not modified inside this function.
     *
     * Parameters:
     *
     * obj - Object to find in the given array.
     * array - Array to check for the given obj.
     */
    remove(obj, array);

    /**
     * Function: isNode
     *
     * Returns true if the given value is an XML node with the node name
     * and if the optional attribute has the specified value.
     *
     * This implementation assumes that the given value is a DOM node if the
     * nodeType property is numeric, that is, if isNaN returns false for
     * value.nodeType.
     *
     * Parameters:
     *
     * value - Object that should be examined as a node.
     * nodeName - String that specifies the node name.
     * attributeName - Optional attribute name to check.
     * attributeValue - Optional attribute value to check.
     */
    isNode(value, nodeName, attributeName, attributeValue);

    /**
     * Function: isAncestorNode
     *
     * Returns true if the given ancestor is an ancestor of the
     * given DOM node in the DOM. This also returns true if the
     * child is the ancestor.
     *
     * Parameters:
     *
     * ancestor - DOM node that represents the ancestor.
     * child - DOM node that represents the child.
     */
    isAncestorNode(ancestor, child);

    /**
     * Function: getChildNodes
     *
     * Returns an array of child nodes that are of the given node type.
     *
     * Parameters:
     *
     * node - Parent DOM node to return the children from.
     * nodeType - Optional node type to return. Default is
     * <mxConstants.NODETYPE_ELEMENT>.
     */
    getChildNodes(node, nodeType);

    /**
     * Function: importNode
     *
     * Cross browser implementation for document.importNode. Uses document.importNode
     * in all browsers but IE, where the node is cloned by creating a new node and
     * copying all attributes and children into it using importNode, recursively.
     *
     * Parameters:
     *
     * doc - Document to import the node into.
     * node - Node to be imported.
     * allChildren - If all children should be imported.
     */
    importNode(doc, node, allChildren);

    /**
     * Function: createXmlDocument
     *
     * Returns a new, empty XML document.
     */
    createXmlDocument();

    /**
     * Function: parseXml
     *
     * Parses the specified XML string into a new XML document and returns the
     * new document.
     *
     * Example:
     *
     * (code)
     * var doc = mxUtils.parseXml(
     *   '<mxGraphModel><root><MyDiagram id="0"><mxCell/></MyDiagram>'+
     *   '<MyLayer id="1"><mxCell parent="0" /></MyLayer><MyObject id="2">'+
     *   '<mxCell style="strokeColor=blue;fillColor=red" parent="1" vertex="1">'+
     *   '<mxGeometry x="10" y="10" width="80" height="30" as="geometry"/>'+
     *   '</mxCell></MyObject></root></mxGraphModel>');
     * (end)
     *
     * Parameters:
     *
     * xml - String that contains the XML data.
     */
    parseXml(xmlString: string): any;

    /**
     * Function: clearSelection
     *
     * Clears the current selection in the page.
     */
    clearSelection();

    /**
     * Function: getPrettyXML
     *
     * Returns a pretty printed string that represents the XML tree for the
     * given node. This method should only be used to print XML for reading,
     * use <getXml> instead to obtain a string for processing.
     *
     * Parameters:
     *
     * node - DOM node to return the XML for.
     * tab - Optional string that specifies the indentation for one level.
     * Default is two spaces.
     * indent - Optional string that represents the current indentation.
     * Default is an empty string.
     */
    getPrettyXml(node, tab, indent);

    /**
     * Function: removeWhitespace
     *
     * Removes the sibling text nodes for the given node that only consists
     * of tabs, newlines and spaces.
     *
     * Parameters:
     *
     * node - DOM node whose siblings should be removed.
     * before - Optional boolean that specifies the direction of the traversal.
     */
    removeWhitespace(node, before);

    /**
     * Function: htmlEntities
     *
     * Replaces characters (less than, greater than, newlines and quotes) with
     * their HTML entities in the given string and returns the result.
     *
     * Parameters:
     *
     * s - String that contains the characters to be converted.
     * newline - If newlines should be replaced. Default is true.
     */
    htmlEntities(s, newline);

    /**
     * Function: isVml
     *
     * Returns true if the given node is in the VML namespace.
     *
     * Parameters:
     *
     * node - DOM node whose tag urn should be checked.
     */
    isVml(node);

    /**
     * Function: getXml
     *
     * Returns the XML content of the specified node. For Internet Explorer,
     * all \r\n\t[\t]* are removed from the XML string and the remaining \r\n
     * are replaced by \n. All \n are then replaced with linefeed, or &#xa; if
     * no linefeed is defined.
     *
     * Parameters:
     *
     * node - DOM node to return the XML for.
     * linefeed - Optional string that linefeeds are converted into. Default is
     * &#xa;
     */
    getXml(node, linefeed);

    /**
     * Function: getTextContent
     *
     * Returns the text content of the specified node.
     *
     * Parameters:
     *
     * node - DOM node to return the text content for.
     */
    getTextContent(node);

    /**
     * Function: setTextContent
     *
     * Sets the text content of the specified node.
     *
     * Parameters:
     *
     * node - DOM node to set the text content for.
     * text - String that represents the text content.
     */
    setTextContent(node, text);

    /**
     * Function: getInnerHtml
     *
     * Returns the inner HTML for the given node as a string or an empty string
     * if no node was specified. The inner HTML is the text representing all
     * children of the node, but not the node itself.
     *
     * Parameters:
     *
     * node - DOM node to return the inner HTML for.
     */
    getInnerHtml(node);

    /**
     * Function: getOuterHtml
     *
     * Returns the outer HTML for the given node as a string or an empty
     * string if no node was specified. The outer HTML is the text representing
     * all children of the node including the node itself.
     *
     * Parameters:
     *
     * node - DOM node to return the outer HTML for.
     */
    getOuterHtml(node);

    /**
     * Function: write
     *
     * Creates a text node for the given string and appends it to the given
     * parent. Returns the text node.
     *
     * Parameters:
     *
     * parent - DOM node to append the text node to.
     * text - String representing the text to be added.
     */
    write(parent, text);

    /**
     * Function: writeln
     *
     * Creates a text node for the given string and appends it to the given
     * parent with an additional linefeed. Returns the text node.
     *
     * Parameters:
     *
     * parent - DOM node to append the text node to.
     * text - String representing the text to be added.
     */
    writeln(parent, text);

    /**
     * Function: br
     *
     * Appends a linebreak to the given parent and returns the linebreak.
     *
     * Parameters:
     *
     * parent - DOM node to append the linebreak to.
     */
    br(parent, count);

    /**
     * Function: button
     *
     * Returns a new button with the given level and function as an onclick
     * event handler.
     *
     * (code)
     * document.body.appendChild(mxUtils.button('Test', function(evt)
     * {
     *   alert('Hello, World!');
     * }));
     * (end)
     *
     * Parameters:
     *
     * label - String that represents the label of the button.
     * funct - Function to be called if the button is pressed.
     * doc - Optional document to be used for creating the button. Default is the
     * current document.
     */
    button(label, funct, doc);

    /**
     * Function: para
     *
     * Appends a new paragraph with the given text to the specified parent and
     * returns the paragraph.
     *
     * Parameters:
     *
     * parent - DOM node to append the text node to.
     * text - String representing the text for the new paragraph.
     */
    para(parent, text);

    /**
     * Function: addTransparentBackgroundFilter
     *
     * Adds a transparent background to the filter of the given node. This
     * background can be used in IE8 standards mode (native IE8 only) to pass
     * events through the node.
     */
    addTransparentBackgroundFilter(node);

    /**
     * Function: linkAction
     *
     * Adds a hyperlink to the specified parent that invokes action on the
     * specified editor.
     *
     * Parameters:
     *
     * parent - DOM node to contain the new link.
     * text - String that is used as the link label.
     * editor - <mxEditor> that will execute the action.
     * action - String that defines the name of the action to be executed.
     * pad - Optional left-padding for the link. Default is 0.
     */
    linkAction(parent, text, editor, action, pad);

    /**
     * Function: linkInvoke
     *
     * Adds a hyperlink to the specified parent that invokes the specified
     * function on the editor passing along the specified argument. The
     * function name is the name of a function of the editor instance,
     * not an action name.
     *
     * Parameters:
     *
     * parent - DOM node to contain the new link.
     * text - String that is used as the link label.
     * editor - <mxEditor> instance to execute the function on.
     * functName - String that represents the name of the function.
     * arg - Object that represents the argument to the function.
     * pad - Optional left-padding for the link. Default is 0.
     */
    linkInvoke(parent, text, editor, functName, arg, pad);

    /**
     * Function: link
     *
     * Adds a hyperlink to the specified parent and invokes the given function
     * when the link is clicked.
     *
     * Parameters:
     *
     * parent - DOM node to contain the new link.
     * text - String that is used as the link label.
     * funct - Function to execute when the link is clicked.
     * pad - Optional left-padding for the link. Default is 0.
     */
    link(parent, text, funct, pad);

    /**
     * Function: fit
     *
     * Makes sure the given node is inside the visible area of the window. This
     * is done by setting the left and top in the style.
     */
    fit(node);

    /**
     * Function: load
     *
     * Loads the specified URL *synchronously* and returns the <mxXmlRequest>.
     * Throws an exception if the file cannot be loaded. See <mxUtils.get> for
     * an asynchronous implementation.
     *
     * Example:
     *
     * (code)
     * try
     * {
     *   var req = mxUtils.load(filename);
     *   var root = req.getDocumentElement();
     *   // Process XML DOM...
     * }
     * catch (ex)
     * {
     *   mxUtils.alert('Cannot load '+filename+': '+ex);
     * }
     * (end)
     *
     * Parameters:
     *
     * url - URL to get the data from.
     */
    load(url);

    /**
     * Function: get
     *
     * Loads the specified URL *asynchronously* and invokes the given functions
     * depending on the request status. Returns the <mxXmlRequest> in use. Both
     * functions take the <mxXmlRequest> as the only parameter. See
     * <mxUtils.load> for a synchronous implementation.
     *
     * Example:
     *
     * (code)
     * mxUtils.get(url, function(req)
     * {
     *    var node = req.getDocumentElement();
     *    // Process XML DOM...
     * });
     * (end)
     *
     * So for example, to load a diagram into an existing graph model, the
     * following code is used.
     *
     * (code)
     * mxUtils.get(url, function(req)
     * {
     *   var node = req.getDocumentElement();
     *   var dec = new mxCodec(node.ownerDocument);
     *   dec.decode(node, graph.getModel());
     * });
     * (end)
     *
     * Parameters:
     *
     * url - URL to get the data from.
     * onload - Optional function to execute for a successful response.
     * onerror - Optional function to execute on error.
     */
    get(url, onload, onerror);

    /**
     * Function: post
     *
     * Posts the specified params to the given URL *asynchronously* and invokes
     * the given functions depending on the request status. Returns the
     * <mxXmlRequest> in use. Both functions take the <mxXmlRequest> as the
     * only parameter. Make sure to use encodeURIComponent for the parameter
     * values.
     *
     * Example:
     *
     * (code)
     * mxUtils.post(url, 'key=value', function(req)
     * {
     * 	mxUtils.alert('Ready: '+req.isReady()+' Status: '+req.getStatus());
     *  // Process req.getDocumentElement() using DOM API if OK...
     * });
     * (end)
     *
     * Parameters:
     *
     * url - URL to get the data from.
     * params - Parameters for the post request.
     * onload - Optional function to execute for a successful response.
     * onerror - Optional function to execute on error.
     */
    post(url, params, onload, onerror);

    /**
     * Function: submit
     *
     * Submits the given parameters to the specified URL using
     * <mxXmlRequest.simulate> and returns the <mxXmlRequest>.
     * Make sure to use encodeURIComponent for the parameter
     * values.
     *
     * Parameters:
     *
     * url - URL to get the data from.
     * params - Parameters for the form.
     * doc - Document to create the form in.
     * target - Target to send the form result to.
     */
    submit(url, params, doc, target);

    /**
     * Function: loadInto
     *
     * Loads the specified URL *asynchronously* into the specified document,
     * invoking onload after the document has been loaded. This implementation
     * does not use <mxXmlRequest>, but the document.load method.
     *
     * Parameters:
     *
     * url - URL to get the data from.
     * doc - The document to load the URL into.
     * onload - Function to execute when the URL has been loaded.
     */
    loadInto(url, doc, onload);

    /**
     * Function: getValue
     *
     * Returns the value for the given key in the given associative array or
     * the given default value if the value is null.
     *
     * Parameters:
     *
     * array - Associative array that contains the value for the key.
     * key - Key whose value should be returned.
     * defaultValue - Value to be returned if the value for the given
     * key is null.
     */
    getValue(array, key, defaultValue);

    /**
     * Function: getNumber
     *
     * Returns the numeric value for the given key in the given associative
     * array or the given default value (or 0) if the value is null. The value
     * is converted to a numeric value using the Number function.
     *
     * Parameters:
     *
     * array - Associative array that contains the value for the key.
     * key - Key whose value should be returned.
     * defaultValue - Value to be returned if the value for the given
     * key is null. Default is 0.
     */
    getNumber(array, key, defaultValue);

    /**
     * Function: getColor
     *
     * Returns the color value for the given key in the given associative
     * array or the given default value if the value is null. If the value
     * is <mxConstants.NONE> then null is returned.
     *
     * Parameters:
     *
     * array - Associative array that contains the value for the key.
     * key - Key whose value should be returned.
     * defaultValue - Value to be returned if the value for the given
     * key is null. Default is null.
     */
    getColor(array, key, defaultValue);

    /**
     * Function: clone
     *
     * Recursively clones the specified object ignoring all fieldnames in the
     * given array of transient fields. <mxObjectIdentity.FIELD_NAME> is always
     * ignored by this function.
     *
     * Parameters:
     *
     * obj - Object to be cloned.
     * transients - Optional array of strings representing the fieldname to be
     * ignored.
     * shallow - Optional boolean argument to specify if a shallow clone should
     * be created, that is, one where all object references are not cloned or,
     * in other words, one where only atomic (strings, numbers) values are
     * cloned. Default is false.
     */
    clone(obj, transients, shallow);

    /**
     * Function: equalPoints
     *
     * Compares all mxPoints in the given lists.
     *
     * Parameters:
     *
     * a - Array of <mxPoints> to be compared.
     * b - Array of <mxPoints> to be compared.
     */
    equalPoints(a, b);

    /**
     * Function: equalEntries
     *
     * Returns true if all entries of the given objects are equal. Values with
     * with Number.NaN are equal to Number.NaN and unequal to any other value.
     *
     * Parameters:
     *
     * a - <mxRectangle> to be compared.
     * b - <mxRectangle> to be compared.
     */
    equalEntries(a, b);

    /**
     * Function: isNaN
     *
     * Returns true if the given value is of type number and isNaN returns true.
     */
    isNaN(value);

    /**
     * Function: extend
     *
     * Assigns a copy of the superclass prototype to the subclass prototype.
     * Note that this does not call the constructor of the superclass at this
     * point, the superclass constructor should be called explicitely in the
     * subclass constructor. Below is an example.
     *
     * (code)
     * MyGraph = function(container, model, renderHint, stylesheet)
     * {
     *   mxGraph.call(this, container, model, renderHint, stylesheet);
     * }
     *
     * mxUtils.extend(MyGraph, mxGraph);
     * (end)
     *
     * Parameters:
     *
     * ctor - Constructor of the subclass.
     * superCtor - Constructor of the superclass.
     */
    extend(ctor, superCtor);

    /**
     * Function: toString
     *
     * Returns a textual representation of the specified object.
     *
     * Parameters:
     *
     * obj - Object to return the string representation for.
     */
    toString(obj);

    /**
     * Function: toRadians
     *
     * Converts the given degree to radians.
     */
    toRadians(deg);

    /**
     * Function: arcToCurves
     *
     * Converts the given arc to a series of curves.
     */
    arcToCurves(x0, y0, r1, r2, angle, largeArcFlag, sweepFlag, x, y);

    /**
     * Function: getBoundingBox
     *
     * Returns the bounding box for the rotated rectangle.
     *
     * Parameters:
     *
     * rect - <mxRectangle> to be rotated.
     * angle - Number that represents the angle (in degrees).
     * cx - Optional <mxPoint> that represents the rotation center. If no
     * rotation center is given then the center of rect is used.
     */
    getBoundingBox(rect, rotation, cx);

    /**
     * Function: getRotatedPoint
     *
     * Rotates the given point by the given cos and sin.
     */
    getRotatedPoint(pt, cos, sin, c);

    /**
     * Returns an integer mask of the port constraints of the given map
     * @param dict the style map to determine the port constraints for
     * @param defaultValue Default value to return if the key is undefined.
     * @return the mask of port constraint directions
     *
     * Parameters:
     *
     * terminal - <mxCelState> that represents the terminal.
     * edge - <mxCellState> that represents the edge.
     * source - Boolean that specifies if the terminal is the source terminal.
     * defaultValue - Default value to be returned.
     */
    getPortConstraints(terminal, edge, source, defaultValue);

    /**
     * Function: reversePortConstraints
     *
     * Reverse the port constraint bitmask. For example, north | east
     * becomes south | west
     */
    reversePortConstraints(constraint);

    /**
     * Function: findNearestSegment
     *
     * Finds the index of the nearest segment on the given cell state for
     * the specified coordinate pair.
     */
    findNearestSegment(state, x, y);

    /**
     * Function: rectangleIntersectsSegment
     *
     * Returns true if the given rectangle intersects the given segment.
     *
     * Parameters:
     *
     * bounds - <mxRectangle> that represents the rectangle.
     * p1 - <mxPoint> that represents the first point of the segment.
     * p2 - <mxPoint> that represents the second point of the segment.
     */
    rectangleIntersectsSegment(bounds, p1, p2);

    /**
     * Function: contains
     *
     * Returns true if the specified point (x, y) is contained in the given rectangle.
     *
     * Parameters:
     *
     * bounds - <mxRectangle> that represents the area.
     * x - X-coordinate of the point.
     * y - Y-coordinate of the point.
     */
    contains(bounds, x, y);

    /**
     * Function: intersects
     *
     * Returns true if the two rectangles intersect.
     *
     * Parameters:
     *
     * a - <mxRectangle> to be checked for intersection.
     * b - <mxRectangle> to be checked for intersection.
     */
    intersects(a, b);

    /**
     * Function: intersects
     *
     * Returns true if the two rectangles intersect.
     *
     * Parameters:
     *
     * a - <mxRectangle> to be checked for intersection.
     * b - <mxRectangle> to be checked for intersection.
     */
    intersectsHotspot(state, x, y, hotspot, min, max);

    /**
     * Function: getOffset
     *
     * Returns the offset for the specified container as an <mxPoint>. The
     * offset is the distance from the top left corner of the container to the
     * top left corner of the document.
     *
     * Parameters:
     *
     * container - DOM node to return the offset for.
     * scollOffset - Optional boolean to add the scroll offset of the document.
     * Default is false.
     */
    getOffset(container, scrollOffset);

    /**
     * Function: getDocumentScrollOrigin
     *
     * Returns the scroll origin of the given document or the current document
     * if no document is given.
     */
    getDocumentScrollOrigin(doc);

    /**
     * Function: getScrollOrigin
     *
     * Returns the top, left corner of the viewrect as an <mxPoint>.
     */
    getScrollOrigin(node);

    /**
     * Function: convertPoint
     *
     * Converts the specified point (x, y) using the offset of the specified
     * container and returns a new <mxPoint> with the result.
     *
     * (code)
     * var pt = mxUtils.convertPoint(graph.container,
     *   mxEvent.getClientX(evt), mxEvent.getClientY(evt));
     * (end)
     *
     * Parameters:
     *
     * container - DOM node to use for the offset.
     * x - X-coordinate of the point to be converted.
     * y - Y-coordinate of the point to be converted.
     */
    convertPoint(container, x, y);

    /**
     * Function: ltrim
     *
     * Strips all whitespaces from the beginning of the string.
     * Without the second parameter, Javascript function will trim these
     * characters:
     *
     * - " " (ASCII 32 (0x20)), an ordinary space
     * - "\t" (ASCII 9 (0x09)), a tab
     * - "\n" (ASCII 10 (0x0A)), a new line (line feed)
     * - "\r" (ASCII 13 (0x0D)), a carriage return
     * - "\0" (ASCII 0 (0x00)), the NUL-byte
     * - "\x0B" (ASCII 11 (0x0B)), a vertical tab
     */
    ltrim(str, chars);

    /**
     * Function: rtrim
     *
     * Strips all whitespaces from the end of the string.
     * Without the second parameter, Javascript function will trim these
     * characters:
     *
     * - " " (ASCII 32 (0x20)), an ordinary space
     * - "\t" (ASCII 9 (0x09)), a tab
     * - "\n" (ASCII 10 (0x0A)), a new line (line feed)
     * - "\r" (ASCII 13 (0x0D)), a carriage return
     * - "\0" (ASCII 0 (0x00)), the NUL-byte
     * - "\x0B" (ASCII 11 (0x0B)), a vertical tab
     */
    rtrim(str, chars);

    /**
     * Function: trim
     *
     * Strips all whitespaces from both end of the string.
     * Without the second parameter, Javascript function will trim these
     * characters:
     *
     * - " " (ASCII 32 (0x20)), an ordinary space
     * - "\t" (ASCII 9 (0x09)), a tab
     * - "\n" (ASCII 10 (0x0A)), a new line (line feed)
     * - "\r" (ASCII 13 (0x0D)), a carriage return
     * - "\0" (ASCII 0 (0x00)), the NUL-byte
     * - "\x0B" (ASCII 11 (0x0B)), a vertical tab
     */
    trim(str, chars);

    /**
     * Function: isNumeric
     *
     * Returns true if the specified value is numeric, that is, if it is not
     * null, not an empty string, not a HEX number and isNaN returns false.
     *
     * Parameters:
     *
     * n - String representing the possibly numeric value.
     */
    isNumeric(n);

    /**
     * Function: mod
     *
     * Returns the remainder of division of n by m. You should use this instead
     * of the built-in operation as the built-in operation does not properly
     * handle negative numbers.
     */
    mod(n, m);

    /**
     * Function: intersection
     *
     * Returns the intersection of two lines as an <mxPoint>.
     *
     * Parameters:
     *
     * x0 - X-coordinate of the first line's startpoint.
     * y0 - X-coordinate of the first line's startpoint.
     * x1 - X-coordinate of the first line's endpoint.
     * y1 - Y-coordinate of the first line's endpoint.
     * x2 - X-coordinate of the second line's startpoint.
     * y2 - Y-coordinate of the second line's startpoint.
     * x3 - X-coordinate of the second line's endpoint.
     * y3 - Y-coordinate of the second line's endpoint.
     */
    intersection(x0, y0, x1, y1, x2, y2, x3, y3);

    /**
     * Function: ptSeqDistSq
     *
     * Returns the square distance between a segment and a point.
     *
     * Parameters:
     *
     * x1 - X-coordinate of the startpoint of the segment.
     * y1 - Y-coordinate of the startpoint of the segment.
     * x2 - X-coordinate of the endpoint of the segment.
     * y2 - Y-coordinate of the endpoint of the segment.
     * px - X-coordinate of the point.
     * py - Y-coordinate of the point.
     */
    ptSegDistSq(x1, y1, x2, y2, px, py);

    /**
     * Function: relativeCcw
     *
     * Returns 1 if the given point on the right side of the segment, 0 if its
     * on the segment, and -1 if the point is on the left side of the segment.
     *
     * Parameters:
     *
     * x1 - X-coordinate of the startpoint of the segment.
     * y1 - Y-coordinate of the startpoint of the segment.
     * x2 - X-coordinate of the endpoint of the segment.
     * y2 - Y-coordinate of the endpoint of the segment.
     * px - X-coordinate of the point.
     * py - Y-coordinate of the point.
     */
    relativeCcw(x1, y1, x2, y2, px, py);

    /**
     * Function: animateChanges
     *
     * See <mxEffects.animateChanges>. This is for backwards compatibility and
     * will be removed later.
     */
    animateChanges(graph, changes);

    /**
     * Function: cascadeOpacity
     *
     * See <mxEffects.cascadeOpacity>. This is for backwards compatibility and
     * will be removed later.
     */
    cascadeOpacity(graph, cell, opacity);

    /**
     * Function: fadeOut
     *
     * See <mxEffects.fadeOut>. This is for backwards compatibility and
     * will be removed later.
     */
    fadeOut(node, from, remove, step, delay, isEnabled);

    /**
     * Function: setOpacity
     *
     * Sets the opacity of the specified DOM node to the given value in %.
     *
     * Parameters:
     *
     * node - DOM node to set the opacity for.
     * value - Opacity in %. Possible values are between 0 and 100.
     */
    setOpacity(node, value);

    /**
     * Function: createImage
     *
     * Creates and returns an image (IMG node) or VML image (v:image) in IE6 in
     * quirks mode.
     *
     * Parameters:
     *
     * src - URL that points to the image to be displayed.
     */
    createImage(src);

    /**
     * Function: sortCells
     *
     * Sorts the given cells according to the order in the cell hierarchy.
     * Ascending is optional and defaults to true.
     */
    sortCells(cells, ascending);

    /**
     * Function: getStylename
     *
     * Returns the stylename in a style of the form [(stylename|key=value);] or
     * an empty string if the given style does not contain a stylename.
     *
     * Parameters:
     *
     * style - String of the form [(stylename|key=value);].
     */
    getStylename(style);

    /**
     * Function: getStylenames
     *
     * Returns the stylenames in a style of the form [(stylename|key=value);]
     * or an empty array if the given style does not contain any stylenames.
     *
     * Parameters:
     *
     * style - String of the form [(stylename|key=value);].
     */
    getStylenames(style);

    /**
     * Function: indexOfStylename
     *
     * Returns the index of the given stylename in the given style. This
     * returns -1 if the given stylename does not occur (as a stylename) in the
     * given style, otherwise it returns the index of the first character.
     */
    indexOfStylename(style, stylename);

    /**
     * Function: addStylename
     *
     * Adds the specified stylename to the given style if it does not already
     * contain the stylename.
     */
    addStylename(style, stylename);

    /**
     * Function: removeStylename
     *
     * Removes all occurrences of the specified stylename in the given style
     * and returns the updated style. Trailing semicolons are not preserved.
     */
    removeStylename(style, stylename);

    /**
     * Function: removeAllStylenames
     *
     * Removes all stylenames from the given style and returns the updated
     * style.
     */
    removeAllStylenames(style);

    /**
     * Function: setCellStyles
     *
     * Assigns the value for the given key in the styles of the given cells, or
     * removes the key from the styles if the value is null.
     *
     * Parameters:
     *
     * model - <mxGraphModel> to execute the transaction in.
     * cells - Array of <mxCells> to be updated.
     * key - Key of the style to be changed.
     * value - New value for the given key.
     */
    setCellStyles(model, cells, key, value);

    /**
     * Function: setStyle
     *
     * Adds or removes the given key, value pair to the style and returns the
     * new style. If value is null or zero length then the key is removed from
     * the style. This is for cell styles, not for CSS styles.
     *
     * Parameters:
     *
     * style - String of the form [(stylename|key=value);].
     * key - Key of the style to be changed.
     * value - New value for the given key.
     */
    setStyle(style, key, value);

    /**
     * Function: setCellStyleFlags
     *
     * Sets or toggles the flag bit for the given key in the cell's styles.
     * If value is null then the flag is toggled.
     *
     * Example:
     *
     * (code)
     * var cells = graph.getSelectionCells();
     * mxUtils.setCellStyleFlags(graph.model,
     * 			cells,
     * 			mxConstants.STYLE_FONTSTYLE,
     * 			mxConstants.FONT_BOLD);
     * (end)
     *
     * Toggles the bold font style.
     *
     * Parameters:
     *
     * model - <mxGraphModel> that contains the cells.
     * cells - Array of <mxCells> to change the style for.
     * key - Key of the style to be changed.
     * flag - Integer for the bit to be changed.
     * value - Optional boolean value for the flag.
     */
    setCellStyleFlags(model, cells, key, flag, value);

    /**
     * Function: setStyleFlag
     *
     * Sets or removes the given key from the specified style and returns the
     * new style. If value is null then the flag is toggled.
     *
     * Parameters:
     *
     * style - String of the form [(stylename|key=value);].
     * key - Key of the style to be changed.
     * flag - Integer for the bit to be changed.
     * value - Optional boolean value for the given flag.
     */
    setStyleFlag(style, key, flag, value);

    /**
     * Function: getAlignmentAsPoint
     *
     * Returns an <mxPoint> that represents the horizontal and vertical alignment
     * for numeric computations. X is -0.5 for center, -1 for right and 0 for
     * left alignment. Y is -0.5 for middle, -1 for bottom and 0 for top
     * alignment. Default values for missing arguments is top, left.
     */
    getAlignmentAsPoint(align, valign);

    /**
     * Function: getSizeForString
     *
     * Returns an <mxRectangle> with the size (width and height in pixels) of
     * the given string. The string may contain HTML markup. Newlines should be
     * converted to <br> before calling this method. The caller is responsible
     * for sanitizing the HTML markup.
     *
     * Example:
     *
     * (code)
     * var label = graph.getLabel(cell).replace(/\n/g, "<br>");
     * var size = graph.getSizeForString(label);
     * (end)
     *
     * Parameters:
     *
     * text - String whose size should be returned.
     * fontSize - Integer that specifies the font size in pixels. Default is
     * <mxConstants.DEFAULT_FONTSIZE>.
     * fontFamily - String that specifies the name of the font family. Default
     * is <mxConstants.DEFAULT_FONTFAMILY>.
     * textWidth - Optional width for text wrapping.
     */
    getSizeForString(text, fontSize, fontFamily, textWidth);

    /**
     * Function: getViewXml
     */
    getViewXml(graph, scale, cells, x0, y0);

    /**
     * Function: getScaleForPageCount
     *
     * Returns the scale to be used for printing the graph with the given
     * bounds across the specifies number of pages with the given format. The
     * scale is always computed such that it given the given amount or fewer
     * pages in the print output. See <mxPrintPreview> for an example.
     *
     * Parameters:
     *
     * pageCount - Specifies the number of pages in the print output.
     * graph - <mxGraph> that should be printed.
     * pageFormat - Optional <mxRectangle> that specifies the page format.
     * Default is <mxConstants.PAGE_FORMAT_A4_PORTRAIT>.
     * border - The border along each side of every page.
     */
    getScaleForPageCount(pageCount, graph, pageFormat, border);

    /**
     * Function: show
     *
     * Copies the styles and the markup from the graph's container into the
     * given document and removes all cursor styles. The document is returned.
     *
     * This function should be called from within the document with the graph.
     * If you experience problems with missing stylesheets in IE then try adding
     * the domain to the trusted sites.
     *
     * Parameters:
     *
     * graph - <mxGraph> to be copied.
     * doc - Document where the new graph is created.
     * x0 - X-coordinate of the graph view origin. Default is 0.
     * y0 - Y-coordinate of the graph view origin. Default is 0.
     * w - Optional width of the graph view.
     * h - Optional height of the graph view.
     */
    show(graph, doc, x0, y0, w, h);

    /**
     * Function: printScreen
     *
     * Prints the specified graph using a new window and the built-in print
     * dialog.
     *
     * This function should be called from within the document with the graph.
     *
     * Parameters:
     *
     * graph - <mxGraph> to be printed.
     */
    printScreen(graph);

    /**
     * Function: popup
     *
     * Shows the specified text content in a new <mxWindow> or a new browser
     * window if isInternalWindow is false.
     *
     * Parameters:
     *
     * content - String that specifies the text to be displayed.
     * isInternalWindow - Optional boolean indicating if an mxWindow should be
     * used instead of a new browser window. Default is false.
     */
    popup(content, isInternalWindow);

    /**
     * Function: alert
     *
     * Displayss the given alert in a new dialog. This implementation uses the
     * built-in alert function. This is used to display validation errors when
     * connections cannot be changed or created.
     *
     * Parameters:
     *
     * message - String specifying the message to be displayed.
     */
    alert(message);

    /**
     * Function: prompt
     *
     * Displays the given message in a prompt dialog. This implementation uses
     * the built-in prompt function.
     *
     * Parameters:
     *
     * message - String specifying the message to be displayed.
     * defaultValue - Optional string specifying the default value.
     */
    prompt(message, defaultValue);

    /**
     * Function: confirm
     *
     * Displays the given message in a confirm dialog. This implementation uses
     * the built-in confirm function.
     *
     * Parameters:
     *
     * message - String specifying the message to be displayed.
     */
    confirm(message);

    /**
     * Function: error
     *
     * Displays the given error message in a new <mxWindow> of the given width.
     * If close is true then an additional close button is added to the window.
     * The optional icon specifies the icon to be used for the window. Default
     * is <mxUtils.errorImage>.
     *
     * Parameters:
     *
     * message - String specifying the message to be displayed.
     * width - Integer specifying the width of the window.
     * close - Optional boolean indicating whether to add a close button.
     * icon - Optional icon for the window decoration.
     */
    error(message: string, width: number, close?: boolean, icon?: any);

    /**
     * Function: makeDraggable
     *
     * Configures the given DOM element to act as a drag source for the
     * specified graph. Returns a a new <mxDragSource>. If
     * <mxDragSource.guideEnabled> is enabled then the x and y arguments must
     * be used in funct to match the preview location.
     *
     * Example:
     *
     * (code)
     * var funct = function(graph, evt, cell, x, y)
     * {
     *   if (graph.canImportCell(cell))
     *   {
     *     var parent = graph.getDefaultParent();
     *     var vertex = null;
     *
     *     graph.getModel().beginUpdate();
     *     try
     *     {
     * 	     vertex = graph.insertVertex(parent, null, 'Hello', x, y, 80, 30);
     *     }
     *     finally
     *     {
     *       graph.getModel().endUpdate();
     *     }
     *
     *     graph.setSelectionCell(vertex);
     *   }
     * }
     *
     * var img = document.createElement('img');
     * img.setAttribute('src', 'editors/images/rectangle.gif');
     * img.style.position = 'absolute';
     * img.style.left = '0px';
     * img.style.top = '0px';
     * img.style.width = '16px';
     * img.style.height = '16px';
     *
     * var dragImage = img.cloneNode(true);
     * dragImage.style.width = '32px';
     * dragImage.style.height = '32px';
     * mxUtils.makeDraggable(img, graph, funct, dragImage);
     * document.body.appendChild(img);
     * (end)
     *
     * Parameters:
     *
     * element - DOM element to make draggable.
     * graphF - <mxGraph> that acts as the drop target or a function that takes a
     * mouse event and returns the current <mxGraph>.
     * funct - Function to execute on a successful drop.
     * dragElement - Optional DOM node to be used for the drag preview.
     * dx - Optional horizontal offset between the cursor and the drag
     * preview.
     * dy - Optional vertical offset between the cursor and the drag
     * preview.
     * autoscroll - Optional boolean that specifies if autoscroll should be
     * used. Default is mxGraph.autoscroll.
     * scalePreview - Optional boolean that specifies if the preview element
     * should be scaled according to the graph scale. If this is true, then
     * the offsets will also be scaled. Default is false.
     * highlightDropTargets - Optional boolean that specifies if dropTargets
     * should be highlighted. Default is true.
     * getDropTarget - Optional function to return the drop target for a given
     * location (x, y). Default is mxGraph.getCellAt.
     */
    makeDraggable(element, graphF, funct, dragElement, dx, dy, autoscroll,
        scalePreview, highlightDropTargets, getDropTarget);
}

declare var mxUtils: MxUtils;

/**
 * Class: mxAbstractCanvas2D
 *
 * Base class for all canvases. A description of the public API is available in <mxXmlCanvas2D>.
 * All color values of <mxConstants.NONE> will be converted to null in the state.
 *
 */
interface MxAbstractCanvas2D {
    /**
     * Variable: state
     *
     * Holds the current state.
     */
    state: MxAbstractCanvas2DState;

    /**
     * Variable: states
     *
     * Stack of states.
     */
    states;

    /**
     * Variable: path
     *
     * Holds the current path as an array.
     */
    path;

    /**
     * Variable: rotateHtml
     *
     * Switch for rotation of HTML. Default is false.
     */
    rotateHtml;

    /**
     * Variable: lastX
     *
     * Holds the last x coordinate.
     */
    lastX;

    /**
     * Variable: lastY
     *
     * Holds the last y coordinate.
     */
    lastY;

    /**
     * Variable: moveOp
     *
     * Contains the string used for moving in paths. Default is 'M'.
     */
    moveOp;

    /**
     * Variable: lineOp
     *
     * Contains the string used for moving in paths. Default is 'L'.
     */
    lineOp;

    /**
     * Variable: quadOp
     *
     * Contains the string used for quadratic paths. Default is 'Q'.
     */
    quadOp;

    /**
     * Variable: curveOp
     *
     * Contains the string used for bezier curves. Default is 'C'.
     */
    curveOp;

    /**
     * Variable: closeOp
     *
     * Holds the operator for closing curves. Default is 'Z'.
     */
    closeOp;

    /**
     * Variable: pointerEvents
     *
     * Boolean value that specifies if events should be handled. Default is false.
     */
    pointerEvents;

    /**
     * Function: createUrlConverter
     *
     * Create a new <mxUrlConverter> and returns it.
     */
    createUrlConverter();

    /**
     * Function: reset
     *
     * Resets the state of this canvas.
     */
    reset();

    /**
     * Function: createState
     *
     * Creates the state of the this canvas.
     */
    createState();

    /**
     * Function: format
     *
     * Rounds all numbers to integers.
     */
    format(value);

    /**
     * Function: addOp
     *
     * Adds the given operation to the path.
     */
    addOp();

    /**
     * Function: rotatePoint
     *
     * Rotates the given point and returns the result as an <mxPoint>.
     */
    rotatePoint(x, y, theta, cx, cy);

    /**
     * Function: save
     *
     * Saves the current state.
     */
    save();

    /**
     * Function: restore
     *
     * Restores the current state.
     */
    restore();

    /**
     * Function: setLink
     *
     * Sets the current link. Hook for subclassers.
     */
    setLink(link);

    /**
     * Function: scale
     *
     * Scales the current state.
     */
    scale(value: number);

    /**
     * Function: translate
     *
     * Translates the current state.
     */
    translate(dx, dy);

    /**
     * Function: setAlpha
     *
     * Sets the current alpha.
     */
    setAlpha(value);

    /**
     * Function: setFillColor
     *
     * Sets the current fill color.
     */
    setFillColor(value);

    /**
     * Function: setGradient
     *
     * Sets the current gradient.
     */
    setGradient(color1, color2, x, y, w, h, direction, alpha1, alpha2);

    /**
     * Function: setStrokeColor
     *
     * Sets the current stroke color.
     */
    setStrokeColor(value);

    /**
     * Function: setStrokeWidth
     *
     * Sets the current stroke width.
     */
    setStrokeWidth(value);

    /**
     * Function: setDashed
     *
     * Enables or disables dashed lines.
     */
    setDashed(value);

    /**
     * Function: setDashPattern
     *
     * Sets the current dash pattern.
     */
    setDashPattern(value);

    /**
     * Function: setLineCap
     *
     * Sets the current line cap.
     */
    setLineCap(value);

    /**
     * Function: setLineJoin
     *
     * Sets the current line join.
     */
    setLineJoin(value);

    /**
     * Function: setMiterLimit
     *
     * Sets the current miter limit.
     */
    setMiterLimit(value);

    /**
     * Function: setFontColor
     *
     * Sets the current font color.
     */
    setFontColor(value);

    /**
     * Function: setFontColor
     *
     * Sets the current font color.
     */
    setFontBackgroundColor(value);

    /**
     * Function: setFontColor
     *
     * Sets the current font color.
     */
    setFontBorderColor(value);

    /**
     * Function: setFontSize
     *
     * Sets the current font size.
     */
    setFontSize(value);

    /**
     * Function: setFontFamily
     *
     * Sets the current font family.
     */
    setFontFamily(value);

    /**
     * Function: setFontStyle
     *
     * Sets the current font style.
     */
    setFontStyle(value);

    /**
     * Function: setShadow
     *
     * Enables or disables and configures the current shadow.
     */
    setShadow(enabled);

    /**
     * Function: setShadowColor
     *
     * Enables or disables and configures the current shadow.
     */
    setShadowColor(value);

    /**
     * Function: setShadowAlpha
     *
     * Enables or disables and configures the current shadow.
     */
    setShadowAlpha(value);

    /**
     * Function: setShadowOffset
     *
     * Enables or disables and configures the current shadow.
     */
    setShadowOffset(dx, dy);

    /**
     * Function: begin
     *
     * Starts a new path.
     */
    begin();

    /**
     * Function: moveTo
     *
     *  Moves the current path the given coordinates.
     */
    moveTo(x, y);

    /**
     * Function: lineTo
     *
     * Draws a line to the given coordinates. Uses moveTo with the op argument.
     */
    lineTo(x, y);

    /**
     * Function: quadTo
     *
     * Adds a quadratic curve to the current path.
     */
    quadTo(x1, y1, x2, y2);

    /**
     * Function: curveTo
     *
     * Adds a bezier curve to the current path.
     */
    curveTo(x1, y1, x2, y2, x3, y3);

    /**
     * Function: arcTo
     *
     * Adds the given arc to the current path. This is a synthetic operation that
     * is broken down into curves.
     */
    arcTo(rx, ry, angle, largeArcFlag, sweepFlag, x, y);

    /**
     * Function: close
     *
     * Closes the current path.
     */
    close(x1, y1, x2, y2, x3, y3);

    /**
     * Function: end
     *
     * Empty implementation for backwards compatibility. This will be removed.
     */
    end();
}

interface MxAbstractCanvas2DState {
    dx: number;
    dy: number;
    scale: number;
    alpha: number;
    fillColor: string;
    fillAlpha: number;
    gradientColor: string;
    gradientAlpha: number;
    gradientDirection: string;
    strokeColor: string;
    strokeWidth: number;
    dashed: boolean;
    dashPattern: string;
    lineCap: string;
    lineJoin: string;
    miterLimit: number;
    fontColor: string;
    fontBackgroundColor: string;
    fontBorderColor: string;
    fontSize: number;
    fontFamily: string;
    fontStyle: number;
    shadow: boolean;
    shadowColor: string;
    shadowAlpha: number;
    shadowDx: number;
    shadowDy: number;
    rotation: number;
    rotationCx: number;
    rotationCy: number;
}

interface MxGraphHandler {
    getInitialCellForEvent(me: MxMouseEvent);
}

interface MxPopupMenuHandler {
    selectOnPopup: boolean;
    factoryMethod;
}

interface MxPopupMenu {
    hideMenu();
    setEnabled(enabled: boolean);
    useLeftButtonForPopup: boolean;
    popup(x, y, cell, evt);
    addItem(title, image, funct, parent, iconCls, enabled, active);
}

interface MxCellEditor {
    /**
    * in-place editor for the specified graph.
    */
    mxCellEditor(graph: MxGraph);
    graph: MxGraph;
    textarea: any;
    editingCell: MxCell;
    trigger: any;
    modified: boolean;
    autoSize: boolean;
    selectText: boolean;
    emptyLabelText: string;
    textNode: any;
    zIndex: number;
    minResize: number;
    wordWrapPadding: number;
    blurEnabled: boolean;
    initialValue: any;
    init();
    startEditing(cell: MxCell, trigger?: any);
    stopEditing(cancel: boolean);
    getEditorBounds(state: any): MxRectangle;
    getEditingCell(): MxCell;
    destroy();





}

interface MxGraph {

    dialect: string;

    cellEditor: MxCellEditor;

    popupMenuHandler: MxPopupMenuHandler;

    graphHandler: MxGraphHandler;

    /**
    * Specifies the return value for edges in <isLabelMovable>. Default is true.
    */
    edgeLabelsMovable;

    /**
     * Specifies the return value for vertices in <isLabelMovable>. Default is false.
     */
    vertexLabelsMovable;

    /**
     * Returns true if the given cell is connectable in this graph.  This implementation uses mxGraphModel.isConnectable.
     * Subclassers can override this to implement specific connectable states for cells in only one graph, that is, without affecting the connectable state of the cell in the model.
     *
     * @param cell	mxCell whose connectable state should be returned
     */
    isCellConnectable(cell: MxCell);
    /**
     * Returns true if the given cell is disconnectable from the source or target terminal.
     * This returns isCellsDisconnectable for all given cells if isCellLocked does not return true for the given cell.
     *
     * @param cell mxCell whose disconnectable state should be returned.
     * @param terminal	mxCell that represents the source or target terminal.
     * @param source Boolean indicating if the source or target terminal is to be disconnected.
     */
    isCellDisconnectable(cell: MxCell, terminal: MxCell, source: boolean);
    /**
     * Returns true if the given terminal point is movable. This is independent from isCellConnectable and isCellDisconnectable and controls if terminal points can be moved in the graph if the edge is not connected.
     * Note that it is required for this to return true to connect unconnected edges.This implementation returns true.
     *
     * @param cell	mxCell whose terminal point should be moved.
     * @param source Boolean indicating if the source or target terminal should be moved.
     */
    isTerminalPointMovable(cell: MxCell, source: boolean);
    /**
     * Returns true if the given cell is moveable.  This returns cellsMovable for all given cells if isCellLocked does not return true for the given cell and its style does not specify mxConstants.STYLE_MOVABLE to be 0.
     *
     * @param cell	mxCell whose movable state should be returned.
     */
    isCellMovable(cell: MxCell);
    /**
     * Returns true if the given cell is resizable.  This returns cellsResizable for all given cells if isCellLocked does not return true for the given cell and its style does not specify mxConstants.STYLE_RESIZABLE to be 0.
     *
     * @param cell	mxCell whose resizable state should be returned.
     */
    isCellResizable(cell: MxCell);
    /**
     * Returns true if the given cell is editable.  This returns cellsEditable for all given cells if isCellLocked does not return true for the given cell and its style does not specify mxConstants.STYLE_EDITABLE to be 0.
     *
     * @param cell	mxCell whose editable state should be returned.
     */
    isCellEditable(cell: MxCell);
    /**
     * Returns true if the given cell is moveable.  This returns cellsDeletable for all given cells if a cells style does not specify mxConstants.STYLE_DELETABLE to be 0.
     *
     * @param cell	mxCell whose deletable state should be returned.
     */
    isCellDeletable(cell: MxCell);
    /**
     * Function: isCellLocked
     *
     * Returns true if the given cell may not be moved, sized, bended,
     * disconnected, edited or selected. This implementation returns true for
     * all vertices with a relative geometry if <locked> is false.
     *
     * Parameters:
     *
     * cell - <mxCell> whose locked state should be returned.
     */
    isCellLocked(cell: MxCell);
    /**
     * Function: isCellsLocked
     *
     * Returns true if the given cell may not be moved, sized, bended,
     * disconnected, edited or selected. This implementation returns true for
     * all vertices with a relative geometry if <locked> is false.
     *
     * Parameters:
     *
     * cell - <mxCell> whose locked state should be returned.
     */
    isCellsLocked();
    /**
     * Returns the cells which are movable in the given array of cells.
     *
     * @param cell	mxCell whose foldable state should be returned.
     */
    isCellFoldable(cell: MxCell): boolean;
    /**
     * Holds the mxCellRenderer for rendering the cells in the graph.
     */
    isEditing(cell?: MxCell): boolean;
    /**
    * Returns true if the given cell is currently being edited.If no cell is specified then this returns true if any cell is currently being edited.
    */
    cellRenderer: MxCellRender;
    /**
     * Sets the collapsed state of the specified cells and all descendants if recurse is true.
     * The change is carried out using cellsFolded.  This method fires mxEvent.FOLD_CELLS while the transaction is in progress.
     * Returns the cells whose collapsed state was changed.
     *
     * @param collapse	    Boolean indicating the collapsed state to be assigned.
     * @param recurse	    Optional boolean indicating if the collapsed state of all descendants should be set.  Default is false.
     * @param cells	        Array of mxCells whose collapsed state should be set.  If null is specified then the foldable selection cells are used.
     * @param checkFoldable	Optional boolean indicating of isCellFoldable should be checked.  Default is false.
     */
    foldCells(collapse, recurse?: boolean, cells?: Array<MxCell>, checkFoldable?: boolean);
    /**
     * Returns the mxGraphView that contains the mxCellStates.
     */
    getView(): MxGraphView;
    /**
     * Returns an array of key, value pairs representing the cell style for the given cell.
     * If no string is defined in the model that specifies the style, then the default style for the cell is returned or EMPTY_ARRAY, if not style can be found.
     * Note: You should try and get the cell state for the given cell and use the cached style in the state before using this method.
     * @param cell mxCell whose style should be returned as an array.
     */
    getCellStyle(cell: MxCell): any;
    /**
     * Returns defaultParent or mxGraphView.currentRoot or the first child child of mxGraphModel.root if both are null.
     * The value returned by this function should be used as the parent for new cells (aka default layer).
     */
    getDefaultParent(): MxCell;
    /**
     * Returns the mxGraphModel that contains the cells.
     */
    getModel(): MxGraphModel;
    /**
     * Returns a string or DOM node that represents the label for the given cell.
     * This implementation uses convertValueToString if labelsVisible is true. Otherwise it returns an empty string.
     *
     * @param cell	mxCell whose label should be returned.
     */
    getLabel(cell: MxCell): string;
    /**
     * Returns true if the label must be rendered as HTML markup.  The default implementation returns htmlLabels.
     *
     * @param cell	mxCell whose label should be displayed as HTML markup.
     */
    isHtmlLabel(cell: MxCell): boolean;

    getSecondLabel(cell: MxCell): string;
    getSelectionModel(): MxGraphSelectionModel;
    insertVertex(parent: MxCell, id: string, value: any, x: number, y: number, width: number, height: number, style?: string, relative?: boolean): MxCell;
    insertEdge(parent: MxCell, id: string, value: any, source: MxCell, target: MxCell, style?: any): MxCell;
    getStylesheet(): any;
    /**
     * Traverses the (directed) graph invoking the given function for each visited vertex and edge.
     * The function is invoked with the current vertex and the incoming edge as a parameter.
     * This implementation makes sure each vertex is only visited once.
     * The function may return false if the traversal should stop at the given vertex.
     *
     * @param vertex mxCell that represents the vertex where the traversal starts.
     * @param directed Optional boolean indicating if edges should only be traversed from source to target.  Default is true.
     * @param func Visitor function that takes the current vertex and the incoming edge as arguments.  The traversal stops if the function returns false.
     * @param edge Optional mxCell that represents the incoming edge.  This is null for the first step of the traversal.
     * @param visited Optional array of cell paths for the visited cells.
     */
    traverse(vertex: MxCell, directed: boolean, func: any, edge?: MxCell, visited?: boolean): void;
    /**
     * Sets the visible state of the specified cells and all connected edges if includeEdges is true.
     * The change is carried out using cellsToggled. This method fires mxEvent.TOGGLE_CELLS while the transaction is in progress.
     * Returns the cells whose visible state was changed.
     *
     * @param show Boolean that specifies the visible state to be assigned.
     * @param cells Array of mxCells whose visible state should be changed.  If null is specified then the selection cells are used.
     * @param includeEdges Optional boolean indicating if the visible state of all connected edges should be changed as well.  Default is true.
     */
    toggleCells(show: boolean, cells: Array<MxCell>, includeEdges?: boolean): Array<MxCell>;
    /**
     * Returns true if the given cell is collapsed in this graph. This implementation uses mxGraphModel.isCollapsed.
     * Subclassers can override this to implement specific collapsed states for cells in only one graph, that is, without affecting the collapsed state of the cell.
     * When using dynamic filter expressions for the collapsed state, then the graph should be revalidated after the filter expression has changed.
     *
     * @param cell Boolean that specifies the visible state to be assigned.
     */
    isCellCollapsed(cell: MxCell): boolean;

    /**
     * Selects all children of the given parent cell or the children of the default parent if no parent is specified.  To select leaf vertices and/or edges use selectCells.
     *
     * @param parent Optional mxCell whose children should be selected.  Default is defaultParent.
     */
    selectAll(parent?: MxCell): void;

    /**
     * Returns the array of selected mxCells.
     *
     */
    getSelectionCells(): Array<MxCell>;

    /**
     * Removes the given cells from the graph including all connected edges if includeEdges is true.
     * The change is carried out using cellsRemoved. This method fires mxEvent.REMOVE_CELLS while the transaction is in progress.
     * The removed cells are returned as an array.
     *
     * @param cells	Array of mxCells to remove.  If null is specified then the selection cells which are deletable are used.
     * @param includeEdges Optional boolean which specifies if all connected edges should be removed as well.  Default is true.
     */
    removeCells(cells: Array<MxCell>, includeEdges?: boolean): void;

    /**
     * Function: selectCellForEvent
     *
     * Selects the given cell by either adding it to the selection or
     * replacing the selection depending on whether the given mouse event is a
     * toggle event.
     *
     * Parameters:
     *
     * cell - <mxCell> to be selected.
     * evt - Optional mouseevent that triggered the selection.
     */
    selectCellForEvent(cell: MxCell, evt: any);

    /**
     * Scales the graph such that the complete diagram fits into <container> and returns the current scale in the view.
     * To fit an initial graph prior to rendering, set mxGraphView.rendering to false prior to changing the model and execute the following after changing the model.
     *
     * @param border Optional number that specifies the border. Default is 0.
     * @param keepOrigin Optional boolean that specifies if the translate should be changed. Default is false.
     */
    fit(border?: number, keepOrigin?: boolean);

    /**
     * Function: zoomToRect
     *
     * Zooms the graph to the specified rectangle.
     */
    zoomToRect(rect);
    zoomToRectAndGetScale(rect): number;

    /**
     * Adds the cells into the given group. The change is carried out using cellsAdded, cellsMoved and cellsResized.
     * This method fires mxEvent.GROUP_CELLS while the transaction is in progress.
     * Returns the new group. A group is only created if there is at least one entry in the given array of cells.
     *
     * @param group mxCell that represents the target group. If null is specified then a new group is created using createGroupCell.
     * @param border Optional integer that specifies the border between the child area and the group bounds.Default is 0.
     * @param cells Optional array of mxCells to be grouped.If null is specified then the selection cells are used.
     */
    groupCells(group, border?: number, cells?: Array<MxCell>): MxCell;

    /**
    * Function: setConnectable
    *
    * Specifies if the graph should allow new connections. This implementation
    * updates <mxConnectionHandler.enabled> in <connectionHandler>.
    *
    * Parameters:
    *
    * connectable - Boolean indicating if new connections should be allowed.
    */
    setConnectable(connectable: boolean);

    /**
     * Sets recursiveResize.
     *
     */
    setRecursiveResize(value: boolean): void

     /**
     * Returns the bounds of the visible graph. Shortcut to mxGraphView.getGraphBounds. See also: getBoundingBoxFromGeometry.
     *
     */
    getGraphBounds(): any;//TODO

    /**
     * Sets htmlLabels.
     *
     *@param value Boolean
     */
    setHtmlLabels(value: boolean);

    getSelectionCell();

    setSelectionCell(cell: MxCell);

    /**
     * Sets the selection cell.
     *
     * @param cells - Array of <mxCells> to be selected.
     */
    setSelectionCells(cells: Array<MxCell>);

    clearSelection();

    container: HTMLElement;
    /**
     * Returns true if no white-space CSS style directive should be used for displaying the given cells label.
     * This implementation returns true if mxConstants.STYLE_WHITE_SPACE in the style of the given cell is ‘wrap’.
     * This is used as a workaround for IE ignoring the white-space directive of child elements if the directive appears in a parent element.
     * It should be overridden to return true if a white-space directive is used in the HTML markup that represents the given cells label.
     * In order for HTML markup to work in labels, isHtmlLabel must also return true for the given cell.
     *
     *@param cell MxCell
     */
    isWrapping(cell: MxCell);

    /**
     * Function: addCell
     *
     * Adds the cell to the parent and connects it to the given source and
     * target terminals. This is a shortcut method. Returns the cell that was
     * added.
     *
     * Parameters:
     *
     * cell - <mxCell> to be inserted into the given parent.
     * parent - <mxCell> that represents the new parent. If no parent is given then the default parent is used.
     * index - Optional index to insert the cells at. Default is to append.
     * source - Optional <mxCell> that represents the source terminal.
     * target - Optional <mxCell> that represents the target terminal.
     */
    addCell(cell: MxCell, parent: MxCell, index?: number, source?: MxCell, target?: MxCell): MxCell;

    /**
     * Function: addCells
     *
     * Adds the cells to the parent at the given index, connecting each cell to
     * the optional source and target terminal. The change is carried out using
     * <cellsAdded>. This method fires <mxEvent.ADD_CELLS> while the
     * transaction is in progress. Returns the cells that were added.
     *
     * Parameters:
     *
     * cells - Array of <mxCells> to be inserted.
     * parent - <mxCell> that represents the new parent. If no parent is
     * given then the default parent is used.
     * index - Optional index to insert the cells at. Default is to append.
     * source - Optional source <mxCell> for all inserted cells.
     * target - Optional target <mxCell> for all inserted cells.
     */
    addCells(cells: Array<MxCell>, parent: MxCell, index?: number, source?: MxCell, target?: MxCell)

    /**
     * Function: isCellSelectable
     *
     * Returns true if the given cell is selectable. This implementation
     * returns <cellsSelectable>.
     *
     * To add a new style for making cells (un)selectable, use the following code.
     *
     * (code)
     * mxGraph.prototype.isCellSelectable = function(cell)
     * {
     *   var state = this.view.getState(cell);
     *   var style = (state != null) ? state.style : this.getCellStyle(cell);
     *
     *   return this.isCellsSelectable() && !this.isCellLocked(cell) && style['selectable'] != 0;
     * };
     * (end)
     *
     * You can then use the new style as shown in this example.
     *
     * (code)
     * graph.insertVertex(parent, null, 'Hello,', 20, 20, 80, 30, 'selectable=0');
     * (end)
     *
     * Parameters:
     *
     * cell - <mxCell> whose selectable state should be returned.
     */
    isCellSelectable(cell: MxCell): boolean;

    /**
     * Returns true if the overflow portion of labels should be hidden. If this
     * returns true then vertex labels will be clipped to the size of the vertices.
     * This implementation returns true if <mxConstants.STYLE_OVERFLOW> in the
     * style of the given cell is 'hidden'.
     *
     * Parameters:
     *
     * state - <mxCell> whose label should be clipped.
     */
    isLabelClipped(cell: MxCell): boolean;

    /**
     * Specifies if cell sizes should be automatically updated after a label
     * change. This implementation sets <autoSizeCells> to the given parameter.
     * To update the size of cells when the cells are added, set
     * <autoSizeCellsOnAdd> to true.
     *
     * Parameters:
     *
     * value - Boolean indicating if cells should be resized automatically.
     */
    setAutoSizeCells(value: boolean);

    /**
     * Function: getPreferredSizeForCell
     *
     * Returns the preferred width and height of the given <mxCell> as an
     * <mxRectangle>. To implement a minimum width, add a new style eg.
     * minWidth in the vertex and override this method as follows.
     *
     * (code)
     * var graphGetPreferredSizeForCell = graph.getPreferredSizeForCell;
     * graph.getPreferredSizeForCell = function(cell)
     * {
     *   var result = graphGetPreferredSizeForCell.apply(this, arguments);
     *   var style = this.getCellStyle(cell);
     *
     *   if (style['minWidth'] > 0)
     *   {
     *     result.width = Math.max(style['minWidth'], result.width);
     *   }
     *
     *   return result;
     * };
     * (end)
     *
     * Parameters:
     *
     * cell - <mxCell> for which the preferred size should be returned.
     */
    getPreferredSizeForCell(cell: MxCell): MxRectangle;

    /**
     * Function: getAllConnectionConstraints
     *
     * Returns an array of all <mxConnectionConstraints> for the given terminal. If
     * the shape of the given terminal is a <mxStencilShape> then the constraints
     * of the corresponding <mxStencil> are returned.
     *
     * Parameters:
     *
     * terminal - <mxCellState> that represents the terminal.
     * source - Boolean that specifies if the terminal is the source or target.
     */
    getAllConnectionConstraints(terminal, source: boolean);

    /**
     * Function: getTooltipForCell
     *
     * Returns the string or DOM node to be used as the tooltip for the given
     * cell. This implementation uses the cells getTooltip function if it
     * exists, or else it returns <convertValueToString> for the cell.
     *
     * Parameters:
     *
     * cell - <mxCell> whose tooltip should be returned.
     */
    getTooltipForCell(cell: MxCell)

    /**
     * Function: setTooltips
     *
     * Specifies if tooltips should be enabled. This implementation updates
     * <mxTooltipHandler.enabled> in <tooltipHandler>.
     *
     * Parameters:
     *
     * enabled - Boolean indicating if tooltips should be enabled.
     */
    setTooltips(enabled: boolean);

    /**
     * Function: setEnabled
     *
     * Specifies if the graph should allow any interactions. This
     * implementation updates <enabled>.
     *
     * Parameters:
     *
     * value - Boolean indicating if the graph should be enabled.
     */
    setEnabled(value: boolean);

    /**
     * Function: isEventIgnored
     *
     * Returns true if the event should be ignored in <fireMouseEvent>.
     */
    isEventIgnored(evtName, me, sender);

    /**
     * Function: createCellRenderer
     *
     * Creates a new <mxCellRenderer> to be used in this graph.
     */
    createCellRenderer(): MxCellRender;

    /**
     * Function: setCellsSelectable
     *
     * Sets <cellsSelectable>.
     */
    setCellsSelectable(value: boolean);

    /**
     * Function: setCellsMovable
     *
     * Specifies if the graph should allow moving of cells. This implementation
     * updates <cellsMsovable>.
     *
     * Parameters:
     *
     * value - Boolean indicating if the graph should allow moving of cells.
     */
    setCellsMovable(value: boolean)

    addCellOverlay(cell: MxCell, overlay: mxCellOverlay);

    /**
     * Function: getCellOverlays
     *
     * Returns the array of <mxCellOverlays> for the given cell or null, if
     * no overlays are defined.
     *
     * Parameters:
     *
     * cell - <mxCell> whose overlays should be returned.
     */
    getCellOverlays(cell: MxCell);

    /**
     * Function: removeCellOverlays
     *
     * Removes all <mxCellOverlays> from the given cell. This method
     * fires a <removeoverlay> event for each <mxCellOverlay> and returns
     * the array of <mxCellOverlays> that was removed from the cell.
     *
     * Parameters:
     *
     * cell - <mxCell> whose overlays should be removed
     */
    removeCellOverlays(cell: MxCell);


    /**
    * Function: labelChanged
    *
    * sets the label of the specified cell to the given value
    * using cellLabelChanged and fires mxEvent.LABEL_CHANGED while the
    * transaction is in progress.Returns the cell whose label was changed.
    *
    * Parameters
    *
    * cell mxCell whose label should be changed.
    * newValue New label to be assigned.
    * trigger Optional event that triggered the change.
    */

    labelChanged(cell, newValue, trigger);

    addMouseListener(listener);

    removeMouseListener(listener);


    /**
     * Function: getChildVertices
     *
     * Returns the visible child vertices of the given parent.
     *
     * Parameters:
     *
     * parent - <mxCell> whose children should be returned.
     */
    getChildVertices(parent);

    /**
     * Function: getChildEdges
     *
     * Returns the visible child edges of the given parent.
     *
     * Parameters:
     *
     * parent - <mxCell> whose child vertices should be returned.
     */
    getChildEdges(parent);

    /**
     * Function: removeCells
     *
     * Removes the given cells from the graph including all connected edges if
     * includeEdges is true. The change is carried out using <cellsRemoved>.
     * This method fires <mxEvent.REMOVE_CELLS> while the transaction is in
     * progress. The removed cells are returned as an array.
     *
     * Parameters:
     *
     * cells - Array of <mxCells> to remove. If null is specified then the
     * selection cells which are deletable are used.
     * includeEdges - Optional boolean which specifies if all connected edges
     * should be removed as well. Default is true.
     */
    removeCells(cells, includeEdges);

    /**
     * Function: moveCells
     *
     * Moves or clones the specified cells and moves the cells or clones by the
     * given amount, adding them to the optional target cell. The evt is the
     * mouse event as the mouse was released. The change is carried out using
     * <cellsMoved>. This method fires <mxEvent.MOVE_CELLS> while the
     * transaction is in progress. Returns the cells that were moved.
     *
     * Use the following code to move all cells in the graph.
     *
     * (code)
     * graph.moveCells(graph.getChildCells(null, true, true), 10, 10);
     * (end)
     *
     * Parameters:
     *
     * cells - Array of <mxCells> to be moved, cloned or added to the target.
     * dx - Integer that specifies the x-coordinate of the vector. Default is 0.
     * dy - Integer that specifies the y-coordinate of the vector. Default is 0.
     * clone - Boolean indicating if the cells should be cloned. Default is false.
     * target - <mxCell> that represents the new parent of the cells.
     * evt - Mouseevent that triggered the invocation.
     */
    moveCells(cells, dx, dy, clone?, target?, evt?);

    /**
     * Function: cellsMoved
        *
     * Moves the specified cells by the given vector, disconnecting the cells
        * using disconnectGraph is disconnect is true.This method fires
            * <mxEvent.CELLS_MOVED> while the transaction is in progress.
     */
    cellsMoved(cells, dx, dy, disconnect?, constrain?, extend?)

    /**
     * Function: refresh
     *
     * Clears all cell states or the states for the hierarchy starting at the
     * given cell and validates the graph. This fires a refresh event as the
     * last step.
     *
     * Parameters:
     *
     * cell - Optional <mxCell> for which the cell states should be cleared.
     */
    refresh(cell);

    /**
     * Variable: foldingEnabled
     *
     * Specifies if folding (collapse and expand via an image icon in the graph
     * should be enabled). Default is true.
     */
    foldingEnabled: boolean;

    /**
     * Variable: minimumGraphSize
     *
     * <mxRectangle> that specifies the minimum size of the graph. This is ignored
     * if the graph container has no scrollbars. Default is null.
     */
    minimumGraphSize;

    /**
     * Function: translateCell
     *
     * Translates the geometry of the given cell and stores the new,
     * translated geometry in the model as an atomic change.
     */
    translateCell(cell, dx, dy);

    /**
     * Function: getSelectionCell
     *
     * Returns the first cell from the array of selected <mxCells>.
     */
    getSelectionCell();

    /**
    * Function: setDropEnabled
    *
    * Specifies if the graph should allow dropping of cells onto or into other
    * cells.
    */

    setDropEnabled(value);

    /**
     * Function: setSplitEnabled
     *
     * Specifies if the graph should allow dropping of cells onto or into other
     * cells.
     */
    setSplitEnabled(value);

    scrollOnMove: boolean;

    getCells(x, y, width, height, parent, result);

    getTolerance();
    setTolerance(value);
    isMouseDown: boolean;
    getCellAt(x, y, parent?, vertices?, edges?, ignoreFn?);

    dblClick(evt: MouseEvent, cell: MxCell);
    /**
     * Function: orderCells
     *
     * Moves the given cells to the front or back. The change is carried out
     * using <cellsOrdered>. This method fires <mxEvent.ORDER_CELLS> while the
     * transaction is in progress.
     *
     * Parameters:
     *
     * back - Boolean that specifies if the cells should be moved to back.
     * cells - Array of <mxCells> to move to the background. If null is
     * specified then the selection cells are used.
     */
    orderCells(back, cells);

    /**
     * Sets the key to value in the styles of the given cells.  This will modify the existing cell styles in-place and override any existing assignment for the given key.  If no cells are specified, then the selection cells are changed.  If no value is specified, then the respective key is removed * from the styles.
     *
     * Parameters:
     * key - String representing the key to be assigned.
     * value - String representing the new value for the key.
     * cells - Optional array of mxCells to change the style for.  Default is the selection cells.
     */
    setCellStyles(key: string, value: string, cells: MxCell[]): void;

     /**
     * Function: destroy
     *
     * Destroys the graph and all its resources.
     */
    destroy();
}

interface MxGraphFactory {
    new (container: HTMLElement, model: MxGraphModel): MxGraph;
}

declare var mxGraph: MxGraphFactory;

/**
* Class: mxStencilRegistry
*
* A singleton class that provides a registry for stencils and the methods
* for painting those stencils onto a canvas or into a DOM.
*/
declare class mxStencilRegistry{
    /**
    * Function: addStencil
    *
    * Adds the given <mxStencil>.
    */
    static addStencil(name, stencil);
    /**
    * Function: getStencil
    *
    * Returns the <mxStencil> for the given name.
    */
    static getStencil(name);
}

/**
* Copyright (c) 2006-2013, JGraph Ltd
*/
/**
* Class: mxStencil
*
* Implements a generic shape which is based on a XML node as a description.
* The node contains a background and a foreground node, which contain the
* definition to render the respective part of the shape. Note that the
* fill, stroke or fillstroke of the background is be the first statement
* of the foreground. This is because the content of the background node
* maybe used to not only render the shape itself, but also its shadow and
* other elements which do not require a fill, stroke or fillstroke.
*
* The shape uses a coordinate system with a width of 100 and a height of
* 100 by default. This can be changed by setting the w and h attribute of
* the shape element. The aspect attribute can be set to "variable" (default)
* or "fixed". If fixed is used, then the aspect which is defined via the w
* and h attribute is kept constant while the shape is scaled.
*
* The possible contents of the background and foreground elements are rect,
* ellipse, roundrect, text, image, include-shape or paths. A path element
* contains move, line, curve, quad, arc and close elements. The rect, ellipse
* and roundrect elements may be thought of as special path elements. All these
* path elements must be followed by either fill, stroke or fillstroke (note
* that text, image and include-shape or not path elements).
*
* The background element can be empty or contain at most one path element. It
* should not contain a text, image or include-shape element. If the background
* element is empty, then no shadow or glass effect will be rendered. If the
* background element is non-empty, then the corresponding fill, stroke or
* fillstroke should be the first element in the subsequent foreground element.
*
* The format of the XML is "a simplified HTML 5 Canvas". Each command changes
* the "current" state, so eg. a linecap, linejoin will be used for all
* subsequent line drawing, unless a save/restore appears, which saves/restores
* a state in a stack.
*
* The connections section contains the fixed connection points for a stencil.
* The perimeter attribute of the constraint element should have a value of 0
* or 1 (default), where 1 (true) specifies that the given point should be
* projected into the perimeter of the given shape.
*
* The x- and y-coordinates are typically between 0 and 1 and define the
* location of the connection point relative to the width and height of the
* shape.
*
* The dashpattern directive sets the current dashpattern. The format for the
* pattern attribute is a space-separated sequence of numbers, eg. 5 5 5 5,
* that specifies the lengths of alternating dashes and spaces in dashed lines.
* The dashpattern should be used together with the dashed directive to
* enabled/disable the dashpattern. The default dashpattern is 3 3.
*
* The strokewidth attribute defines a strokewidth behaviour for the shape. It
* can contain a numeric value or the keyword "inherit", which means that the
* strokeWidth of the cell is only changed on scaling, not on resizing.
* If numeric values are used, the strokeWidth of the cell is changed on both
* scaling and resizing and the value defines the multiple that is applied to
* the width.
*
* To support i18n in the text element, use the localized attribute of 1 to use
* the str as a key in <mxResources.get>. To handle all str attributes of all
* text nodes like this, set the <mxStencil.defaultLocalized> value to true.
*
* Constructor: mxStencil
*
* Constructs a new generic shape by setting <desc> to the given XML node and
* invoking <parseDescription> and <parseConstraints>.
*
* Parameters:
*
* desc - XML node that contains the stencil description.
*/
declare class mxStencil {

    /*
    * Constructor: mxStencil
    *
    * Constructs a new generic shape by setting < desc > to the given XML node and
    * invoking < parseDescription > and<parseConstraints>.
    *
    * Parameters:
    *
    * desc - XML node that contains the stencil description.
    */
    constructor(desc)
    /**
    * Variable: defaultLocalized
    *
    * Static global variable that specifies the default value for the localized
    * attribute of the text element. Default is false.
    */
    defaultLocalized;

    /**
    * Function: allowEval
    *
    * Static global switch that specifies if the use of eval is allowed for
    * evaluating text content. Default is true. Set this to false if stencils may
    * contain user input (see the section on security in the manual).
    */
    allowEval;

    /**
    * Variable: desc
    *
    * Holds the XML node with the stencil description.
    */
    desc;

    /**
    * Variable: constraints
    *
    * Holds an array of <mxConnectionConstraints> as defined in the shape.
    */
    constraints;

    /**
    * Variable: aspect
    *
    * Holds the aspect of the shape. Default is 'auto'.
    */
    aspect;

    /**
    * Variable: w0
    *
    * Holds the width of the shape. Default is 100.
    */
    w0;

    /**
    * Variable: h0
    *
    * Holds the height of the shape. Default is 100.
    */
    h0;

    /**
    * Variable: bgNodes
    *
    * Holds the XML node with the stencil description.
    */
    bgNode;

    /**
    * Variable: fgNodes
    *
    * Holds the XML node with the stencil description.
    */
    fgNode;

    /**
    * Variable: strokewidth
    *
    * Holds the strokewidth direction from the description.
    */
    strokewidth;

    /**
    * Function: parseDescription
    *
    * Reads <w0>, <h0>, <aspect>, <bgNodes> and <fgNodes> from <desc>.
    */
    parseDescription();

    /**
    * Function: parseConstraints
    *
    * Reads the constraints from <desc> into <constraints> using
    * <parseConstraint>.
    */
    parseConstraints();

    /**
    * Function: parseConstraint
    *
    * Parses the given XML node and returns its <mxConnectionConstraint>.
    */
    parseConstraint(node);

    /**
    * Function: evaluateTextAttribute
    *
    * Gets the given attribute as a text. The return value from <evaluateAttribute>
    * is used as a key to <mxResources.get> if the localized attribute in the text
    * node is 1 or if <defaultLocalized> is true.
    */
    evaluateTextAttribute(node, attribute, state);
    /**
    * Function: evaluateAttribute
    *
    * Gets the attribute for the given name from the given node. If the attribute
    * does not exist then the text content of the node is evaluated and if it is
    * a function it is invoked with <state> as the only argument and the return
    * value is used as the attribute value to be returned.
    */
    evaluateAttribute(node, attribute, shape);
    /**
    * Function: drawShape
    *
    * Draws this stencil inside the given bounds.
    */
    drawShape(canvas, shape, x, y, w, h);
    /**
    * Function: drawShape
    *
    * Draws this stencil inside the given bounds.
    */
    drawChildren(canvas, shape, x, y, w, h, node, disableShadow);
    /**
    * Function: computeAspect
    *
    * Returns a rectangle that contains the offset in x and y and the horizontal
    * and vertical scale in width and height used to draw this shape inside the
    * given <mxRectangle>.
    *
    * Parameters:
    *
    * shape - <mxShape> to be drawn.
    * bounds - <mxRectangle> that should contain the stencil.
    * direction - Optional direction of the shape to be darwn.
    */
    computeAspect(shape, x, y, w, h, direction);

    /**
    * Function: drawNode
    *
    * Draws this stencil inside the given bounds.
    */
    drawNode(canvas, shape, node, aspect, disableShadow);
}

/**
* Renders cells into a document object model.
*/
interface MxCellRender {
    /**
    * Returns the bounds to be used to draw the control (folding icon) of the given state.
    *
    * @param state Boolean that specifies the visible state to be assigned.
    * @param w Boolean that specifies the visible state to be assigned.
    * @param h Boolean that specifies the visible state to be assigned.
    */
    getControlBounds(state: any, w: number, h: number);

    initControl(state, control, handleEvents, clickHandler);

    installCellOverlayListeners(state, overlay, shape);

    getLabelValue (state);
}

/**
* Implements a layout manager that runs a given layout after any changes to the graph
*/
interface MxLayoutManager {
    /**
    * Returns the layout to be executed for the given graph and parent.
    */
    getLayout(parent: MxCell): any;
}

interface MxLayoutManagerFactory {
    new (graph: MxGraph): MxLayoutManager
}

declare var mxLayoutManager: MxLayoutManagerFactory;

interface MxCellFactory {
    new (value?: any, geometry?: MxGeometry, style?: string): MxCell
}

/**
* Cells are the elements of the graph model.  They represent the state of the groups, vertices and edges in a graph.
*/
interface MxCell {
    /**
    * Holds the edges.
    */
    edges: Array<MxCell>;
    /**
    * Returns the number of child cells.
    */
    getChildCount(): number;
    /**
     * Function: getParent
     *
     * Returns the cell's parent.
     */
    getParent();
    /**
     * Function: setParent
     *
     * Sets the parent cell.
     *
     * Parameters:
     *
     * parent - <mxCell> that represents the new parent.
     */
    setParent(parent: MxCell);
    /**
    * Specifies whether the cell is connectable.  Default is true.
    */
    /**
     * Function: getIndex
     *
     * Returns the index of the specified child in the child array.
     *
     * Parameters:
     *
     * child - Child whose index should be returned.
     */
    getIndex(child: MxCell);

    connectable: boolean;
    /**
    * Reference to the source terminal.
    */
    source: MxCell;
    /**
    * Reference to the target terminal.
    */
    target: MxCell;
    /**
    * Holds the user object. Default is null.
    */
    value: any;
    /**
    * Returns true if the cell is an edge.
    */
    isEdge(): boolean;
    /**
     * Function: setEdge
     *
     * Specifies if the cell is an edge. This should only be assigned at
     * construction of the cell and not be changed during its lifecycle.
     *
     * Parameters:
     *
     * edge - Boolean that specifies if the cell is an edge.
     */
    setEdge(edge);
    /**
    * Returns true if the cell is a vertex.
    */
    isVertex(): boolean;
    /**
    * Holds the mxGeometry.  Default is null.
    */
    geometry: MxGeometry;
    /**
    * Returns the mxGeometry that describes the geometry.
    */
    getGeometry(): MxGeometry;
    /**
    * Sets the mxGeometry to be used as the geometry.
    *
    * @param geometry mxGeometry
    */
    setGeometry(geometry: MxGeometry): void;
    /**
    * Returns a string that describes the style.
    */
    getStyle(): string;
    /**
    * Sets the string to be used as the style.
    *
    * @param style string
    */
    setStyle(style: string): void;
    /**
    * Specifies whether the cell is a vertex.  Default is false.
    */
    vertex: boolean;
    /**
    * Inserts the specified child into the child array at the specified index and updates the parent reference of the child.
    * If not childIndex is specified then the child is appended to the child array.
    * Returns the inserted child.
    */
    insert(mxCell: MxCell, index?: number): MxCell;

    getTooltip(): any;

    children: Array<MxCell>;

    isVisible();

    setVisible(visible);

    insertEdge(edge, isOutgoing);

    removeEdge(edge, isOutgoing);
    setValue(value);
}

interface MxImage {
}

interface MxImageFactory {
    new (src: string, width: number, height: number): MxImage
}

declare var mxImage: MxImageFactory;

interface MxGraphSelectionModel {
    addListener(name: string, fn: any);
    /**
     * Function: clear
     *
     * Clears the selection and fires a <change> event if the selection was not
     * empty.
     */
    clear();
    addCells(cells: MxCell[]);
    setSingleSelection(value: boolean);
    addCells(cells);
}


interface MxGraphModel extends mxEventSource{
    /**
     * Increments the updateLevel by one.  The event notification is queued until updateLevel reaches 0 by use of endUpdate.
     * All changes on mxGraphModel are transactional, that is, they are executed in a single undoable change on the model (without transaction isolation).
     *
     */
    beginUpdate(): void;
    /**
    * Decrements the updateLevel by one and fires an <undo> event if the updateLevel reaches 0.
    * This function indirectly fires a <change> event by invoking the notify function on the currentEdit und then creates a new currentEdit using createUndoableEdit.
    */
    endUpdate(): void;
    /**
    * Sets the collapsed state of the given mxCell using mxCollapseChange and adds the change to the current transaction..
    *
    * @param cell	mxCell whose collapsed state should be changed.
    * @param collapsed	Boolean that specifies the new collpased state.
    */
    setCollapsed(cell: MxCell, collapsed: boolean);

    /**
     * Returns the incoming edges of the given cell without loops.
     *
     * @param cell	mxCell whose incoming edges should be returned.
     */
    getIncomingEdges(cell: MxCell): Array<MxCell>;

    /**
     * Returns the outgoing edges of the given cell without loops.
     *
     * @param cell	mxCell whose outgoing edges should be returned.
     */
    getOutgoingEdges(cell: MxCell): Array<MxCell>;

    /**
     * Returns the style of the given mxCell.
     *
     * @param cell mxCell whose style should be returned.
     */
    getStyle(cell: MxCell): any;

    /**
     * Returns the root of the model or the topmost parent of the given cell.
     *
     * @param cell Optional mxCell that specifies the child.
     */
    getRoot(cell?: MxCell): any; //todo

    /**
     * Returns the <mxGeometry> of the given <mxCell>.
     *
     * Parameters:
     *
     * cell - <mxCell> whose geometry should be returned.
     */
    getGeometry(cell: MxCell): MxGeometry;

    /**
     * Function: getChildCount
     *
     * Returns the number of children in the given cell.
     *
     * Parameters:
     *
     * cell - <mxCell> whose number of children should be returned.
     */
    getChildCount(cell);

    /**
     * Returns the cell for the specified Id or null if no cell can be found for the given Id.
     * Parameters:
     * id - A string representing the Id of the cell.
     * Returns:
     * Returns the cell for the given Id.
     */
    getCell(id: string): any;

    /**
     * Function: getConnections
     *
     * Returns all edges of the given cell without loops.
     *
     * Parameters:
     *
     * cell - <mxCell> whose edges should be returned.
     *
     */
    getConnections(cell): any[];

    /**
     * Function: setVisible
     *
     * Sets the visible state of the given <mxCell> using <mxVisibleChange> and
     * adds the change to the current transaction.
     *
     * Parameters:
     *
     * cell - <mxCell> whose visible state should be changed.
     * visible - Boolean that specifies the new visible state.
     */
    setVisible(cell, visible);

    /**
     * Function: createUndoableEdit
     *
     * Creates a new <mxUndoableEdit> that implements the
     * notify function to fire a <change> and <notify> event
     * through the <mxUndoableEdit>'s source.
     */
    createUndoableEdit();

    /**
     * Function: getParent
     *
     * Returns the parent of the given cell.
     *
     * Parameters:
     *
     * cell - <mxCell> whose parent should be returned.
     */
    getParent(cell);

    /**
     * Function: remove
     *
     * Removes the specified cell from the model using <mxChildChange> and adds
     * the change to the current transaction. This operation will remove the
     * cell and all of its children from the model. Returns the removed cell.
     *
     * Parameters:
     *
     * cell - <mxCell> that should be removed.
     */
    remove(cell);

    /**
     * Function: add
     *
     * Adds the specified child to the parent at the given index using
     * <mxChildChange> and adds the change to the current transaction. If no
     * index is specified then the child is appended to the parent's array of
     * children. Returns the inserted child.
     *
     * Parameters:
     *
     * parent - <mxCell> that specifies the parent to contain the child.
     * child - <mxCell> that specifies the child to be inserted.
     * index - Optional integer that specifies the index of the child.
     */
    add(parent, child, index);

    /**
     * Function: clear
     *
     * Sets a new root using <createRoot>.
     */
    clear();
}

interface MxGraphLayout {
    /**
    * Executes the layout algorithm for the children of the given parent.
    *
    * @param parent mxCell whose children should be layed out.
    */
    execute(parent: MxCell): void;
}

interface MxGraphLayoutFactory {
    new (graph: MxGraph)
}

interface MxGraphView {
    /**
    * Specifies if the style should be updated in each validation step.
    * If this is false then the style is only updated if the state is created or if the style of the cell was changed.
    * Default is false.
    */
    updateStyle: boolean;
    /**
    * Returns the mxCellState for the given cell. If create is true, then the state is created if it does not yet exist.
    *
    * @param cell mxCell for which the mxCellState should be returned.
    * @param create Optional boolean indicating if a new state should be created if it does not yet exist. Default is false.
    */
    getState(cell: MxCell, create?: boolean): MxCellState;

    /**
     * Function: getOverlayPane
     *
     * Returns the DOM node that represents the layer above the drawing layer.
     */
    getOverlayPane(): HTMLElement;
    /**
     * Function: getScale
     *
     * Returns the <scale>.
     */
    getScale(): number;

    /**
     * Removes the state of the given cell and all descendants if the given cell is not the current root.
     * cell         Optional mxCell for which the state should be removed.  Default is the root of the model.
     * force        Boolean indicating if the current root should be ignored for recursion.
     */
    clear(cell?, force?, recurse?);

    /**
     * Calls validateCell and validateCellState and updates the graphBounds using getBoundingBox.  Finally the background is validated using validateBackground.
     * cell         Optional mxCell to be used as the root of the validation.  Default is currentRoot or the root of the model.
     */
    validate(cell?);

    /* Updates the offset of labels on edges.
    */
    updateEdgeLabelOffset(state: MxCellState);
}

interface MxDivResizer {
    /**
    * Hook for subclassers to return the width of the document (without scrollbars).
    */
    getDocumentWidth(): number;
    /**
    * Hook for subclassers to return the height of the document (without scrollbars).
    */
    getDocumentHeight(): number;
    /**
    * Updates the style of the DIV after the window has been resized.
    */
    resize(): void;
}

interface MxDivResizerFactory {
    /**
    * Constructs an object that maintains the size of a div element when the window is being resized.
    * This is only required for Internet Explorer as it ignores the respective stylesheet information for DIV elements.
    *
    * @param div Reference to the DOM node whose size should be maintained.
    * @param container	Optional Container that contains the div.  Default is the window.
    */
    new (div: HTMLElement, container?: HTMLElement): MxDivResizer
}
declare var mxDivResizer: MxDivResizer;

interface MxGeometry extends MxRectangle {
    /**
     * Stores alternate values for x, y, width and height in a rectangle. See
     * <swap> to exchange the values. Default is null.
     */
    alternateBounds: MxRectangle;

    /**
     * Defines the source <mxPoint> of the edge. This is used if the
     * corresponding edge does not have a source vertex. Otherwise it is
     * ignored. Default is  null.
     */
    sourcePoint: MxPoint;

    /**
     * Defines the target <mxPoint> of the edge. This is used if the
     * corresponding edge does not have a target vertex. Otherwise it is
     * ignored. Default is null.
     */
    targetPoint: MxPoint;

    /**
     * Array of <mxPoints> which specifies the control points along the edge.
     * These points are the intermediate points on the edge, for the endpoints
     * use <targetPoint> and <sourcePoint> or set the terminals of the edge to
     * a non-null value. Default is null.
     */
    points: Array<MxPoint>;

    /**
     * For edges, this holds the offset (in pixels) from the position defined
     * by <x> and <y> on the edge. For relative geometries (for vertices), this
     * defines the absolute offset from the point defined by the relative
     * coordinates. For absolute geometries (for vertices), this defines the
     * offset for the label. Default is null.
     */
    offset: MxPoint;

    /**
     * Specifies if the coordinates in the geometry are to be interpreted as
     * relative coordinates. For edges, this is used to define the location of
     * the edge label relative to the edge as rendered on the display. For
     * vertices, this specifies the relative location inside the bounds of the
     * parent cell.
     *
     * If this is false, then the coordinates are relative to the origin of the
     * parent cell or, for edges, the edge label position is relative to the
     * center of the edge as rendered on screen.
     *
     * Default is false.
     */
    relative: boolean;

    /**
     * Swaps the x, y, width and height with the values stored in
     * <alternateBounds> and puts the previous values into <alternateBounds> as
     * a rectangle. This operation is carried-out in-place, that is, using the
     * existing geometry instance. If this operation is called during a graph
     * model transactional change, then the geometry should be cloned before
     * calling this method and setting the geometry of the cell using
     * <mxGraphModel.setGeometry>.
     */
    swap();

    /**
     * Returns the <mxPoint> representing the source or target point of this
     * edge. This is only used if the edge has no source or target vertex.
     *
     * Parameters:
     *
     * isSource - Boolean that specifies if the source or target point
     * should be returned.
     */
    getTerminalPoint(isSource: boolean);

    /**
     * Function: setTerminalPoint
     *
     * Sets the <sourcePoint> or <targetPoint> to the given <mxPoint> and
     * returns the new point.
     *
     * Parameters:
     *
     * point - Point to be used as the new source or target point.
     * isSource - Boolean that specifies if the source or target point
     * should be set.
     */
    setTerminalPoint(point: MxPoint, isSource: boolean);

    /**
     * Function: rotate
     *
     * Rotates the geometry by the given angle around the given center. That is,
     * <x> and <y> of the geometry, the <sourcePoint>, <targetPoint> and all
     * <points> are translated by the given amount. <x> and <y> are only
     * translated if <relative> is false.
     *
     * Parameters:
     *
     * angle - Number that specifies the rotation angle in degrees.
     * cx - <mxPoint> that specifies the center of the rotation.
     */
    rotatefunction(angle: number, cx: MxPoint);

    /**
     * Function: translate
     *
     * Translates the geometry by the specified amount. That is, <x> and <y> of the
     * geometry, the <sourcePoint>, <targetPoint> and all <points> are translated
     * by the given amount. <x> and <y> are only translated if <relative> is false.
     * If <TRANSLATE_CONTROL_POINTS> is false, then <points> are not modified by
     * this function.
     *
     * Parameters:
     *
     * dx - Number that specifies the x-coordinate of the translation.
     * dy - Number that specifies the y-coordinate of the translation.
     */
    translate(dx: number, dy: number) ;

    /**
     * Scales the geometry by the given amount. That is, <x> and <y> of the
     * geometry, the <sourcePoint>, <targetPoint> and all <points> are scaled
     * by the given amount. <x>, <y>, <width> and <height> are only scaled if
     * <relative> is false. If <fixedAspect> is true, then the smaller value
     * is used to scale the width and the height.
     *
     * Parameters:
     *
     * sx - Number that specifies the horizontal scale factor.
     * sy - Number that specifies the vertical scale factor.
     * fixedAspect - Optional boolean to keep the aspect ratio fixed.
     */
    scale(sx: number, sy: number, fixedAspect?: boolean);

    /**
     * Returns true if the given object equals this geometry.
     */
    equals(obj: any);
}

interface MxRectangle extends MxPoint {

    /**
    * Variable: width
    *
    * Holds the width of the rectangle. Default is 0.
    */
    width;

    /**
     * Variable: height
     *
     * Holds the height of the rectangle. Default is 0.
     */
    height;

    /**
     * Function: setRect
     *
     * Sets this rectangle to the specified values
     */
    setRect(x: number, y: number, w: number, h: number);

    /**
     * Function: getCenterX
     *
     * Returns the x-coordinate of the center point.
     */
    getCenterX();

    /**
     * Function: getCenterY
     *
     * Returns the y-coordinate of the center point.
     */
    getCenterY();

    /**
     * Function: add
     *
     * Adds the given rectangle to this rectangle.
     */
    add(rect: MxRectangle);

    /**
     * Function: grow
     *
     * Grows the rectangle by the given amount, that is, this method subtracts
     * the given amount from the x- and y-coordinates and adds twice the amount
     * to the width and height.
     */
    grow(amount: number);

    /**
     * Function: getPoint
     *
     * Returns the top, left corner as a new <mxPoint>.
     */
    getPoint();

    /**
     * Function: rotate90
     *
     * Rotates this rectangle by 90 degree around its center point.
     */
    rotate90();

    /**
     * Function: equals
     *
     * Returns true if the given object equals this rectangle.
     */
    equals(obj: any);

    /**
     * Function: fromRectangle
     *
     * Returns a new <mxRectangle> which is a copy of the given rectangle.
     */
    fromRectangle(rect: MxRectangle);
}

interface MxRectangleFactory {
    /**
    * Constructs a new rectangle for the optional parameters.  If no parameters are given then the respective default values are used.
    *
    * @param x Holds the x of the rectangle.  Default is 0.
    * @param y Holds the y of the rectangle.  Default is 0.
    * @param width Holds the width of the rectangle.  Default is 0.
    * @param height Holds the height of the rectangle.  Default is 0.
    */
    new (x?: number, y?: number, width?: number, height?: number): MxRectangle;
}

interface MxPoint {
    /**
    * Holds the x-coordinate of the point.
    * Default is 0.
    */
    x: number;

    /**
    * Holds the y-coordinate of the point.
    * Default is 0.
    */
    y: number;

    /**
     * Returns true if the given object equals this point.
     */
    equals(obj): boolean;

    /**
     * Returns a clone of this <mxPoint>.
     */
    clone(): MxPoint;

}

interface MxPointFactory {
    new (x: number, y: number): MxPoint
}

interface MxCellState extends MxRectangle {
    view: MxGraphView;
    style: any;
    cell: MxCell;
    /* The offset to display the labels for the MxCell
    */
    absoluteOffset: MxPoint;
    absolutePoints: MxPoint[];
}

declare var mxCellState: MxCellState;

interface MxConstants {
    DEFAULT_HOTSPOT: string;//Defines the portion of the cell which is to be used as a connectable region.
    MIN_HOTSPOT_SIZE: string;//	Defines the minimum size in pixels of the portion of the cell which is to be used as a connectable region.
    MAX_HOTSPOT_SIZE: string;//	Defines the maximum size in pixels of the portion of the cell which is to be used as a connectable region.
    RENDERING_HINT_EXACT: string;//	Defines the exact rendering hint.
    RENDERING_HINT_FASTER: string;//	Defines the faster rendering hint.
    RENDERING_HINT_FASTEST: string;//	Defines the fastest rendering hint.
    DIALECT_SVG: string;//	Defines the SVG display dialect name.
    DIALECT_VML: string;//	Defines the VML display dialect name.
    DIALECT_MIXEDHTML: string;//	Defines the mixed HTML display dialect name.
    DIALECT_PREFERHTML: string;//	Defines the preferred HTML display dialect name.
    DIALECT_STRICTHTML: string;//	Defines the strict HTML display dialect.
    NS_SVG: string;//	Defines the SVG namespace.
    NS_XHTML: string;//	Defines the XHTML namespace.
    NS_XLINK: string;//	Defines the XLink namespace.
    SHADOWCOLOR: string;//	Defines the color to be used to draw shadows in shapes and windows.
    SHADOW_OFFSET_X: string;//	Specifies the x-offset of the shadow.
    SHADOW_OFFSET_Y: string;//	Specifies the y - offset of the shadow.
    SHADOW_OPACITY: string;//	Defines the opacity for shadows.
    NODETYPE_ELEMENT: number;//	DOM node of type ELEMENT.
    NODETYPE_ATTRIBUTE: string;//	DOM node of type ATTRIBUTE.
    NODETYPE_TEXT: string;//	DOM node of type TEXT.
    NODETYPE_CDATA: string;//	DOM node of type CDATA.
    NODETYPE_ENTITY_REFERENCE: string;//	DOM node of type ENTITY_REFERENCE.
    NODETYPE_ENTITY: string;//	DOM node of type ENTITY.
    NODETYPE_PROCESSING_INSTRUCTION: string;//	DOM node of type PROCESSING_INSTRUCTION.
    NODETYPE_COMMENT: string;//	DOM node of type COMMENT.
    NODETYPE_DOCUMENT: string;//	DOM node of type DOCUMENT.
    NODETYPE_DOCUMENTTYPE: string;//	DOM node of type DOCUMENTTYPE.
    NODETYPE_DOCUMENT_FRAGMENT: string;//	DOM node of type DOCUMENT_FRAGMENT.
    NODETYPE_NOTATION: string;//	DOM node of type NOTATION.
    TOOLTIP_VERTICAL_OFFSET: string;//	Defines the vertical offset for the tooltip.
    DEFAULT_VALID_COLOR: string;//	Specifies the default valid color.
    DEFAULT_INVALID_COLOR: string;//	Specifies the default invalid color.
    OUTLINE_HIGHLIGHT_COLOR: string;//	Specifies the default highlight color for shape outlines.
    HIGHLIGHT_STROKEWIDTH: string;//	Defines the strokewidth to be used for the highlights.
    CURSOR_MOVABLE_VERTEX: string;//	Defines the cursor for a movable vertex.
    CURSOR_MOVABLE_EDGE: string;//	Defines the cursor for a movable edge.
    CURSOR_LABEL_HANDLE: string;//	Defines the cursor for a movable label.
    CURSOR_TERMINAL_HANDLE: string;//	Defines the cursor for a terminal handle.
    CURSOR_BEND_HANDLE: string;//	Defines the cursor for a movable bend.
    CURSOR_VIRTUAL_BEND_HANDLE: string;//	Defines the cursor for a movable bend.
    CURSOR_CONNECT: string;//	Defines the cursor for a connectable state.
    HIGHLIGHT_COLOR: string;//	Defines the color to be used for the cell highlighting.
    TARGET_HIGHLIGHT_COLOR: string;//	Defines the color to be used for highlighting a target cell for a new or changed connection.
    INVALID_CONNECT_TARGET_COLOR: string;//	Defines the color to be used for highlighting a invalid target cells for a new or changed connections.
    DROP_TARGET_COLOR: string;//	Defines the color to be used for the highlighting target parent cells (for drag and drop).
    VALID_COLOR: string;//	Defines the color to be used for the coloring valid connection previews.
    INVALID_COLOR: string;//	Defines the color to be used for the coloring invalid connection previews.
    EDGE_SELECTION_COLOR: string;//	Defines the color to be used for the selection border of edges.
    VERTEX_SELECTION_COLOR: string;//	Defines the color to be used for the selection border of vertices.
    VERTEX_SELECTION_STROKEWIDTH: number;//	Defines the strokewidth to be used for vertex selections.
    /**
	 * Defines the dashed state to be used for the vertex selection
	 * border. Default is true.
	 */
    VERTEX_SELECTION_DASHED: boolean;
    EDGE_SELECTION_STROKEWIDTH: number;//	Defines the strokewidth to be used for edge selections.
    /**
	 * Defines the dashed state to be used for the edge selection
	 * border. Default is true.
	 */
    EDGE_SELECTION_DASHED: boolean;
    SELECTION_DASHED: string;//	Defines the dashed state to be used for the vertex selection border.
    GUIDE_COLOR: string;//	Defines the color to be used for the guidelines in mxGraphHandler.
    GUIDE_STROKEWIDTH: string;//	Defines the strokewidth to be used for the guidelines in mxGraphHandler.
    OUTLINE_COLOR: string;//	Defines the color to be used for the outline rectangle border.
    OUTLINE_STROKEWIDTH: string;//	Defines the strokewidth to be used for the outline rectangle stroke width.
    HANDLE_SIZE: string;//	Defines the default size for handles.
    LABEL_HANDLE_SIZE: string;//	Defines the default size for label handles.
    HANDLE_FILLCOLOR: string;//	Defines the color to be used for the handle fill color.
    HANDLE_STROKECOLOR: string;//	Defines the color to be used for the handle stroke color.
    LABEL_HANDLE_FILLCOLOR: string;//	Defines the color to be used for the label handle fill color.
    CONNECT_HANDLE_FILLCOLOR: string;//	Defines the color to be used for the connect handle fill color.
    LOCKED_HANDLE_FILLCOLOR: string;//	Defines the color to be used for the locked handle fill color.
    OUTLINE_HANDLE_FILLCOLOR: string;//	Defines the color to be used for the outline sizer fill color.
    OUTLINE_HANDLE_STROKECOLOR: string;//	Defines the color to be used for the outline sizer stroke color.
    DEFAULT_FONTFAMILY: string;//	Defines the default family for all fonts in points.
    DEFAULT_FONTSIZE: number;//	Defines the default size for all fonts in points.
    LINE_HEIGHT: string;//	Defines the default line height for text labels.
    ABSOLUTE_LINE_HEIGHT: string;//	Specifies if absolute line heights should be used(px) in CSS.
    DEFAULT_FONTSTYLE: number;//	Defines the default style for all fonts.
    DEFAULT_STARTSIZE: string;//	Defines the default start size for swimlanes.
    DEFAULT_MARKERSIZE: string;//	Defines the default size for all markers.
    DEFAULT_IMAGESIZE: string;//	Defines the default width and height for images used in the label shape.
    ENTITY_SEGMENT: string;//	Defines the length of the horizontal segment of an Entity Relation.
    RECTANGLE_ROUNDING_FACTOR: string;//	Defines the rounding factor for rounded rectangles in percent between 0 and 1.
    LINE_ARCSIZE: string;//	Defines the size of the arcs for rounded edges.
    ARROW_SPACING: string;//	Defines the spacing between the arrow shape and its terminals.
    ARROW_WIDTH: string;//	Defines the width of the arrow shape.
    ARROW_SIZE: string;//	Defines the size of the arrowhead in the arrow shape.
    PAGE_FORMAT_A4_PORTRAIT: string;//	Defines the rectangle for the A4 portrait page format.
    PAGE_FORMAT_LETTER_PORTRAIT: string;//	Defines the rectangle for the Letter portrait page format.
    NONE: string;//	Defines the value for none.
    STYLE_PERIMETER: string;//	Defines the key for the perimeter style.
    STYLE_SOURCE_PORT: string;//	Defines the ID of the cell that should be used for computing the perimeter point of the source for an edge.
    STYLE_TARGET_PORT: string;//	Defines the ID of the cell that should be used for computing the perimeter point of the target for an edge.
    STYLE_PORT_CONSTRAINT: string;//	Defines the direction(s) that edges are allowed to connect to cells in.
    STYLE_PORT_CONSTRAINT_ROTATION: string;//	Define whether port constraint directions are rotated with vertex rotation.
    STYLE_OPACITY: string;//	Defines the key for the opacity style.
    STYLE_TEXT_OPACITY: string;//	Defines the key for the text opacity style.
    STYLE_OVERFLOW: string;//	Defines the key for the overflow style.
    STYLE_ORTHOGONAL: string;//	Defines if the connection points on either end of the edge should be computed so that the edge is vertical or horizontal if possible and if the point is not at a fixed location.
    STYLE_EXIT_X: string;//	Defines the key for the horizontal relative coordinate connection point of an edge with its source terminal.
    STYLE_EXIT_Y: string;//	Defines the key for the vertical relative coordinate connection point of an edge with its source terminal.
    STYLE_EXIT_PERIMETER: string;//	Defines if the perimeter should be used to find the exact entry point along the perimeter of the source.
    STYLE_ENTRY_X: string;//	Defines the key for the horizontal relative coordinate connection point of an edge with its target terminal.
    STYLE_ENTRY_Y: string;//	Defines the key for the vertical relative coordinate connection point of an edge with its target terminal.
    STYLE_ENTRY_PERIMETER: string;//	Defines if the perimeter should be used to find the exact entry point along the perimeter of the target.
    STYLE_WHITE_SPACE: string;//	Defines the key for the white - space style.
    STYLE_ROTATION: string;//	Defines the key for the rotation style.
    STYLE_FILLCOLOR: string;//	Defines the key for the fill color.
    STYLE_SWIMLANE_FILLCOLOR: string;//	Defines the key for the fill color of the swimlane background.
    STYLE_MARGIN: string;//	Defines the key for the margin between the ellipses in the double ellipse shape.
    STYLE_GRADIENTCOLOR: string;//	Defines the key for the gradient color.
    STYLE_GRADIENT_DIRECTION: string;//	Defines the key for the gradient direction.
    STYLE_STROKECOLOR: string;//	Defines the key for the strokeColor style.
    STYLE_SEPARATORCOLOR: string;//	Defines the key for the separatorColor style.
    STYLE_STROKEWIDTH: string;//	Defines the key for the strokeWidth style.
    STYLE_ALIGN: string;//	Defines the key for the align style.
    STYLE_VERTICAL_ALIGN: string;//	Defines the key for the verticalAlign style.
    STYLE_LABEL_WIDTH: string;//	Defines the key for the width of the label if the label position is not center.
    STYLE_LABEL_POSITION: string;//	Defines the key for the horizontal label position of vertices.
    STYLE_VERTICAL_LABEL_POSITION: string;//	Defines the key for the vertical label position of vertices.
    STYLE_IMAGE_ASPECT: string;//	Defines the key for the image aspect style.
    STYLE_IMAGE_ALIGN: string;//	Defines the key for the align style.
    STYLE_IMAGE_VERTICAL_ALIGN: string;//	Defines the key for the verticalAlign style.
    STYLE_GLASS: string;//	Defines the key for the glass style.
    STYLE_IMAGE: string;//	Defines the key for the image style.
    STYLE_IMAGE_WIDTH: string;//	Defines the key for the imageWidth style.
    STYLE_IMAGE_HEIGHT: string;//	Defines the key for the imageHeight style.
    STYLE_IMAGE_BACKGROUND: string;//	Defines the key for the image background color.
    STYLE_IMAGE_BORDER: string;//	Defines the key for the image border color.
    STYLE_FLIPH: string;//	Defines the key for the horizontal image flip.
    STYLE_FLIPV: string;//	Defines the key for the vertical flip.
    STYLE_NOLABEL: string;//	Defines the key for the noLabel style.
    STYLE_NOEDGESTYLE: string;//	Defines the key for the noEdgeStyle style.
    STYLE_LABEL_BACKGROUNDCOLOR: string;//	Defines the key for the label background color.
    STYLE_LABEL_BORDERCOLOR: string;//	Defines the key for the label border color.
    STYLE_LABEL_PADDING: string;//	Defines the key for the label padding, ie.
    STYLE_INDICATOR_SHAPE: string;//	Defines the key for the indicator shape used within an mxLabel.
    STYLE_INDICATOR_IMAGE: string;//	Defines the key for the indicator image used within an mxLabel.
    STYLE_INDICATOR_COLOR: string;//	Defines the key for the indicatorColor style.
    STYLE_INDICATOR_STROKECOLOR: string;//	Defines the key for the indicator stroke color in mxLabel.
    STYLE_INDICATOR_GRADIENTCOLOR: string;//	Defines the key for the indicatorGradientColor style.
    STYLE_INDICATOR_SPACING: string;//	The defines the key for the spacing between the label and the indicator in mxLabel.
    STYLE_INDICATOR_WIDTH: string;//	Defines the key for the indicator width.
    STYLE_INDICATOR_HEIGHT: string;//	Defines the key for the indicator height.
    STYLE_INDICATOR_DIRECTION: string;//	Defines the key for the indicatorDirection style.
    STYLE_SHADOW: string;//	Defines the key for the shadow style.
    STYLE_SEGMENT: string;//	Defines the key for the segment style.
    STYLE_ENDARROW: string;//	Defines the key for the end arrow marker.
    STYLE_STARTARROW: string;//	Defines the key for the start arrow marker.
    STYLE_ENDSIZE: string;//	Defines the key for the endSize style.
    STYLE_STARTSIZE: string;//	Defines the key for the startSize style.
    STYLE_SWIMLANE_LINE: string;//	Defines the key for the swimlaneLine style.
    STYLE_ENDFILL: string;//	Defines the key for the endFill style.
    STYLE_STARTFILL: string;//	Defines the key for the startFill style.
    STYLE_DASHED: string;//	Defines the key for the dashed style.
    STYLE_DASH_PATTERN: string;//	Defines the key for the dash pattern style.
    STYLE_ROUNDED: string;//	Defines the key for the rounded style.
    STYLE_CURVED: string;//	Defines the key for the curved style.
    STYLE_ARCSIZE: number;//	Defines the rounding factor for a rounded rectangle in percent(without the percent sign).
    STYLE_SMOOTH: string;//	An experimental style for edges.
    STYLE_SOURCE_PERIMETER_SPACING: string;//	Defines the key for the source perimeter spacing.
    STYLE_TARGET_PERIMETER_SPACING: string;//	Defines the key for the target perimeter spacing.
    STYLE_PERIMETER_SPACING: string;//	Defines the key for the perimeter spacing.
    STYLE_SPACING: string;//	Defines the key for the spacing.
    STYLE_SPACING_TOP: string;//	Defines the key for the spacingTop style.
    STYLE_SPACING_LEFT: string;//	Defines the key for the spacingLeft style.
    STYLE_SPACING_BOTTOM: string;//	Defines the key for the spacingBottom style The value represents the spacing, in pixels, added to the bottom side of a label in a vertex(style applies to vertices only).
    STYLE_SPACING_RIGHT: string;//	Defines the key for the spacingRight style The value represents the spacing, in pixels, added to the right side of a label in a vertex(style applies to vertices only).
    STYLE_HORIZONTAL: string;//	Defines the key for the horizontal style.
    STYLE_DIRECTION: string;//	Defines the key for the direction style.
    STYLE_ELBOW: string;//	Defines the key for the elbow style.
    STYLE_FONTCOLOR: string;//	Defines the key for the fontColor style.
    STYLE_FONTFAMILY: string;//	Defines the key for the fontFamily style.
    STYLE_FONTSIZE: string;//	Defines the key for the fontSize style (in points).
    STYLE_FONTSTYLE: string;//	Defines the key for the fontStyle style.
    STYLE_ASPECT: string;//	Defines the key for the aspect style.
    STYLE_AUTOSIZE: string;//	Defines the key for the autosize style.
    STYLE_FOLDABLE: string;//	Defines the key for the foldable style.
    STYLE_EDITABLE: string;//	Defines the key for the editable style.
    STYLE_BENDABLE: string;//	Defines the key for the bendable style.
    STYLE_MOVABLE: string;//	Defines the key for the movable style.
    STYLE_RESIZABLE: string;//	Defines the key for the resizable style.
    STYLE_ROTATABLE: string;//	Defines the key for the rotatable style.
    STYLE_CLONEABLE: string;//	Defines the key for the cloneable style.
    STYLE_DELETABLE: string;//	Defines the key for the deletable style.
    STYLE_SHAPE: string;//	Defines the key for the shape.
    STYLE_EDGE: string;//	Defines the key for the edge style.
    STYLE_LOOP: string;//	Defines the key for the loop style.
    STYLE_ROUTING_CENTER_X: string;//	Defines the key for the horizontal routing center.
    STYLE_ROUTING_CENTER_Y: string;//	Defines the key for the vertical routing center.
    FONT_BOLD: number;//	Constant for bold fonts.
    FONT_ITALIC: number;//	Constant for italic fonts.
    FONT_UNDERLINE: number;//	Constant for underlined fonts.
    FONT_SHADOW: string;//	Constant for fonts with a shadow.
    SHAPE_RECTANGLE: string;//	Name under which mxRectangleShape is registered in mxCellRenderer.
    SHAPE_ELLIPSE: string;//	Name under which mxEllipse is registered in mxCellRenderer.
    SHAPE_DOUBLE_ELLIPSE: string;//	Name under which mxDoubleEllipse is registered in mxCellRenderer.
    SHAPE_RHOMBUS: string;//	Name under which mxRhombus is registered in mxCellRenderer.
    SHAPE_LINE: string;//	Name under which mxLine is registered in mxCellRenderer.
    SHAPE_IMAGE: string;//	Name under which mxImageShape is registered in mxCellRenderer.
    SHAPE_ARROW: string;//	Name under which mxArrow is registered in mxCellRenderer.
    SHAPE_LABEL: string;//	Name under which mxLabel is registered in mxCellRenderer.
    SHAPE_CYLINDER: string;//	Name under which mxCylinder is registered in mxCellRenderer.
    SHAPE_SWIMLANE: string;//	Name under which mxSwimlane is registered in mxCellRenderer.
    SHAPE_CONNECTOR: string;//	Name under which mxConnector is registered in mxCellRenderer.
    SHAPE_ACTOR: string;//	Name under which mxActor is registered in mxCellRenderer.
    SHAPE_CLOUD: string;//	Name under which mxCloud is registered in mxCellRenderer.
    SHAPE_TRIANGLE: string;//	Name under which mxTriangle is registered in mxCellRenderer.
    SHAPE_HEXAGON: string;//	Name under which mxHexagon is registered in mxCellRenderer.
    ARROW_CLASSIC: string;//	Constant for classic arrow markers.
    ARROW_BLOCK: string;//	Constant for block arrow markers.
    ARROW_OPEN: string;//	Constant for open arrow markers.
    ARROW_OVAL: string;//	Constant for oval arrow markers.
    ARROW_DIAMOND: string;//	Constant for diamond arrow markers.
    ALIGN_LEFT: string;//	Constant for left horizontal alignment.
    ALIGN_CENTER: string;//	Constant for center horizontal alignment.
    ALIGN_RIGHT: string;//	Constant for right horizontal alignment.
    ALIGN_TOP: string;//	Constant for top vertical alignment.
    ALIGN_MIDDLE: string;//	Constant for middle vertical alignment.
    ALIGN_BOTTOM: string;//	Constant for bottom vertical alignment.
    DIRECTION_NORTH: string;//Constant for direction north.
    DIRECTION_SOUTH: string;//	Constant for direction south.
    DIRECTION_EAST: string;//	Constant for direction east.
    DIRECTION_WEST: string;//	Constant for direction west.
    DIRECTION_MASK_NONE: string;//	Constant for no direction.
    DIRECTION_MASK_WEST: string;//	Bitwise mask for west direction.
    DIRECTION_MASK_NORTH: string;//	Bitwise mask for north direction.
    DIRECTION_MASK_SOUTH: string;//	Bitwise mask for south direction.
    DIRECTION_MASK_EAST: string;//	Bitwise mask for east direction.
    DIRECTION_MASK_ALL: string;//	Bitwise mask for all directions.
    ELBOW_VERTICAL: string;//	Constant for elbow vertical.
    ELBOW_HORIZONTAL: string;//	Constant for elbow horizontal.
    EDGESTYLE_ELBOW: string;//	Name of the elbow edge style.
    EDGESTYLE_ENTITY_RELATION: string;//	Name of the entity relation edge style.
    EDGESTYLE_LOOP: string;//	Name of the loop edge style.
    EDGESTYLE_SIDETOSIDE: string;//	Name of the side to side edge style.
    EDGESTYLE_TOPTOBOTTOM: string;//	Name of the top to bottom edge style.
    EDGESTYLE_ORTHOGONAL: string;//	Name of the generic orthogonal edge style.
    EDGESTYLE_SEGMENT: string;//	Name of the generic segment edge style.
    PERIMETER_ELLIPSE: string;//	Name of the ellipse perimeter.
    PERIMETER_RECTANGLE: string;//	Name of the rectangle perimeter.
    PERIMETER_RHOMBUS: string;//	Name of the rhombus perimeter.
    PERIMETER_HEXAGON: string;//	Name of the hexagon perimeter.
    PERIMETER_TRIANGLE: string;//	Name of the triangle perimeter.
}

/**
 * Class: mxMarker
 *
 * A static class that implements all markers for VML and SVG using a
 * registry. NOTE: The signatures in this class will change.
 *
 */
declare class mxMarker {
    /**
	 * Variable: markers
	 *
	 * Maps from markers names to functions to paint the markers.
	 */
    markers: Array<any>;

	/**
	 * Function: addMarker
	 *
	 * Adds a factory method that updates a given endpoint and returns a
	 * function to paint the marker onto the given canvas.
	 */
    static addMarker(markerType: string, funct): any;

	/**
	 * Function: createMarker
	 *
	 * Returns a function to paint the given marker.
	 */
    createMarker(canvas, shape, type, pe, unitX, unitY, size, source, sw, filled): any;
}

declare var mxConstants: MxConstants;
interface MxGeometryFactory {
    new (x?: number, y?: number, width?: number, height?: number): MxGeometry
}
declare var mxGeometry: MxGeometryFactory;

declare var mxRubberband: any;

declare var mxPerimeter: any;
declare var mxRadialTreeLayout: MxGraphLayoutFactory;

declare var mxXmlCanvas2D: any;
interface MxAbstractCanvas2DFactory {
    new (): MxAbstractCanvas2D
}
declare var mxAbstractCanvas2D: MxAbstractCanvas2DFactory;

declare var mxImageExport: any;
declare var mxXmlRequest: any;

declare class mxCellRenderer implements MxCellRender {

    redrawControl(state: any, forced?: boolean)

    createControl(state: any);

    getControlBounds(state: any, w: number, h: number);

    initControl(state, control, handleEvents, clickHandler);

    static registerShape(key: string, shape);

    installCellOverlayListeners(state, overlay, shape);

    getLabelValue(state);

}
declare var mxRhombus: any;

declare var mxTriangle: any;

declare var mxPoint: MxPointFactory;
declare var mxRectangle: MxRectangleFactory;
declare var mxConnectionConstraint: any;
declare var mxEdgeHandler: any;

//declare var mxCell: MxCellFactory;

declare var mxBasePath: string;

declare class mxShape {
    scale: number;
    style: any;
    dialect: string;
    node: any;
    init(container: HTMLElement);
    isRounded: boolean;
    addPoints(path, pts, rounded: boolean, arcSize: number, close: boolean): void;
    fill: string;
    bounds: MxRectangle;
    redraw();
    redrawPath(path, x: number, y: number, w: number, h: number);
    getLabelBounds(rect: MxRectangle);
    createSvg();
    clear();
    destroy();
}

declare class mxSwimlane extends mxShape {

}

declare class mxActor extends mxShape {

}

declare class mxRectangleShape extends mxShape {

}

declare class mxImageShape extends mxShape {

    constructor(bounds?: MxRectangle, image?: string, fill?: string, stroke?: string, strokewidth?: number)
    /**
     * Variable: image
     *
     * String that specifies the URL of the image.
     */
    image: string;
    /**
     * Variable: preserveImageAspect
     *
     * Switch to preserve image aspect. Default is true.
     */
    preserveImageAspect: boolean;
    /**
     * Function: paintVertexShape
     *
     * Generic background painting implementation.
     */
    paintVertexShape(c: any, x: number, y: number, w: number, h: number);
}

declare class mxPolyline extends mxShape {
}

declare class mxConnector extends mxPolyline {
}

declare class mxCylinder extends mxShape {
    redrawPath(path, x: number, y: number, w: number, h: number, isForeground?: boolean)
}

declare var mxSvgCanvas2D;

declare class mxEvent {
    static CHANGE: string;
    static CLICK: string
    static MOUSE_DOWN: string;
    static addListener(element: any, eventName: string, funct: any);
    static removeListener(element: any, eventName: string, funct: any);
    static disableContextMenu(element: HTMLElement);
    static addGestureListeners(node, startListener, moveListener?, endListener?);
    static isShiftDown(evt: any);
    static isAltDown(evt: any);
    static isControlDown(evt: any);
    static isMetaDown(evt: any);
    static consume(evt: any, preventDefault?, stopPropagation?);
    static isRightMouseButton(evt: any);
    static isMiddleMouseButton(evt: any);
    static isLeftMouseButton(evt: any);

}

interface MxMouseEvent {
    getCell(): MxCell;
}

declare class mxMouseEvent implements MxMouseEvent {
    constructor(evt, state);
    getCell(): MxCell;
}

declare class mxEdgeStyle {
    static Loop;
    static ElbowConnector;
    static OrthConnector;
    static SegmentConnector;
    static TopToBottom;
}

declare class mxStyleRegistry {
	/**
	 * Function: putValue
	 *
	 * Puts the given object into the registry under the given name.
	 */
    static putValue(name, obj);
}

declare class mxEventSource {
    eventListeners;
    eventsEnabled: boolean;
    eventSource;
    /**
     * Function: addListener
     *
     * Binds the specified function to the given event name. If no event name
     * is given, then the listener is registered for all events.
     *
     * The parameters of the listener are the sender and an <mxEventObject>.
     */
    addListener(name: string, funct)

    /**
     * Function: fireEvent
     *
     * Dispatches the given event to the listeners which are registered for
     * the event. The sender argument is optional. The current execution scope
     * ("this") is used for the listener invocation (see <mxUtils.bind>).
     *
     * Example:
     *
     * (code)
     * fireEvent(new mxEventObject("eventName", key1, val1, .., keyN, valN))
     * (end)
     *
     * Parameters:
     *
     * evt - <mxEventObject> that represents the event.
     * sender - Optional sender to be passed to the listener. Default value is
     * the return value of <getEventSource>.
     */
    fireEvent(evt, sender?);
}

declare class mxCellOverlay extends mxEventSource {
    constructor(image?: MxImage, tooltip?: string, align?: string, verticalAlign?: string, offset?: number, cursor?: string)

    image: MxImage;
    tooltip: string;
    align: string;
    verticalAlign: string;
    offset: number|MxPoint;
    cursor: string;
    getBounds(state): MxRectangle;
}

declare class mxCell implements MxCell {
    constructor(value?: any, geometry?: MxGeometry, style?: string);
    id: any;
    value;
    geometry;
    style;
    vertex;
    edge;
    connectable;
    visible;
    collapsed;
    parent;
    source;
    target;
    children;
    edges;
    mxTransient;
    getId();
    setId(id);
    getValue();
    setValue(value);
    valueChanged(newValue);
    getGeometry();
    setGeometry(geometry);
    getStyle();
    setStyle(style);
    isVertex();
    setVertex(vertex);
    isEdge();
    setEdge(edge);
    isConnectable();
    setConnectable(connectable);
    isVisible();
    setVisible(visible);
    isCollapsed();
    setCollapsed(collapsed);
    getParent();
    setParent(parent);
    getTerminal(source);
    setTerminal(terminal, isSource);
    getChildCount();
    getIndex(child);
    getChildAt(index);
    insert(child, index?);
    remove(index);
    removeFromParent();
    getEdgeCount();
    getEdgeIndex(edge);
    getEdgeAt(index);
    insertEdge(edge, isOutgoing);
    removeEdge(edge, isOutgoing);
    removeFromTerminal(isSource);
    getAttribute(name, defaultValue);
    setAttribute(name, value);
    getTooltip();
    getLabel();

    /**
     * Returns a clone of the cell.  Uses cloneValue to clone the user object.  All fields in mxTransient are ignored during the cloning.
     */
    clone(): mxCell;

    cloneValue();
}

/**
 * Class: mxEventObject
 *
 * The mxEventObject is a wrapper for all properties of a single event.
 * Additionally, it also offers functions to consume the event and check if it
 * was consumed as follows:
 *
 * (code)
 * evt.consume();
 * INV: evt.isConsumed() == true
 * (end)
 *
 * Constructor: mxEventObject
 *
 * Constructs a new event object with the specified name. An optional
 * sequence of key, value pairs can be appended to define properties.
 *
 * Example:
 *
 * (code)
 * new mxEventObject("eventName", key1, val1, .., keyN, valN)
 * (end)
 */
declare class mxEventObject {
    constructor(name, ...args: any[])
}

declare class mxPopupMenu implements MxPopupMenu {
    hideMenu();
    setEnabled(enabled: boolean);
    useLeftButtonForPopup: boolean;
    popup(x, y, cell, evt);
    addItem (title, image, funct, parent, iconCls, enabled, active);

}

declare class mxGraphModel implements MxGraphModel {
    public eventListeners;
    public eventsEnabled: boolean;
    public eventSource;
    public addListener(name: string, funct);
    public fireEvent(evt, sender?);
    public beginUpdate(): void;
    public endUpdate(): void;
    public setCollapsed(cell: MxCell, collapsed: boolean);
    public getIncomingEdges(cell: MxCell): Array<MxCell>;
    public getOutgoingEdges(cell: MxCell): Array<MxCell>;
    public getStyle(cell: MxCell): any;
    public getRoot(cell?: MxCell): any; //todo
    public getGeometry(cell: MxCell): MxGeometry;
    public getChildCount(cell);
    public getCell(id: string): any;
    public getConnections(cell): any[];
    public setVisible(cell, visible);
    public createUndoableEdit();
    public getParent(cell);
    public remove(cell);
    public add(parent, child, index);
    public clear();
}
