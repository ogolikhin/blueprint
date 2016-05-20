/*
jQWidgets v3.9.1 (2015-Oct)
Copyright (c) 2011-2015 jQWidgets.
License: http://jqwidgets.com/license/
*/

(function(a){a.extend(a.jqx._jqxGrid.prototype,{getcolumnindex:function(b){var c=this.getcolumn(b);var d=this.columns.records.indexOf(c);return d},setcolumnindex:function(d,h,i){var g=this.getcolumn(d);if(g.pinned){return}if(g.hidden){return}if(g.checkboxcolumn){return}if(g.grouped){return}var k=this.columns.records.indexOf(g);this.columns.records.splice(k,1);this.columns.records.splice(h,0,g);var e=0;var m=this.headerZIndex;this.columnsrow.children().detach();var j=this.toThemeProperty("jqx-grid-cell");j+=" "+this.toThemeProperty("jqx-grid-cell-pinned");if(this.filterrow){a(this.filterrow.children()[0]).children().detach();this.filterrow[0].cells=[]}var l=this;var b=null;if(l.filterrow!=undefined){var b=a(l.filterrow.children()[0])}this.columnsrow[0].cells=[];var f=false;a.each(this.columns.records,function(n,q){var o=this.uielement;l.columnsrow.append(o);if(!l.rtl){o.css("z-index",m--)}else{o.css("z-index",m++)}var p=this.width;o.css("left",e);l.columnsrow[0].cells[l.columnsrow[0].cells.length]=o[0];if(l.filterrow){var r=a('<div style="overflow: hidden; position: absolute; height: 100%;" class="'+j+'"></div>');b.append(r);r.css("left",e);r.css("z-index",m+1);r.width(this.width);r[0].left=e;r.append(this._filterwidget);l.filterrow[0].cells[l.filterrow[0].cells.length]=r[0]}if(this.hidden){f=true}if(!(this.hidden&&this.hideable)){e+=p}});if(this.groupable){var c=this.groups.length;if(c>0){if(k-c>=0){k-=c;h-=c}}}if(this.rowdetails){if(k-1>=0){k--;h--}}if(this.selectionmode=="checkbox"){if(k-1>=0){k--;h--}}var g=this._columns[k];this._columns.splice(k,1);this._columns.splice(h,0,g);this._raiseEvent(24,{columntext:g.text,datafield:g.datafield,oldindex:k,newindex:h});if(i==false){return}if(f||g.columntype=="checkbox"){this.prerenderrequired=true;this.rendergridcontent(true,false);this._updatecolumnwidths();this._updatecellwidths()}else{this._updatecolumnwidths();this._updatecellwidths()}if(this._updatefilterrowui&&this.filterable&&this.showfilterrow){this._updatefilterrowui()}if(this.showeverpresentrow){this._updateaddnewrowui()}this._rendercolumngroups();this._renderrows(this.virtualsizeinfo)},_pinnedColumnsLength:function(){var b=0;a.each(this.columns.records,function(){if(this.pinned){b++}if(this.grouped){b++}});if(this.selectionmode=="checkbox"){b++}return b},_handlecolumnsreorder:function(){var d=this;var g=-1;var c=false;if(!d.columnsreorder){return}var f="mousemove.reorder"+this.element.id;var e="mousedown.reorder"+this.element.id;var h="mouseup.reorder"+this.element.id;var b=false;if(this.isTouchDevice()&&this.touchmode!==true){b=true;f=a.jqx.mobile.getTouchEventName("touchmove")+".reorder"+this.element.id;e=a.jqx.mobile.getTouchEventName("touchstart")+".reorder"+this.element.id;h=a.jqx.mobile.getTouchEventName("touchend")+".reorder"+this.element.id}this.removeHandler(a(document),f);this.addHandler(a(document),f,function(j){if(d.resizing){return true}if(d.reordercolumn!=null){var k=parseInt(j.pageX);var r=parseInt(j.pageY);if(b){var o=d.getTouches(j);var n=o[0];if(n!=undefined){k=parseInt(n.pageX);r=parseInt(n.pageY)}}var m=d.host.coord();var s=parseInt(m.left);var t=parseInt(m.top);if(d.dragmousedownoffset==undefined||d.dragmousedownoffset==null){d.dragmousedownoffset={left:0,top:0}}var q=parseInt(k)-parseInt(d.dragmousedownoffset.left);var i=parseInt(r)-parseInt(d.dragmousedownoffset.top);d.reordercolumn.css({left:q+"px",top:i+"px"});c=false;if(k>=s&&k<=s+d.host.width()){if(r>=t&&r<=t+d.host.height()){c=true}}g=-1;if(c){d.reordercolumnicon.removeClass(d.toThemeProperty("jqx-grid-dragcancel-icon"));d.reordercolumnicon.addClass(d.toThemeProperty("jqx-grid-drag-icon"));var p=d.columnsheader.coord();var l=p.top+d.columnsheader.height();if(d.columnsdropline!=null){if(r>=p.top&&r<=l){g=d._handlereordercolumnsdroplines(k)}else{d.columnsdropline.fadeOut("slow")}}}else{if(d.columnsdropline!=null){d.columnsdropline.fadeOut("slow")}d.reordercolumnicon.removeClass(d.toThemeProperty("jqx-grid-drag-icon"));d.reordercolumnicon.addClass(d.toThemeProperty("jqx-grid-dragcancel-icon"))}if(b){j.preventDefault();j.stopPropagation();return false}}});this.columnsbounds=new Array();this.removeHandler(a(document),e);this.addHandler(a(document),e,function(j){if(d.resizing){return true}d.columnsbounds=new Array();var l=d.host.coord().left;var k=d.host.coord().top;if(d.showtoolbar){k+=d.toolbarheight}if(d.groupable&&d.showgroupsheader){k+=d.groupsheaderheight}var i=0;a.each(d.columns.records,function(n){var o=this;if(o.hidden){d.columnsbounds[d.columnsbounds.length]={top:k,column:o,left:l,width:0,height:2+d.rowsheight};return true}if(i==0){if(!d.rtl){l=parseInt(d.host.coord().left)-d.hScrollInstance.value}else{if(d.hScrollBar.css("visibility")!="hidden"){l=parseInt(d.host.coord().left)-d.hScrollInstance.max+d.hScrollInstance.value}else{l=parseInt(d.host.coord().left)+d.hScrollInstance.value}}}i++;var m=2+d.columnsheight;if(d.columnshierarchy){k=a(o.uielement).coord().top;m=a(o.uielement).height()}d.columnsbounds[d.columnsbounds.length]={top:k,column:o,left:l,width:o.width,height:m};l+=o.width})});this.removeHandler(a(document),h);this.addHandler(a(document),h,function(i){if(d.resizing){return true}d.__drag=false;a(document.body).removeClass("jqx-disableselect");var l=parseInt(i.pageX);var t=parseInt(i.pageY);if(b){var o=d.getTouches(i);var n=o[0];l=parseInt(n.pageX);t=parseInt(n.pageY)}var m=d.host.coord();var v=parseInt(m.left);var w=parseInt(m.top);var j=d.groupsheader.height();if(d.showtoolbar){w+=d.toolbarheight}d.columndragstarted=false;d.dragmousedown=null;if(d.reordercolumn!=null){var k=a.data(d.reordercolumn[0],"reorderrecord");var x=d.columns.records.indexOf(d.getcolumn(k));d.reordercolumn.remove();d.reordercolumn=null;var p=0;p+=d._pinnedColumnsLength();if(k!=null){if(c){if(g!=-1){var q=g.index;if(q>=p){var s=d.columns.records[q];if(s!=undefined){var u=d.columns.records.indexOf(d.getcolumn(s.datafield));if(s.datafield==null){var u=d.columns.records.indexOf(d.getcolumnbytext(s.text))}if(d.columngroups){var r=s;if(x<u){if(g.position=="before"){r=d.columns.records[u-1]}}if(r.columngroup!=d.getcolumn(k).columngroup){if(d.columnsdropline!=null){d.columnsdropline.remove();d.columnsdropline=null}return}}if(x<u){if(g.position=="before"){d.setcolumnindex(k,u-1)}else{d.setcolumnindex(k,u)}}else{if(x>u){d.setcolumnindex(k,u)}}if(d.autosavestate){if(d.savestate){d.savestate()}}}}}}if(d.columnsdropline!=null){d.columnsdropline.remove();d.columnsdropline=null}}}})},getcolumnbytext:function(c){var b=null;if(this.columns.records){a.each(this.columns.records,function(){if(this.text==c){b=this;return false}})}return b},_handlereordercolumnsdroplines:function(h){var b=this;var g=-1;var d=b._pinnedColumnsLength();var f=parseInt(b.host.coord().left);var c=f+b.host.width();var e=b.vScrollBar.css("visibility")!="hidden"?19:0;if(!b.rtl){e=0}a.each(b.columnsbounds,function(i){if(i>=d){if(this.width==0){return true}if(h<=this.left+this.width/2){if(h>c){b.columnsdropline.fadeOut();return false}b.columnsdropline.css("left",e+parseInt(this.left)+"px");b.columnsdropline.css("top",parseInt(this.top)+"px");b.columnsdropline.height(this.height);b.columnsdropline.fadeIn("slow");g={index:i,position:"before"};return false}else{if(h>=this.left+this.width/2){if(this.left+this.width>c){b.columnsdropline.fadeOut();return false}b.columnsdropline.css("left",e+1+this.left+this.width);b.columnsdropline.css("top",this.top);b.columnsdropline.height(this.height);b.columnsdropline.fadeIn("slow");g={index:i,position:"after"}}}}});return g},_createreordercolumn:function(c,e,h){var g=this;var f=e;if(g.reordercolumn){g.reordercolumn.remove()}if(g.columnsdropline){g.columnsdropline.remove()}g.reordercolumn=a("<div></div>");var j=c.clone();g.reordercolumn.css("z-index",999999);j.css("border-width","1px");j.css("opacity","0.4");var i=a(j.find("."+g.toThemeProperty("jqx-grid-column-menubutton")));if(i.length>0){i.css("display","none")}var b=a(j.find(".jqx-icon-close"));if(b.length>0){b.css("display","none")}g.reordercolumnicon=a('<div style="z-index: 9999; position: absolute; left: 100%; top: 50%; margin-left: -18px; margin-top: -7px;"></div>');g.reordercolumnicon.addClass(g.toThemeProperty("jqx-grid-drag-icon"));g.reordercolumn.css("float","left");g.reordercolumn.css("position","absolute");var d=g.host.coord();j.width(c.width()+16);g.reordercolumn.append(j);g.reordercolumn.height(c.height());g.reordercolumn.width(j.width());g.reordercolumn.append(g.reordercolumnicon);a(document.body).append(g.reordercolumn);j.css("margin-left",0);j.css("left",0);j.css("top",0);g.reordercolumn.css("left",f.left+g.dragmousedown.left);g.reordercolumn.css("top",f.top+g.dragmousedown.top);if(h!=undefined&&h){g.columnsdropline=a('<div style="z-index: 9999; display: none; position: absolute;"></div>');g.columnsdropline.width(2);g.columnsdropline.addClass(g.toThemeProperty("jqx-grid-group-drag-line"));a(document.body).append(g.columnsdropline)}},_handlecolumnsdragreorder:function(c,f){if(this.reordercolumn){this.reordercolumn.remove()}if(this.columnsdropline){this.columnsdropline.remove()}this.dragmousedown=null;this.dragmousedownoffset=null;this.columndragstarted=false;this.reordercolumn=null;var g=this;var e;var b=false;if(this.isTouchDevice()&&this.touchmode!==true){b=true}var d="mousedown.drag";var e="mousemove.drag";if(b){d=a.jqx.mobile.getTouchEventName("touchstart")+".drag";e=a.jqx.mobile.getTouchEventName("touchmove")+".drag"}else{this.addHandler(f,"dragstart",function(h){return false})}this.addHandler(f,d,function(j){if(false==c.draggable){return true}if(g.resizing){return true}g.__drag=true;var i=j.pageX;var h=j.pageY;if(b){var k=g.getTouches(j);var m=k[0];i=m.pageX;h=m.pageY}g.dragmousedown={left:i,top:h};var l=a(j.target).coord();g.dragmousedownoffset={left:parseInt(i)-parseInt(l.left),top:parseInt(h-l.top)};return true});this.addHandler(f,e,function(j){if(!c.draggable){return true}if(undefined==c.datafield){return true}if(c.pinned){return true}if(g.resizing){return true}if(g.dragmousedown){var i=j.pageX;var h=j.pageY;if(b){var l=g.getTouches(j);var n=l[0];if(n!=undefined){i=n.pageX;h=n.pageY}}e={left:i,top:h};if(!g.columndragstarted&&g.reordercolumn==null){var k=Math.abs(e.left-g.dragmousedown.left);var m=Math.abs(e.top-g.dragmousedown.top);if(k>3||m>3){g._createreordercolumn(f,e,true);a(document.body).addClass("jqx-disableselect");a.data(g.reordercolumn[0],"reorderrecord",c.datafield)}}}})}})})(jqxBaseFramework);