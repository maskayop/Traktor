                                    if (window['ct'] && typeof window['ct'] === 'function') {
                                    window['ct']('replace', { selector: 'call_phone_mtz_1', value: '8 (495) 153 85 18', type: 'class', useObserver: false });
                                    }
                                window.calltouch_phone = "74951538518";            if (window['ct'] && typeof window['ct'] === 'function') {
            window['ct']('create_session', {
                sessionId: 4023220950,
                country: "ru",
                siteId: 17519,
                modId: '710bff10',
                setCookie: true,
                endSessionTime: 1784192781,
                domain: 'mtz.ru',
                setCtCookie: '3700000002094603078',
                setLkCookie: null,
                denialTime: 15,
                phones: {"64876":{"subPoolName":"\u041e\u0441\u0442\u0430\u043b\u044c\u043d\u044b\u0435","phoneId":"211504","phoneNumber":"74951538518","phoneCode":"495","phoneBody":"1538518"}},
                emails: [],
                ecommerceGa4Enabled: false,
                ecommerceTimeout: 1000,
                calltouchDnsHost: '',
                dataGoEnabled: false,
                GA4: [],
                quietMediaEnabled: false,
                fields: 'base,webgl,canvas,audio',
                isGtagEcom: false,
                cookieHash: '',
                firstPartyUrl: ''
            });
                        window['ct']('session_data', {"mod_id":"710bff10","source":"yandex","medium":"organic","utm_source":"","utm_medium":"","utm_campaign":"","utm_content":"","utm_term":"","keyword":"\u043d\u0430\u0436\u0430\u0442\u0438\u0435 \u043d\u0430 \u043f\u0435\u0434\u0430\u043b\u044c \u0441\u0446\u0435\u043f\u043b\u0435\u043d\u0438\u044f \u043c\u0442\u0437 1221","city":"kazan","region":"tatarstan","country":"","url":"https:\/\/mtz.ru\/novosti\/13-rukovodstva\/51-belarus-92p-rukovodstvo-po-ekspluatatsii","deviceType":"desktop"});
                        } else {
            var xmlHttp = new XMLHttpRequest();
            xmlHttp.open( "GET", 'https://mod.calltouch.ru/set_attrs_by_get.php?siteId=17519&sessionId=4023220950&attrs={"clientError_NO_CT_CREATE_SESSION": 1}', true );
            xmlHttp.send( null );
            }
            
