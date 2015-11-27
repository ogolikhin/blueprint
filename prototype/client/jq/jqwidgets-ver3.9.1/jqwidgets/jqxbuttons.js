/*
jQWidgets v3.9.1 (2015-Oct)
Copyright (c) 2011-2015 jQWidgets.
License: http://jqwidgets.com/license/
*/

(function(a){a.jqx.cssroundedcorners=function(b){var c={all:"jqx-rc-all",top:"jqx-rc-t",bottom:"jqx-rc-b",left:"jqx-rc-l",right:"jqx-rc-r","top-right":"jqx-rc-tr","top-left":"jqx-rc-tl","bottom-right":"jqx-rc-br","bottom-left":"jqx-rc-bl"};for(prop in c){if(!c.hasOwnProperty(prop)){continue}if(b==prop){return c[prop]}}};a.jqx.jqxWidget("jqxButton","",{});a.extend(a.jqx._jqxButton.prototype,{defineInstance:function(){var b={cursor:"arrow",roundedCorners:"all",disabled:false,height:null,width:null,overrideTheme:false,enableHover:true,enableDefault:true,enablePressed:true,rtl:false,_ariaDisabled:false,_scrollAreaButton:false,template:"default",aria:{"aria-disabled":{name:"disabled",type:"boolean"}}};a.extend(true,this,b);return b},createInstance:function(d){var b=this;b._setSize();if(!b._ariaDisabled){b.host.attr("role","button")}if(!b.overrideTheme){b.host.addClass(b.toThemeProperty(a.jqx.cssroundedcorners(b.roundedCorners)));if(b.enableDefault){b.host.addClass(b.toThemeProperty("jqx-button"))}b.host.addClass(b.toThemeProperty("jqx-widget"))}b.isTouchDevice=a.jqx.mobile.isTouchDevice();if(!b._ariaDisabled){a.jqx.aria(this)}if(b.cursor!="arrow"){if(!b.disabled){b.host.css({cursor:b.cursor})}else{b.host.css({cursor:"arrow"})}}var g="mouseenter mouseleave mousedown focus blur";if(b._scrollAreaButton){var g="mousedown"}if(b.isTouchDevice){b.addHandler(b.host,a.jqx.mobile.getTouchEventName("touchstart"),function(h){b.isPressed=true;b.refresh()});b.addHandler(a(document),a.jqx.mobile.getTouchEventName("touchend")+"."+b.element.id,function(h){b.isPressed=false;b.refresh()})}b.addHandler(b.host,g,function(h){switch(h.type){case"mouseenter":if(!b.isTouchDevice){if(!b.disabled&&b.enableHover){b.isMouseOver=true;b.refresh()}}break;case"mouseleave":if(!b.isTouchDevice){if(!b.disabled&&b.enableHover){b.isMouseOver=false;b.refresh()}}break;case"mousedown":if(!b.disabled){b.isPressed=true;b.refresh()}break;case"focus":if(!b.disabled){b.isFocused=true;b.refresh()}break;case"blur":if(!b.disabled){b.isFocused=false;b.refresh()}break}});b.mouseupfunc=function(h){if(!b.disabled){if(b.isPressed||b.isMouseOver){b.isPressed=false;b.refresh()}}};b.addHandler(a(document),"mouseup.button"+b.element.id,b.mouseupfunc);try{if(document.referrer!=""||window.frameElement){if(window.top!=null&&window.top!=window.self){var f="";if(window.parent&&document.referrer){f=document.referrer}if(f.indexOf(document.location.host)!=-1){var e=function(h){b.isPressed=false;b.refresh()};if(window.top.document){b.addHandler(a(window.top.document),"mouseup",e)}}}}}catch(c){}b.propertyChangeMap.roundedCorners=function(h,j,i,k){h.host.removeClass(h.toThemeProperty(a.jqx.cssroundedcorners(i)));h.host.addClass(h.toThemeProperty(a.jqx.cssroundedcorners(k)))};b.propertyChangeMap.width=function(h,j,i,k){h._setSize();h.refresh()};b.propertyChangeMap.height=function(h,j,i,k){h._setSize();h.refresh()};b.propertyChangeMap.disabled=function(h,j,i,k){if(i!=k){h.host[0].disabled=k;h.host.attr("disabled",k);h.refresh();if(!k){h.host.css({cursor:h.cursor})}else{h.host.css({cursor:"default"})}a.jqx.aria(h,"aria-disabled",h.disabled)}};b.propertyChangeMap.rtl=function(h,j,i,k){if(i!=k){h.refresh()}};b.propertyChangeMap.template=function(h,j,i,k){if(i!=k){h.host.removeClass("jqx-"+i);h.refresh()}};b.propertyChangeMap.theme=function(h,j,i,k){h.host.removeClass();if(h.enableDefault){h.host.addClass(h.toThemeProperty("jqx-button"))}h.host.addClass(h.toThemeProperty("jqx-widget"));if(!h.overrideTheme){h.host.addClass(h.toThemeProperty(a.jqx.cssroundedcorners(h.roundedCorners)))}h._oldCSSCurrent=null;h.refresh()};if(b.disabled){b.element.disabled=true;b.host.attr("disabled",true)}},resize:function(c,b){this.width=c;this.height=b;this._setSize()},val:function(){var c=this;var b=c.host.find("input");if(b.length>0){if(arguments.length==0||typeof(value)=="object"){return b.val()}b.val(value);c.refresh();return b.val()}if(arguments.length==0||typeof(value)=="object"){if(c.element.nodeName.toLowerCase()=="button"){return a(c.element).text()}return c.element.value}c.element.value=arguments[0];if(c.element.nodeName.toLowerCase()=="button"){a(c.element).text(arguments[0])}c.refresh()},_setSize:function(){var b=this;if(b.width!=null&&(b.width.toString().indexOf("px")!=-1||b.width.toString().indexOf("%")!=-1)){b.host.css("width",b.width)}else{if(b.width!=undefined&&!isNaN(b.width)){b.host.css("width",b.width)}}if(b.height!=null&&(b.height.toString().indexOf("px")!=-1||b.height.toString().indexOf("%")!=-1)){b.host.css("height",b.height)}else{if(b.height!=undefined&&!isNaN(b.height)){b.host.css("height",parseInt(b.height))}}},_removeHandlers:function(){var b=this;b.removeHandler(b.host,"selectstart");b.removeHandler(b.host,"click");b.removeHandler(b.host,"focus");b.removeHandler(b.host,"blur");b.removeHandler(b.host,"mouseenter");b.removeHandler(b.host,"mouseleave");b.removeHandler(b.host,"mousedown");b.removeHandler(a(document),"mouseup.button"+b.element.id,b.mouseupfunc);if(b.isTouchDevice){b.removeHandler(b.host,a.jqx.mobile.getTouchEventName("touchstart"));b.removeHandler(a(document),a.jqx.mobile.getTouchEventName("touchend")+"."+b.element.id)}b.mouseupfunc=null;delete b.mouseupfunc},focus:function(){this.host.focus()},destroy:function(){var b=this;b._removeHandlers();var c=a.data(b.element,"jqxButton");if(c){delete c.instance}b.host.removeClass();b.host.removeData();b.host.remove();delete b.set;delete b.get;delete b.call;delete b.element;delete b.host},render:function(){this.refresh()},refresh:function(){var c=this;if(c.overrideTheme){return}var e=c.toThemeProperty("jqx-fill-state-focus");var i=c.toThemeProperty("jqx-fill-state-disabled");var b=c.toThemeProperty("jqx-fill-state-normal");if(!c.enableDefault){b=""}var h=c.toThemeProperty("jqx-fill-state-hover");var f=c.toThemeProperty("jqx-fill-state-pressed");var g=c.toThemeProperty("jqx-fill-state-pressed");if(!c.enablePressed){f=""}var d="";if(!c.host){return}c.host[0].disabled=c.disabled;if(c.disabled){d=b+" "+i;if(c.template!=="default"&&c.template!==""){d+=" jqx-"+c.template}c.host.addClass(d);c._oldCSSCurrent=d;return}else{if(c.isMouseOver&&!c.isTouchDevice){if(c.isPressed){d=g}else{d=h}}else{if(c.isPressed){d=f}else{d=b}}}if(c.isFocused){d+=" "+e}if(c.template!=="default"&&c.template!==""){d+=" jqx-"+c.template}if(d!=c._oldCSSCurrent){if(c._oldCSSCurrent){c.host.removeClass(c._oldCSSCurrent)}c.host.addClass(d);c._oldCSSCurrent=d}if(c.rtl){c.host.addClass(c.toThemeProperty("jqx-rtl"));c.host.css("direction","rtl")}}});a.jqx.jqxWidget("jqxLinkButton","",{});a.extend(a.jqx._jqxLinkButton.prototype,{defineInstance:function(){this.disabled=false;this.height=null;this.width=null;this.rtl=false;this.href=null},createInstance:function(d){var c=this;this.host.onselectstart=function(){return false};this.host.attr("role","button");var b=this.height||this.host.height();var e=this.width||this.host.width();this.href=this.host.attr("href");this.target=this.host.attr("target");this.content=this.host.text();this.element.innerHTML="";this.host.append("<input type='button' class='jqx-wrapper'/>");var f=this.host.find("input");f.addClass(this.toThemeProperty("jqx-reset"));f.width(e);f.height(b);f.val(this.content);this.host.find("tr").addClass(this.toThemeProperty("jqx-reset"));this.host.find("td").addClass(this.toThemeProperty("jqx-reset"));this.host.find("tbody").addClass(this.toThemeProperty("jqx-reset"));this.host.css("color","inherit");this.host.addClass(this.toThemeProperty("jqx-link"));f.css({width:e});f.css({height:b});var g=d==undefined?{}:d[0]||{};f.jqxButton(g);if(this.disabled){this.host[0].disabled=true}this.propertyChangeMap.disabled=function(h,j,i,k){h.host[0].disabled=k;h.host.find("input").jqxButton({disabled:k})};this.addHandler(f,"click",function(h){if(!this.disabled){c.onclick(h)}return false})},onclick:function(b){if(this.target!=null){window.open(this.href,this.target)}else{window.location=this.href}}});a.jqx.jqxWidget("jqxRepeatButton","jqxButton",{});a.extend(a.jqx._jqxRepeatButton.prototype,{defineInstance:function(){this.delay=50},createInstance:function(e){var c=this;var d=a.jqx.mobile.isTouchDevice();var b=!d?"mouseup."+this.base.element.id:"touchend."+this.base.element.id;var f=!d?"mousedown."+this.base.element.id:"touchstart."+this.base.element.id;this.addHandler(a(document),b,function(g){if(c.timeout!=null){clearTimeout(c.timeout);c.timeout=null;c.refresh()}if(c.timer!=undefined){clearInterval(c.timer);c.timer=null;c.refresh()}});this.addHandler(this.base.host,f,function(g){if(c.timer!=null){clearInterval(c.timer)}c.timeout=setTimeout(function(){clearInterval(c.timer);c.timer=setInterval(function(h){c.ontimer(h)},c.delay)},150)});this.mousemovefunc=function(g){if(!d){if(g.which==0){if(c.timer!=null){clearInterval(c.timer);c.timer=null}}}};this.addHandler(this.base.host,"mousemove",this.mousemovefunc)},destroy:function(){var c=a.jqx.mobile.isTouchDevice();var b=!c?"mouseup."+this.base.element.id:"touchend."+this.base.element.id;var e=!c?"mousedown."+this.base.element.id:"touchstart."+this.base.element.id;this.removeHandler(this.base.host,"mousemove",this.mousemovefunc);this.removeHandler(this.base.host,e);this.removeHandler(a(document),b);this.timer=null;delete this.mousemovefunc;delete this.timer;var d=a.data(this.base.element,"jqxRepeatButton");if(d){delete d.instance}a(this.base.element).removeData();this.base.destroy();delete this.base},stop:function(){clearInterval(this.timer);this.timer=null},ontimer:function(b){var b=new a.Event("click");if(this.base!=null&&this.base.host!=null){this.base.host.trigger(b)}}});a.jqx.jqxWidget("jqxToggleButton","jqxButton",{});a.extend(a.jqx._jqxToggleButton.prototype,{defineInstance:function(){this.toggled=false;this.uiToggle=true;this.aria={"aria-checked":{name:"toggled",type:"boolean"},"aria-disabled":{name:"disabled",type:"boolean"}}},createInstance:function(c){var b=this;b.base.overrideTheme=true;b.isTouchDevice=a.jqx.mobile.isTouchDevice();a.jqx.aria(this);b.propertyChangeMap.roundedCorners=function(d,f,e,g){d.base.host.removeClass(d.toThemeProperty(a.jqx.cssroundedcorners(e)));d.base.host.addClass(d.toThemeProperty(a.jqx.cssroundedcorners(g)))};b.propertyChangeMap.toggled=function(d,f,e,g){d.refresh()};b.propertyChangeMap.disabled=function(d,f,e,g){d.base.disabled=g;d.refresh()};b.addHandler(b.base.host,"click",function(d){if(!b.base.disabled&&b.uiToggle){b.toggle()}});if(!b.isTouchDevice){b.addHandler(b.base.host,"mouseenter",function(d){if(!b.base.disabled){b.refresh()}});b.addHandler(b.base.host,"mouseleave",function(d){if(!b.base.disabled){b.refresh()}})}b.addHandler(b.base.host,"mousedown",function(d){if(!b.base.disabled){b.refresh()}});b.addHandler(a(document),"mouseup.togglebutton"+b.base.element.id,function(d){if(!b.base.disabled){b.refresh()}})},destroy:function(){this._removeHandlers();this.base.destroy()},_removeHandlers:function(){this.removeHandler(this.base.host,"click");this.removeHandler(this.base.host,"mouseenter");this.removeHandler(this.base.host,"mouseleave");this.removeHandler(this.base.host,"mousedown");this.removeHandler(a(document),"mouseup.togglebutton"+this.base.element.id)},toggle:function(){this.toggled=!this.toggled;this.refresh();a.jqx.aria(this,"aria-checked",this.toggled)},unCheck:function(){this.toggled=false;this.refresh()},check:function(){this.toggled=true;this.refresh()},refresh:function(){var c=this;var h=c.base.toThemeProperty("jqx-fill-state-disabled");var b=c.base.toThemeProperty("jqx-fill-state-normal");if(!c.base.enableDefault){b=""}var g=c.base.toThemeProperty("jqx-fill-state-hover");var e=c.base.toThemeProperty("jqx-fill-state-pressed");var f=c.base.toThemeProperty("jqx-fill-state-pressed");var d="";c.base.host[0].disabled=c.base.disabled;if(c.base.disabled){d=b+" "+h;c.base.host.addClass(d);return}else{if(c.base.isMouseOver&&!c.isTouchDevice){if(c.base.isPressed||c.toggled){d=f}else{d=g}}else{if(c.base.isPressed||c.toggled){d=e}else{d=b}}}if(c.base.template!=="default"&&c.base.template!==""){d+=" jqx-"+c.base.template}if(c.base.host.hasClass(h)&&h!=d){c.base.host.removeClass(h)}if(c.base.host.hasClass(b)&&b!=d){c.base.host.removeClass(b)}if(c.base.host.hasClass(g)&&g!=d){c.base.host.removeClass(g)}if(c.base.host.hasClass(e)&&e!=d){c.base.host.removeClass(e)}if(c.base.host.hasClass(f)&&f!=d){c.base.host.removeClass(f)}if(!c.base.host.hasClass(d)){c.base.host.addClass(d)}}})})(jqxBaseFramework);