function P(t,e,r,o){var n=arguments.length,a=n<3?e:o===null?o=Object.getOwnPropertyDescriptor(e,r):o,c;if(typeof Reflect=="object"&&typeof Reflect.decorate=="function")a=Reflect.decorate(t,e,r,o);else for(var s=t.length-1;s>=0;s--)(c=t[s])&&(a=(n<3?c(a):n>3?c(e,r,a):c(e,r))||a);return n>3&&a&&Object.defineProperty(e,r,a),a}var ce=class{constructor(e,r,o=!1){this.evaluate=e,this.policy=r,this.isVolatile=o}};var N;(function(t){t[t.needsArrayObservation=1101]="needsArrayObservation",t[t.onlySetDOMPolicyOnce=1201]="onlySetDOMPolicyOnce",t[t.bindingInnerHTMLRequiresTrustedTypes=1202]="bindingInnerHTMLRequiresTrustedTypes",t[t.twoWayBindingRequiresObservables=1203]="twoWayBindingRequiresObservables",t[t.hostBindingWithoutHost=1204]="hostBindingWithoutHost",t[t.unsupportedBindingBehavior=1205]="unsupportedBindingBehavior",t[t.directCallToHTMLTagNotAllowed=1206]="directCallToHTMLTagNotAllowed",t[t.onlySetTemplatePolicyOnce=1207]="onlySetTemplatePolicyOnce",t[t.cannotSetTemplatePolicyAfterCompilation=1208]="cannotSetTemplatePolicyAfterCompilation",t[t.blockedByDOMPolicy=1209]="blockedByDOMPolicy",t[t.invalidHydrationAttributeMarker=1210]="invalidHydrationAttributeMarker",t[t.duplicateRenderInstruction=1211]="duplicateRenderInstruction",t[t.missingElementDefinition=1401]="missingElementDefinition",t[t.noRegistrationForContext=1501]="noRegistrationForContext",t[t.noFactoryForResolver=1502]="noFactoryForResolver",t[t.invalidResolverStrategy=1503]="invalidResolverStrategy",t[t.cannotAutoregisterDependency=1504]="cannotAutoregisterDependency",t[t.cannotResolveKey=1505]="cannotResolveKey",t[t.cannotConstructNativeFunction=1506]="cannotConstructNativeFunction",t[t.cannotJITRegisterNonConstructor=1507]="cannotJITRegisterNonConstructor",t[t.cannotJITRegisterIntrinsic=1508]="cannotJITRegisterIntrinsic",t[t.cannotJITRegisterInterface=1509]="cannotJITRegisterInterface",t[t.invalidResolver=1510]="invalidResolver",t[t.invalidKey=1511]="invalidKey",t[t.noDefaultResolver=1512]="noDefaultResolver",t[t.cyclicDependency=1513]="cyclicDependency",t[t.connectUpdateRequiresController=1514]="connectUpdateRequiresController"})(N||(N={}));var W=t=>typeof t=="function",$=t=>typeof t=="string",Po=()=>{};var jl=Object.create(null),w={warn(t,e){},error(t,e){return new Error(`Error ${t}`)},addMessages(t){Object.assign(jl,t)}};var No=Object.freeze([]);function ye(){let t=new Map;return Object.freeze({register(e){return t.has(e.type)?!1:(t.set(e.type,e),!0)},getByType(e){return t.get(e)},getForInstance(e){if(e!=null)return t.get(e.constructor)}})}function xt(){let t=new WeakMap;return function(e){let r=t.get(e);if(r===void 0){let o=Reflect.getPrototypeOf(e);for(;r===void 0&&o!==null;)r=t.get(o),o=Reflect.getPrototypeOf(o);r=r===void 0?[]:r.slice(0),t.set(e,r)}return r}}function I(t){t.prototype.toJSON=Po}var yt=class extends ce{createObserver(){return this}bind(e){return this.evaluate(e.source,e.context)}};I(yt);function Pt(t,e){return new yt(t,e)}var Pe=class{constructor(e,r){this.sub1=void 0,this.sub2=void 0,this.spillover=void 0,this.subject=e,this.sub1=r}has(e){return this.spillover===void 0?this.sub1===e||this.sub2===e:this.spillover.indexOf(e)!==-1}subscribe(e){let r=this.spillover;if(r===void 0){if(this.has(e))return;if(this.sub1===void 0){this.sub1=e;return}if(this.sub2===void 0){this.sub2=e;return}this.spillover=[this.sub1,this.sub2,e],this.sub1=void 0,this.sub2=void 0}else r.indexOf(e)===-1&&r.push(e)}unsubscribe(e){let r=this.spillover;if(r===void 0)this.sub1===e?this.sub1=void 0:this.sub2===e&&(this.sub2=void 0);else{let o=r.indexOf(e);o!==-1&&r.splice(o,1)}}notify(e){let r=this.spillover,o=this.subject;if(r===void 0){let n=this.sub1,a=this.sub2;n!==void 0&&n.handleChange(o,e),a!==void 0&&a.handleChange(o,e)}else for(let n=0,a=r.length;n<a;++n)r[n].handleChange(o,e)}},Ne=class{constructor(e){this.subscribers={},this.subjectSubscribers=null,this.subject=e}notify(e){var r,o;(r=this.subscribers[e])===null||r===void 0||r.notify(e),(o=this.subjectSubscribers)===null||o===void 0||o.notify(e)}subscribe(e,r){var o,n;let a;r?a=(o=this.subscribers[r])!==null&&o!==void 0?o:this.subscribers[r]=new Pe(this.subject):a=(n=this.subjectSubscribers)!==null&&n!==void 0?n:this.subjectSubscribers=new Pe(this.subject),a.subscribe(e)}unsubscribe(e,r){var o,n;r?(o=this.subscribers[r])===null||o===void 0||o.unsubscribe(e):(n=this.subjectSubscribers)===null||n===void 0||n.unsubscribe(e)}};var X=[],Sr=[],Gl=globalThis.requestAnimationFrame,yr=!0;function Wl(){if(Sr.length)throw Sr.shift()}function Xl(t){try{t.call()}catch(e){if(yr)Sr.push(e),setTimeout(Wl,0);else throw X.length=0,e}}function xr(){let e=0;for(;e<X.length;)if(Xl(X[e]),e++,e>1024){for(let r=0,o=X.length-e;r<o;r++)X[r]=X[r+e];X.length-=e,e=0}X.length=0}function Fo(t){X.push(t),X.length<2&&(yr?Gl(xr):xr())}var ee=Object.freeze({enqueue:Fo,next:()=>new Promise(Fo),process:xr,setMode:t=>yr=t});var ie=Object.freeze({unknown:void 0,coupled:1}),B=(()=>{let t=ee.enqueue,e=/(:|&&|\|\||if|\?\.)/,r=new WeakMap,o,n=u=>{throw w.error(N.needsArrayObservation)};function a(u){var i;let h=(i=u.$fastController)!==null&&i!==void 0?i:r.get(u);return h===void 0&&(Array.isArray(u)?h=n(u):r.set(u,h=new Ne(u))),h}let c=xt();class s{constructor(i){this.name=i,this.field=`_${i}`,this.callback=`${i}Changed`}getValue(i){return o!==void 0&&o.watch(i,this.name),i[this.field]}setValue(i,h){let m=this.field,b=i[m];if(b!==h){i[m]=h;let x=i[this.callback];W(x)&&x.call(i,b,h),a(i).notify(this.name)}}}class d extends Pe{constructor(i,h,m=!1){super(i,h),this.expression=i,this.isVolatileBinding=m,this.needsRefresh=!0,this.needsQueue=!0,this.isAsync=!0,this.first=this,this.last=null,this.propertySource=void 0,this.propertyName=void 0,this.notifier=void 0,this.next=void 0}setMode(i){this.isAsync=this.needsQueue=i}bind(i){let h=this.observe(i.source,i.context);return!i.isBound&&this.requiresUnbind(i)&&i.onUnbind(this),h}requiresUnbind(i){return i.sourceLifetime!==ie.coupled||this.first!==this.last||this.first.propertySource!==i.source}unbind(i){this.dispose()}observe(i,h){this.needsRefresh&&this.last!==null&&this.dispose();let m=o;o=this.needsRefresh?this:void 0,this.needsRefresh=this.isVolatileBinding;let b;try{b=this.expression(i,h)}finally{o=m}return b}disconnect(){this.dispose()}dispose(){if(this.last!==null){let i=this.first;for(;i!==void 0;)i.notifier.unsubscribe(this,i.propertyName),i=i.next;this.last=null,this.needsRefresh=this.needsQueue=this.isAsync}}watch(i,h){let m=this.last,b=a(i),x=m===null?this.first:{};if(x.propertySource=i,x.propertyName=h,x.notifier=b,b.subscribe(this,h),m!==null){if(!this.needsRefresh){let Q;o=void 0,Q=m.propertySource[m.propertyName],o=this,i===Q&&(this.needsRefresh=!0)}m.next=x}this.last=x}handleChange(){this.needsQueue?(this.needsQueue=!1,t(this)):this.isAsync||this.call()}call(){this.last!==null&&(this.needsQueue=this.isAsync,this.notify(this))}*records(){let i=this.first;for(;i!==void 0;)yield i,i=i.next}}return I(d),Object.freeze({setArrayObserverFactory(u){n=u},getNotifier:a,track(u,i){o&&o.watch(u,i)},trackVolatile(){o&&(o.needsRefresh=!0)},notify(u,i){a(u).notify(i)},defineProperty(u,i){$(i)&&(i=new s(i)),c(u).push(i),Reflect.defineProperty(u,i.name,{enumerable:!0,get(){return i.getValue(this)},set(h){i.setValue(this,h)}})},getAccessors:c,binding(u,i,h=this.isVolatileBinding(u)){return new d(u,i,h)},isVolatileBinding(u){return e.test(u.toString())}})})();function We(t,e){B.defineProperty(t,e)}var wo=(()=>{let t=null;return{get(){return t},set(e){t=e}}})(),te=Object.freeze({default:{index:0,length:0,get event(){return te.getEvent()},eventDetail(){return this.event.detail},eventTarget(){return this.event.target}},getEvent(){return wo.get()},setEvent(t){wo.set(t)}});var Pr=class extends ce{createObserver(e){return B.binding(this.evaluate,e,this.isVolatile)}};function Nt(t,e,r=B.isVolatileBinding(t)){return new Pr(t,e,r)}var S=Object.freeze({none:0,attribute:1,booleanAttribute:2,property:3,content:4,tokenList:5,event:6});var Ul=/^[\u0000-\u0020\u007F]+|[\u0000-\u0020\u007F]+$/g,ql=/[\u0000-\u0020\u007F]+/g,Yl=/^(?:javascript|vbscript|data):/;function Nr(t){return t.replace(Ul,"")}function Kl(t){try{return decodeURIComponent(t)}catch{return t}}function Jl(t){let e=Nr(t);for(let r=0;r<3;++r){let o=Kl(e);if(o===e)break;e=Nr(o)}return e=e.replace(ql,"").toLowerCase(),Yl.test(e)}function Ql(t){let e=Nr(t);return Jl(e)?"":e}function re(t,e,r,o){return(n,a,c,...s)=>{c!=null&&(c=Ql(`${c}`)),o(n,a,c,...s)}}function p(t,e,r,o){throw w.error(N.blockedByDOMPolicy,{aspectName:r,tagName:t??"text"})}var Zl={"a.href":re,"area.href":re,"embed.src":p,"form.action":re,"frame.src":re,"iframe.src":re,"iframe.srcdoc":p,"link.href":p,"object.data":p,"script.src":p,"script.text":p},ec={"button.formaction":re,"input.formaction":re,"object.codebase":p},tc={"button.formAction":re,"input.formAction":re,"object.codeBase":p,"script.innerText":p,"script.textContent":p,"style.innerText":p,"style.textContent":p};function rc(t,e,r){var o;let n=e===S.attribute?ec:e===S.property?tc:void 0;if(n){let a=`${t}.${r}`;return(o=n[a])!==null&&o!==void 0?o:Zl[a]}}var oc={onabort:p,onauxclick:p,onbeforeinput:p,onbeforematch:p,onblur:p,oncancel:p,oncanplay:p,oncanplaythrough:p,onchange:p,onclick:p,onclose:p,oncontextlost:p,oncontextmenu:p,oncontextrestored:p,oncopy:p,oncuechange:p,oncut:p,ondblclick:p,ondrag:p,ondragend:p,ondragenter:p,ondragleave:p,ondragover:p,ondragstart:p,ondrop:p,ondurationchange:p,onemptied:p,onended:p,onerror:p,onfocus:p,onformdata:p,oninput:p,oninvalid:p,onkeydown:p,onkeypress:p,onkeyup:p,onload:p,onloadeddata:p,onloadedmetadata:p,onloadstart:p,onmousedown:p,onmouseenter:p,onmouseleave:p,onmousemove:p,onmouseout:p,onmouseover:p,onmouseup:p,onpaste:p,onpause:p,onplay:p,onplaying:p,onprogress:p,onratechange:p,onreset:p,onresize:p,onscroll:p,onsecuritypolicyviolation:p,onseeked:p,onseeking:p,onselect:p,onslotchange:p,onstalled:p,onsubmit:p,onsuspend:p,ontimeupdate:p,ontoggle:p,onvolumechange:p,onwaiting:p,onwebkitanimationend:p,onwebkitanimationiteration:p,onwebkitanimationstart:p,onwebkittransitionend:p,onwheel:p};function nc(t,e){if(t===S.property&&e==="innerHTML")return p;if(t===S.attribute||t===S.property||t===S.event)return oc[e]}function To(t,e,r,o){var n;let a=(n=rc((t??"").toLowerCase(),e,r))!==null&&n!==void 0?n:nc(e,r);return a?a(t,e,r,o):o}var Ao=t=>t,ac=globalThis.trustedTypes?globalThis.trustedTypes.createPolicy("fast-html",{createHTML:Ao}):{createHTML:Ao},Co=Object.freeze({createHTML(t){return ac.createHTML(t)},protect:To}),Fr=Co,oe=Object.freeze({get policy(){return Fr},setPolicy(t){if(Fr!==Co)throw w.error(N.onlySetDOMPolicyOnce);Fr=t},setAttribute(t,e,r){r==null?t.removeAttribute(e):t.setAttribute(e,r)},setBooleanAttribute(t,e,r){r?t.setAttribute(e,""):t.removeAttribute(e)}});var wr="boolean",Ho="reflect",Fe=Object.freeze({locate:xt()}),Do={toView(t){return t?"":null},fromView(t){return!!t}};var Xe=class t{constructor(e,r,o=r.toLowerCase(),n=Ho,a){this.guards=new Set,this.Owner=e,this.name=r,this.attribute=o,this.mode=n,this.converter=a,this.fieldName=`_${r}`,this.callbackName=`${r}Changed`,this.hasCallback=this.callbackName in e.prototype,n===wr&&a===void 0&&(this.converter=Do)}setValue(e,r){let o=e[this.fieldName],n=this.converter;n!==void 0&&(r=n.fromView(r)),o!==r&&(e[this.fieldName]=r,this.tryReflectToAttribute(e),this.hasCallback&&e[this.callbackName](o,r),e.$fastController.notify(this.name))}getValue(e){return B.track(e,this.name),e[this.fieldName]}onAttributeChangedCallback(e,r){this.guards.has(e)||(this.guards.add(e),this.mode===wr?this.setValue(e,r!==null):this.setValue(e,r),this.guards.delete(e))}tryReflectToAttribute(e){let r=this.mode,o=this.guards;o.has(e)||r==="fromView"||ee.enqueue(()=>{o.add(e);let n=e[this.fieldName];switch(r){case Ho:{let a=this.converter;oe.setAttribute(e,this.attribute,a!==void 0?a.toView(n):n);break}case wr:oe.setBooleanAttribute(e,this.attribute,n);break}o.delete(e)})}static collect(e,...r){let o=[];r.push(Fe.locate(e));for(let n=0,a=r.length;n<a;++n){let c=r[n];if(c!==void 0)for(let s=0,d=c.length;s<d;++s){let u=c[s];$(u)?o.push(new t(e,u)):o.push(new t(e,u.property,u.attribute,u.mode,u.converter))}}return o}};function F(t,e){let r;function o(n,a){arguments.length>1&&(r.property=a),Fe.locate(n.constructor).push(r)}if(arguments.length>1){r={},o(t,e);return}return r=t===void 0?{}:t,o}var Tr;function $o(t){return t.reduce((e,r)=>(r instanceof O?e.push(...$o(r.styles)):e.push(r),e),[])}var O=class t{get strategy(){return this._strategy===null&&(Tr||t.setDefaultStrategy(t.supportsAdoptedStyleSheets?lc():ic()),this.withStrategy(Tr)),this._strategy}constructor(e){this.styles=e,this.targets=new WeakSet,this._strategy=null}addStylesTo(e){this.strategy.addStylesTo(e),this.targets.add(e)}removeStylesFrom(e){this.strategy.removeStylesFrom(e),this.targets.delete(e)}isAttachedTo(e){return this.targets.has(e)}withStrategy(e){return this._strategy=new e($o(this.styles)),this}static setDefaultStrategy(e){Tr=e}static normalize(e){return e===void 0?void 0:Array.isArray(e)?new t(e):e instanceof t?e:new t([e])}};O.supportsAdoptedStyleSheets=Array.isArray(document.adoptedStyleSheets)&&"replace"in CSSStyleSheet.prototype;function lc(){let t=new Map;return class{constructor(r){this.sheets=r.map(o=>{if(o instanceof CSSStyleSheet)return o;let n=t.get(o);return n===void 0&&(n=new CSSStyleSheet,n.replaceSync(o),t.set(o,n)),n})}addStylesTo(r){let o=r;o.adoptedStyleSheets=[...o.adoptedStyleSheets,...this.sheets]}removeStylesFrom(r){let o=r;o.adoptedStyleSheets=o.adoptedStyleSheets.filter(n=>this.sheets.indexOf(n)===-1)}}}var cc=0;function ic(){return class{constructor(e){this.styles=e,this.styleClass=`fast-${++cc}`}addStylesTo(e){let r=e===document?document.body:e;for(let o=0;o<this.styles.length;o++){let n=document.createElement("style");n.innerHTML=this.styles[o],n.className=this.styleClass,r.append(n)}}removeStylesFrom(e){let r=e===document?document.body:e,o=r.querySelectorAll(`.${this.styleClass}`);for(let n=0,a=o.length;n<a;++n)r.removeChild(o[n])}}}var Ar={define(t,e,r,o,n){o.define(t,e,r)}};var sc={},Lo=new WeakMap,Cr=ye(),Ft=new WeakMap,Io=new WeakMap,we=Object.freeze({register(t){if(!Cr.register(t))return!1;let e=Mo(t.registry);return Object.prototype.hasOwnProperty.call(e,t.name)||B.defineProperty(e,t.name),e[t.name]=t.type,!0},getByType:Cr.getByType,getForInstance:Cr.getForInstance,whenRegistered:dc});function Mo(t=customElements){if(t===customElements)return sc;let e=Lo.get(t);return e||(e={},Lo.set(t,e)),e}function Oo(t){return t===void 0?void 0:we.getByType(t)}function dc(t,e=customElements){let r=Mo(e);Object.prototype.hasOwnProperty.call(r,t)||B.defineProperty(r,t);let o=Oo(r[t]);return o!==void 0?Promise.resolve(o):new Promise(n=>{let a=B.getNotifier(r),c={handleChange:()=>{let s=Oo(r[t]);s!==void 0&&(a.unsubscribe(c,t),n(s))}};a.subscribe(c,t)})}function Ro(t,e){if(Io.has(e))return!1;let r=Ft.get(t),o=r===void 0;r===void 0&&(r=new Set,Ft.set(t,r));let n=new WeakRef(e);return r.add(n),Io.set(e,n),o}function Eo(t,e){let r=Ft.get(t);if(r!==void 0){Ft.delete(t);for(let o of r){let n=o.deref();n!==void 0&&e(n)}}}var zo={mode:"open"},_o={},Vo=new Set,Tt=new WeakMap,wt=new WeakMap,ge=new WeakMap,jo=new WeakMap,uc=new WeakMap;function pc(t){return W(t)}function gc(t){return typeof t?.then=="function"}function Go(t,e){if(wt.delete(t),t.template===void 0&&e!==void 0&&(t.template=e),t.template!==void 0)return ge.delete(t),Tt.delete(t),t.template}function Hr(t,e=t.registry,r){if(!r?.length)return;let o=t,n=jo.get(o);if(!n?.has(e)){n===void 0&&(n=new WeakSet,jo.set(o,n)),n.add(e);for(let a of r)a(t)}}function At(t){var e;return(e=uc.get(t))!==null&&e!==void 0?e:null}function Dr(t){if(t.template!==void 0)return ge.delete(t),t.template;let e=wt.get(t);if(e)return e;let r=Tt.get(t);if(!r)return;ge.delete(t);let o;try{o=r(t)}catch(n){throw ge.set(t,n),n}if(gc(o)){let n=Promise.resolve(o).then(a=>Go(t,a)).catch(a=>{throw wt.delete(t),ge.set(t,a),a});return wt.set(t,n),n}return Go(t,o)}function hc(t,e){let r=t;if(e===void 0){ge.delete(r);return}ge.set(r,e)}var z=class t{get isDefined(){return this.platformDefined}constructor(e,r=e.definition){var o;this.platformDefined=!1,$(r)&&(r={name:r}),this.type=e,this.name=r.name,this.registry=(o=r.registry)!==null&&o!==void 0?o:customElements,pc(r.template)?Tt.set(this,r.template):this.template=r.template;let n=e.prototype,a=Xe.collect(e,r.attributes),c=new Array(a.length),s={},d={};for(let u=0,i=a.length;u<i;++u){let h=a[u];c[u]=h.attribute,s[h.name]=h,d[h.attribute]=h,B.defineProperty(n,h)}Reflect.defineProperty(e,"observedAttributes",{value:c,enumerable:!0}),this.attributes=a,this.propertyLookup=s,this.attributeLookup=d,this.shadowOptions=r.shadowOptions===void 0?zo:r.shadowOptions===null?void 0:Object.assign(Object.assign({},zo),r.shadowOptions),this.elementOptions=r.elementOptions===void 0?_o:Object.assign(Object.assign({},_o),r.elementOptions),this.styles=O.normalize(r.styles),this.schema=r.schema,we.register(this)}define(e=this.registry,r){let o=this.type;if(!e.get(this.name)){if(Hr(this,e,r),this.template===void 0&&Tt.has(this))return Promise.resolve().then(()=>Dr(this)).then(a=>{if(a!==void 0&&!e.get(this.name)){let c=this.platformDefined;this.platformDefined=!0,Ar.define(this.name,o,this.elementOptions,e,c)}}).catch(a=>{hc(this,a),B.notify(this,"template")}),this;let n=this.platformDefined;this.platformDefined=!0,Ar.define(this.name,o,this.elementOptions,e,n)}return this}static compose(e,r){let o=Vo.has(e)||we.getByType(e)?new t(class extends e{},r):new t(e,r);return Promise.resolve(o)}static registerBaseType(e){Vo.add(e)}};z.getByType=we.getByType;z.getForInstance=we.getForInstance;B.defineProperty(z.prototype,"template");var fc={bubbles:!0,composed:!0,cancelable:!0},$r="isConnected",Uo=new WeakMap,Lr=Symbol("fast-late-attribute-observer");function Ct(t){var e,r;return(r=(e=t.shadowRoot)!==null&&e!==void 0?e:Uo.get(t))!==null&&r!==void 0?r:null}var Wo,M;(function(t){t[t.connecting=0]="connecting",t[t.connected=1]="connected",t[t.disconnecting=2]="disconnecting",t[t.disconnected=3]="disconnected"})(M||(M={}));var se=class t{get isConnected(){return B.track(this,$r),this.stage===M.connected}get context(){var e,r;return(r=(e=this.view)===null||e===void 0?void 0:e.context)!==null&&r!==void 0?r:te.default}get isBound(){var e,r;return(r=(e=this.view)===null||e===void 0?void 0:e.isBound)!==null&&r!==void 0?r:!1}get sourceLifetime(){var e;return(e=this.view)===null||e===void 0?void 0:e.sourceLifetime}get template(){var e;if(this._template===null){let r=this.definition;this.source.resolveTemplate?this._template=this.source.resolveTemplate():r.template&&(this._template=(e=r.template)!==null&&e!==void 0?e:null)}return this._template}set template(e){this._template!==e&&(this._template=e,this.needsInitialization||this.renderTemplate(e))}get shadowOptions(){return this._shadowRootOptions}set shadowOptions(e){if(this._shadowRootOptions===void 0&&e!==void 0){this._shadowRootOptions=e;let r=this.source.shadowRoot;r?this.hasExistingShadowRoot=!0:(r=this.source.attachShadow(e),e.mode==="closed"&&Uo.set(this.source,r))}}get mainStyles(){var e;if(this._mainStyles===null){let r=this.definition;this.source.resolveStyles?this._mainStyles=this.source.resolveStyles():r.styles&&(this._mainStyles=(e=r.styles)!==null&&e!==void 0?e:null)}return this._mainStyles}set mainStyles(e){this._mainStyles!==e&&(this._mainStyles!==null&&this.removeStyles(this._mainStyles),this._mainStyles=e,this.needsInitialization||this.addStyles(e))}constructor(e,r){this.boundObservables=null,this.needsInitialization=!0,this.hasExistingShadowRoot=!1,this.isPrerendered=new Promise(a=>{this._resolvePrerendered=a}),this.isHydrated=new Promise(a=>{this._resolveHydrated=a}),this._template=null,this.stage=M.disconnected,this.guardBehaviorConnection=!1,this.behaviors=null,this.behaviorsConnected=!1,this._mainStyles=null,this.$fastController=this,this.view=null,this._notifier=new Ne(e),this.source=e,this.definition=r,this.shadowOptions=r.shadowOptions;let o=Reflect.getPrototypeOf(e),n=o===null?[]:B.getAccessors(o);if(n.length>0){let a=this.boundObservables=Object.create(null);for(let c=0,s=n.length;c<s;++c){let d=n[c].name,u=e[d];u!==void 0&&(delete e[d],a[d]=u)}Object.keys(a).length===0&&(this.boundObservables=null)}this.captureBoundObservables()}get subject(){return this._notifier.subject}notify(e){this._notifier.notify(e)}subscribe(e,r){this._notifier.subscribe(e,r)}unsubscribe(e,r){this._notifier.unsubscribe(e,r)}onUnbind(e){var r;(r=this.view)===null||r===void 0||r.onUnbind(e)}addBehavior(e){var r,o;let n=(r=this.behaviors)!==null&&r!==void 0?r:this.behaviors=new Map,a=(o=n.get(e))!==null&&o!==void 0?o:0;a===0?(n.set(e,1),e.addedCallback&&e.addedCallback(this),e.connectedCallback&&!this.guardBehaviorConnection&&(this.stage===M.connected||this.stage===M.connecting)&&e.connectedCallback(this)):n.set(e,a+1)}removeBehavior(e,r=!1){let o=this.behaviors;if(o===null)return;let n=o.get(e);n!==void 0&&(n===1||r?(o.delete(e),e.disconnectedCallback&&this.stage!==M.disconnected&&e.disconnectedCallback(this),e.removedCallback&&e.removedCallback(this)):o.set(e,n-1))}addStyles(e){var r;if(!e)return;let o=this.source;e instanceof HTMLElement?((r=Ct(o))!==null&&r!==void 0?r:this.source).append(e):e.isAttachedTo(o)||e.addStylesTo(o)}removeStyles(e){var r;if(!e)return;let o=this.source;e instanceof HTMLElement?((r=Ct(o))!==null&&r!==void 0?r:o).removeChild(e):e.isAttachedTo(o)&&e.removeStylesFrom(o)}connect(){this.stage===M.disconnected&&(this.stage=M.connecting,this.captureBoundObservables(),this.syncLateAttributes(),this.observeLateAttributes(),this.bindObservables(),this.connectBehaviors(),this.needsInitialization?(this.renderTemplate(this.template),this.addStyles(this.mainStyles),this.needsInitialization=!1):this.view!==null&&this.view.bind(this.source),this.stage=M.connected,B.notify(this,$r))}bindObservables(){if(this.boundObservables!==null){let e=this.source,r=this.boundObservables,o=Object.keys(r);for(let n=0,a=o.length;n<a;++n){let c=o[n];e[c]=r[c]}this.boundObservables=null}}captureBoundObservables(){let e=this.source,r=Object.getOwnPropertyNames(e),o=a=>{let c=Reflect.getPrototypeOf(e);for(;c!==null;){let s=Reflect.getOwnPropertyDescriptor(c,a);if(s?.get||s?.set)return!0;c=Reflect.getPrototypeOf(c)}return!1},n=this.boundObservables;for(let a=0,c=r.length;a<c;++a){let d=r[a];if(!o(d))continue;let u=e[d];if(u===void 0){delete e[d];continue}delete e[d],(n??(n=this.boundObservables=Object.create(null)))[d]=u}}syncLateAttributes(){let e=At(this.definition);if(e!==null)for(let r of Object.keys(e))this.source.hasAttribute(r)&&this.onAttributeChangedCallback(r,null,this.source.getAttribute(r))}observeLateAttributes(){let e=At(this.definition);if(e===null)return;let r=this.source;r[Lr]===void 0&&(r[Lr]=new MutationObserver(o=>{let n=r.$fastController,a=At(n.definition);if(a!==null)for(let c=0,s=o.length;c<s;++c){let d=o[c].attributeName;d===null||a[d]===void 0||n.onAttributeChangedCallback(d,null,r.getAttribute(d))}}),r[Lr].observe(r,{attributes:!0,attributeFilter:Object.keys(e)}))}connectBehaviors(){if(this.behaviorsConnected===!1){let e=this.behaviors;if(e!==null){this.guardBehaviorConnection=!0;for(let r of e.keys())r.connectedCallback&&r.connectedCallback(this);this.guardBehaviorConnection=!1}this.behaviorsConnected=!0}}disconnectBehaviors(){if(this.behaviorsConnected===!0){let e=this.behaviors;if(e!==null)for(let r of e.keys())r.disconnectedCallback&&r.disconnectedCallback(this);this.behaviorsConnected=!1}}disconnect(){this.stage===M.connected&&(this.stage=M.disconnecting,B.notify(this,$r),this.view!==null&&this.view.unbind(),this.disconnectBehaviors(),this.stage=M.disconnected)}onAttributeChangedCallback(e,r,o){let n=this.definition.attributeLookup[e];n!==void 0&&n.onAttributeChangedCallback(this.source,o)}emit(e,r,o){return this.stage===M.connected?this.source.dispatchEvent(new CustomEvent(e,Object.assign(Object.assign({detail:r},fc),o))):!1}renderTemplate(e){var r;let o=this.source,n=(r=Ct(o))!==null&&r!==void 0?r:o;if(this.view!==null)this.view.dispose(),this.view=null;else if((!this.needsInitialization||this.hasExistingShadowRoot)&&(!this.hasExistingShadowRoot||!this.needsInitialization))for(let a=n.firstChild;a!==null;a=n.firstChild)n.removeChild(a);if(e){let a=this.hasExistingShadowRoot&&this.needsInitialization,c=!1;a&&t.hydrationHook&&(c=t.hydrationHook(this,e,o,n)),c||this.renderClientSide(e,o,n),this._resolvePrerendered(a),this._resolveHydrated(c)}else this.needsInitialization&&(this._resolvePrerendered(!1),this._resolveHydrated(!1))}renderClientSide(e,r,o){if(this.hasExistingShadowRoot){for(let n=o.firstChild;n!==null;n=o.firstChild)o.removeChild(n);this.hasExistingShadowRoot=!1}this.view=e.render(r,o,r),this.view.sourceLifetime=ie.coupled}static forCustomElement(e,r=!1){let o=e.$fastController;if(o!==void 0&&!r)return o;let n=z.getForInstance(e);if(n===void 0)throw w.error(N.missingElementDefinition);if(n.template===void 0&&Ro(n,e)){let a=B.getNotifier(n),c={handleChange:()=>{a.unsubscribe(c,"template"),Eo(n,s=>{let d=s;t.forCustomElement(d,!0),d.isConnected&&d.$fastController.connect()})}};a.subscribe(c,"template")}return e.$fastController=new Wo(e,n)}static setStrategy(e){Wo=e}static installHydrationHook(e){t.hydrationHook=e}};se.hydrationHook=null;I(se);se.setStrategy(se);function Ht(t){var e;return"adoptedStyleSheets"in t?t:(e=Ct(t))!==null&&e!==void 0?e:t.getRootNode()}var Dt=class t{constructor(e){let r=t.styleSheetCache;this.sheets=e.map(o=>{if(o instanceof CSSStyleSheet)return o;let n=r.get(o);return n===void 0&&(n=new CSSStyleSheet,n.replaceSync(o),r.set(o,n)),n})}addStylesTo(e){qo(Ht(e),this.sheets)}removeStylesFrom(e){Yo(Ht(e),this.sheets)}};Dt.styleSheetCache=new Map;var mc=0,vc=()=>`fast-${++mc}`;function Xo(t){return t===document?document.body:t}var Ir=class{constructor(e){this.styles=e,this.styleClass=vc()}addStylesTo(e){e=Xo(Ht(e));let r=this.styles,o=this.styleClass;for(let n=0;n<r.length;n++){let a=document.createElement("style");a.innerHTML=r[n],a.className=o,e.append(a)}}removeStylesFrom(e){e=Xo(Ht(e));let r=e.querySelectorAll(`.${this.styleClass}`);for(let o=0,n=r.length;o<n;++o)e.removeChild(r[o])}},qo=(t,e)=>{t.adoptedStyleSheets=[...t.adoptedStyleSheets,...e]},Yo=(t,e)=>{t.adoptedStyleSheets=t.adoptedStyleSheets.filter(r=>e.indexOf(r)===-1)};if(O.supportsAdoptedStyleSheets){try{document.adoptedStyleSheets.push(),document.adoptedStyleSheets.splice(),qo=(t,e)=>{t.adoptedStyleSheets.push(...e)},Yo=(t,e)=>{for(let r of e){let o=t.adoptedStyleSheets.indexOf(r);o!==-1&&t.adoptedStyleSheets.splice(o,1)}}}catch{}O.setDefaultStrategy(Dt)}else O.setDefaultStrategy(Ir);function Ko(t){let e=class extends t{constructor(){super(),se.forCustomElement(this)}$emit(r,o,n){return this.$fastController.emit(r,o,n)}connectedCallback(){this.$fastController.connect()}disconnectedCallback(){this.$fastController.disconnect()}attributeChangedCallback(r,o,n){this.$fastController.onAttributeChangedCallback(r,o,n)}};return z.registerBaseType(e),e}function bc(t){return typeof t?.then=="function"}function Bc(t,e,r){return Array.isArray(e)&&(r=e,e=void 0),(W(t)?z.compose(t,e):z.compose(this,t)).then(n=>{Hr(n,n.registry,r);let a=Dr(n);return bc(a)?a.then(()=>n.define().type):n.define().type})}function kc(t){return Ko(t)}var he=Object.assign(Ko(HTMLElement),{from:kc,define:Bc});var Or=ye(),Mr=Object.freeze({getForInstance:Or.getForInstance,getByType:Or.getByType,define(t){return Or.register({type:t}),t}});function Jo(t,e){let r=[],o="";for(let n=0,a=t.length-1;n<a;++n){o+=t[n];let c=e[n];Mr.getForInstance(c)!==void 0&&(c=c.createCSS()),c instanceof O||c instanceof CSSStyleSheet?(o.trim()!==""&&(r.push(o),o=""),r.push(c)):o+=c}return o+=t[t.length-1],o.trim()!==""&&r.push(o),r}var A=((t,...e)=>new O(Jo(t,e))),$t=class{constructor(e){this.value=e.length===0?"":e.length===1?e[0]:new O(e)}createCSS(){return this.value}};Mr.define($t);A.partial=(t,...e)=>new $t(Jo(t,e));var Rr=`fast-${Math.random().toString(36).substring(2,8)}`,Lt=`${Rr}{`,Ue=`}${Rr}`,Sc=Ue.length,xc=0,qe=()=>`${Rr}-${++xc}`,It=Object.freeze({interpolation:t=>`${Lt}${t}${Ue}`,attribute:t=>`${qe()}="${Lt}${t}${Ue}"`,comment:t=>`<!--${Lt}${t}${Ue}-->`}),Ye=Object.freeze({parse(t,e){let r=t.split(Lt);if(r.length===1)return null;let o=[];for(let n=0,a=r.length;n<a;++n){let c=r[n],s=c.indexOf(Ue),d;if(s===-1)d=c;else{let u=c.substring(0,s);o.push(e[u]),d=c.substring(s+Sc)}d!==""&&o.push(d)}return o}});var Er=ye(),L=Object.freeze({getForInstance:Er.getForInstance,getByType:Er.getByType,define(t,e){return e=e||{},e.type=t,Er.register(e),t},assignAspect(t,e){if(!e){t.aspectType=S.content;return}switch(t.sourceAspect=e,e[0]){case":":t.targetAspect=e.substring(1),t.aspectType=t.targetAspect==="classList"?S.tokenList:S.property;break;case"?":t.targetAspect=e.substring(1),t.aspectType=S.booleanAttribute;break;case"@":t.targetAspect=e.substring(1),t.aspectType=S.event;break;default:t.targetAspect=e,t.aspectType=S.attribute;break}}});var fe=class{constructor(e){this.options=e}createHTML(e){return It.attribute(e(this))}createBehavior(){return this}};I(fe);var Ot=class extends fe{get id(){return this._id}set id(e){this._id=e,this._controllerProperty=`${e}-c`}bind(e){let r=e.targets[this.targetNodeId];r[this._controllerProperty]=e,this.updateTarget(e.source,this.computeNodes(r)),this.observe(r),e.onUnbind(this)}unbind(e){let r=e.targets[this.targetNodeId];this.updateTarget(e.source,No),this.disconnect(r),r[this._controllerProperty]=null}getSource(e){return e[this._controllerProperty].source}updateTarget(e,r){e[this.options.property]=r}computeNodes(e){let r=this.getNodes(e);return"filter"in this.options&&(r=r.filter(this.options.filter)),r}};var yc=Symbol(),en=new WeakMap;function Pc(t){let e=Object.freeze(Object.assign(Object.assign({},t),{[yc]:!0}));return en.set(e,e),e}function Qo(t){return t.nodeType===Node.COMMENT_NODE}function Zo(t){let e=t.getAttribute("data-fe");if(e===null)return null;let r=e.trim();if(!/^\d+$/.test(r))throw w.error(N.invalidHydrationAttributeMarker,{value:e});let o=parseInt(r,10);if(o<1)throw w.error(N.invalidHydrationAttributeMarker,{value:e});return o}var Te=Pc({attributeMarkerName:"data-fe",contentBindingStartMarker(){return"fe:b"},contentBindingEndMarker(){return"fe:/b"},repeatStartMarker(){return"fe:r"},repeatEndMarker(){return"fe:/r"},elementBoundaryStartMarker(){return"fe:e"},elementBoundaryEndMarker(){return"fe:/e"},parseAttributeBindingCount:Zo,resolveAttributeBindings(t,e,r){let o=Zo(t);return o!==null?{factoryIndices:Array.from({length:o},(a,c)=>e+c),nextFactoryPointer:e+o,cleanup:()=>t.removeAttribute("data-fe")}:null},resolveContentBinding(t,e,r){return t==="fe:b"?{factoryIndices:[e],nextFactoryPointer:e+1}:null},isContentBindingStartMarker(t){return t==="fe:b"},isContentBindingEndMarker(t){return t==="fe:/b"},isRepeatViewStartMarker(t){return t==="fe:r"},isRepeatViewEndMarker(t){return t==="fe:/r"},isElementBoundaryStartMarker(t){return Qo(t)&&t.data==="fe:e"},isElementBoundaryEndMarker(t){return Qo(t)&&t.data==="fe:/e"}}),Nc=en.get(Te);function tn(){return Nc}var Ke=Symbol.for("fe-hydration");function Mt(t){return t[Ke]===Ke}var rn="unknown",on="content following `<!--fe:b-->` content binding marker",nn="matching `<!--fe:/b-->` content binding close marker",an="matching `<!--fe:/e-->` element boundary close marker";function ln(t,e){let r=e?`: ${e}`:"";return`Hydration mismatch in <${t}>${r}. Install hydrationDebugger() from "@microsoft/fast-element/hydration.js" and pass it as enableHydration({ debugger: hydrationDebugger() }) for an "Expected / Received" report including the SSR HTML snippet.`}function zr(t){return`no more attribute bindings (template defines ${t})`}function cn(t){return`no more content bindings (template defines ${t})`}function sn(t,e){let r=(t??rn).toLowerCase();return ln(r,e)}var Fc={formatBindingMismatch(t,e,r,o){return{message:sn(o,void 0)}},formatStructuralError(t,e,r){return{message:sn(e,r)}}},wc=Fc;function Rt(){return wc}function Et(t){var e;return t?(e=t.getRootNode().host)===null||e===void 0?void 0:e.nodeName:void 0}var _r,Vr,_s=(_r=Object.getOwnPropertyDescriptor(Object.getPrototypeOf(customElements),"define"))===null||_r===void 0?void 0:_r.value,Vs=(Vr=Object.getOwnPropertyDescriptor(Array.prototype,Symbol.iterator))===null||Vr===void 0?void 0:Vr.value,Tc=new WeakMap;function dn(t){return Tc.get(Object.getPrototypeOf(t))===!0}function zt(t){return t.nodeType===8}function Ac(t){return t.nodeType===3}function pn(t){return zt(t)||Ac(t)?t.data.length:t.childNodes.length}var gn=133;function jr(t,e){let r=document.createRange();return r.setStart(t,0),r.setEnd(e,pn(e)),r}function un(t,e,r=t){let o=jr(t,e),n=o.startContainer===t?document.createTreeWalker(o.commonAncestorContainer,gn):null,a=e.contains(r),c=!1;n!==null&&(n.currentNode=r);function s(d){if(n===null||!c&&d&&n.currentNode.contains(e))return null;let u=n.currentNode,i=d?n.nextSibling():n.nextNode();return c||!a||e.contains(i)?(i===e&&(a=!0),i):(n.currentNode=u,null)}return[s,()=>{if(n!==null&&!c&&(o.startContainer!==t||o.endContainer!==e||o.endOffset!==pn(e)||n.currentNode!==t&&n.currentNode!==e&&!o.intersectsNode(n.currentNode))){let d=n.currentNode;n=document.createTreeWalker(n.root,n.whatToShow,{acceptNode:u=>o.comparePoint(u,0)===0?NodeFilter.FILTER_ACCEPT:NodeFilter.FILTER_REJECT}),n.currentNode=d,c=!0}}]}function hn(t,e){let r=t.parentNode;if(r===null||r!==e.parentNode||t!==e&&!(t.compareDocumentPosition(e)&Node.DOCUMENT_POSITION_FOLLOWING)){let[c,s]=un(t,e);return[c,s,()=>{}]}let o=t,n=null;function a(c=!1){if(n!==null)return n[0](c);if(c&&o===e)return null;let s=o;do if(c){if(s===e)return null;s=s.nextSibling}else{let d=s;for(s=s.firstChild;s===null;){if(d===e)return null;if(s=d.nextSibling,d=d.parentNode,d===r||d===null)break}}while(s!==null&&!(gn&1<<s.nodeType-1));return s!==null&&(o=s),s}return[a,()=>n?.[1](),()=>{n??(n=un(t,e,o))}]}var Je=class extends Error{constructor(e,r,o,n,a){super(e),this.factories=r,this.node=o,this.expected=n,this.received=a}},fn=Element.prototype.removeAttribute,mn=Element.prototype.getAttribute,Cc=Element.prototype.hasAttribute;function bn(t,e,r){let o=t;for(;o!==null;){let n=Object.getOwnPropertyDescriptor(o,e);if(n!==void 0)return n.value===r;o=Object.getPrototypeOf(o)}return!1}function vn(t,e){return e?Cc.call(t,"is")||!bn(t,"removeAttribute",fn):t.localName.includes("-")||t.hasAttribute("is")||"attributeChangedCallback"in t||t.removeAttribute!==fn}function Ae(t,e,r){let o=Rt().formatStructuralError(t,Et(t),r);throw new Je(o.message,e,t,o.expected,o.received)}function Bn(t,e,r){var o;let[n,a,c]=hn(t,e),s={},d={},u=tn(),i=u===Te;u!==Te&&c();let h=$c(r),m=h,b=t;for(;b!==null;){switch(b.nodeType){case Node.ELEMENT_NODE:{let x=b,Q=i&&x.localName.includes("-"),St=Q&&dn(x);if((Q?!bn(x,"getAttribute",mn):x.getAttribute!==mn)&&c(),i){let je=Te.parseAttributeBindingCount(x);if(je!==null){je>4294967295&&Te.resolveAttributeBindings(x,m,h);for(let Ge=0;Ge<je;Ge++){let yo=r[m++];yo||Ae(b,r,zr(r.length)),Qe(yo,b,s)}vn(x,St)&&c(),x.removeAttribute("data-fe"),a()}break}let Z=u.resolveAttributeBindings(x,m,h);if(Z!==null){m=Z.nextFactoryPointer;for(let je of Z.factoryIndices){let Ge=r[je];Ge||Ae(b,r,zr(r.length)),Qe(Ge,b,s)}vn(x,!1)&&c(),(o=Z.cleanup)===null||o===void 0||o.call(Z),a()}break}case 8:{let x=b.data;if(i?x==="fe:e":u.isElementBoundaryStartMarker(b))b.data="",Dc(n,r,b,u,i);else{let Q;if(i){if(x!=="fe:b")break;Q=m++}else{let Z=u.resolveContentBinding(x,m,h);if(Z===null)break;m=Z.nextFactoryPointer,Q=Z.factoryIndices[0]}let St=r[Q];St||Ae(b,r,cn(r.length)),Hc(b,n,St,r,s,d,u,i)}break}}b=n()}return{targets:s,boundaries:d}}function Hc(t,e,r,o,n,a,c,s){let d=e(!0);t.data="",d===null&&Ae(t,o,on);let u=d,i=0;for(;d!==null;){if(zt(d)){if(s?d.data==="fe:b":c.isContentBindingStartMarker(d.data))i++;else if(s?d.data==="fe:/b":c.isContentBindingEndMarker(d.data)){if(i===0)break;i--}}d=e(!0)}d===null&&Ae(t,o,nn),d.data="",d!==u&&d.previousSibling!==null&&(a[r.targetNodeId]={first:u,last:d.previousSibling}),Qe(r,d,n)}function Dc(t,e,r,o,n){let a=0,c=t(!0);for(;c!==null;){if(zt(c)){if(n?c.data==="fe:e":o.isElementBoundaryStartMarker(c))c.data="",a++;else if((n?c.data==="fe:/e":o.isElementBoundaryEndMarker(c))&&(c.data="",a--===0))return}c=t(!0)}Ae(r,e,an)}function $c(t){let e=0;for(;e<t.length&&t[e].targetNodeId==="h";)e++;return e}function Qe(t,e,r){let o=t.targetNodeId;if(o===void 0)throw new Error("Factory could not be target to the node");r[o]=e}function Ce(t,e){let r=t.parentNode,o=t,n;for(;o!==e;){if(n=o.nextSibling,!n)throw new Error(`Unmatched first/last child inside "${e.getRootNode().host.nodeName}".`);r.removeChild(o),o=n}r.removeChild(e)}var Ze=class{constructor(){this.index=0,this.length=0}get event(){return te.getEvent()}get isEven(){return this.index%2===0}get isOdd(){return this.index%2!==0}get isFirst(){return this.index===0}get isInMiddle(){return!this.isFirst&&!this.isLast}get isLast(){return this.index===this.length-1}eventDetail(){return this.event.detail}eventTarget(){return this.event.target}},me=class extends Ze{constructor(e,r,o){super(),this.fragment=e,this.factories=r,this.targets=o,this.behaviors=null,this.unbindables=[],this.source=null,this.isBound=!1,this.sourceLifetime=ie.unknown,this._skipAttrUpdates=!1,this.isPrerendered=Promise.resolve(!1),this.isHydrated=Promise.resolve(!1),this.context=this,this.firstChild=e.firstChild,this.lastChild=e.lastChild}appendTo(e){e.appendChild(this.fragment)}insertBefore(e){if(this.fragment.hasChildNodes())e.parentNode.insertBefore(this.fragment,e);else{let r=this.lastChild;if(e.previousSibling===r)return;let o=e.parentNode,n=this.firstChild,a;for(;n!==r;)a=n.nextSibling,o.insertBefore(n,e),n=a;o.insertBefore(r,e)}}remove(){let e=this.fragment,r=this.lastChild,o=this.firstChild,n;for(;o!==r;)n=o.nextSibling,e.appendChild(o),o=n;e.appendChild(r)}dispose(){Ce(this.firstChild,this.lastChild),this.unbind()}onUnbind(e){this.unbindables.push(e)}bind(e,r=this){if(this.source===e&&this.context===r)return;let o=this.behaviors;if(o===null){this.source=e,this.context=r,this.behaviors=o=new Array(this.factories.length);let n=this.factories;for(let a=0,c=n.length;a<c;++a){let s=n[a].createBehavior();s.bind(this),o[a]=s}}else{this.source!==null&&this.evaluateUnbindables(),this.isBound=!1,this.source=e,this.context=r;for(let n=0,a=o.length;n<a;++n)o[n].bind(this)}this.isBound=!0}unbind(){!this.isBound||this.source===null||(this.evaluateUnbindables(),this.source=null,this.context=this,this.isBound=!1)}evaluateUnbindables(){let e=this.unbindables;for(let r=0,o=e.length;r<o;++r)e[r].unbind(this);e.length=0}static disposeContiguousBatch(e){if(e.length!==0){Ce(e[0].firstChild,e[e.length-1].lastChild);for(let r=0,o=e.length;r<o;++r)e[r].unbind()}}};I(me);B.defineProperty(me.prototype,"index");B.defineProperty(me.prototype,"length");var kn,ve={unhydrated:"unhydrated",hydrating:"hydrating",hydrated:"hydrated"},Gr=class extends Error{constructor(e,r,o,n,a,c){super(e),this.factory=r,this.fragment=o,this.templateString=n,this.expected=a,this.received=c}},Wr=class extends Ze{get hydrationStage(){return this._hydrationStage}get targets(){return this._targets}get bindingViewBoundaries(){return this._bindingViewBoundaries}constructor(e,r,o,n){super(),this.firstChild=e,this.lastChild=r,this.sourceTemplate=o,this.hostBindingTarget=n,this[kn]=Ke,this.context=this,this.source=null,this.isBound=!1,this.sourceLifetime=ie.unknown,this.unbindables=[],this.fragment=null,this.behaviors=null,this._hydrationStage=ve.unhydrated,this._bindingViewBoundaries={},this._targets={},this.factories=o.compile().factories}insertBefore(e){if(this.fragment!==null)if(this.fragment.hasChildNodes())e.parentNode.insertBefore(this.fragment,e);else{let r=this.lastChild;if(e.previousSibling===r)return;let o=e.parentNode,n=this.firstChild,a;for(;n!==r;)a=n.nextSibling,o.insertBefore(n,e),n=a;o.insertBefore(r,e)}}appendTo(e){this.fragment!==null&&e.appendChild(this.fragment)}remove(){let e=this.fragment||(this.fragment=document.createDocumentFragment()),r=this.lastChild,o=this.firstChild,n;for(;o!==r;){if(n=o.nextSibling,!n)throw new Error(`Unmatched first/last child inside "${r.getRootNode().host.nodeName}".`);e.appendChild(o),o=n}e.appendChild(r)}bind(e,r=this){if(this.source===e&&this.context===r)return;this.hydrationStage!==ve.hydrated&&(this._hydrationStage=ve.hydrating);let o=this.behaviors;if(o===null){this.source=e,this.context=r;try{let{targets:a,boundaries:c}=Bn(this.firstChild,this.lastChild,this.factories);this._targets=a,this._bindingViewBoundaries=c}catch(a){if(a instanceof Je){let c=this.sourceTemplate.html;typeof c!="string"&&(c=c.innerHTML),a.templateString=c}throw a}this.behaviors=o=new Array(this.factories.length);let n=this.factories;for(let a=0,c=n.length;a<c;++a){let s=n[a];if(s.targetNodeId==="h"&&this.hostBindingTarget&&Qe(s,this.hostBindingTarget,this._targets),s.targetNodeId in this.targets){let d=s.createBehavior();d.bind(this),o[a]=d}else{let d=this.sourceTemplate.html;typeof d!="string"&&(d=d.innerHTML);let u=jr(this.firstChild,this.lastChild).cloneContents(),i=Rt().formatBindingMismatch(s,this.firstChild,this.lastChild,Et(this.firstChild));throw new Gr(i.message,s,u,d,i.expected,i.received)}}}else{this.source!==null&&this.evaluateUnbindables(),this.isBound=!1,this.source=e,this.context=r;for(let n=0,a=o.length;n<a;++n)o[n].bind(this)}this.isBound=!0,this._hydrationStage=ve.hydrated}unbind(){!this.isBound||this.source===null||(this.evaluateUnbindables(),this.source=null,this.context=this,this.isBound=!1)}dispose(){Ce(this.firstChild,this.lastChild),this.unbind()}onUnbind(e){this.unbindables.push(e)}evaluateUnbindables(){let e=this.unbindables;for(let r=0,o=e.length;r<o;++r)e[r].unbind(this);e.length=0}};kn=Ke;I(Wr);function Lc(t){return t.create!==void 0}function Sn(t,e){if(t.nodeType!==8){t.textContent=e;return}let r=t.$fastText;r!==void 0&&(r.textContent=e),e===""?r?.remove():(r===void 0&&(t.$fastText=r=document.createTextNode(e)),r.parentNode!==t.parentNode&&t.parentNode.insertBefore(r,t))}function Ic(t,e,r,o){if(r==null&&(r=""),Lc(r)){Sn(t,"");let n=t.$fastView;if(n===void 0)if(Mt(o)&&Mt(r)&&o.bindingViewBoundaries[this.targetNodeId]!==void 0&&o.hydrationStage!==ve.hydrated){let a=o.bindingViewBoundaries[this.targetNodeId];n=r.hydrate(a.first,a.last)}else n=r.create();else t.$fastTemplate!==r&&(n.isComposed&&(n.remove(),n.unbind()),n=r.create());n.isComposed?n.needsBindOnly&&(n.needsBindOnly=!1,n.bind(o.source,o.context)):(n.isComposed=!0,n.bind(o.source,o.context),n.insertBefore(t),t.$fastView=n,t.$fastTemplate=r)}else{let n=t.$fastView;if(n!==void 0&&n.isComposed&&(n.isComposed=!1,n.remove(),n.needsBindOnly?n.needsBindOnly=!1:n.unbind()),n===void 0&&Mt(o)&&o.hydrationStage!==ve.hydrated){let a=o.bindingViewBoundaries[this.targetNodeId];a!==void 0&&(t.nodeType===8&&a.first===a.last&&a.first.nodeType===3?t.$fastText=a.first:Ce(a.first,a.last),delete o.bindingViewBoundaries[this.targetNodeId])}Sn(t,r)}}function Oc(t,e,r){var o;let n=`${this.id}-t`,a=(o=t[n])!==null&&o!==void 0?o:t[n]={v:0,cv:Object.create(null)},c=a.cv,s=a.v,d=t[e];if(r!=null&&r.length){let u=r.split(/\s+/);for(let i=0,h=u.length;i<h;++i){let m=u[i];m!==""&&(c[m]=s,d.add(m))}}if(a.v=s+1,s!==0){s-=1;for(let u in c)c[u]===s&&d.remove(u)}}var Mc={[S.attribute]:oe.setAttribute,[S.booleanAttribute]:oe.setBooleanAttribute,[S.property]:(t,e,r)=>t[e]=r,[S.content]:Ic,[S.tokenList]:Oc,[S.event]:()=>{}},_=class{constructor(e){this.updateTarget=null,this.aspectType=S.content,this.dataBinding=e}createHTML(e){return It.interpolation(e(this))}createBehavior(){var e;if(this.updateTarget===null){let r=Mc[this.aspectType],o=(e=this.dataBinding.policy)!==null&&e!==void 0?e:this.policy;if(!r)throw w.error(N.unsupportedBindingBehavior);this.data=`${this.id}-d`,this.updateTarget=o.protect(this.targetTagName,this.aspectType,this.targetAspect,r)}return this}bind(e){var r;let o=e.targets[this.targetNodeId];switch(this.aspectType){case S.event:o[this.data]=e,o.addEventListener(this.targetAspect,this,this.dataBinding.options);break;case S.content:e.onUnbind(this);default:{let n=(r=o[this.data])!==null&&r!==void 0?r:o[this.data]=this.dataBinding.createObserver(this,this);n.target=o,n.controller=e;let a=n.bind(e);if(e._skipAttrUpdates&&(this.aspectType===S.attribute||this.aspectType===S.booleanAttribute))break;this.updateTarget(o,this.targetAspect,a,e);break}}}unbind(e){let o=e.targets[this.targetNodeId].$fastView;o!==void 0&&o.isComposed&&(o.unbind(),o.needsBindOnly=!0)}handleEvent(e){let r=e.currentTarget[this.data];if(r.isBound){te.setEvent(e);let o=this.dataBinding.evaluate(r.source,r.context);te.setEvent(null),o!==!0&&e.preventDefault()}}handleChange(e,r){let o=r.controller;if(!o.isBound)return;let n=r.target;this.updateTarget(n,this.targetAspect,r.bind(o),o)}};L.define(_,{aspected:!0});var Nn=(t,e)=>`${t}.${e}`,xn={},U={index:0,node:null};function yn(t){t.startsWith("fast-")||w.warn(N.hostBindingWithoutHost,{name:t})}var Rc=new Proxy(document.createElement("div"),{get(t,e){yn(e);let r=Reflect.get(t,e);return W(r)?r.bind(t):r},set(t,e,r){return yn(e),Reflect.set(t,e,r)}}),Xr=class{constructor(e,r,o){this.fragment=e,this.directives=r,this.policy=o,this.proto=null,this.nodeIds=new Set,this.descriptors={},this.factories=[]}addFactory(e,r,o,n,a){var c,s;this.nodeIds.has(o)||(this.nodeIds.add(o),this.addTargetDescriptor(r,o,n)),e.id=(c=e.id)!==null&&c!==void 0?c:qe(),e.targetNodeId=o,e.targetTagName=a,e.policy=(s=e.policy)!==null&&s!==void 0?s:this.policy,this.factories.push(e)}freeze(){return this.proto=Object.create(null,this.descriptors),this}addTargetDescriptor(e,r,o){let n=this.descriptors;if(r==="r"||r==="h"||n[r])return;if(!n[e]){let c=e.lastIndexOf("."),s=e.substring(0,c),d=parseInt(e.substring(c+1),10);this.addTargetDescriptor(s,e,d)}let a=xn[r];if(!a){let c=`_${r}`;xn[r]=a={get(){var s;return(s=this[c])!==null&&s!==void 0?s:this[c]=this[e].childNodes[o]}}}n[r]=a}createView(e){let r=this.fragment.cloneNode(!0),o=Object.create(this.proto);o.r=r,o.h=e??Rc;for(let n of this.nodeIds)Reflect.get(o,n);return new me(r,this.factories,o)}};function Fn(t,e,r,o,n,a=!1){let c=r.attributes,s=t.directives;for(let d=0,u=c.length;d<u;++d){let i=c[d],h=i.value,m=Ye.parse(h,s),b=null;m===null?a&&(b=new _(Pt(()=>h,t.policy)),L.assignAspect(b,i.name)):b=_t.aggregate(m,t.policy),b!==null&&(r.removeAttributeNode(i),d--,u--,t.addFactory(b,e,o,n,r.tagName))}}function Ec(t,e,r,o,n){let a=Ye.parse(e.textContent,t.directives);if(a===null)return U.node=e.nextSibling,U.index=n+1,U;let c,s=c=e;for(let d=0,u=a.length;d<u;++d){let i=a[d];if(d!==0&&(n++,o=Nn(r,n),c=s.parentNode.insertBefore(document.createTextNode(""),s.nextSibling)),$(i))c.textContent=i;else{if(L.assignAspect(i),i instanceof _){let h=document.createComment("");c.parentNode.replaceChild(h,c),c=h}else c.textContent=" ";t.addFactory(i,r,o,n,null)}s=c}return U.index=n+1,U.node=s.nextSibling,U}function wn(t,e,r){let o=0,n=e.firstChild;for(;n;){let a=zc(t,r,n,o);n=a.node,o=a.index}}function zc(t,e,r,o){let n=Nn(e,o);switch(r.nodeType){case 1:Fn(t,e,r,n,o),wn(t,r,n);break;case 3:return Ec(t,r,e,n,o);case 8:{let a=Ye.parse(r.data,t.directives);a!==null&&t.addFactory(_t.aggregate(a),e,n,o,null);break}}return U.index=o+1,U.node=r.nextSibling,U}function _c(t,e){if(!t||t.nodeType!==8&&t.nodeType!==3)return!1;let r=Ye.parse(t.data,e);return r!==null&&(t.nodeType===8||!$(r[0]))}var Pn="TEMPLATE",_t={compile(t,e,r=oe.policy){let o;if($(t)){o=document.createElement(Pn),o.innerHTML=r.createHTML(t);let c=o.content.firstElementChild;c!==null&&c.tagName===Pn&&(o=c)}else o=t;!o.content.firstChild&&!o.content.lastChild&&o.content.appendChild(document.createComment(""));let n=document.adoptNode(o.content),a=new Xr(n,e,r);return Fn(a,"",o,"h",0,!0),(_c(n.firstChild,e)||n.childNodes.length===1&&Object.keys(e).length>0)&&n.insertBefore(document.createComment(""),n.firstChild),wn(a,n,"r"),U.node=null,a.freeze()},setDefaultStrategy(t){this.compile=t},aggregate(t,e=oe.policy){if(t.length===1)return t[0];let r,o=!1,n,a=t.length,c=t.map(u=>$(u)?()=>u:(r=u.sourceAspect||r,o=o||u.dataBinding.isVolatile,n=n||u.dataBinding.policy,u.dataBinding.evaluate)),s=(u,i)=>{let h="";for(let m=0;m<a;++m)h+=c[m](u,i);return h},d=new _(Nt(s,n??e,o));return L.assignAspect(d,r),d}};var et=class extends fe{bind(e){e.source[this.options]=e.targets[this.targetNodeId]}};L.define(et);var Vt=t=>new et(t);var Vc=/([ \x09\x0a\x0c\x0d])([^\0-\x1F\x7F-\x9F "'>=/]+)([ \x09\x0a\x0c\x0d]*=[ \x09\x0a\x0c\x0d]*(?:[^ \x09\x0a\x0c\x0d"'`<>=]*|"[^"]*|'[^']*))$/,jc=Object.create(null),V=class{constructor(e,r=jc){this.html=e,this.factories=r}createHTML(e){let r=this.factories;for(let o in r)e(r[o]);return this.html}};V.empty=new V("");L.define(V);function Gc(t,e,r,o=L.getForInstance(t)){if(o.aspected){let n=Vc.exec(e);n!==null&&L.assignAspect(t,n[2])}return t.createHTML(r)}var tt=class t{constructor(e,r={},o){this.policy=o,this.result=null,this.html=e,this.factories=r}compile(){return this.result===null&&(this.result=_t.compile(this.html,this.factories,this.policy)),this.result}inline(){return new V($(this.html)?this.html:this.html.innerHTML,this.factories)}withPolicy(e){if(this.result)throw w.error(N.cannotSetTemplatePolicyAfterCompilation);if(this.policy)throw w.error(N.onlySetTemplatePolicyOnce);return this.policy=e,this}render(e,r,o){let n=this.create(o);return n.bind(e),n.appendTo(r),n}create(e){return this.compile().createView(e)}static create(e,r,o){let n="",a=Object.create(null),c=s=>{var d;let u=(d=s.id)!==null&&d!==void 0?d:s.id=qe();return a[u]=s,u};for(let s=0,d=e.length-1;s<d;++s){let u=e[s],i=r[s],h;if(n+=u,W(i))i=new _(Nt(i));else if(i instanceof ce)i=new _(i);else if(!(h=L.getForInstance(i))){let m=i;i=new _(Pt(()=>m))}n+=Gc(i,u,c,h)}return new t(n+e[e.length-1],a,o)}};I(tt);var j=((t,...e)=>{if(Array.isArray(t)&&Array.isArray(t.raw))return tt.create(t,e);throw w.error(N.directCallToHTMLTagNotAllowed)});j.partial=t=>new V(t);var Tn="slotchange",rt=class extends Ot{observe(e){e.addEventListener(Tn,this)}disconnect(e){e.removeEventListener(Tn,this)}getNodes(e){return e.assignedNodes(this.options)}handleEvent(e){let r=e.currentTarget;this.updateTarget(this.getSource(r),this.computeNodes(r))}};L.define(rt);function Ur(t){return $(t)&&(t={property:t}),new rt(t)}function be(t){return t?typeof t=="string"?new V(t):"inline"in t?t.inline():t:V.empty}var He=class{};function jt(t){return j` <slot name="end" ${Vt("end")}>${be(t.end)}</slot> `.inline()}function Gt(t){return j` <slot name="start" ${Vt("start")}>${be(t.start)}</slot> `.inline()}function Wt(t,...e){let r=Fe.locate(t);e.forEach(o=>{Object.getOwnPropertyNames(o.prototype).forEach(a=>{a!=="constructor"&&Object.defineProperty(t.prototype,a,Object.getOwnPropertyDescriptor(o.prototype,a))}),Fe.locate(o).forEach(a=>r.push(a))})}var E=Object.freeze({prefix:"fluent",shadowRootMode:"open",registry:globalThis.customElements});var Wc=":host([hidden]){display:none}";function De(t){return`${Wc}:host{display:${t}}`}var $e="var(--colorNeutralForeground1)",An="var(--colorNeutralForeground1Hover)",Cn="var(--colorNeutralForeground1Pressed)";var qr="var(--colorNeutralForeground2)",Hn="var(--colorNeutralForeground2Hover)",Dn="var(--colorNeutralForeground2Pressed)";var Yr="var(--colorNeutralForeground2BrandHover)",Kr="var(--colorNeutralForeground2BrandPressed)";var q="var(--colorNeutralForeground3)",$n="var(--colorNeutralForeground3Hover)",Ln="var(--colorNeutralForeground3Pressed)";var ot="var(--colorNeutralForegroundDisabled)";var ne="var(--colorNeutralForegroundOnBrand)",Xt="var(--colorNeutralForegroundInverted)",In="var(--colorNeutralForegroundInvertedHover)",On="var(--colorNeutralForegroundInvertedPressed)";var Mn="var(--colorNeutralForegroundStaticInverted)";var Jr="var(--colorBrandForeground1)",Rn="var(--colorBrandForeground2)";var En="var(--colorNeutralForeground1Static)";var Be="var(--colorNeutralBackground1)",zn="var(--colorNeutralBackground1Hover)",_n="var(--colorNeutralBackground1Pressed)";var Vn="var(--colorNeutralBackground4)";var jn="var(--colorNeutralBackground5)";var Gn="var(--colorSubtleBackground)",Wn="var(--colorSubtleBackgroundHover)",Xn="var(--colorSubtleBackgroundPressed)";var de="var(--colorTransparentBackground)",Qr="var(--colorTransparentBackgroundHover)",Zr="var(--colorTransparentBackgroundPressed)";var Ut="var(--colorNeutralBackgroundDisabled)";var qt="var(--colorBrandBackground)",Un="var(--colorBrandBackgroundHover)",qn="var(--colorBrandBackgroundPressed)";var eo="var(--colorCompoundBrandBackground)",to="var(--colorCompoundBrandBackgroundHover)",ro="var(--colorCompoundBrandBackgroundPressed)";var Yn="var(--colorBrandBackground2)";var Yt="var(--colorNeutralStrokeAccessible)",Kn="var(--colorNeutralStrokeAccessibleHover)",Jn="var(--colorNeutralStrokeAccessiblePressed)";var Qn="var(--colorNeutralStroke1)",Zn="var(--colorNeutralStroke1Hover)",ea="var(--colorNeutralStroke1Pressed)";var Kt="var(--colorNeutralStroke2)";var ta="var(--colorBrandStroke2)";var nt="var(--colorNeutralStrokeDisabled)";var ae="var(--colorTransparentStroke)";var at="var(--colorStrokeFocus2)";var ra="var(--colorPaletteRedBackground1)";var oa="var(--colorPaletteRedBackground3)";var na="var(--colorPaletteRedBorder1)";var aa="var(--colorPaletteRedForeground1)";var oo="var(--colorPaletteRedForeground3)";var la="var(--colorPaletteGreenBackground1)";var ca="var(--colorPaletteGreenBackground3)";var ia="var(--colorPaletteGreenBorder2)",sa="var(--colorPaletteGreenForeground1)",da="var(--colorPaletteGreenForeground2)",ua="var(--colorPaletteGreenForeground3)";var pa="var(--colorPaletteDarkOrangeBackground1)";var ga="var(--colorPaletteDarkOrangeBackground3)";var ha="var(--colorPaletteDarkOrangeBorder1)";var fa="var(--colorPaletteDarkOrangeForeground1)";var no="var(--colorPaletteDarkOrangeForeground3)",ma="var(--colorPaletteYellowBackground1)";var va="var(--colorPaletteYellowBackground3)";var ba="var(--colorPaletteYellowBorder1)";var Jt="var(--colorPaletteYellowForeground2)";var Qt="var(--borderRadiusNone)",Zt="var(--borderRadiusSmall)",er="var(--borderRadiusMedium)",Ba="var(--borderRadiusLarge)";var Le="var(--borderRadiusCircular)",tr="var(--fontFamilyBase)";var ka="var(--fontSizeBase100)",Ie="var(--fontSizeBase200)",Sa="var(--fontSizeBase300)",xa="var(--fontSizeBase400)";var ya="var(--fontWeightRegular)";var rr="var(--fontWeightSemibold)";var Pa="var(--lineHeightBase100)",Oe="var(--lineHeightBase200)",Na="var(--lineHeightBase300)",Fa="var(--lineHeightBase400)";var wa="var(--shadow2)",or="var(--shadow4)";var nr="var(--strokeWidthThin)",ar="var(--strokeWidthThick)";var ue="var(--spacingHorizontalXXS)",lt="var(--spacingHorizontalXS)",lr="var(--spacingHorizontalSNudge)",Ta="var(--spacingHorizontalS)";var Aa="var(--spacingHorizontalM)",Ca="var(--spacingHorizontalL)";var Ha="var(--durationFaster)";var Da="var(--durationNormal)";var cr="var(--curveEasyEase)";var ct={submit:"submit",reset:"reset",button:"button"};var $a=`${E.prefix}-button`;var La=`${E.prefix}-badge`;var Ia=A.partial`
  ${De("inline-flex")} :host {
    position: relative;
    box-sizing: border-box;
    align-items: center;
    justify-content: center;
    font-family: ${tr};
    font-weight: ${rr};
    font-size: ${Ie};
    line-height: ${Oe};
    min-width: 20px;
    height: 20px;
    padding-inline: calc(${lt} + ${ue});
    border-radius: ${Le};
    border-color: ${ae};
    background-color: ${qt};
    color: ${ne};
    contain: content;
  }

  ::slotted(svg) {
    font-size: 12px;
  }

  :host(:not([appearance='ghost']))::after {
    position: absolute;
    content: '';
    top: 0;
    left: 0;
    bottom: 0;
    right: 0;
    border-style: solid;
    border-width: ${nr};
    border-color: inherit;
    border-radius: inherit;
  }
`,Oa=A.partial`
  :host([size='tiny']) {
    width: 6px;
    height: 6px;
    font-size: 4px;
    line-height: 4px;
    padding-inline: 0;
    min-width: unset;
  }
  :host([size='tiny']) ::slotted(svg) {
    font-size: 6px;
  }
  :host([size='extra-small']) {
    width: 10px;
    height: 10px;
    font-size: 6px;
    line-height: 6px;
    padding-inline: 0;
    min-width: unset;
  }
  :host([size='extra-small']) ::slotted(svg) {
    font-size: 10px;
  }
  :host([size='small']) {
    min-width: 16px;
    height: 16px;
    font-size: ${ka};
    line-height: ${Pa};
    padding-inline: calc(${ue} + ${ue});
  }
  :host([size='small']) ::slotted(svg) {
    font-size: 12px;
  }
  :host([size='large']) {
    min-width: 24px;
    height: 24px;
    font-size: ${Ie};
    line-height: ${Oe};
    padding-inline: calc(${lt} + ${ue});
  }
  :host([size='large']) ::slotted(svg) {
    font-size: 16px;
  }
  :host([size='extra-large']) {
    min-width: 32px;
    height: 32px;
    font-size: ${Ie};
    line-height: ${Oe};
    padding-inline: calc(${lr} + ${ue});
  }
  :host([size='extra-large']) ::slotted(svg) {
    font-size: 20px;
  }
`,Ma=A.partial`
  :host([color='danger']) {
    background-color: ${oa};
    color: ${ne};
  }

  :host([color='important']) {
    background-color: ${$e};
    color: ${Be};
  }

  :host([color='informative']) {
    background-color: ${jn};
    color: ${q};
  }

  :host([color='severe']) {
    background-color: ${ga};
    color: ${ne};
  }

  :host([color='subtle']) {
    background-color: ${Be};
    color: ${$e};
  }

  :host([color='success']) {
    background-color: ${ca};
    color: ${ne};
  }

  :host([color='warning']) {
    background-color: ${va};
    color: ${En};
  }
`,Ra=A.partial`
  :host([appearance='ghost']) {
    color: ${Jr};
    background-color: initial;
  }

  :host([appearance='ghost'][color='danger']) {
    color: ${oo};
  }

  :host([appearance='ghost'][color='important']) {
    color: ${$e};
  }

  :host([appearance='ghost'][color='informative']) {
    color: ${q};
  }

  :host([appearance='ghost'][color='severe']) {
    color: ${no};
  }

  :host([appearance='ghost'][color='subtle']) {
    color: ${Xt};
  }

  :host([appearance='ghost'][color='success']) {
    color: ${ua};
  }

  :host([appearance='ghost'][color='warning']) {
    color: ${Jt};
  }
`,Ea=A.partial`
  :host([appearance='outline']) {
    border-color: currentColor;
    color: ${Jr};
    background-color: initial;
  }

  :host([appearance='outline'][color='danger']) {
    color: ${oo};
  }

  :host([appearance='outline'][color='important']) {
    color: ${q};
    border-color: ${Yt};
  }

  :host([appearance='outline'][color='informative']) {
    color: ${q};
    border-color: ${Kt};
  }

  :host([appearance='outline'][color='severe']) {
    color: ${no};
  }

  :host([appearance='outline'][color='subtle']) {
    color: ${Mn};
  }

  :host([appearance='outline'][color='success']) {
    color: ${da};
  }

  :host([appearance='outline'][color='warning']) {
    color: ${Jt};
  }
`,za=A.partial`
  :host([appearance='tint']) {
    background-color: ${Yn};
    color: ${Rn};
    border-color: ${ta};
  }

  :host([appearance='tint'][color='danger']) {
    background-color: ${ra};
    color: ${aa};
    border-color: ${na};
  }

  :host([appearance='tint'][color='important']) {
    background-color: ${q};
    color: ${Be};
    border-color: ${ae};
  }

  :host([appearance='tint'][color='informative']) {
    background-color: ${Vn};
    color: ${q};
    border-color: ${Kt};
  }

  :host([appearance='tint'][color='severe']) {
    background-color: ${pa};
    color: ${fa};
    border-color: ${ha};
  }

  :host([appearance='tint'][color='subtle']) {
    background-color: ${Be};
    color: ${q};
    border-color: ${Kt};
  }

  :host([appearance='tint'][color='success']) {
    background-color: ${la};
    color: ${sa};
    border-color: ${ia};
  }

  :host([appearance='tint'][color='warning']) {
    background-color: ${ma};
    color: ${Jt};
    border-color: ${ba};
  }
`;var _a=A`
  :host([shape='square']) {
    border-radius: ${Qt};
  }

  :host([shape='rounded']) {
    border-radius: ${er};
  }

  :host([shape='rounded']:is([size='tiny'], [size='extra-small'], [size='small'])) {
    border-radius: ${Zt};
  }

  ${za}
  ${Ea}
  ${Ra}
  ${Ma}
  ${Oa}
  ${Ia}

  @media (forced-colors: active) {
    :host,
    :host([appearance='outline']),
    :host([appearance='tint']) {
      border-color: CanvasText;
    }
  }
`;function Xc(t={}){return j`
    ${Gt(t)}
    <slot>${be(t.defaultContent)}</slot>
    ${jt(t)}
  `}var Va=Xc();var ja={name:La,registry:E.registry,styles:_a,template:Va};var it=class extends he{};Wt(it,He);var Uc=A`
  ${De("inline-flex")}

  :host {
    --icon-spacing: ${lr};
    position: relative;
    contain: layout style;
    vertical-align: middle;
    align-items: center;
    box-sizing: border-box;
    justify-content: center;
    text-align: center;
    text-decoration-line: none;
    margin: 0;
    min-height: 32px;
    outline-style: none;
    background-color: ${Be};
    color: ${$e};
    border: ${nr} solid ${Qn};
    padding: 0 ${Aa};
    min-width: 96px;
    border-radius: ${er};
    font-size: ${Sa};
    font-family: ${tr};
    font-weight: ${rr};
    line-height: ${Na};
    transition-duration: ${Ha};
    transition-property: background, border, color;
    transition-timing-function: ${cr};
    cursor: pointer;
    user-select: none;
  }

  .content {
    display: inherit;
  }

  :host(:hover) {
    background-color: ${zn};
    color: ${An};
    border-color: ${Zn};
  }

  :host(:hover:active) {
    background-color: ${_n};
    border-color: ${ea};
    color: ${Cn};
    outline-style: none;
  }

  :host(:focus-visible) {
    border-color: ${ae};
    outline: ${ar} solid ${ae};
    box-shadow: ${or}, 0 0 0 2px ${at};
  }

  @media screen and (prefers-reduced-motion: reduce) {
    :host {
      transition-duration: 0.01ms;
    }
  }

  ::slotted(svg) {
    font-size: 20px;
    height: 20px;
    width: 20px;
    fill: currentColor;
  }

  ::slotted([slot='start']) {
    margin-inline-end: var(--icon-spacing);
  }

  ::slotted([slot='end']),
  [slot='end'] {
    flex-shrink: 0;
    margin-inline-start: var(--icon-spacing);
  }

  :host([icon-only]) {
    min-width: 32px;
    max-width: 32px;
  }

  :host([size='small']) {
    --icon-spacing: ${lt};
    min-height: 24px;
    min-width: 64px;
    padding: 0 ${Ta};
    border-radius: ${Zt};
    font-size: ${Ie};
    line-height: ${Oe};
    font-weight: ${ya};
  }

  :host([size='small'][icon-only]) {
    min-width: 24px;
    max-width: 24px;
  }

  :host([size='large']) {
    min-height: 40px;
    border-radius: ${Ba};
    padding: 0 ${Ca};
    font-size: ${xa};
    line-height: ${Fa};
  }

  :host([size='large'][icon-only]) {
    min-width: 40px;
    max-width: 40px;
  }

  :host([size='large']) ::slotted(svg) {
    font-size: 24px;
    height: 24px;
    width: 24px;
  }

  :host(:is([shape='circular'], [shape='circular']:focus-visible)) {
    border-radius: ${Le};
  }

  :host(:is([shape='square'], [shape='square']:focus-visible)) {
    border-radius: ${Qt};
  }

  :host([appearance='primary']) {
    background-color: ${qt};
    color: ${ne};
    border-color: transparent;
  }

  :host([appearance='primary']:hover) {
    background-color: ${Un};
  }

  :host([appearance='primary']:is(:hover, :hover:active):not(:focus-visible)) {
    border-color: transparent;
  }

  :host([appearance='primary']:is(:hover, :hover:active)) {
    color: ${ne};
  }

  :host([appearance='primary']:hover:active) {
    background-color: ${qn};
  }

  :host([appearance='primary']:focus-visible) {
    border-color: ${ne};
    box-shadow: ${wa}, 0 0 0 2px ${at};
  }

  :host([appearance='outline']) {
    background-color: ${de};
  }

  :host([appearance='outline']:hover) {
    background-color: ${Qr};
  }

  :host([appearance='outline']:hover:active) {
    background-color: ${Zr};
  }

  :host([appearance='subtle']) {
    background-color: ${Gn};
    color: ${qr};
    border-color: transparent;
  }

  :host([appearance='subtle']:hover) {
    background-color: ${Wn};
    color: ${Hn};
    border-color: transparent;
  }

  :host([appearance='subtle']:hover:active) {
    background-color: ${Xn};
    color: ${Dn};
    border-color: transparent;
  }

  :host([appearance='subtle']:hover) ::slotted(svg) {
    fill: ${Yr};
  }

  :host([appearance='subtle']:hover:active) ::slotted(svg) {
    fill: ${Kr};
  }

  :host([appearance='transparent']) {
    background-color: ${de};
    color: ${qr};
  }

  :host([appearance='transparent']:hover) {
    background-color: ${Qr};
    color: ${Yr};
  }

  :host([appearance='transparent']:hover:active) {
    background-color: ${Zr};
    color: ${Kr};
  }

  :host(:is([appearance='transparent'], [appearance='transparent']:is(:hover, :active))) {
    border-color: transparent;
  }
`,Ga=A`
  ${Uc}

  :host(:is(:disabled, [disabled], [disabled-focusable], [appearance]:disabled, [appearance][disabled], [appearance][disabled-focusable])),
  :host(:is(:disabled, [disabled], [disabled-focusable], [appearance]:disabled, [appearance][disabled], [appearance][disabled-focusable]):hover),
  :host(:is(:disabled, [disabled], [disabled-focusable], [appearance]:disabled, [appearance][disabled], [appearance][disabled-focusable]):hover:active) {
    background-color: ${Ut};
    border-color: ${nt};
    color: ${ot};
    cursor: not-allowed;
  }

  :host([appearance='primary']:is(:disabled, [disabled], [disabled-focusable])),
  :host([appearance='primary']:is(:disabled, [disabled], [disabled-focusable]):is(:hover, :hover:active)) {
    border-color: transparent;
  }

  :host([appearance='outline']:is(:disabled, [disabled], [disabled-focusable])),
  :host([appearance='outline']:is(:disabled, [disabled], [disabled-focusable]):is(:hover, :hover:active)) {
    background-color: ${de};
  }

  :host([appearance='subtle']:is(:disabled, [disabled], [disabled-focusable])),
  :host([appearance='subtle']:is(:disabled, [disabled], [disabled-focusable]):is(:hover, :hover:active)) {
    background-color: ${de};
    border-color: transparent;
  }

  :host([appearance='transparent']:is(:disabled, [disabled], [disabled-focusable])),
  :host([appearance='transparent']:is(:disabled, [disabled], [disabled-focusable]):is(:hover, :hover:active)) {
    border-color: transparent;
    background-color: ${de};
  }

  @media (forced-colors: active) {
    :host {
      background-color: ButtonFace;
      color: ButtonText;
    }

    :host(:is(:hover, :focus-visible)) {
      border-color: Highlight !important;
    }

    :host([appearance='primary']:not(:is(:hover, :focus-visible))) {
      background-color: Highlight;
      color: HighlightText;
      forced-color-adjust: none;
    }

    :host(
        :is(
            :disabled,
            [disabled],
            [disabled-focusable],
            [appearance]:disabled,
            [appearance][disabled],
            [appearance][disabled-focusable]
          )
      ) {
      background-color: ButtonFace;
      color: GrayText;
      border-color: ButtonText;
    }
  }
`;function qc(t={}){return j`
    <template
      @click="${(e,r)=>e.clickHandler(r.event)}"
      @keypress="${(e,r)=>e.keypressHandler(r.event)}"
    >
      ${Gt(t)}
      <span class="content" part="content">
        <slot ${Ur("defaultSlottedContent")}></slot>
      </span>
      ${jt(t)}
    </template>
  `}var Wa=qc();var Xa={name:$a,registry:E.registry,styles:Ga,template:Wa};function ir(t){let e=t.ownerDocument;t?.isConnected&&t?.hasAttribute("autofocus")&&t?.checkVisibility?.({contentVisibilityAuto:!0,visibilityProperty:!0})&&[null,t,e.body,e.documentElement].includes(e.activeElement)&&ee.enqueue(()=>{t.focus()})}var ao=class ao extends he{disabledChanged(){this.setTabIndex()}disabledFocusableChanged(e,r){this.elementInternals&&(this.elementInternals.ariaDisabled=`${!!r}`)}get form(){return this.elementInternals.form}get labels(){return Object.freeze(Array.from(this.elementInternals.labels))}typeChanged(e,r){r!==ct.submit&&(this.formSubmissionFallbackControl?.remove(),this.shadowRoot?.querySelector('slot[name="internal"]')?.remove())}clickHandler(e){if(e&&this.disabledFocusable){e.stopImmediatePropagation();return}return this.press(),!0}connectedCallback(){super.connectedCallback(),this.elementInternals.ariaDisabled=`${!!this.disabledFocusable}`,this.setTabIndex(),ir(this)}constructor(){super(),this.disabledFocusable=!1,this.elementInternals=this.attachInternals(),this.elementInternals.role="button"}createAndInsertFormSubmissionFallbackControl(){let e=this.formSubmissionFallbackControlSlot??document.createElement("slot");e.setAttribute("name","internal"),this.shadowRoot?.appendChild(e),this.formSubmissionFallbackControlSlot=e;let r=this.formSubmissionFallbackControl??document.createElement("button");r.style.display="none",r.setAttribute("type","submit"),r.setAttribute("slot","internal"),this.formNoValidate&&r.toggleAttribute("formnovalidate",!0),this.elementInternals.form?.id&&r.setAttribute("form",this.elementInternals.form.id),this.name&&r.setAttribute("name",this.name),this.value&&r.setAttribute("value",this.value),this.formAction&&r.setAttribute("formaction",this.formAction??""),this.formEnctype&&r.setAttribute("formenctype",this.formEnctype??""),this.formMethod&&r.setAttribute("formmethod",this.formMethod??""),this.formTarget&&r.setAttribute("formtarget",this.formTarget??""),this.append(r),this.formSubmissionFallbackControl=r}formDisabledCallback(e){this.disabled=e}keypressHandler(e){if(e&&this.disabledFocusable){e.stopImmediatePropagation();return}if(e.key==="Enter"||e.key===" "){this.click();return}return!0}press(){switch(this.type){case ct.reset:{this.resetForm();break}case ct.submit:{this.submitForm();break}}}resetForm(){this.elementInternals.form?.reset()}setTabIndex(){if(this.disabled){this.removeAttribute("tabindex");return}this.tabIndex=Number(this.getAttribute("tabindex")??0)<0?-1:0}submitForm(){if(!(!this.elementInternals.form||this.disabled||this.type!==ct.submit)){if(!this.name&&!this.formAction&&!this.formEnctype&&!this.formAttribute&&!this.formMethod&&!this.formNoValidate&&!this.formTarget){this.elementInternals.form.requestSubmit();return}try{this.elementInternals.setFormValue(this.value??""),this.elementInternals.form.requestSubmit(this)}catch{this.createAndInsertFormSubmissionFallbackControl(),this.elementInternals.setFormValue(null),this.elementInternals.form.requestSubmit(this.formSubmissionFallbackControl)}}}};ao.formAssociated=!0;var C=ao;P([We],C.prototype,"defaultSlottedContent",void 0);P([F({mode:"boolean"})],C.prototype,"disabled",void 0);P([F({attribute:"disabled-focusable",mode:"boolean"})],C.prototype,"disabledFocusable",void 0);P([F({attribute:"formaction"})],C.prototype,"formAction",void 0);P([F({attribute:"form"})],C.prototype,"formAttribute",void 0);P([F({attribute:"formenctype"})],C.prototype,"formEnctype",void 0);P([F({attribute:"formmethod"})],C.prototype,"formMethod",void 0);P([F({attribute:"formnovalidate",mode:"boolean"})],C.prototype,"formNoValidate",void 0);P([F({attribute:"formtarget"})],C.prototype,"formTarget",void 0);P([F],C.prototype,"name",void 0);P([F],C.prototype,"type",void 0);P([F],C.prototype,"value",void 0);var st=class extends C{};Wt(st,He);var ip=CSS.supports("anchor-name: --a"),sp="anchor"in HTMLElement.prototype,lo=CSS.supports("selector(:state(g))");var Ua=new Map;function k(t){return Ua.get(t)??Ua.set(t,lo?`:state(${t})`:`[state--${t}]`).get(t)}function co(t,e,r){if(!(!e||!t)){if(!lo){t.shadowRoot.host.toggleAttribute(`state--${e}`,r);return}if(r??!t.states.has(e)){t.states.add(e);return}t.states.delete(e)}}var hp=k("active"),fp=k("bad-input"),H=k("checked"),mp=k("custom-error"),vp=k("description"),bp=k("disabled"),ke=":is([disabled], :disabled)",Bp=k("error"),kp=k("flip-block"),Sp=k("focus-visible"),xp=k("has-message"),yp=k("indeterminate"),Pp=k("multiple"),Np=k("open"),Fp=k("pattern-mismatch"),wp=k("placeholder-shown"),Tp=k("pressed"),Ap=k("range-overflow"),Cp=k("range-underflow"),Hp=k("required"),Dp=k("selected"),$p=k("step-mismatch"),Lp=k("submenu"),Ip=k("too-long"),Op=k("too-short"),Mp=k("type-mismatch"),Rp=k("user-invalid"),Ep=k("valid"),zp=k("value-missing");var io=class io extends he{constructor(){super(...arguments),this.initialValue="on",this._keydownPressed=!1,this.dirtyChecked=!1,this.elementInternals=this.attachInternals(),this._validationFallbackMessage="",this._value=this.initialValue}get checked(){return B.track(this,"checked"),!!this._checked}set checked(e){this._checked=e,this.setFormValue(e?this.value:null),this.setValidity(),this.setAriaChecked(),co(this.elementInternals,"checked",e),B.notify(this,"checked")}disabledChanged(e,r){this.disabled?this.removeAttribute("tabindex"):this.tabIndex=Number(this.getAttribute("tabindex")??0)<0?-1:0,this.elementInternals.ariaDisabled=this.disabled?"true":"false",co(this.elementInternals,"disabled",this.disabled)}disabledAttributeChanged(e,r){this.disabled=!!r}initialCheckedChanged(e,r){this.dirtyChecked||(this.checked=!!r)}initialValueChanged(e,r){this._value=r}requiredChanged(e,r){this.elementInternals&&(this.setValidity(),this.elementInternals.ariaRequired=this.required?"true":"false")}get form(){return this.elementInternals.form}get labels(){return Object.freeze(Array.from(this.elementInternals.labels))}get validationMessage(){if(this.elementInternals?.validationMessage)return this.elementInternals.validationMessage;if(!this._validationFallbackMessage){let e=document.createElement("input");e.type="checkbox",e.required=!0,e.checked=!1,this._validationFallbackMessage=e.validationMessage}return this._validationFallbackMessage}get validity(){return this.elementInternals.validity}get value(){return B.track(this,"value"),this._value}set value(e){this._value=e,this.elementInternals&&(this.setFormValue(e),this.setValidity()),B.notify(this,"value")}get willValidate(){return this.elementInternals.willValidate}checkValidity(){return this.elementInternals.checkValidity()}clickHandler(e){if(this.disabled)return;this.dirtyChecked=!0;let r=this.checked;return this.toggleChecked(),r!==this.checked&&(this.$emit("change"),this.$emit("input")),!0}connectedCallback(){super.connectedCallback(),this.disabled=!!this.disabledAttribute,this.setAriaChecked(),this.setValidity(),ir(this)}inputHandler(e){return this.setFormValue(this.value),this.setValidity(),!0}keydownHandler(e){if(e.key!==" ")return!0;this._keydownPressed=!0}keyupHandler(e){if(!this._keydownPressed||e.key!==" ")return!0;this._keydownPressed=!1,this.click()}formResetCallback(){this.checked=this.initialChecked??!1,this.dirtyChecked=!1,this.setValidity()}reportValidity(){return this.elementInternals.reportValidity()}setAriaChecked(e=this.checked){this.elementInternals&&(this.elementInternals.ariaChecked=e?"true":"false")}setFormValue(e,r){this.elementInternals?.setFormValue(e,e??r)}setCustomValidity(e){this.elementInternals.setValidity({customError:!0},e),this.setValidity()}setValidity(e,r,o){if(this.elementInternals){if(this.disabled||!this.required){this.elementInternals.setValidity({});return}this.elementInternals.setValidity({valueMissing:!!this.required&&!this.checked,...e},r??this.validationMessage,o)}}toggleChecked(e=!this.checked){this.checked=e}};io.formAssociated=!0;var G=io;P([We],G.prototype,"disabled",void 0);P([F({attribute:"disabled",mode:"boolean"})],G.prototype,"disabledAttribute",void 0);P([F({attribute:"form"})],G.prototype,"formAttribute",void 0);P([F({attribute:"checked",mode:"boolean"})],G.prototype,"initialChecked",void 0);P([F({attribute:"value",mode:"fromView"})],G.prototype,"initialValue",void 0);P([F],G.prototype,"name",void 0);P([F({mode:"boolean"})],G.prototype,"required",void 0);var Yc=0;function so(t="id-"){let e=`${t}${Yc++}`;return document.getElementById(e)?so(t):e}var qa=`${E.prefix}-switch`;var Ya=A`
  ${De("inline-flex")}

  :host {
    box-sizing: border-box;
    align-items: center;
    flex-direction: row;
    outline: none;
    user-select: none;
    contain: content;
    padding: 0 ${ue};
    width: 40px;
    height: 20px;
    background-color: ${de};
    border: 1px solid ${Yt};
    border-radius: ${Le};
  }

  :host(:enabled) {
    cursor: pointer;
  }

  :host(:hover) {
    background: none;
    border-color: ${Kn};
  }
  :host(:active) {
    border-color: ${Jn};
  }
  :host(${ke}),
  :host([readonly]) {
    border: 1px solid ${nt};
    background-color: none;
    pointer: default;
  }
  :host(${H}) {
    background: ${eo};
    border-color: ${eo};
  }
  :host(${H}:hover) {
    background: ${to};
    border-color: ${to};
  }
  :host(${H}:active) {
    background: ${ro};
    border-color: ${ro};
  }
  :host(${H}${ke}) {
    background: ${Ut};
    border-color: ${nt};
  }
  .checked-indicator {
    height: 14px;
    width: 14px;
    border-radius: 50%;
    margin-inline-start: 0;
    background-color: ${q};
    transition-duration: ${Da};
    transition-timing-function: ${cr};
    transition-property: margin-inline-start;
  }
  :host(${H}) .checked-indicator {
    background-color: ${Xt};
    margin-inline-start: calc(100% - 14px);
  }
  :host(${H}:hover) .checked-indicator {
    background: ${In};
  }
  :host(${H}:active) .checked-indicator {
    background: ${On};
  }
  :host(:hover) .checked-indicator {
    background-color: ${$n};
  }
  :host(:active) .checked-indicator {
    background-color: ${Ln};
  }
  :host(${ke}) .checked-indicator,
  :host([readonly]) .checked-indicator {
    background: ${ot};
  }
  :host(${H}${ke}) .checked-indicator {
    background: ${ot};
  }

  :host(:focus-visible) {
    outline: none;
  }

  :host(:not([slot='input']):focus-visible) {
    border-color: ${ae};
    outline: ${ar} solid ${ae};
    outline-offset: 1px;
    box-shadow: ${or}, 0 0 0 2px ${at};
  }

  @media (forced-colors: active) {
    :host {
      border-color: InactiveBorder;
    }
    :host(${H}),
    :host(${H}:active),
    :host(${H}:hover) {
      background: Highlight;
      border-color: Highlight;
    }
    .checked-indicator,
    :host(:hover) .checked-indicator,
    :host(:active) .checked-indicator {
      background-color: CanvasText;
    }
    :host(${H}) .checked-indicator,
    :host(${H}:hover) .checked-indicator,
    :host(${H}:active) .checked-indicator {
      background-color: HighlightText;
    }
    :host(${ke}) .checked-indicator,
    :host(${H}${ke}) .checked-indicator {
      background-color: GrayText;
    }
  }
`;function Kc(t={}){return j`
    <template
      @click="${(e,r)=>e.clickHandler(r.event)}"
      @input="${(e,r)=>e.inputHandler(r.event)}"
      @keydown="${(e,r)=>e.keydownHandler(r.event)}"
      @keyup="${(e,r)=>e.keyupHandler(r.event)}"
    >
      <slot name="switch">${be(t.switch)}</slot>
    </template>
  `}var Ka=Kc({switch:'<span class="checked-indicator" part="checked-indicator"></span>'});var Ja={name:qa,registry:E.registry,styles:Ya,template:Ka};var sr=class extends G{constructor(){super(),this.elementInternals.role="switch"}};var Jc="adoptedStyleSheets"in document,Qc="CSSScopeRule"in window,uo=new Map,po=new Map,ut=new Map,go=new Map,dt=new CSSStyleSheet;function Me(t,e=document){if(!(!e||!oi(e))){if(!Jc||e instanceof HTMLElement&&!e.shadowRoot&&!Qc){let r=e===document?document.documentElement:e;ii(t,r);return}[document,document.documentElement,document.body].includes(e)?ni(t):ai(t,e)}}var Zc=/^[a-zA-Z_-][a-zA-Z0-9_-]*$/;function ei(t){return Zc.test(t)?t:""}var ti=/(;|{|}|\/\*|\*\/|@import|url\s*\(|expression\s*\(|javascript:)/i;function ri(t){return ti.test(t)?"":t}function ho(t){return uo.has(t)||uo.set(t,Object.keys(t).reduce((e,r)=>{let o=ei(r),n=ri(t[r].toString());return o&&n?`${e}--${o}:${n};`:e},"")),uo.get(t)}function oi(t){return[document,document.documentElement].includes(t)||t instanceof HTMLElement&&!!t.closest("body")}function ni(t){if(t===null){document.adoptedStyleSheets.includes(dt)&&dt.replaceSync("");return}dt.replaceSync(`
    html {
      ${ho(t)}
    }
  `),document.adoptedStyleSheets.includes(dt)||document.adoptedStyleSheets.push(dt)}function ai(t,e){if(t===null){e.shadowRoot&&ut.has(e)?ut.get(e).replaceSync(""):(delete e.dataset.fluentTheme,Qa(e));return}e.shadowRoot?li(e).replaceSync(`
      :host {
        ${ho(t)}
      }
    `):(e.dataset.fluentTheme=ci(t),Qa(e))}function li(t){if(!ut.has(t)){let e=new CSSStyleSheet;ut.set(t,e),t.shadowRoot?.adoptedStyleSheets.push(e)}return ut.get(t)}function ci(t){if(!po.has(t)){let e=so("fluent-theme-"),r=new CSSStyleSheet;po.set(t,e),r.replaceSync(`
      @scope ([data-fluent-theme="${e}"]) {
        :scope {
          ${ho(t)}
        }
      }
    `),document.adoptedStyleSheets.push(r)}return po.get(t)}function ii(t,e){let r;if(t===null){if(!go.has(e))return;r=go.get(e)}else go.set(e,t),r=t;for(let[o,n]of Object.entries(r))t===null?e.style.removeProperty(`--${o}`):e.style.setProperty(`--${o}`,n.toString())}var{userAgent:si}=navigator,di=/\bAppleWebKit\/[\d+\.]+\b/.test(si);function Qa(t){if(!di)return;let e="visibility",r="hidden",o=t.style.getPropertyValue(e);t.style.setProperty(e,r),ee.process(),t.style.setProperty(e,o)}var l={2:"#050505",4:"#0a0a0a",6:"#0f0f0f",8:"#141414",10:"#1a1a1a",12:"#1f1f1f",14:"#242424",16:"#292929",18:"#2e2e2e",20:"#333333",22:"#383838",24:"#3d3d3d",26:"#424242",28:"#474747",30:"#4d4d4d",32:"#525252",34:"#575757",36:"#5c5c5c",38:"#616161",40:"#666666",42:"#6b6b6b",44:"#707070",46:"#757575",48:"#7a7a7a",50:"#808080",52:"#858585",54:"#8a8a8a",56:"#8f8f8f",58:"#949494",60:"#999999",62:"#9e9e9e",64:"#a3a3a3",66:"#a8a8a8",68:"#adadad",70:"#b3b3b3",72:"#b8b8b8",74:"#bdbdbd",76:"#c2c2c2",78:"#c7c7c7",80:"#cccccc",82:"#d1d1d1",84:"#d6d6d6",86:"#dbdbdb",88:"#e0e0e0",90:"#e6e6e6",92:"#ebebeb",94:"#f0f0f0",96:"#f5f5f5",98:"#fafafa",99:"#fcfcfc"},T={5:"rgba(255, 255, 255, 0.05)",10:"rgba(255, 255, 255, 0.1)",20:"rgba(255, 255, 255, 0.2)",30:"rgba(255, 255, 255, 0.3)",40:"rgba(255, 255, 255, 0.4)",50:"rgba(255, 255, 255, 0.5)",60:"rgba(255, 255, 255, 0.6)",70:"rgba(255, 255, 255, 0.7)",80:"rgba(255, 255, 255, 0.8)",90:"rgba(255, 255, 255, 0.9)"},R={5:"rgba(0, 0, 0, 0.05)",10:"rgba(0, 0, 0, 0.1)",20:"rgba(0, 0, 0, 0.2)",30:"rgba(0, 0, 0, 0.3)",40:"rgba(0, 0, 0, 0.4)",50:"rgba(0, 0, 0, 0.5)",60:"rgba(0, 0, 0, 0.6)",70:"rgba(0, 0, 0, 0.7)",80:"rgba(0, 0, 0, 0.8)",90:"rgba(0, 0, 0, 0.9)"},Za={5:"rgba(26, 26, 26, 0.05)",10:"rgba(26, 26, 26, 0.1)",20:"rgba(26, 26, 26, 0.2)",30:"rgba(26, 26, 26, 0.3)",40:"rgba(26, 26, 26, 0.4)",50:"rgba(26, 26, 26, 0.5)",60:"rgba(26, 26, 26, 0.6)",70:"rgba(26, 26, 26, 0.7)",80:"rgba(26, 26, 26, 0.8)",90:"rgba(26, 26, 26, 0.9)"},el={5:"rgba(31, 31, 31, 0.05)",10:"rgba(31, 31, 31, 0.1)",20:"rgba(31, 31, 31, 0.2)",30:"rgba(31, 31, 31, 0.3)",40:"rgba(31, 31, 31, 0.4)",50:"rgba(31, 31, 31, 0.5)",60:"rgba(31, 31, 31, 0.6)",70:"rgba(31, 31, 31, 0.7)",80:"rgba(31, 31, 31, 0.8)",90:"rgba(31, 31, 31, 0.9)"},fo={5:"rgba(36, 36, 36, 0.05)",10:"rgba(36, 36, 36, 0.1)",20:"rgba(36, 36, 36, 0.2)",30:"rgba(36, 36, 36, 0.3)",40:"rgba(36, 36, 36, 0.4)",50:"rgba(36, 36, 36, 0.5)",60:"rgba(36, 36, 36, 0.6)",70:"rgba(36, 36, 36, 0.7)",80:"rgba(36, 36, 36, 0.8)",90:"rgba(36, 36, 36, 0.9)"},g="#ffffff",Re="#000000";var tl={shade50:"#130204",shade40:"#230308",shade30:"#420610",shade20:"#590815",shade10:"#690a19",primary:"#750b1c",tint10:"#861b2c",tint20:"#962f3f",tint30:"#ac4f5e",tint40:"#d69ca5",tint50:"#e9c7cd",tint60:"#f9f0f2"};var mo={shade50:"#200205",shade40:"#3b0509",shade30:"#6e0811",shade20:"#960b18",shade10:"#b10e1c",primary:"#c50f1f",tint10:"#cc2635",tint20:"#d33f4c",tint30:"#dc626d",tint40:"#eeacb2",tint50:"#f6d1d5",tint60:"#fdf3f4"},rl={shade50:"#210809",shade40:"#3f1011",shade30:"#751d1f",shade20:"#9f282b",shade10:"#bc2f32",primary:"#d13438",tint10:"#d7494c",tint20:"#dc5e62",tint30:"#e37d80",tint40:"#f1bbbc",tint50:"#f8dadb",tint60:"#fdf6f6"},ol={shade50:"#230900",shade40:"#411200",shade30:"#7a2101",shade20:"#a62d01",shade10:"#c43501",primary:"#da3b01",tint10:"#de501c",tint20:"#e36537",tint30:"#e9835e",tint40:"#f4bfab",tint50:"#f9dcd1",tint60:"#fdf6f3"};var nl={shade50:"#200d03",shade40:"#3d1805",shade30:"#712d09",shade20:"#9a3d0c",shade10:"#b6480e",primary:"#ca5010",tint10:"#d06228",tint20:"#d77440",tint30:"#df8e64",tint40:"#efc4ad",tint50:"#f7dfd2",tint60:"#fdf7f4"},al={shade50:"#271002",shade40:"#4a1e04",shade30:"#8a3707",shade20:"#bc4b09",shade10:"#de590b",primary:"#f7630c",tint10:"#f87528",tint20:"#f98845",tint30:"#faa06b",tint40:"#fdcfb4",tint50:"#fee5d7",tint60:"#fff9f5"},ll={shade50:"#291600",shade40:"#4d2a00",shade30:"#8f4e00",shade20:"#c26a00",shade10:"#e67e00",primary:"#ff8c00",tint10:"#ff9a1f",tint20:"#ffa83d",tint30:"#ffba66",tint40:"#ffddb3",tint50:"#ffedd6",tint60:"#fffaf5"},cl={shade50:"#251a00",shade40:"#463100",shade30:"#835b00",shade20:"#b27c00",shade10:"#d39300",primary:"#eaa300",tint10:"#edad1c",tint20:"#efb839",tint30:"#f2c661",tint40:"#f9e2ae",tint50:"#fcefd3",tint60:"#fefbf4"},il={shade50:"#282400",shade40:"#4c4400",shade30:"#817400",shade20:"#c0ad00",shade10:"#e4cc00",primary:"#fde300",tint10:"#fde61e",tint20:"#fdea3d",tint30:"#feee66",tint40:"#fef7b2",tint50:"#fffad6",tint60:"#fffef5"},sl={shade50:"#1f1900",shade40:"#3a2f00",shade30:"#6c5700",shade20:"#937700",shade10:"#ae8c00",primary:"#c19c00",tint10:"#c8a718",tint20:"#d0b232",tint30:"#dac157",tint40:"#ecdfa5",tint50:"#f5eece",tint60:"#fdfbf2"},dl={shade50:"#181202",shade40:"#2e2103",shade30:"#553e06",shade20:"#745408",shade10:"#89640a",primary:"#986f0b",tint10:"#a47d1e",tint20:"#b18c34",tint30:"#c1a256",tint40:"#e0cea2",tint50:"#efe4cb",tint60:"#fbf8f2"},ul={shade50:"#170e07",shade40:"#2b1a0e",shade30:"#50301a",shade20:"#6c4123",shade10:"#804d29",primary:"#8e562e",tint10:"#9c663f",tint20:"#a97652",tint30:"#bb8f6f",tint40:"#ddc3b0",tint50:"#edded3",tint60:"#faf7f4"};var pl={shade50:"#0c1501",shade40:"#162702",shade30:"#294903",shade20:"#376304",shade10:"#427505",primary:"#498205",tint10:"#599116",tint20:"#6ba02b",tint30:"#85b44c",tint40:"#bdd99b",tint50:"#dbebc7",tint60:"#f6faf0"},gl={shade50:"#002111",shade40:"#003d20",shade30:"#00723b",shade20:"#009b51",shade10:"#00b85f",primary:"#00cc6a",tint10:"#19d279",tint20:"#34d889",tint30:"#5ae0a0",tint40:"#a8f0cd",tint50:"#cff7e4",tint60:"#f3fdf8"},hl={shade50:"#031a02",shade40:"#063004",shade30:"#0b5a08",shade20:"#0e7a0b",shade10:"#11910d",primary:"#13a10e",tint10:"#27ac22",tint20:"#3db838",tint30:"#5ec75a",tint40:"#a7e3a5",tint50:"#cef0cd",tint60:"#f2fbf2"},vo={shade50:"#031403",shade40:"#052505",shade30:"#094509",shade20:"#0c5e0c",shade10:"#0e700e",primary:"#107c10",tint10:"#218c21",tint20:"#359b35",tint30:"#54b054",tint40:"#9fd89f",tint50:"#c9eac9",tint60:"#f1faf1"},fl={shade50:"#021102",shade40:"#032003",shade30:"#063b06",shade20:"#085108",shade10:"#0a5f0a",primary:"#0b6a0b",tint10:"#1a7c1a",tint20:"#2d8e2d",tint30:"#4da64d",tint40:"#9ad29a",tint50:"#c6e7c6",tint60:"#f0f9f0"},ml={shade50:"#001d1f",shade40:"#00373a",shade30:"#00666d",shade20:"#008b94",shade10:"#00a5af",primary:"#00b7c3",tint10:"#18bfca",tint20:"#32c8d1",tint30:"#58d3db",tint40:"#a6e9ed",tint50:"#cef3f5",tint60:"#f2fcfd"},vl={shade50:"#001516",shade40:"#012728",shade30:"#02494c",shade20:"#026467",shade10:"#037679",primary:"#038387",tint10:"#159195",tint20:"#2aa0a4",tint30:"#4cb4b7",tint40:"#9bd9db",tint50:"#c7ebec",tint60:"#f0fafa"};var bl={shade50:"#000f12",shade40:"#001b22",shade30:"#00333f",shade20:"#004555",shade10:"#005265",primary:"#005b70",tint10:"#0f6c81",tint20:"#237d92",tint30:"#4496a9",tint40:"#94c8d4",tint50:"#c3e1e8",tint60:"#eff7f9"};var Bl={shade50:"#001322",shade40:"#002440",shade30:"#004377",shade20:"#005ba1",shade10:"#006cbf",primary:"#0078d4",tint10:"#1a86d9",tint20:"#3595de",tint30:"#5caae5",tint40:"#a9d3f2",tint50:"#d0e7f8",tint60:"#f3f9fd"},kl={shade50:"#000c16",shade40:"#00172a",shade30:"#002c4e",shade20:"#003b6a",shade10:"#00467e",primary:"#004e8c",tint10:"#125e9a",tint20:"#286fa8",tint30:"#4a89ba",tint40:"#9abfdc",tint50:"#c7dced",tint60:"#f0f6fa"};var Sl={shade50:"#0d1126",shade40:"#182047",shade30:"#2c3c85",shade20:"#3c51b4",shade10:"#4760d5",primary:"#4f6bed",tint10:"#637cef",tint20:"#778df1",tint30:"#93a4f4",tint40:"#c8d1fa",tint50:"#e1e6fc",tint60:"#f7f9fe"},xl={shade50:"#00061d",shade40:"#000c36",shade30:"#001665",shade20:"#001e89",shade10:"#0023a2",primary:"#0027b4",tint10:"#173bbd",tint20:"#3050c6",tint30:"#546fd2",tint40:"#a3b2e8",tint50:"#ccd5f3",tint60:"#f2f4fc"},yl={shade50:"#120f25",shade40:"#221d46",shade30:"#3f3682",shade20:"#5649b0",shade10:"#6656d1",primary:"#7160e8",tint10:"#8172eb",tint20:"#9184ee",tint30:"#a79cf1",tint40:"#d2ccf8",tint50:"#e7e4fb",tint60:"#f9f8fe"},Pl={shade50:"#0f0717",shade40:"#1c0e2b",shade30:"#341a51",shade20:"#46236e",shade10:"#532982",primary:"#5c2e91",tint10:"#6b3f9e",tint20:"#7c52ab",tint30:"#9470bd",tint40:"#c6b1de",tint50:"#e0d3ed",tint60:"#f7f4fb"};var Nl={shade50:"#160418",shade40:"#29072e",shade30:"#4c0d55",shade20:"#671174",shade10:"#7a1589",primary:"#881798",tint10:"#952aa4",tint20:"#a33fb1",tint30:"#b55fc1",tint40:"#d9a7e0",tint50:"#eaceef",tint60:"#faf2fb"},Fl={shade50:"#1f091d",shade40:"#3a1136",shade30:"#6d2064",shade20:"#932b88",shade10:"#af33a1",primary:"#c239b3",tint10:"#c94cbc",tint20:"#d161c4",tint30:"#da7ed0",tint40:"#edbbe7",tint50:"#f5daf2",tint60:"#fdf5fc"},wl={shade50:"#1c0b1f",shade40:"#35153a",shade30:"#63276d",shade20:"#863593",shade10:"#9f3faf",primary:"#b146c2",tint10:"#ba58c9",tint20:"#c36bd1",tint30:"#cf87da",tint40:"#e6bfed",tint50:"#f2dcf5",tint60:"#fcf6fd"},Tl={shade50:"#24091b",shade40:"#441232",shade30:"#80215d",shade20:"#ad2d7e",shade10:"#cd3595",primary:"#e43ba6",tint10:"#e750b0",tint20:"#ea66ba",tint30:"#ef85c8",tint40:"#f7c0e3",tint50:"#fbddf0",tint60:"#fef6fb"};var Al={shade50:"#1f0013",shade40:"#390024",shade30:"#6b0043",shade20:"#91005a",shade10:"#ac006b",primary:"#bf0077",tint10:"#c71885",tint20:"#ce3293",tint30:"#d957a8",tint40:"#eca5d1",tint50:"#f5cee6",tint60:"#fcf2f9"},Cl={shade50:"#13000c",shade40:"#240017",shade30:"#43002b",shade20:"#5a003b",shade10:"#6b0045",primary:"#77004d",tint10:"#87105d",tint20:"#98246f",tint30:"#ad4589",tint40:"#d696c0",tint50:"#e9c4dc",tint60:"#faf0f6"},Hl={shade50:"#141313",shade40:"#252323",shade30:"#444241",shade20:"#5d5958",shade10:"#6e6968",primary:"#7a7574",tint10:"#8a8584",tint20:"#9a9594",tint30:"#afabaa",tint40:"#d7d4d4",tint50:"#eae8e8",tint60:"#faf9f9"},Dl={shade50:"#0f0e0e",shade40:"#1c1b1a",shade30:"#343231",shade20:"#474443",shade10:"#54514f",primary:"#5d5a58",tint10:"#706d6b",tint20:"#84817e",tint30:"#9e9b99",tint40:"#cecccb",tint50:"#e5e4e3",tint60:"#f8f8f8"};var $l={shade50:"#111314",shade40:"#1f2426",shade30:"#3b4447",shade20:"#505c60",shade10:"#5f6d71",primary:"#69797e",tint10:"#79898d",tint20:"#89989d",tint30:"#a0adb2",tint40:"#cdd6d8",tint50:"#e4e9ea",tint60:"#f8f9fa"},Ll={shade50:"#090a0b",shade40:"#111315",shade30:"#202427",shade20:"#2b3135",shade10:"#333a3f",primary:"#394146",tint10:"#4d565c",tint20:"#626c72",tint30:"#808a90",tint40:"#bcc3c7",tint50:"#dbdfe1",tint60:"#f6f7f8"};var v={red:rl,green:vo,darkOrange:ol,yellow:il,berry:Fl,lightGreen:hl,marigold:cl},Y={darkRed:tl,cranberry:mo,pumpkin:nl,peach:ll,gold:sl,brass:dl,brown:ul,forest:pl,seafoam:gl,darkGreen:fl,lightTeal:ml,teal:vl,steel:bl,blue:Bl,royalBlue:kl,cornflower:Sl,navy:xl,lavender:yl,purple:Pl,grape:Nl,lilac:wl,pink:Tl,magenta:Al,plum:Cl,beige:Hl,mink:Dl,platinum:$l,anchor:Ll},f={cranberry:mo,green:vo,orange:al};var dr=["red","green","darkOrange","yellow","berry","lightGreen","marigold"],ur=["darkRed","cranberry","pumpkin","peach","gold","brass","brown","forest","seafoam","darkGreen","lightTeal","teal","steel","blue","royalBlue","cornflower","navy","lavender","purple","grape","lilac","pink","magenta","plum","beige","mink","platinum","anchor"];var D={success:"green",warning:"orange",danger:"cranberry"};var pt=dr.reduce((t,e)=>{let r=e.slice(0,1).toUpperCase()+e.slice(1),o={[`colorPalette${r}Background1`]:v[e].tint60,[`colorPalette${r}Background2`]:v[e].tint40,[`colorPalette${r}Background3`]:v[e].primary,[`colorPalette${r}Foreground1`]:v[e].shade10,[`colorPalette${r}Foreground2`]:v[e].shade30,[`colorPalette${r}Foreground3`]:v[e].primary,[`colorPalette${r}BorderActive`]:v[e].primary,[`colorPalette${r}Border1`]:v[e].tint40,[`colorPalette${r}Border2`]:v[e].primary};return Object.assign(t,o)},{});pt.colorPaletteYellowForeground1=v.yellow.shade30;pt.colorPaletteRedForegroundInverted=v.red.tint20;pt.colorPaletteGreenForegroundInverted=v.green.tint20;pt.colorPaletteYellowForegroundInverted=v.yellow.tint40;var ui=ur.reduce((t,e)=>{let r=e.slice(0,1).toUpperCase()+e.slice(1),o={[`colorPalette${r}Background2`]:Y[e].tint40,[`colorPalette${r}Foreground2`]:Y[e].shade30,[`colorPalette${r}BorderActive`]:Y[e].primary};return Object.assign(t,o)},{}),Il={...pt,...ui},Se=Object.entries(D).reduce((t,[e,r])=>{let o=e.slice(0,1).toUpperCase()+e.slice(1),n={[`colorStatus${o}Background1`]:f[r].tint60,[`colorStatus${o}Background2`]:f[r].tint40,[`colorStatus${o}Background3`]:f[r].primary,[`colorStatus${o}Foreground1`]:f[r].shade10,[`colorStatus${o}Foreground2`]:f[r].shade30,[`colorStatus${o}Foreground3`]:f[r].primary,[`colorStatus${o}ForegroundInverted`]:f[r].tint30,[`colorStatus${o}BorderActive`]:f[r].primary,[`colorStatus${o}Border1`]:f[r].tint40,[`colorStatus${o}Border2`]:f[r].primary};return Object.assign(t,n)},{});Se.colorStatusDangerBackground3Hover=f[D.danger].shade10;Se.colorStatusDangerBackground3Pressed=f[D.danger].shade20;Se.colorStatusWarningForeground1=f[D.warning].shade20;Se.colorStatusWarningForeground3=f[D.warning].shade20;Se.colorStatusWarningBorder2=f[D.warning].shade20;var Ol=t=>({colorNeutralForeground1:l[14],colorNeutralForeground1Hover:l[14],colorNeutralForeground1Pressed:l[14],colorNeutralForeground1Selected:l[14],colorNeutralForeground2:l[26],colorNeutralForeground2Hover:l[14],colorNeutralForeground2Pressed:l[14],colorNeutralForeground2Selected:l[14],colorNeutralForeground2BrandHover:t[80],colorNeutralForeground2BrandPressed:t[70],colorNeutralForeground2BrandSelected:t[80],colorNeutralForeground3:l[38],colorNeutralForeground3Hover:l[26],colorNeutralForeground3Pressed:l[26],colorNeutralForeground3Selected:l[26],colorNeutralForeground3BrandHover:t[80],colorNeutralForeground3BrandPressed:t[70],colorNeutralForeground3BrandSelected:t[80],colorNeutralForeground4:l[44],colorNeutralForeground5:l[38],colorNeutralForeground5Hover:l[14],colorNeutralForeground5Pressed:l[14],colorNeutralForeground5Selected:l[14],colorNeutralForegroundDisabled:l[74],colorNeutralForegroundInvertedDisabled:T[40],colorBrandForegroundLink:t[70],colorBrandForegroundLinkHover:t[60],colorBrandForegroundLinkPressed:t[40],colorBrandForegroundLinkSelected:t[70],colorNeutralForeground2Link:l[26],colorNeutralForeground2LinkHover:l[14],colorNeutralForeground2LinkPressed:l[14],colorNeutralForeground2LinkSelected:l[14],colorCompoundBrandForeground1:t[80],colorCompoundBrandForeground1Hover:t[70],colorCompoundBrandForeground1Pressed:t[60],colorBrandForeground1:t[80],colorBrandForeground2:t[70],colorBrandForeground2Hover:t[60],colorBrandForeground2Pressed:t[30],colorNeutralForeground1Static:l[14],colorNeutralForegroundStaticInverted:g,colorNeutralForegroundInverted:g,colorNeutralForegroundInvertedHover:g,colorNeutralForegroundInvertedPressed:g,colorNeutralForegroundInvertedSelected:g,colorNeutralForegroundInverted2:g,colorNeutralForegroundOnBrand:g,colorNeutralForegroundInvertedLink:g,colorNeutralForegroundInvertedLinkHover:g,colorNeutralForegroundInvertedLinkPressed:g,colorNeutralForegroundInvertedLinkSelected:g,colorBrandForegroundInverted:t[100],colorBrandForegroundInvertedHover:t[110],colorBrandForegroundInvertedPressed:t[100],colorBrandForegroundOnLight:t[80],colorBrandForegroundOnLightHover:t[70],colorBrandForegroundOnLightPressed:t[50],colorBrandForegroundOnLightSelected:t[60],colorNeutralBackground1:g,colorNeutralBackground1Hover:l[96],colorNeutralBackground1Pressed:l[88],colorNeutralBackground1Selected:l[92],colorNeutralBackground2:l[98],colorNeutralBackground2Hover:l[94],colorNeutralBackground2Pressed:l[86],colorNeutralBackground2Selected:l[90],colorNeutralBackground3:l[96],colorNeutralBackground3Hover:l[92],colorNeutralBackground3Pressed:l[84],colorNeutralBackground3Selected:l[88],colorNeutralBackground4:l[94],colorNeutralBackground4Hover:l[98],colorNeutralBackground4Pressed:l[96],colorNeutralBackground4Selected:g,colorNeutralBackground5:l[92],colorNeutralBackground5Hover:l[96],colorNeutralBackground5Pressed:l[94],colorNeutralBackground5Selected:l[98],colorNeutralBackground6:l[90],colorNeutralBackground7:"#00000000",colorNeutralBackground7Hover:l[92],colorNeutralBackground7Pressed:l[84],colorNeutralBackground7Selected:"#00000000",colorNeutralBackground8:l[99],colorNeutralBackgroundInverted:l[16],colorNeutralBackgroundInvertedHover:l[24],colorNeutralBackgroundInvertedPressed:l[12],colorNeutralBackgroundInvertedSelected:l[22],colorNeutralBackgroundStatic:l[20],colorNeutralBackgroundAlpha:T[50],colorNeutralBackgroundAlpha2:T[80],colorSubtleBackground:"transparent",colorSubtleBackgroundHover:l[96],colorSubtleBackgroundPressed:l[88],colorSubtleBackgroundSelected:l[92],colorSubtleBackgroundLightAlphaHover:T[70],colorSubtleBackgroundLightAlphaPressed:T[50],colorSubtleBackgroundLightAlphaSelected:"transparent",colorSubtleBackgroundInverted:"transparent",colorSubtleBackgroundInvertedHover:R[10],colorSubtleBackgroundInvertedPressed:R[30],colorSubtleBackgroundInvertedSelected:R[20],colorTransparentBackground:"transparent",colorTransparentBackgroundHover:"transparent",colorTransparentBackgroundPressed:"transparent",colorTransparentBackgroundSelected:"transparent",colorNeutralBackgroundDisabled:l[94],colorNeutralBackgroundDisabled2:g,colorNeutralBackgroundInvertedDisabled:T[10],colorNeutralStencil1:l[90],colorNeutralStencil2:l[98],colorNeutralStencil1Alpha:R[10],colorNeutralStencil2Alpha:R[5],colorBackgroundOverlay:R[40],colorScrollbarOverlay:R[50],colorBrandBackground:t[80],colorBrandBackgroundHover:t[70],colorBrandBackgroundPressed:t[40],colorBrandBackgroundSelected:t[60],colorCompoundBrandBackground:t[80],colorCompoundBrandBackgroundHover:t[70],colorCompoundBrandBackgroundPressed:t[60],colorBrandBackgroundStatic:t[80],colorBrandBackground2:t[160],colorBrandBackground2Hover:t[150],colorBrandBackground2Pressed:t[130],colorBrandBackground3Static:t[60],colorBrandBackground4Static:t[40],colorBrandBackgroundInverted:g,colorBrandBackgroundInvertedHover:t[160],colorBrandBackgroundInvertedPressed:t[140],colorBrandBackgroundInvertedSelected:t[150],colorNeutralCardBackground:l[98],colorNeutralCardBackgroundHover:g,colorNeutralCardBackgroundPressed:l[96],colorNeutralCardBackgroundSelected:l[92],colorNeutralCardBackgroundDisabled:l[94],colorNeutralStrokeAccessible:l[38],colorNeutralStrokeAccessibleHover:l[34],colorNeutralStrokeAccessiblePressed:l[30],colorNeutralStrokeAccessibleSelected:t[80],colorNeutralStroke1:l[82],colorNeutralStroke1Hover:l[78],colorNeutralStroke1Pressed:l[70],colorNeutralStroke1Selected:l[74],colorNeutralStroke2:l[88],colorNeutralStroke3:l[94],colorNeutralStroke4:l[92],colorNeutralStroke4Hover:l[88],colorNeutralStroke4Pressed:l[84],colorNeutralStroke4Selected:l[92],colorNeutralStrokeSubtle:l[88],colorNeutralStrokeOnBrand:g,colorNeutralStrokeOnBrand2:g,colorNeutralStrokeOnBrand2Hover:g,colorNeutralStrokeOnBrand2Pressed:g,colorNeutralStrokeOnBrand2Selected:g,colorBrandStroke1:t[80],colorBrandStroke2:t[140],colorBrandStroke2Hover:t[120],colorBrandStroke2Pressed:t[80],colorBrandStroke2Contrast:t[140],colorCompoundBrandStroke:t[80],colorCompoundBrandStrokeHover:t[70],colorCompoundBrandStrokePressed:t[60],colorNeutralStrokeDisabled:l[88],colorNeutralStrokeDisabled2:l[92],colorNeutralStrokeInvertedDisabled:T[40],colorTransparentStroke:"transparent",colorTransparentStrokeInteractive:"transparent",colorTransparentStrokeDisabled:"transparent",colorNeutralStrokeAlpha:R[5],colorNeutralStrokeAlpha2:T[20],colorStrokeFocus1:g,colorStrokeFocus2:Re,colorNeutralShadowAmbient:"rgba(0,0,0,0.12)",colorNeutralShadowKey:"rgba(0,0,0,0.14)",colorNeutralShadowAmbientLighter:"rgba(0,0,0,0.06)",colorNeutralShadowKeyLighter:"rgba(0,0,0,0.07)",colorNeutralShadowAmbientDarker:"rgba(0,0,0,0.20)",colorNeutralShadowKeyDarker:"rgba(0,0,0,0.24)",colorBrandShadowAmbient:"rgba(0,0,0,0.30)",colorBrandShadowKey:"rgba(0,0,0,0.25)"});var gt={borderRadiusNone:"0",borderRadiusSmall:"2px",borderRadiusMedium:"4px",borderRadiusLarge:"6px",borderRadiusXLarge:"8px",borderRadius2XLarge:"12px",borderRadius3XLarge:"16px",borderRadius4XLarge:"24px",borderRadius5XLarge:"32px",borderRadius6XLarge:"40px",borderRadiusCircular:"10000px"};var pr={curveAccelerateMax:"cubic-bezier(0.9,0.1,1,0.2)",curveAccelerateMid:"cubic-bezier(1,0,1,1)",curveAccelerateMin:"cubic-bezier(0.8,0,0.78,1)",curveDecelerateMax:"cubic-bezier(0.1,0.9,0.2,1)",curveDecelerateMid:"cubic-bezier(0,0,0,1)",curveDecelerateMin:"cubic-bezier(0.33,0,0.1,1)",curveEasyEaseMax:"cubic-bezier(0.8,0,0.2,1)",curveEasyEase:"cubic-bezier(0.33,0,0.67,1)",curveLinear:"cubic-bezier(0,0,1,1)"};var gr={durationUltraFast:"50ms",durationFaster:"100ms",durationFast:"150ms",durationNormal:"200ms",durationGentle:"250ms",durationSlow:"300ms",durationSlower:"400ms",durationUltraSlow:"500ms"};var ht={fontSizeBase100:"10px",fontSizeBase200:"12px",fontSizeBase300:"14px",fontSizeBase400:"16px",fontSizeBase500:"20px",fontSizeBase600:"24px",fontSizeHero700:"28px",fontSizeHero800:"32px",fontSizeHero900:"40px",fontSizeHero1000:"68px"},ft={lineHeightBase100:"14px",lineHeightBase200:"16px",lineHeightBase300:"20px",lineHeightBase400:"22px",lineHeightBase500:"28px",lineHeightBase600:"32px",lineHeightHero700:"36px",lineHeightHero800:"40px",lineHeightHero900:"52px",lineHeightHero1000:"92px"},mt={fontWeightRegular:400,fontWeightMedium:500,fontWeightSemibold:600,fontWeightBold:700},vt={fontFamilyBase:"'Segoe UI', 'Segoe UI Web (West European)', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', sans-serif",fontFamilyMonospace:"Consolas, 'Courier New', Courier, monospace",fontFamilyNumeric:"Bahnschrift, 'Segoe UI', 'Segoe UI Web (West European)', -apple-system, BlinkMacSystemFont, Roboto, 'Helvetica Neue', sans-serif"};var y={none:"0",xxs:"2px",xs:"4px",sNudge:"6px",s:"8px",mNudge:"10px",m:"12px",l:"16px",xl:"20px",xxl:"24px",xxxl:"32px"},hr={spacingHorizontalNone:y.none,spacingHorizontalXXS:y.xxs,spacingHorizontalXS:y.xs,spacingHorizontalSNudge:y.sNudge,spacingHorizontalS:y.s,spacingHorizontalMNudge:y.mNudge,spacingHorizontalM:y.m,spacingHorizontalL:y.l,spacingHorizontalXL:y.xl,spacingHorizontalXXL:y.xxl,spacingHorizontalXXXL:y.xxxl},fr={spacingVerticalNone:y.none,spacingVerticalXXS:y.xxs,spacingVerticalXS:y.xs,spacingVerticalSNudge:y.sNudge,spacingVerticalS:y.s,spacingVerticalMNudge:y.mNudge,spacingVerticalM:y.m,spacingVerticalL:y.l,spacingVerticalXL:y.xl,spacingVerticalXXL:y.xxl,spacingVerticalXXXL:y.xxxl};var bt={strokeWidthThin:"1px",strokeWidthThick:"2px",strokeWidthThicker:"3px",strokeWidthThickest:"4px"};function Ee(t,e,r=""){return{[`shadow2${r}`]:`0 0 2px ${t}, 0 1px 2px ${e}`,[`shadow4${r}`]:`0 0 2px ${t}, 0 2px 4px ${e}`,[`shadow8${r}`]:`0 0 2px ${t}, 0 4px 8px ${e}`,[`shadow16${r}`]:`0 0 2px ${t}, 0 8px 16px ${e}`,[`shadow28${r}`]:`0 0 8px ${t}, 0 14px 28px ${e}`,[`shadow64${r}`]:`0 0 8px ${t}, 0 32px 64px ${e}`}}var ze=t=>{let e=Ol(t);return{...gt,...ht,...ft,...vt,...mt,...bt,...hr,...fr,...gr,...pr,...e,...Il,...Se,...Ee(e.colorNeutralShadowAmbient,e.colorNeutralShadowKey),...Ee(e.colorBrandShadowAmbient,e.colorBrandShadowKey,"Brand")}};var mr={10:"#061724",20:"#082338",30:"#0a2e4a",40:"#0c3b5e",50:"#0e4775",60:"#0f548c",70:"#115ea3",80:"#0f6cbd",90:"#2886de",100:"#479ef5",110:"#62abf5",120:"#77b7f7",130:"#96c6fa",140:"#b4d6fa",150:"#cfe4fa",160:"#ebf3fc"};var K=dr.reduce((t,e)=>{let r=e.slice(0,1).toUpperCase()+e.slice(1),o={[`colorPalette${r}Background1`]:v[e].shade40,[`colorPalette${r}Background2`]:v[e].shade30,[`colorPalette${r}Background3`]:v[e].primary,[`colorPalette${r}Foreground1`]:v[e].tint30,[`colorPalette${r}Foreground2`]:v[e].tint40,[`colorPalette${r}Foreground3`]:v[e].tint20,[`colorPalette${r}BorderActive`]:v[e].tint30,[`colorPalette${r}Border1`]:v[e].primary,[`colorPalette${r}Border2`]:v[e].tint20};return Object.assign(t,o)},{});K.colorPaletteRedForeground3=v.red.tint30;K.colorPaletteRedBorder2=v.red.tint30;K.colorPaletteGreenForeground3=v.green.tint40;K.colorPaletteGreenBorder2=v.green.tint40;K.colorPaletteDarkOrangeForeground3=v.darkOrange.tint30;K.colorPaletteDarkOrangeBorder2=v.darkOrange.tint30;K.colorPaletteRedForegroundInverted=v.red.primary;K.colorPaletteGreenForegroundInverted=v.green.primary;K.colorPaletteYellowForegroundInverted=v.yellow.shade30;var bo=ur.reduce((t,e)=>{let r=e.slice(0,1).toUpperCase()+e.slice(1),o={[`colorPalette${r}Background2`]:Y[e].shade30,[`colorPalette${r}Foreground2`]:Y[e].tint40,[`colorPalette${r}BorderActive`]:Y[e].tint30};return Object.assign(t,o)},{});bo.colorPaletteDarkRedBackground2=Y.darkRed.shade20;bo.colorPalettePlumBackground2=Y.plum.shade20;var Ml={...K,...bo},le=Object.entries(D).reduce((t,[e,r])=>{let o=e.slice(0,1).toUpperCase()+e.slice(1),n={[`colorStatus${o}Background1`]:f[r].shade40,[`colorStatus${o}Background2`]:f[r].shade30,[`colorStatus${o}Background3`]:f[r].primary,[`colorStatus${o}Foreground1`]:f[r].tint30,[`colorStatus${o}Foreground2`]:f[r].tint40,[`colorStatus${o}Foreground3`]:f[r].tint20,[`colorStatus${o}BorderActive`]:f[r].tint30,[`colorStatus${o}ForegroundInverted`]:f[r].shade10,[`colorStatus${o}Border1`]:f[r].primary,[`colorStatus${o}Border2`]:f[r].tint20};return Object.assign(t,n)},{});le.colorStatusDangerBackground3Hover=f[D.danger].shade10;le.colorStatusDangerBackground3Pressed=f[D.danger].shade20;le.colorStatusDangerForeground3=f[D.danger].tint40;le.colorStatusDangerBorder2=f[D.danger].tint30;le.colorStatusSuccessForeground3=f[D.success].tint40;le.colorStatusSuccessBorder2=f[D.success].tint40;le.colorStatusWarningForegroundInverted=f[D.warning].shade20;var _e=ze(mr);var Rl=t=>({colorNeutralForeground1:g,colorNeutralForeground1Hover:g,colorNeutralForeground1Pressed:g,colorNeutralForeground1Selected:g,colorNeutralForeground2:l[84],colorNeutralForeground2Hover:g,colorNeutralForeground2Pressed:g,colorNeutralForeground2Selected:g,colorNeutralForeground2BrandHover:t[100],colorNeutralForeground2BrandPressed:t[90],colorNeutralForeground2BrandSelected:t[100],colorNeutralForeground3:l[68],colorNeutralForeground3Hover:l[84],colorNeutralForeground3Pressed:l[84],colorNeutralForeground3Selected:l[84],colorNeutralForeground3BrandHover:t[100],colorNeutralForeground3BrandPressed:t[90],colorNeutralForeground3BrandSelected:t[100],colorNeutralForeground4:l[60],colorNeutralForeground5:l[68],colorNeutralForeground5Hover:g,colorNeutralForeground5Pressed:g,colorNeutralForeground5Selected:g,colorNeutralForegroundDisabled:l[36],colorNeutralForegroundInvertedDisabled:T[40],colorBrandForegroundLink:t[100],colorBrandForegroundLinkHover:t[110],colorBrandForegroundLinkPressed:t[90],colorBrandForegroundLinkSelected:t[100],colorNeutralForeground2Link:l[84],colorNeutralForeground2LinkHover:g,colorNeutralForeground2LinkPressed:g,colorNeutralForeground2LinkSelected:g,colorCompoundBrandForeground1:t[100],colorCompoundBrandForeground1Hover:t[110],colorCompoundBrandForeground1Pressed:t[90],colorBrandForeground1:t[100],colorBrandForeground2:t[110],colorBrandForeground2Hover:t[130],colorBrandForeground2Pressed:t[160],colorNeutralForeground1Static:l[14],colorNeutralForegroundStaticInverted:g,colorNeutralForegroundInverted:l[14],colorNeutralForegroundInvertedHover:l[14],colorNeutralForegroundInvertedPressed:l[14],colorNeutralForegroundInvertedSelected:l[14],colorNeutralForegroundInverted2:l[14],colorNeutralForegroundOnBrand:g,colorNeutralForegroundInvertedLink:g,colorNeutralForegroundInvertedLinkHover:g,colorNeutralForegroundInvertedLinkPressed:g,colorNeutralForegroundInvertedLinkSelected:g,colorBrandForegroundInverted:t[80],colorBrandForegroundInvertedHover:t[70],colorBrandForegroundInvertedPressed:t[60],colorBrandForegroundOnLight:t[80],colorBrandForegroundOnLightHover:t[70],colorBrandForegroundOnLightPressed:t[50],colorBrandForegroundOnLightSelected:t[60],colorNeutralBackground1:l[16],colorNeutralBackground1Hover:l[24],colorNeutralBackground1Pressed:l[12],colorNeutralBackground1Selected:l[22],colorNeutralBackground2:l[12],colorNeutralBackground2Hover:l[20],colorNeutralBackground2Pressed:l[8],colorNeutralBackground2Selected:l[18],colorNeutralBackground3:l[8],colorNeutralBackground3Hover:l[16],colorNeutralBackground3Pressed:l[4],colorNeutralBackground3Selected:l[14],colorNeutralBackground4:l[4],colorNeutralBackground4Hover:l[12],colorNeutralBackground4Pressed:Re,colorNeutralBackground4Selected:l[10],colorNeutralBackground5:Re,colorNeutralBackground5Hover:l[8],colorNeutralBackground5Pressed:l[2],colorNeutralBackground5Selected:l[6],colorNeutralBackground6:l[20],colorNeutralBackground7:"#00000000",colorNeutralBackground7Hover:l[10],colorNeutralBackground7Pressed:l[4],colorNeutralBackground7Selected:"#00000000",colorNeutralBackground8:l[16],colorNeutralBackgroundInverted:g,colorNeutralBackgroundInvertedHover:l[96],colorNeutralBackgroundInvertedPressed:l[88],colorNeutralBackgroundInvertedSelected:l[92],colorNeutralBackgroundStatic:l[24],colorNeutralBackgroundAlpha:Za[50],colorNeutralBackgroundAlpha2:el[70],colorSubtleBackground:"transparent",colorSubtleBackgroundHover:l[22],colorSubtleBackgroundPressed:l[18],colorSubtleBackgroundSelected:l[20],colorSubtleBackgroundLightAlphaHover:fo[80],colorSubtleBackgroundLightAlphaPressed:fo[50],colorSubtleBackgroundLightAlphaSelected:"transparent",colorSubtleBackgroundInverted:"transparent",colorSubtleBackgroundInvertedHover:R[10],colorSubtleBackgroundInvertedPressed:R[30],colorSubtleBackgroundInvertedSelected:R[20],colorTransparentBackground:"transparent",colorTransparentBackgroundHover:"transparent",colorTransparentBackgroundPressed:"transparent",colorTransparentBackgroundSelected:"transparent",colorNeutralBackgroundDisabled:l[8],colorNeutralBackgroundDisabled2:l[16],colorNeutralBackgroundInvertedDisabled:T[10],colorNeutralStencil1:l[34],colorNeutralStencil2:l[20],colorNeutralStencil1Alpha:T[10],colorNeutralStencil2Alpha:T[5],colorBackgroundOverlay:R[50],colorScrollbarOverlay:T[60],colorBrandBackground:t[70],colorBrandBackgroundHover:t[80],colorBrandBackgroundPressed:t[40],colorBrandBackgroundSelected:t[60],colorCompoundBrandBackground:t[100],colorCompoundBrandBackgroundHover:t[110],colorCompoundBrandBackgroundPressed:t[90],colorBrandBackgroundStatic:t[80],colorBrandBackground2:t[20],colorBrandBackground2Hover:t[40],colorBrandBackground2Pressed:t[10],colorBrandBackground3Static:t[60],colorBrandBackground4Static:t[40],colorBrandBackgroundInverted:g,colorBrandBackgroundInvertedHover:t[160],colorBrandBackgroundInvertedPressed:t[140],colorBrandBackgroundInvertedSelected:t[150],colorNeutralCardBackground:l[20],colorNeutralCardBackgroundHover:l[24],colorNeutralCardBackgroundPressed:l[18],colorNeutralCardBackgroundSelected:l[22],colorNeutralCardBackgroundDisabled:l[8],colorNeutralStrokeAccessible:l[68],colorNeutralStrokeAccessibleHover:l[74],colorNeutralStrokeAccessiblePressed:l[70],colorNeutralStrokeAccessibleSelected:t[100],colorNeutralStroke1:l[40],colorNeutralStroke1Hover:l[46],colorNeutralStroke1Pressed:l[42],colorNeutralStroke1Selected:l[44],colorNeutralStroke2:l[32],colorNeutralStroke3:l[24],colorNeutralStroke4:l[24],colorNeutralStroke4Hover:l[18],colorNeutralStroke4Pressed:l[14],colorNeutralStroke4Selected:l[24],colorNeutralStrokeSubtle:l[4],colorNeutralStrokeOnBrand:l[16],colorNeutralStrokeOnBrand2:g,colorNeutralStrokeOnBrand2Hover:g,colorNeutralStrokeOnBrand2Pressed:g,colorNeutralStrokeOnBrand2Selected:g,colorBrandStroke1:t[100],colorBrandStroke2:t[50],colorBrandStroke2Hover:t[50],colorBrandStroke2Pressed:t[30],colorBrandStroke2Contrast:t[50],colorCompoundBrandStroke:t[100],colorCompoundBrandStrokeHover:t[110],colorCompoundBrandStrokePressed:t[90],colorNeutralStrokeDisabled:l[26],colorNeutralStrokeDisabled2:l[24],colorNeutralStrokeInvertedDisabled:T[40],colorTransparentStroke:"transparent",colorTransparentStrokeInteractive:"transparent",colorTransparentStrokeDisabled:"transparent",colorNeutralStrokeAlpha:T[10],colorNeutralStrokeAlpha2:T[20],colorStrokeFocus1:Re,colorStrokeFocus2:g,colorNeutralShadowAmbient:"rgba(0,0,0,0.24)",colorNeutralShadowKey:"rgba(0,0,0,0.28)",colorNeutralShadowAmbientLighter:"rgba(0,0,0,0.12)",colorNeutralShadowKeyLighter:"rgba(0,0,0,0.14)",colorNeutralShadowAmbientDarker:"rgba(0,0,0,0.40)",colorNeutralShadowKeyDarker:"rgba(0,0,0,0.48)",colorBrandShadowAmbient:"rgba(0,0,0,0.30)",colorBrandShadowKey:"rgba(0,0,0,0.25)"});var Ve=t=>{let e=Rl(t);return{...gt,...ht,...ft,...vt,...mt,...bt,...hr,...fr,...gr,...pr,...e,...Ml,...le,...Ee(e.colorNeutralShadowAmbient,e.colorNeutralShadowKey),...Ee(e.colorBrandShadowAmbient,e.colorBrandShadowKey,"Brand")}};var Bt=Ve(mr);st.define(Xa);sr.define(Ja);it.define(ja);function El(t){return t&&["Light","Dark"].includes(t.theme)&&["accentColor","accentFillColor","accentTextColor"].every(e=>/^#[0-9a-f]{6}$/i.test(t[e]||""))}function kt(t,e,r){let o=a=>a.match(/[a-f0-9]{2}/gi).map(c=>parseInt(c,16)),n=o(t);return"#"+o(e).map((a,c)=>Math.round(n[c]+(a-n[c])*r).toString(16).padStart(2,"0")).join("")}function pi(t){let e=t.match(/[a-f0-9]{2}/gi).map(o=>{let n=parseInt(o,16)/255;return n<=.04045?n/12.92:((n+.055)/1.055)**2.4}),r=e[0]*.2126+e[1]*.7152+e[2]*.0722;return(r+.05)/.05>1.05/(r+.05)?"#000000":"#ffffff"}function Bo(t){return Object.fromEntries(Array.from({length:16},(e,r)=>{let o=(r+1)*10;return[o,o<=80?kt("#000000",t,(o/80)**1.2):kt(t,"#ffffff",(o-80)/90)]}))}function zl(t){let e=t.theme==="Dark",r=t.accentFillColor,o=kt(r,e?"#ffffff":"#000000",.08),n=kt(r,e?"#ffffff":"#000000",.16);return{colorBrandBackground:r,colorBrandBackgroundHover:o,colorBrandBackgroundPressed:n,colorBrandBackgroundSelected:n,colorCompoundBrandBackground:r,colorCompoundBrandBackgroundHover:o,colorCompoundBrandBackgroundPressed:n,colorBrandForeground1:t.accentTextColor,colorBrandForeground2:t.accentTextColor,colorBrandForegroundLink:t.accentTextColor,colorBrandForegroundLinkHover:t.accentTextColor,colorBrandForegroundLinkPressed:t.accentTextColor,colorCompoundBrandForeground1:r,colorCompoundBrandForeground1Hover:o,colorCompoundBrandForeground1Pressed:n,colorCompoundBrandStroke:r,colorBrandStroke1:r,colorBrandStroke2:kt(r,e?"#202020":"#ffffff",.7),colorNeutralForegroundOnBrand:pi(r),colorNeutralBackground1:e?"#2c2c2c":"#ffffff",colorNeutralBackground2:e?"#202020":"#f5f6f7",colorNeutralForeground1:e?"#f5f5f5":"#1a1a1a",colorNeutralForeground2:e?"#cecece":"#4a4a4a",fontFamilyBase:'"Segoe UI Variable Text", "Segoe UI", "Microsoft YaHei UI", "Microsoft YaHei", system-ui, sans-serif',fontFamilyNumeric:'"Segoe UI Variable Display", "Segoe UI", system-ui, sans-serif',borderRadiusMedium:"4px",borderRadiusLarge:"8px"}}var J=document.documentElement,xe,br="",pe,So,_l;function Br(t){J.dataset.appearanceState=t?"synced":"reconnecting";for(let e of document.querySelectorAll(".theme-sync")){let r=xe?.theme==="Dark"?"深色":"浅色";e.textContent=t?`${r} · 与电脑同步`:"外观同步重连中",e.title=xe?`电脑外观：${r}；${xe.accentSource==="System"?"系统":"自定义"}强调色 ${xe.accentColor}`:"正在读取电脑外观"}}function kr(t){if(!El(t))return!1;Br(!0);let e=JSON.stringify(t);if(e===br)return!0;br=e,xe=Object.freeze({...t});let r=t.theme==="Dark",o={...r?Ve(Bo(t.accentColor)):ze(Bo(t.accentColor)),...zl(t)};Me(o),J.dataset.theme=r?"dark":"light",J.style.colorScheme=r?"dark":"light",J.style.setProperty("--cx-accent",t.accentColor),J.style.setProperty("--cx-accent-fill",t.accentFillColor),J.style.setProperty("--cx-accent-text",t.accentTextColor),J.style.setProperty("--cx-on-accent",o.colorNeutralForegroundOnBrand);let n=t.accentFillColor.match(/[a-f0-9]{2}/gi).map(a=>parseInt(a,16)).join(", ");return J.style.setProperty("--cx-accent-rgb",n),document.querySelector('meta[name="theme-color"]')?.setAttribute("content",o.colorNeutralBackground2),Br(!0),document.dispatchEvent(new CustomEvent("cx-appearance-changed",{detail:xe})),!0}try{let t=document.querySelector('meta[name="cx-appearance"]')?.content;if(!kr(t?JSON.parse(t):null)){let e=matchMedia("(prefers-color-scheme: dark)").matches;J.dataset.theme=e?"dark":"light",Me(e?Bt:_e)}}catch{Me(_e)}var ko=!1;async function vr(){if(ko)return;ko=!0;let t=br,e=new AbortController;_l=e;let r=setTimeout(()=>e.abort(),5e3);try{let o=await fetch("/api/appearance",{cache:"no-store",signal:e.signal});if(!o.ok)throw new Error("Appearance unavailable");let n=await o.json();if(br===t&&!kr(n))throw new Error("Invalid appearance")}catch{e.signal.aborted||Br(!1)}finally{clearTimeout(r),ko=!1}}function Vl(){pe?.close(),pe=void 0,clearInterval(So),So=void 0,_l?.abort()}function xo(){Vl(),"EventSource"in window&&(pe=new EventSource("/api/appearance/events"),pe.onmessage=t=>{try{kr(JSON.parse(t.data))}catch{}},pe.onopen=()=>{vr()},pe.onerror=()=>{Br(!1)}),So=setInterval(()=>{!document.hidden&&(!pe||pe.readyState!==EventSource.OPEN)&&vr()},5e3),vr()}window.CXAppearance=Object.freeze({apply:kr,get current(){return xe}});window.addEventListener("pagehide",Vl);window.addEventListener("pageshow",t=>{t.persisted&&xo()});window.addEventListener("online",xo);document.addEventListener("visibilitychange",()=>{document.hidden||vr()});xo();J.classList.add("fluent-ready");export{kr as applyAppearance};
