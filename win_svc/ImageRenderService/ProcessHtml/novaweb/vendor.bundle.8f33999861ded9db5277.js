!function(e){function t(n){if(r[n])return r[n].exports;var o=r[n]={exports:{},id:n,loaded:!1};return e[n].call(o.exports,o,o.exports,t),o.loaded=!0,o.exports}var n=window.webpackJsonp;window.webpackJsonp=function(a,u){for(var c,l,i=0,f=[];i<a.length;i++)l=a[i],o[l]&&f.push.apply(f,o[l]),o[l]=0;for(c in u)e[c]=u[c];for(n&&n(a,u);f.length;)f.shift().call(null,t);if(u[0])return r[0]=0,t(0)};var r={},o={2:0};return t.e=function(e,n){if(0===o[e])return n.call(null,t);if(void 0!==o[e])o[e].push(n);else{o[e]=[n];var r=document.getElementsByTagName("head")[0],a=document.createElement("script");a.type="text/javascript",a.charset="utf-8",a.async=!0,a.src=t.p+""+e+"."+({0:"app",1:"locales"}[e]||e)+".bundle."+{0:"be0102ca90d3b49fb774",1:"f57519b97c068aeaf5f4"}[e]+".js",r.appendChild(a)}},t.m=e,t.c=r,t.p="./novaweb",t(0)}({0:function(e,t,n){e.exports=n(315)},315:function(e,t,n){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),n(316),n(317),n(318)},316:function(e,t){},317:function(e,t){},318:function(e,t){try{var n=new window.CustomEvent("test");if(n.preventDefault(),n.defaultPrevented!==!0)throw new Error("Could not prevent default")}catch(e){var r=function(e,t){var n,r;return t=t||{bubbles:!1,cancelable:!1,detail:void 0},n=document.createEvent("CustomEvent"),n.initCustomEvent(e,t.bubbles,t.cancelable,t.detail),r=n.preventDefault,n.preventDefault=function(){r.call(this);try{Object.defineProperty(this,"defaultPrevented",{get:function(){return!0}})}catch(e){this.defaultPrevented=!0}},n};r.prototype=window.Event.prototype,window.CustomEvent=r}}});