window.ctw = {};
window.ctw.clientFormConfig = {}
window.ctw.clientFormConfig.getClientFormsSettingsUrl = "//mod.calltouch.ru/callback_widget_user_form_find.php";
window.ctw.clientFormConfig.sendClientFormsRequestUrl = "//mod.calltouch.ru/callback_request_user_form_create.php";
(function (targetWindow, nameSpace, params){
!function(){var e={6396:function(e){e.exports=function(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}}},t={};function n(r){var o=t[r];if(void 0!==o)return o.exports;var a=t[r]={exports:{}};return e[r](a,a.exports,n),a.exports}n.n=function(e){var t=e&&e.__esModule?function(){return e.default}:function(){return e};return n.d(t,{a:t}),t},n.d=function(e,t){for(var r in t)n.o(t,r)&&!n.o(e,r)&&Object.defineProperty(e,r,{enumerable:!0,get:t[r]})},n.o=function(e,t){return Object.prototype.hasOwnProperty.call(e,t)},function(){"use strict";var e=n(6396),t=n.n(e);function r(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable})),n.push.apply(n,r)}return n}function o(e){for(var n=1;n<arguments.length;n++){var o=null!=arguments[n]?arguments[n]:{};n%2?r(Object(o),!0).forEach(function(n){t()(e,n,o[n])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(o)):r(Object(o)).forEach(function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(o,t))})}return e}function a(e,t,n,r){try{var a=Boolean(window.event&&window.event.target&&"A"===window.event.target.nodeName),c=Boolean(window.event&&(window.event.target&&"submit"===window.event.target.type||"submit"===window.event.type)),i=function(){var e;if(e||"undefined"==typeof XMLHttpRequest)try{e=new ActiveXObject("Msxml2.XMLHTTP")}catch(t){try{e=new ActiveXObject("Microsoft.XMLHTTP")}catch(t){e=!1}}else e=new XMLHttpRequest;return e}(),s=t?"POST":"GET";i.open(s,e,!a&&!c&&!r),a||c||r||(i.timeout=6e4),i.setRequestHeader("Content-type","application/json"),i.onreadystatechange=function(){if(4===i.readyState&&n)if(200===i.status){var e=function(e){var t;try{t=JSON.parse(e)}catch(e){}return t}(i.response);e?e.data?n(!0,o({},e.data)):e.error?n(!1,o({},e.error)):n(!1,{type:"unknown_error",message:"Unknown JSON format",details:{}}):n(!1,{type:"unknown_error",message:"JSON parse error",details:{}})}else 0===i.status?n(!1,{type:"unknown_error",message:"Request timeout exceeded or connection reset",details:{}}):n(!1,{type:"unknown_error",message:"Unexpected HTTP code: ".concat(i.statusText),details:{}})},i.send(t)}catch(e){n&&n(!1,{type:"unknown_error",message:"Unexpected js exception",details:{}})}}function c(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);t&&(r=r.filter(function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable})),n.push.apply(n,r)}return n}function i(e){for(var n=1;n<arguments.length;n++){var r=null!=arguments[n]?arguments[n]:{};n%2?c(Object(r),!0).forEach(function(n){t()(e,n,r[n])}):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):c(Object(r)).forEach(function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(r,t))})}return e}!function(e,t,n){var r=e||window,o=t||"window.ctw";r[o]||(r[o]={});var c=r[o].clientFormConfig||{},s=c.getClientFormsSettingsUrl,u=c.sendClientFormsRequestUrl;r[o].getRouteKeyData=function(e,t){var r=1e6*Math.random(),o="".concat(s,"?siteId=").concat(n.siteId,"&routeKey=").concat(e,"&pageUrl=").concat(n.pageUrl,"&sessionId=").concat(n.sessionId);a("".concat(o,"&rand=").concat(Math.floor(r)),null,t)};var d=function(e,t,r,o){var c=arguments.length>4&&void 0!==arguments[4]?arguments[4]:null,d=arguments.length>5&&void 0!==arguments[5]?arguments[5]:[],l=arguments.length>6&&void 0!==arguments[6]?arguments[6]:null,p=arguments.length>7?arguments[7]:void 0,f="boolean"==typeof p&&p,w=Array.isArray(p)&&p,y=1e6*Math.random(),g={siteId:n.siteId,sessionId:n.sessionId,workMode:1,pageUrl:n.pageUrl,tags:d,phone:t,routeKey:e,fields:r,scheduleTime:c,unitId:l};w&&(g.customFields=w),a("".concat(s,"?routeKey=").concat(e,"&siteId=").concat(n.siteId,"&pageUrl=").concat(n.pageUrl,"&sessionId=").concat(n.sessionId,"&rand=").concat(Math.floor(y)),null,function(e,n){if(e&&n.widgetData)if(n.widgetData.isNeedTwoFactorRequest){var r=document.querySelector("#CalltouchWidgetFrame");if(r&&r.contentWindow&&n.widgetData){var c=n.widgetData.widgetId;r&&r.contentWindow.openTwoFactorForm(t,c,function(e,t){var n=e.twoFactorCode,r=e.reqUuid;a("".concat(u,"?rand=2f").concat(Math.floor(y)),JSON.stringify(i(i({},g),{},{twoFactorCode:n,reqUuid:r})),function(e,n){t(e,n),e&&o(e,n)},f)})}}else a("".concat(u,"?rand=").concat(Math.floor(y)),JSON.stringify(g),o,f)},f)};r[o].createRequest=d,r[o+"_"+n.modId]={createRequest:d}}(targetWindow,nameSpace,params)}()}();
})(window, "ctw", {"siteId":17519,"sessionId":4023220950,"pageUrl":"https:\/\/mtz.ru\/novosti\/13-rukovodstva\/51-belarus-92p-rukovodstvo-po-ekspluatatsii","modId":"710bff10"})
            var call_value = '4023220950';
            var call_value_710bff10 = call_value;
            if(window.onSessionCallValue) {
            onSessionCallValue('4023220950', '');
            }
            