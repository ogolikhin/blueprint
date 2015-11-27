/*
jQWidgets v3.9.1 (2015-Oct)
Copyright (c) 2011-2015 jQWidgets.
License: http://jqwidgets.com/license/
*/

(function(a){a.jqx.jqxWidget("jqxInput","",{});a.extend(a.jqx._jqxInput.prototype,{defineInstance:function(){var c=this;var b={disabled:false,filter:c._filter,sort:c._sort,highlight:c._highlight,dropDownWidth:null,renderer:c._renderer,opened:false,$popup:a("<ul></ul>"),source:[],roundedCorners:true,searchMode:"default",placeHolder:"",width:null,height:null,value:"",rtl:false,displayMember:"",valueMember:"",events:["select","open","close","change"],popupZIndex:20000,items:8,item:'<li><a href:"#"></a></li>',minLength:1,maxLength:null};a.extend(true,this,b);return b},createInstance:function(b){this.render()},render:function(){if(this.element.nodeName.toLowerCase()=="textarea"){this.element.style.overflow="auto"}else{if(this.element.nodeName.toLowerCase()=="div"){this.baseHost=this.element;var b=this.host.find("input");var d=false;a.each(b,function(){var f=this.type;if(f==null||f=="text"||f=="textarea"){b=a(this);d=true;return false}});if(!d){throw new Error("jqxInput: Missing Text Input in the Input Group")}if(b.length>0){this.baseHost=a(this.element);this.host=b;this.element=b[0];this.baseHost.addClass(this.toThemeProperty("jqx-widget"));this.baseHost.addClass(this.toThemeProperty("jqx-rc-all"));this.baseHost.addClass(this.toThemeProperty("jqx-input-group"));var c=this.baseHost.children();var e=this;a.each(c,function(f){a(this).addClass(e.toThemeProperty("jqx-input-group-addon"));a(this).removeClass(e.toThemeProperty("jqx-rc-all"));if(f==0){a(this).addClass(e.toThemeProperty("jqx-rc-l"))}if(f==c.length-1){a(this).addClass(e.toThemeProperty("jqx-rc-r"))}if(this!=e.element){a(this).addClass(e.toThemeProperty("jqx-fill-state-normal"))}})}}}this.addHandlers();if(this.rtl){this.host.addClass(this.toThemeProperty("jqx-rtl"))}this.host.attr("role","textbox");a.jqx.aria(this,"aria-autocomplete","both");a.jqx.aria(this,"aria-disabled",this.disabled);a.jqx.aria(this,"aria-readonly",false);a.jqx.aria(this,"aria-multiline",false);if(this.source&&this.source.length){a.jqx.aria(this,"aria-haspopup",true)}if(this.value!=""){this.element.value=this.value}this._oldsource=this.source;this._updateSource()},_updateSource:function(){var d=this;var b=function(f){var e=new Array();e=a.map(f,function(h){if(h==undefined){return null}if(typeof h==="string"||h instanceof String){return{label:h,value:h}}if(typeof h!="string"&&h instanceof String==false){var g="";var i="";if(d.displayMember!=""&&d.displayMember!=undefined){if(h[d.displayMember]){g=h[d.displayMember]}}if(d.valueMember!=""&&d.valueMember!=undefined){i=h[d.valueMember]}if(g==""){g=h.label}if(i==""){i=h.value}return{label:g,value:i}}return h});return e};if(this.source&&this.source._source){this.adapter=this.source;if(this.adapter._source.localdata!=null){this.adapter.unbindBindingUpdate(this.element.id);this.adapter.bindBindingUpdate(this.element.id,function(e){d.source=b(d.adapter.records)})}else{var c={};if(this.adapter._options.data){a.extend(d.adapter._options.data,c)}else{if(this.source._source.data){a.extend(c,this.source._source.data)}this.adapter._options.data=c}this.adapter.unbindDownloadComplete(this.element.id);this.adapter.bindDownloadComplete(this.element.id,function(e){d.source=b(d.adapter.records)})}this.source.dataBind();return}if(!a.isFunction(this.source)){this.source=b(this.source)}},_refreshClasses:function(c){var b=c?"addClass":"removeClass";this.host[b](this.toThemeProperty("jqx-widget-content"));this.host[b](this.toThemeProperty("jqx-input"));this.host[b](this.toThemeProperty("jqx-widget"));this.$popup[b](this.toThemeProperty("jqx-popup"));if(a.jqx.browser.msie){this.$popup[b](this.toThemeProperty("jqx-noshadow"))}this.$popup[b](this.toThemeProperty("jqx-input-popup"));this.$popup[b](this.toThemeProperty("jqx-menu"));this.$popup[b](this.toThemeProperty("jqx-menu-vertical"));this.$popup[b](this.toThemeProperty("jqx-menu-dropdown"));this.$popup[b](this.toThemeProperty("jqx-widget"));this.$popup[b](this.toThemeProperty("jqx-widget-content"));if(this.roundedCorners){this.host[b](this.toThemeProperty("jqx-rc-all"));this.$popup[b](this.toThemeProperty("jqx-rc-all"))}if(this.disabled){this.host[b](this.toThemeProperty("jqx-fill-state-disabled"))}else{this.host.removeClass(this.toThemeProperty("jqx-fill-state-disabled"))}},selectAll:function(){var b=this.host;setTimeout(function(){if("selectionStart" in b[0]){b[0].focus();b[0].setSelectionRange(0,b[0].value.length)}else{var c=b[0].createTextRange();c.collapse(true);c.moveEnd("character",b[0].value.length);c.moveStart("character",0);c.select()}},10)},selectLast:function(){var b=this.host;this.selectStart(b[0].value.length)},selectFirst:function(){var b=this.host;this.selectStart(0)},selectStart:function(c){var b=this.host;setTimeout(function(){if("selectionStart" in b[0]){b[0].focus();b[0].setSelectionRange(c,c)}else{var d=b[0].createTextRange();d.collapse(true);d.moveEnd("character",c);d.moveStart("character",c);d.select()}},10)},focus:function(){try{this.host.focus();var c=this;setTimeout(function(){c.host.focus()},25)}catch(b){}},resize:function(c,b){this.width=c;this.height=b;this.refresh()},refresh:function(){this._refreshClasses(false);this._refreshClasses(true);if(!this.baseHost){if(this.width){this.host.width(this.width)}if(this.height){this.host.height(this.height)}}else{if(this.width){this.baseHost.width(this.width)}if(this.height){this.baseHost.height(this.height);var d=this;var c=0;var b=this.baseHost.height()-2;if(a.jqx.browser.msie&&a.jqx.browser.version<8){this.baseHost.css("display","inline-block")}a.each(this.baseHost.children(),function(){a(this).css("height","100%");if(a.jqx.browser.msie&&a.jqx.browser.version<8){a(this).css("height",b+"px")}if(this!==d.element){c+=a(this).outerWidth()+2}});this.host.css("width",this.baseHost.width()-c-4+"px");if(a.jqx.browser.msie&&a.jqx.browser.version<9){this.host.css("min-height",b+"px");this.host.css("line-height",b+"px")}}}this.host.attr("disabled",this.disabled);if(this.maxLength){this.host.attr("maxlength",this.maxLength)}if(!this.host.attr("placeholder")){this._refreshPlaceHolder()}},_refreshPlaceHolder:function(){if("placeholder" in this.element){this.host.attr("placeHolder",this.placeHolder)}else{var b=this;if(this.element.value==""){this.element.value=this.placeHolder;this.host.focus(function(){if(b.element.value==b.placeHolder){b.element.value=""}});this.host.blur(function(){if(b.element.value==""||b.element.value==b.placeHolder){b.element.value=b.placeHolder}})}}},destroy:function(){this.removeHandlers();if(this.baseHost){this.baseHost.remove()}else{this.host.remove()}if(this.$popup){this.$popup.remove()}},propertyChangedHandler:function(b,c,e,d){if(c=="placeHolder"){b._refreshPlaceHolder();return}if(c==="theme"){a.jqx.utilities.setTheme(e,d,b.host)}if(c=="opened"){if(d){b.open()}else{b.close()}return}if(c=="source"){b._oldsource=d;b._updateSource()}if(c=="displayMember"||c=="valueMember"){b.source=b._oldsource;b._updateSource()}if(c=="disabled"){a.jqx.aria(b,"aria-disabled",b.disabled)}if(c=="value"){b.element.value=d}b.refresh()},select:function(c,d){var e=this.$popup.find(".jqx-fill-state-pressed").attr("data-value");var b=this.$popup.find(".jqx-fill-state-pressed").attr("data-name");this.element.value=this.renderer(b,this.element.value);this.selectedItem={label:b,value:e};this.host.attr("data-value",e);this.host.attr("data-label",b);this._raiseEvent("0",{item:{label:b,value:e},label:b,value:e});this._raiseEvent("3",{item:{label:b,value:e},label:b,value:e});return this.close()},val:function(b){if(arguments.length==0||(b!=null&&typeof(b)=="object"&&!b.label&&!b.value)){if(this.displayMember!=""&&this.valueMember!=""&&this.selectedItem){if(this.element.value===""){return""}return this.selectedItem}return this.element.value}if(b&&b.label){this.selectedItem={label:b.label,value:b.value};this.host.attr("data-value",b.value);this.host.attr("data-label",b.label);this.value=b;this.element.value=b.label;return this.element.value}this.value=b;this.element.value=b;this.host.attr("data-value",b);this.host.attr("data-label",b);if(b&&b.label){this._raiseEvent("3",{item:{label:b.label,value:b.value},label:b.label,value:b.value})}else{this._raiseEvent("3",{item:{label:b,value:b},label:b,value:b})}return this.element.value},_raiseEvent:function(f,c){if(c==undefined){c={owner:null}}var d=this.events[f];c.owner=this;var e=new a.Event(d);e.owner=this;e.args=c;if(e.preventDefault){e.preventDefault()}var b=this.host.trigger(e);return b},_renderer:function(b){return b},open:function(){if(a.jqx.isHidden(this.host)){return}var c=a.extend({},this.host.coord(true),{height:this.host[0].offsetHeight});if(this.$popup.parent().length==0){var e=this.element.id+"_popup";this.$popup[0].id=e;a.jqx.aria(this,"aria-owns",e)}this.$popup.appendTo(a(document.body)).css({position:"absolute",zIndex:this.popupZIndex,top:c.top+c.height,left:c.left}).show();var b=0;var d=this.$popup.children();a.each(d,function(){b+=a(this).outerHeight(true)-1});this.$popup.height(b);this.opened=true;this._raiseEvent("1",{popup:this.$popup});a.jqx.aria(this,"aria-expanded",true);return this},close:function(){this.$popup.hide();this.opened=false;this._raiseEvent("2",{popup:this.$popup});a.jqx.aria(this,"aria-expanded",false);return this},suggest:function(c){var b;this.query=this.element.value;if(!this.query||this.query.length<this.minLength){return this.opened?this.close():this}if(a.isFunction(this.source)){b=this.source(this.query,a.proxy(this.load,this))}else{b=this.source}if(b){return this.load(b)}return this},load:function(b){var c=this;b=a.grep(b,function(d){return c.filter(d)});b=this.sort(b);if(!b.length){if(this.opened){return this.close()}else{return this}}return this._render(b.slice(0,this.items)).open()},_filter:function(b){var c=this.query;var d=b;if(b.label!=null){d=b.label}else{if(this.displayMember){d=b[this.displayMember]}}switch(this.searchMode){case"none":break;case"containsignorecase":default:return a.jqx.string.containsIgnoreCase(d,c);case"contains":return a.jqx.string.contains(d,c);case"equals":return a.jqx.string.equals(d,c);case"equalsignorecase":return a.jqx.string.equalsIgnoreCase(d,c);case"startswith":return a.jqx.string.startsWith(d,c);case"startswithignorecase":return a.jqx.string.startsWithIgnoreCase(d,c);case"endswith":return a.jqx.string.endsWith(d,c);case"endswithignorecase":return a.jqx.string.endsWithIgnoreCase(d,c)}},_sort:function(b){var h=[],d=[],f=[],e;for(var c=0;c<b.length;c++){var e=b[c];var g=e;if(e.label){g=e.label}else{if(this.displayMember){g=e[this.displayMember]}}if(g.toString().toLowerCase().indexOf(this.query.toString().toLowerCase())===0){h.push(e)}else{if(g.toString().indexOf(this.query)>=0){d.push(e)}else{if(g.toString().toLowerCase().indexOf(this.query.toString().toLowerCase())>=0){f.push(e)}}}}return h.concat(d,f)},_highlight:function(c){var d=this.query;d=d.replace(/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g,"\\$&");var b=new RegExp("("+d+")","ig");return c.replace(b,function(e,f){return"<b>"+f+"</b>"})},_render:function(b){var c=this;b=a(b).map(function(e,f){var g=f;if(f.value!=undefined){if(f.label!=undefined){e=a(c.item).attr({"data-name":f.label,"data-value":f.value})}else{e=a(c.item).attr({"data-name":f.value,"data-value":f.value})}}else{if(f.label!=undefined){e=a(c.item).attr({"data-value":f.label,"data-name":f.label})}else{if(c.displayMember!=undefined&&c.displayMember!=""){e=a(c.item).attr({"data-name":f[c.displayMember],"data-value":f[c.valueMember]})}else{e=a(c.item).attr({"data-value":f,"data-name":f})}}}if(f.label){g=f.label}else{if(c.displayMember){g=f[c.displayMember]}}e.find("a").html(c.highlight(g));var d="";if(c.rtl){d=" "+c.toThemeProperty("jqx-rtl")}e[0].className=c.toThemeProperty("jqx-item")+" "+c.toThemeProperty("jqx-menu-item")+" "+c.toThemeProperty("jqx-rc-all")+d;return e[0]});b.first().addClass(this.toThemeProperty("jqx-fill-state-pressed"));this.$popup.html(b);if(!this.dropDownWidth){this.$popup.width(this.host.outerWidth()-6)}else{this.$popup.width(this.dropDownWidth)}return this},next:function(c){var d=this.$popup.find(".jqx-fill-state-pressed").removeClass(this.toThemeProperty("jqx-fill-state-pressed")),b=d.next();if(!b.length){b=a(this.$popup.find("li")[0])}b.addClass(this.toThemeProperty("jqx-fill-state-pressed"))},prev:function(c){var d=this.$popup.find(".jqx-fill-state-pressed").removeClass(this.toThemeProperty("jqx-fill-state-pressed")),b=d.prev();if(!b.length){b=this.$popup.find("li").last()}b.addClass(this.toThemeProperty("jqx-fill-state-pressed"))},addHandlers:function(){this.addHandler(this.host,"focus",a.proxy(this.onFocus,this));this.addHandler(this.host,"blur",a.proxy(this.onBlur,this));this.addHandler(this.host,"keypress",a.proxy(this.keypress,this));this.addHandler(this.host,"keyup",a.proxy(this.keyup,this));this.addHandler(this.host,"keydown",a.proxy(this.keydown,this));this.addHandler(this.$popup,"mousedown",a.proxy(this.click,this));if(this.host.on){this.$popup.on("mouseenter","li",a.proxy(this.mouseenter,this))}else{this.$popup.bind("mouseenter","li",a.proxy(this.mouseenter,this))}},removeHandlers:function(){this.removeHandler(this.host,"focus",a.proxy(this.onFocus,this));this.removeHandler(this.host,"blur",a.proxy(this.onBlur,this));this.removeHandler(this.host,"keypress",a.proxy(this.keypress,this));this.removeHandler(this.host,"keyup",a.proxy(this.keyup,this));this.removeHandler(this.host,"keydown",a.proxy(this.keydown,this));this.removeHandler(this.$popup,"mousedown",a.proxy(this.click,this));if(this.host.off){this.$popup.off("mouseenter","li",a.proxy(this.mouseenter,this))}else{this.$popup.unbind("mouseenter","li",a.proxy(this.mouseenter,this))}},move:function(b){if(!this.opened){return}switch(b.keyCode){case 9:case 13:case 27:b.preventDefault();break;case 38:if(!b.shiftKey){b.preventDefault();this.prev()}break;case 40:if(!b.shiftKey){b.preventDefault();this.next()}break}b.stopPropagation()},keydown:function(b){this.suppressKeyPressRepeat=~a.inArray(b.keyCode,[40,38,9,13,27]);this.move(b)},keypress:function(b){if(this.suppressKeyPressRepeat){return}this.move(b)},keyup:function(c){switch(c.keyCode){case 40:case 38:case 16:case 17:case 18:break;case 9:case 13:if(!this.opened){return}this.select(c,this);break;case 27:if(!this.opened){return}this.close();break;default:var b=this;if(this.timer){clearTimeout(this.timer)}this.timer=setTimeout(function(){b.suggest()},300)}c.stopPropagation();c.preventDefault()},clear:function(){this.host.val("")},onBlur:function(c){var b=this;setTimeout(function(){b.close()},150);b.host.removeClass(b.toThemeProperty("jqx-fill-state-focus"));this.value=this.host.val()},onFocus:function(c){var b=this;b.host.addClass(b.toThemeProperty("jqx-fill-state-focus"))},click:function(b){b.stopPropagation();b.preventDefault();this.select(b,this)},mouseenter:function(b){this.$popup.find(".jqx-fill-state-pressed").removeClass(this.toThemeProperty("jqx-fill-state-pressed"));a(b.currentTarget).addClass(this.toThemeProperty("jqx-fill-state-pressed"))}})})(jqxBaseFramework);