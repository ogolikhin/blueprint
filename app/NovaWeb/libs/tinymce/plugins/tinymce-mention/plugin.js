﻿/*global tinymce */

(function (tinymce) {
    'use strict';

    var noJQuery = function () { };

    noJQuery.prototype = {

        constructor: noJQuery,
        isIE: function () {
            var uA = navigator.userAgent;
            return (uA.indexOf('Trident') != -1 && uA.indexOf('rv:11') != -1) || (uA.indexOf('Trident') != -1 && uA.indexOf('MSIE') != -1);
        },
        extend: function () {
            for (var i = 1; i < arguments.length; i++)
                for (var key in arguments[i])
                    if (arguments[i].hasOwnProperty(key))
                        arguments[0][key] = arguments[i][key];
            return arguments[0];
        },
        inArray: function (elem, arr, i) {
            return arr == null ? -1 : arr.indexOf(elem, i);
        },
        trim: function (text) {
            return (text || "").trim();
        },
        grep: function (elems, callback, inv) {
            var ret = [];

            for (var i = 0, length = elems.length; i < length; i++) {
                if (!inv !== !callback(elems[i], i)) {
                    ret.push(elems[i]);
                }
            }

            return ret;
        },
        getText: function (elems) {
            var ret = "", elem;

            for (var i = 0; elems[i]; i++) {
                elem = elems[i];
                if (elem.nodeType === 3 || elem.nodeType === 4) {
                    ret += elem.nodeValue;
                } else if (elem.nodeType !== 8) {
                    ret += jsH.getText(elem.childNodes);
                }
            }

            return ret;
        },
        isFunction: function (obj) {
            return typeof obj === 'function';
        },
        offset: function (el) {
            var rect = el.getBoundingClientRect();

            return {
                top: rect.top + document.body.scrollTop,
                left: rect.left + document.body.scrollLeft
            };

        },
        innerHeight: function (el) {
            var style = window.getComputedStyle(el, null);
            var height = style.getPropertyValue("height");
            if (height === 'auto') {
                height = el.offsetHeight;
            }


            return height;
        },
        position: function (el) {
            return { left: el.offsetLeft, top: el.offsetTop };

        },
        each: function (obj, callback, args) {
            var value, i = 0,
				length = obj.length,
				isArray = Array.isArray(obj);

            if (args) {
                if (isArray) {
                    for (; i < length; i++) {
                        value = callback.apply(obj[i], args);

                        if (value === false) {
                            break;
                        }
                    }
                } else {
                    for (i in obj) {
                        value = callback.apply(obj[i], args);

                        if (value === false) {
                            break;
                        }
                    }
                }
            } else {
                if (isArray) {
                    for (; i < length; i++) {
                        value = callback.call(obj[i], i, obj[i]);

                        if (value === false) {
                            break;
                        }
                    }
                } else {
                    for (i in obj) {
                        value = callback.call(obj[i], i, obj[i]);

                        if (value === false) {
                            break;
                        }
                    }
                }
            }

            return obj;
        },
        removeClass: function (el, className) {
            if (el.classList)
                el.classList.remove(className);
            else
                el.className = el.className.replace(new RegExp('(^|\\b)' + className.split(' ').join('|') + '(\\b|$)', 'gi'), ' ');
        },
        addClass: function (el, className) {
            if (el) {
                if (el.classList)
                    el.classList.add(className);
                else {
                    el.className += ' ' + className;
                }
            }
        },
        data: function (item, attr) {
            var value = item.getAttribute('data-' + attr);
            var o = {};
            o[attr] = value;
            return o;
        },
        getAllDataAttributes: function (el) {
            var o = {};
            Array.prototype.slice.call(el.attributes).forEach(function (at) {
                if (/^data-/.test(at.name)) {
                    o[at.name.replace(/^data-/, "")] = at.value;
                }
            });
            return o;
        },
        closest: function (el, selector) {

            if (el.matches) {
                while (el.matches && !el.matches(selector)) {
                    el = el.parentNode
                }
            } else if (el.msMatchesSelector) {
                while (el.msMatchesSelector && !el.msMatchesSelector(selector)) {
                    el = el.parentNode
                }
            } else {
                el = null;
            }
            return el;

        },
        isEmptyObject: function (obj) {
            return Object.keys(obj).length === 0 && obj.constructor === Object;

        }
    };

    var AutoComplete = function (ed, options) {
        this.jsH = new noJQuery();

        this.editor = ed;

        this.options = this.jsH.extend({}, {
            source: [],
            delay: 500,
            queryBy: 'name',
            items: 10
        }, options);

        this.matcher = this.options.matcher || this.matcher;
        this.renderDropdown = this.options.renderDropdown || this.renderDropdown;
        this.render = this.options.render || this.render;
        this.insert = this.options.insert || this.insert;
        this.highlighter = this.options.highlighter || this.highlighter;

        this.query = '';
        this.hasFocus = true;

        this.renderInput();

        this.bindEvents();
    };

    AutoComplete.prototype = {

        constructor: AutoComplete,

        renderInput: function () {
            var rawHtml = '<span id="autocomplete">' +
				'<span id="autocomplete-delimiter">' + this.options.delimiter + '</span>' +
				'<span id="autocomplete-searchtext"><span class="dummy">\uFEFF</span></span>' +
				'</span>';

            this.editor.execCommand('mceInsertContent', false, rawHtml);
            this.editor.focus();
            this.editor.selection.select(this.editor.selection.dom.select('span#autocomplete-searchtext span')[0]);
            this.editor.selection.collapse(0);
        },

        bindEvents: function () {
            this.editor.on('keyup', this.editorKeyUpProxy = this.rteKeyUp.bind(this));
            this.editor.on('keydown', this.editorKeyDownProxy = this.rteKeyDown.bind(this), true);
            this.editor.on('click', this.editorClickProxy = this.rteClicked.bind(this));

            document.body.addEventListener('click', this.bodyClickProxy = this.rteLostFocus.bind(this));

            this.editor.getWin().addEventListener('scroll', this.rteScroll = function () { this.cleanUp(true); }.bind(this));
        },

        unbindEvents: function () {
            this.editor.off('keyup', this.editorKeyUpProxy);
            this.editor.off('keydown', this.editorKeyDownProxy);
            this.editor.off('click', this.editorClickProxy);

            document.body.removeEventListener('click', this.bodyClickProxy);

            this.editor.getWin().removeEventListener('scroll', this.rteScroll);
        },

        rteKeyUp: function (e) {
            switch (e.which || e.keyCode) {
                //DOWN ARROW
                case 40:
                    //UP ARROW
                case 38:
                    //SHIFT
                case 16:
                    //CTRL
                case 17:
                    //ALT
                case 18:
                    break;

                    //BACKSPACE
                case 8:
                    if (this.query === '') {
                        this.cleanUp(true);
                    } else {
                        this.lookup();
                    }
                    break;

                    //TAB
                case 9:
                    //ENTER
                case 13:
                    var item = (this.dropdown !== undefined) ? this.dropdown.querySelectorAll('li.active') : [];
                    if (item.length) {
                        this.select(this.jsH.getAllDataAttributes(item[0]));
                        this.cleanUp(false);
                    } else {
                        this.cleanUp(true);
                    }
                    break;

                    //ESC
                case 27:
                    this.cleanUp(true);
                    break;

                default:
                    this.lookup();
            }
        },

        rteKeyDown: function (e) {
            switch (e.which || e.keyCode) {
                //TAB
                case 9:
                    //ENTER
                case 13:
                    //ESC
                case 27:
                    e.preventDefault();
                    break;

                    //UP ARROW
                case 38:
                    e.preventDefault();
                    if (this.dropdown !== undefined) {
                        this.highlightPreviousResult();
                    }
                    break;
                    //DOWN ARROW
                case 40:
                    e.preventDefault();
                    if (this.dropdown !== undefined) {
                        this.highlightNextResult();
                    }
                    break;
            }

            e.stopPropagation();
        },

        rteClicked: function (e) {
            var target = e.target,
				id;

            if (target.parentNode && target.parentNode.getAttribute) {
                id = target.parentNode.getAttribute("id");
            }

            if (this.hasFocus && id !== 'autocomplete-searchtext') {
                this.cleanUp(true);
            }
        },


        rteLostFocus: function () {
            if (this.hasFocus) {
                this.cleanUp(true);
            }
        },

        lookup: function () {
            var editorBody = this.editor.getBody().querySelector('#autocomplete-searchtext');
            this.query = this.jsH.trim(editorBody.innerText).replace('\ufeff', '');

            if (this.dropdown === undefined) {
                this.show();
            }

            clearTimeout(this.searchTimeout);
            this.searchTimeout = setTimeout(function () {
                // Added delimiter parameter as last argument for backwards compatibility.
                var items = this.jsH.isFunction(this.options.source) ? this.options.source(this.query, this.process.bind(this), this.options.delimiter) : this.options.source;
                if (items) {
                    this.process(items);
                }
            }.bind(this), this.options.delay);
        },
        matcher: function (item) {
            return ~item[this.options.queryBy].toLowerCase().indexOf(this.query.toLowerCase());
        },


        sorter: function (items) {
            var beginswith = [],
				caseSensitive = [],
				caseInsensitive = [],
				item;

            while ((item = items.shift()) !== undefined) {
                if (!item[this.options.queryBy].toLowerCase().indexOf(this.query.toLowerCase())) {
                    beginswith.push(item);
                } else if (~item[this.options.queryBy].indexOf(this.query)) {
                    caseSensitive.push(item);
                } else {
                    caseInsensitive.push(item);
                }
            }

            return beginswith.concat(caseSensitive, caseInsensitive);
        },

        highlighter: function (text) {
            return text.replace(new RegExp('(' + this.query.replace(/([.?*+^$[\]\\(){}|-])/g, '\\$1') + ')', 'ig'), function ($1, match) {
                return '<strong>' + match + '</strong>';
            });
        },

        show: function () {
            var offset = this.editor.inline ? this.offsetInline() : this.offset();

            var div = document.createElement("div");
            div.innerHTML = this.renderDropdown();
            this.dropdown = div.firstChild;
            this.dropdown.style.top = offset.top + "px";
            this.dropdown.style.left = offset.left + "px";

            document.body.appendChild(this.dropdown);

            this.dropdown.addEventListener('click', this.autoCompleteClick.bind(this));
        },

        process: function (data) {
            if (!this.hasFocus) {
                return;
            }

            var _this = this,
				result = [],
				items = this.jsH.grep(data, function (item) {
				    return _this.matcher(item);
				});

            items = _this.sorter(items);

            items = items.slice(0, this.options.items);

            this.dropdown.innerHTML = '';

            for (var i = 0; i < items.length; i++) {
                var item = items[i];

                var div = document.createElement("div");
                div.innerHTML = this.render(item);
                var li = div.firstChild;

                li.innerHTML = li.innerHTML.replace(li.innerText, this.highlighter(li.innerText));

                this.jsH.each(item, function (key, val) {
                    li.setAttribute('data-' + key, val);
                });

                this.dropdown.appendChild(li);
            }

            if (this.dropdown.childNodes.length > 0) {
                this.dropdown.style.display = 'block';


            } else {
                this.dropdown.style.display = 'none';
            }
        },


        renderDropdown: function () {
            return '<ul class="rte-autocomplete mce-mention dropdown-menu"><li class="loading"></li></ul>'; //need to add a class starting with "mce-" to not make the inline editor disappear
        },

        render: function (item) {
            return '<li>' +
				'<a href="javascript:;"><span>' + item[this.options.queryBy] + '</span></a>' +
				'</li>';
        },

        autoCompleteClick: function (e) {
            var item = this.jsH.getAllDataAttributes(this.jsH.closest(e.target, 'li'));

            if (!this.jsH.isEmptyObject(item)) {
                this.select(item);
                this.cleanUp(false);
            }
            e.stopPropagation();
            e.preventDefault();
        },

        highlightPreviousResult: function () {
            this.highlightResult(0);
        },

        highlightNextResult: function () {
            this.highlightResult(1);
        },
        highlightResult: function (direction) {
            var activeLi = this.dropdown.querySelector('li.active'),
				items = Array.prototype.slice.call(this.dropdown.children),
				length = items.length,
				currentIndex = 0,
				index = 0;

            if (direction === 0) {
                currentIndex = activeLi === null ? length : items.indexOf(activeLi);
                index = (currentIndex === 0) ? length - 1 : --currentIndex;
            } else {
                currentIndex = activeLi === null ? -1 : items.indexOf(activeLi);
                index = (currentIndex === length - 1) ? 0 : ++currentIndex;
            }

            var liArray = this.dropdown.querySelectorAll('li');
            for (var i = 0; i < liArray.length; i++) {
                this.jsH.removeClass(liArray[i], 'active');
            }

            this.jsH.addClass(items[index], 'active');
        },

        select: function (item) {
            this.editor.focus();
            var selection = this.editor.dom.select('span#autocomplete')[0];
            this.editor.dom.remove(selection);
            this.editor.execCommand('mceInsertContent', false, this.insert(item));
        },

        insert: function (item) {
            return '<span>' + item[this.options.queryBy] + '</span>&nbsp;';
        },

        cleanUp: function (rollback) {
            this.unbindEvents();
            this.hasFocus = false;

            if (this.dropdown !== undefined) {
                this.dropdown.parentNode.removeChild(this.dropdown);

                delete this.dropdown;
            }

            if (rollback) {
                var text = this.query;
                var selection = this.editor.dom.select('span#autocomplete')[0];

                var p = document.createElement('p');
                p.innerText = this.options.delimiter + text;
                var replacement = p.firstChild;
                var focus = this.jsH.offset(this.editor.selection.getNode()).top === (this.jsH.offset(selection).top + ((selection.offsetHeigh - window.getComputedStyle(selection).getPropertyValue("height")) / 2));

                this.editor.dom.replace(replacement, selection);

                if (focus) {
                    this.editor.selection.select(replacement);
                    this.editor.selection.collapse();
                }
            }
        },

        offset: function () {
            var rtePosition = this.jsH.offset(this.editor.getContainer()),
				contentAreaPosition = this.jsH.position(this.editor.getContentAreaContainer()),
				nodePosition = this.jsH.position(this.editor.dom.select('span#autocomplete')[0]),
				scrollTop = this.jsH.isIE() ? this.editor.getDoc().documentElement.scrollTop : this.editor.getDoc().body.scrollTop;

            return {
                top: rtePosition.top + contentAreaPosition.top + nodePosition.top + this.jsH.innerHeight(this.editor.selection.getNode()) - scrollTop + 5,
                left: rtePosition.left + contentAreaPosition.left + nodePosition.left
            };
        },

        offsetInline: function () {
            var nodePosition = this.jsH.offset(this.editor.dom.select('span#autocomplete')[0]);

            return {
                top: nodePosition.top + this.jsH.innerHeight(this.editor.selection.getNode()) + 5, //TODO
                left: nodePosition.left
            };
        }

    };

    tinymce.create('tinymce.plugins.Mention', {

        init: function (ed) {

            var autoComplete,
				autoCompleteData = ed.getParam('mentions');

            var jsH = new noJQuery();

            // If the delimiter is undefined set default value to ['@'].
            // If the delimiter is a string value convert it to an array. (backwards compatibility)
            autoCompleteData.delimiter = (autoCompleteData.delimiter !== undefined) ? !Array.isArray(autoCompleteData.delimiter) ? [autoCompleteData.delimiter] : autoCompleteData.delimiter : ['@'];

            function prevCharIsSpace() {
                var start = ed.selection.getRng(true).startOffset,
					text = ed.selection.getRng(true).startContainer.data || '',
					charachter = text.substr(start - 1, 1);

                return (!!jsH.trim(charachter).length) ? false : true;
            }

            ed.on('keypress', function (e) {
                var delimiterIndex = jsH.inArray(String.fromCharCode(e.which || e.keyCode), autoCompleteData.delimiter);
                if (delimiterIndex > -1 && prevCharIsSpace()) {
                    if (autoComplete === undefined || (autoComplete.hasFocus !== undefined && !autoComplete.hasFocus)) {
                        e.preventDefault();
                        // Clone options object and set the used delimiter.
                        autoComplete = new AutoComplete(ed, jsH.extend({}, autoCompleteData, { delimiter: autoCompleteData.delimiter[delimiterIndex] }));
                    }
                }
            });

        },

        getInfo: function () {
            return {
                longname: 'mention',
                author: 'Steven Devooght',
                version: tinymce.majorVersion + '.' + tinymce.minorVersion
            };
        }
    });

    tinymce.PluginManager.add('mention', tinymce.plugins.Mention);

}(tinymce));