/*
jQWidgets v3.9.1 (2015-Oct)
Copyright (c) 2011-2015 jQWidgets.
License: http://jqwidgets.com/license/
*/

(function(a){a.jqx.jqxWidget("jqxValidator","",{});a.extend(a.jqx._jqxValidator.prototype,{defineInstance:function(){var b={rules:null,scroll:true,focus:true,scrollDuration:300,scrollCallback:null,position:"right",arrow:true,animation:"fade",animationDuration:150,closeOnClick:true,onError:null,onSuccess:null,ownerElement:null,_events:["validationError","validationSuccess"],hintPositionOffset:5,_inputHint:[],rtl:false,hintType:"tooltip"};a.extend(true,this,b);return b},createInstance:function(){if(this.hintType=="label"&&this.animationDuration==150){this.animationDuration=0}this._configureInputs();this._removeEventListeners();this._addEventListeners()},destroy:function(){this._removeEventListeners();this.hide()},validate:function(q){var b=true,p,f=Infinity,j,h,c,k=[],o;this.updatePosition();var l=this;var d=0;for(var g=0;g<this.rules.length;g+=1){if(typeof this.rules[g].rule==="function"){d++}}this.positions=new Array();for(var g=0;g<this.rules.length;g+=1){var n=a(this.rules[g].input);if(typeof this.rules[g].rule==="function"){var m=function(s,r){p=s;if(false==p){b=false;var i=a(r.input);c=a(r.input);k.push(c);var t=c.offset();if(t){j=t.top;if(f>j){f=j;h=c}}}d--;if(d==0){if(typeof q==="function"){l._handleValidation(b,f,h,k);if(q){q(b)}}}};this._validateRule(this.rules[g],m)}else{p=this._validateRule(this.rules[g])}if(false==p){b=false;c=a(this.rules[g].input);k.push(c);var e=c.offset();if(e){j=e.top;if(f>j){f=j;h=c}}}}if(d==0){this._handleValidation(b,f,h,k);return b}else{return undefined}},validateInput:function(b){var e=this._getRulesForInput(b),d=true;for(var c=0;c<e.length;c+=1){if(!this._validateRule(e[c])){d=false}}return d},hideHint:function(b){var d=this._getRulesForInput(b);for(var c=0;c<d.length;c+=1){this._hideHintByRule(d[c])}},hide:function(){var c;for(var b=0;b<this.rules.length;b+=1){c=this.rules[b];this._hideHintByRule(this.rules[b])}},updatePosition:function(){var c;this.positions=new Array();for(var b=0;b<this.rules.length;b+=1){c=this.rules[b];if(c.hint){this._hintLayout(c.hint,a(c.input),c.position,c)}}},_getRulesForInput:function(b){var d=[];for(var c=0;c<this.rules.length;c+=1){if(this.rules[c].input===b){d.push(this.rules[c])}}return d},_validateRule:function(f,i){var b=a(f.input),h,e=true;var d=this;var g=function(k){if(!k){var j=d.animation;d.animation=null;if(f.hint){d._hideHintByRule(f)}if(a(b).css("display")=="none"){d._hideHintByRule(f);return}if(a(b).parents().length==0){d._hideHintByRule(f);return}h=f.hintRender.apply(d,[f.message,b]);d._hintLayout(h,b,f.position,f);d._showHint(h);f.hint=h;d._removeLowPriorityHints(f);if(i){i(false,f)}d.animation=j}else{d._hideHintByRule(f);if(i){i(true,f)}}};var c=false;if(typeof f.rule==="function"){c=f.rule.call(this,b,g);if(c==true&&i){i(true,f)}}if(typeof f.rule==="function"&&c==false){if(typeof f.hintRender==="function"&&!f.hint&&!this._higherPriorityActive(f)&&b.is(":visible")){h=f.hintRender.apply(this,[f.message,b]);this._removeLowPriorityHints(f);this._hintLayout(h,b,f.position,f);this._showHint(h);f.hint=h}e=false;if(i){i(false,f)}}else{this._hideHintByRule(f)}return e},_hideHintByRule:function(e){var c=a(e.input);var b=this,f;var d=function(){if(b.hintType!="label"){return}var g=b;if(g.position=="top"||g.position=="left"){if(c.prev().hasClass(".jqx-validator-error-label")){return}}else{if(c.next().hasClass(".jqx-validator-error-label")){return}}if(c[0].nodeName.toLowerCase()!="input"){if(c.find("input").length>0){if(c.find(".jqx-input").length>0){c.find(".jqx-input").removeClass(g.toThemeProperty("jqx-validator-error-element"))}else{if(c.is(".jqx-checkbox")){c.find(".jqx-checkbox-default").removeClass(g.toThemeProperty("jqx-validator-error-element"))}}if(c.is(".jqx-radiobutton")){c.find(".jqx-radiobutton-default").removeClass(g.toThemeProperty("jqx-validator-error-element"))}else{c.removeClass(g.toThemeProperty("jqx-validator-error-element"))}}}else{c.removeClass(g.toThemeProperty("jqx-validator-error-element"))}};if(e){f=e.hint;if(f){if(this.positions){if(this.positions[Math.round(f.offset().top)+"_"+Math.round(f.offset().left)]){this.positions[Math.round(f.offset().top)+"_"+Math.round(f.offset().left)]=null}}if(this.animation==="fade"){f.fadeOut(this.animationDuration,function(){f.remove();d()})}else{f.remove();d()}}e.hint=null}},_handleValidation:function(b,e,d,c){if(!b){this._scrollHandler(e);if(this.focus){d.focus()}this._raiseEvent(0,{invalidInputs:c});if(typeof this.onError==="function"){this.onError(c)}}else{this._raiseEvent(1);if(typeof this.onSuccess==="function"){this.onSuccess()}}},_scrollHandler:function(c){if(this.scroll){var b=this;a("html,body").animate({scrollTop:c},this.scrollDuration,function(){if(typeof b.scrollCallback==="function"){b.scrollCallback.call(b)}})}},_higherPriorityActive:function(d){var e=false,c;for(var b=this.rules.length-1;b>=0;b-=1){c=this.rules[b];if(e&&c.input===d.input&&c.hint){return true}if(c===d){e=true}}return false},_removeLowPriorityHints:function(d){var e=false,c;for(var b=0;b<this.rules.length;b+=1){c=this.rules[b];if(e&&c.input===d.input){this._hideHintByRule(c)}if(c===d){e=true}}},_getHintRuleByInput:function(b){var d;for(var c=0;c<this.rules.length;c+=1){d=this.rules[c];if(a(d.input)[0]===b[0]&&d.hint){return d}}return null},_removeEventListeners:function(){var f,b,e;for(var d=0;d<this.rules.length;d+=1){f=this.rules[d];e=f.action.split(",");b=a(f.input);for(var c=0;c<e.length;c+=1){this.removeHandler(b,a.trim(e[c])+".jqx-validator")}}},_addEventListeners:function(){var f,c;if(this.host.parents(".jqx-window").length>0){var b=this;var g=function(){b.updatePosition()};var e=this.host.parents(".jqx-window");this.addHandler(e,"closed",function(){b.hide()});this.addHandler(e,"moved",g);this.addHandler(e,"moving",g);this.addHandler(e,"resized",g);this.addHandler(e,"resizing",g);this.addHandler(a(document.parentWindow),"scroll",function(){if(b.scroll){g()}})}for(var d=0;d<this.rules.length;d+=1){f=this.rules[d];c=a(f.input);this._addListenerTo(c,f)}},_addListenerTo:function(c,h){var b=this,f=h.action.split(",");var e=false;if(this._isjQWidget(c)){e=true}for(var d=0;d<f.length;d+=1){var g=a.trim(f[d]);if(e&&(g=="blur"||g=="focus")){if(c&&c[0].nodeName.toLowerCase()!="input"){c=c.find("input")}}this.addHandler(c,g+".jqx-validator",function(i){b._validateRule(h)})}},_configureInputs:function(){var b,d;this.rules=this.rules||[];for(var c=0;c<this.rules.length;c+=1){this._handleInput(c)}},_handleInput:function(b){var c=this.rules[b];if(!c.position){c.position=this.position}if(!c.message){c.message="Validation Failed!"}if(!c.action){c.action="blur"}if(!c.hintRender){c.hintRender=this._hintRender}if(!c.rule){c.rule=null}else{this._handleRule(c)}},_handleRule:function(f){var c=f.rule,e,d,b=false;if(typeof c==="string"){if(c.indexOf("=")>=0){c=c.split("=");d=c[1].split(",");c=c[0]}e=this["_"+c];if(e){f.rule=function(g,h){return e.apply(this,[g].concat(d))}}else{b=true}}else{if(typeof c!=="function"){b=true}else{f.rule=c}}if(b){throw new Error("Wrong parameter!")}},_required:function(b){switch(this._getType(b)){case"textarea":case"password":case"jqx-input":case"text":var d=a.data(b[0]);if(d.jqxMaskedInput){var e=b.jqxMaskedInput("promptChar"),c=b.jqxMaskedInput("value");return c&&c.indexOf(e)<0}else{if(d.jqxNumberInput){return b.jqxNumberInput("inputValue")!==""}else{if(d.jqxDateTimeInput){return true}else{return a.trim(b.val())!==""}}}case"checkbox":return b.is(":checked");case"radio":return b.is(":checked");case"div":if(b.is(".jqx-checkbox")){return b.jqxCheckBox("checked")}if(b.is(".jqx-radiobutton")){return b.jqxRadioButton("checked")}return false}return false},_notNumber:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=/\d/;return !c.test(d)})},_startWithLetter:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=/\d/;return !c.test(d.substring(0,1))})},_number:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=new Number(d);return !isNaN(c)&&isFinite(c)})},_phone:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=/^\(\d{3}\)(\d){3}-(\d){4}$/;return c.test(d)})},_length:function(c,d,b){return this._minLength(c,d)&&this._maxLength(c,b)},_maxLength:function(c,b){b=parseInt(b,10);return this._validateText(c,function(d){return d.length<=b})},_minLength:function(c,b){b=parseInt(b,10);return this._validateText(c,function(d){return d.length>=b})},_email:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;return c.test(d)})},_zipCode:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=/^(^\d{5}$)|(^\d{5}-\d{4}$)|(\d{3}-\d{2}-\d{4})$/;return c.test(d)})},_ssn:function(b){return this._validateText(b,function(d){if(d==""){return true}var c=/\d{3}-\d{2}-\d{4}/;return c.test(d)})},_validateText:function(b,d){var c;if(this._isTextInput(b)){if(this._isjQWidget(b)){if(b.find("input").length>0){c=b.find("input").val()}else{c=b.val()}}else{c=b.val()}return d(c)}return false},_isjQWidget:function(b){var c=a.data(b[0]);if(c.jqxMaskedInput||c.jqxNumberInput||c.jqxDateTimeInput){return true}return false},_isTextInput:function(b){var c=this._getType(b);return c==="text"||c==="textarea"||c==="password"||b.is(".jqx-input")},_getType:function(c){if(!c[0]){return}var b=c[0].tagName.toLowerCase(),d;if(b==="textarea"){return"textarea"}else{if(c.is(".jqx-input")){return"jqx-input"}else{if(b==="input"){d=a(c).attr("type")?a(c).attr("type").toLowerCase():"text";return d}}}return b},_hintRender:function(e,c){if(this.hintType=="label"){var f=a('<label class="'+this.toThemeProperty("jqx-validator-error-label")+'"></label>');f.html(e);var d=this;if(this.closeOnClick){f.click(function(){d.hideHint(c.selector)})}if(this.position=="left"||this.position=="top"){f.insertBefore(a(c))}else{f.insertAfter(a(c))}return f}var f=a('<div class="'+this.toThemeProperty("jqx-validator-hint")+' jqx-rc-all"></div>'),b=this;f.html(e);if(this.closeOnClick){f.click(function(){b.hideHint(c.selector)})}if(this.ownerElement==null){f.appendTo(document.body)}else{if(this.ownerElement.innerHTML){f.appendTo(a(this.ownerElement))}else{f.appendTo(this.ownerElement)}}return f},_hintLayout:function(h,c,b,f){if(this._hintRender===f.hintRender){var i;i=this._getPosition(c,b,h,f);if(this.hintType=="label"){var e="2px";if(this.position=="left"||this.position=="top"){e="-2px"}if(c[0].nodeName.toLowerCase()!="input"){if(c.find("input").length>0){if(c.find(".jqx-input").length>0){c.find(".jqx-input").addClass(this.toThemeProperty("jqx-validator-error-element"))}else{if(c.is(".jqx-checkbox")){c.find(".jqx-checkbox-default").addClass(this.toThemeProperty("jqx-validator-error-element"))}}if(c.is(".jqx-radiobutton")){c.find(".jqx-radiobutton-default").addClass(this.toThemeProperty("jqx-validator-error-element"))}else{c.addClass(this.toThemeProperty("jqx-validator-error-element"))}}}else{c.addClass(this.toThemeProperty("jqx-validator-error-element"))}var d=a("<span></span>");d.addClass(this.toThemeProperty("jqx-validator-hint"));d.html(h.text());d.appendTo(a(document.body));var g=d.outerWidth();d.remove();h.css({position:"relative",left:a(c).css("margin-left"),width:a(c).width(),top:e});if(b=="center"){h.css("width",g);h.css("left","0px");h.css("margin-left","auto");h.css("margin-right","auto")}return}h.css({position:"absolute",left:i.left,top:i.top});if(this.arrow){this._addArrow(c,h,b,i)}}},_showHint:function(b){if(b){if(this.animation==="fade"){b.fadeOut(0);b.fadeIn(this.animationDuration)}}},_getPosition:function(i,f,d,g){var e=i.offset(),h,c;var b=i.outerWidth();var j=i.outerHeight();if(this.rtl&&f.indexOf("left")>=0){f="right"}if(this.rtl&&f.indexOf("right")>=0){f="left"}if(this.ownerElement!=null){e={left:0,top:0};e.top=parseInt(e.top)+i.position().top;e.left=parseInt(e.left)+i.position().left}if(g&&g.hintPositionRelativeElement){var k=a(g.hintPositionRelativeElement);e=k.offset();b=k.width();j=k.height()}if(f.indexOf("top")>=0){h=e.top-j}else{if(f.indexOf("bottom")>=0){h=e.top+d.outerHeight()+this.hintPositionOffset+5}else{h=e.top}}if(f.indexOf("center")>=0){c=e.left+this.hintPositionOffset+(b-d.outerWidth())/2}else{if(f.indexOf("left")>=0){c=e.left-d.outerWidth()-this.hintPositionOffset}else{if(f.indexOf("right")>=0){c=e.left+b+this.hintPositionOffset}else{c=e.left+this.hintPositionOffset}}}if(f.indexOf(":")>=0){f=f.split(":")[1].split(",");c+=parseInt(f[0],10);h+=parseInt(f[1],10)}if(!this.positions){this.positions=new Array()}if(this.positions[Math.round(h)+"_"+Math.round(c)]){if(this.positions[Math.round(h)+"_"+Math.round(c)].top==h){h+=i.outerHeight()}}this.positions[Math.round(h)+"_"+Math.round(c)]={left:c,top:h};return{left:c,top:h}},_addArrow:function(j,e,g,k){var l=a('<div class="'+this.toThemeProperty("jqx-validator-hint-arrow")+'"></div>'),d,i;if(this.rtl&&g.indexOf("left")>=0){g="right"}if(this.rtl&&g.indexOf("right")>=0){g="left"}e.children(".jqx-validator-hint-arrow").remove();e.append(l);var c=l.outerHeight(),f=l.outerWidth(),h=e.outerHeight(),b=e.outerWidth();this._addImage(l);if(g.indexOf("top")>=0){i=h-c}else{if(g.indexOf("bottom")>=0){i=-c}else{i=(h-c)/2-c/2}}if(g.indexOf("center")>=0){d=(b-f)/2}else{if(g.indexOf("left")>=0){d=b-f/2-1}else{if(g.indexOf("right")>=0){d=-f/2}}}if(g.indexOf("topright")>=0||g.indexOf("bottomright")>=0){d=0}if(g.indexOf("topleft")>=0||g.indexOf("bottomleft")>=0){d=b-f}l.css({position:"absolute",left:d,top:i})},_addImage:function(b){var c=b.css("background-image");c=c.replace('url("',"");c=c.replace('")',"");c=c.replace("url(","");c=c.replace(")","");b.css("background-image","none");b.append('<img src="'+c+'" alt="Arrow" style="position: relative; top: 0px; left: 0px; width: '+b.width()+"px; height: "+b.height()+'px;" />')},_raiseEvent:function(b,d){var c=a.Event(this._events[b]);c.args=d;return this.host.trigger(c)},propertyChangedHandler:function(b,c,e,d){if(c==="rules"){this._configureInputs();this._removeEventListeners();this._addEventListeners()}}})})(jqxBaseFramework);