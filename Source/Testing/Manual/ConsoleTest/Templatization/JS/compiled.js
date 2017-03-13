function render(root, ctx){
var ljs_useCtx = WAVE.isObject(ctx);
var ljs_1 = document.createElement('div');
ljs_1.addEventListener('click', function() { console.log('kaka') }, false);
var ljs_useCtx = WAVE.isObject(ctx);
var ljs_2 = document.createElement('div');
ljs_2.setAttribute('class', ljs_useCtx ? WAVE.strHTMLTemplate("title", ctx) : "title");
ljs_2.setAttribute('data-alert', ljs_useCtx ? WAVE.strHTMLTemplate("alert('just data')", ctx) : "alert('just data')");
ljs_2.setAttribute('data-alert-script', ljs_useCtx ? WAVE.strHTMLTemplate("<script>alert('script')</script>", ctx) : "<script>alert('script')</script>");
ljs_1.appendChild(ljs_2);
var ljs_useCtx = WAVE.isObject(ctx);
var ljs_3 = document.createElement('div');
ljs_3.setAttribute('id', ljs_useCtx ? WAVE.strHTMLTemplate("rate", ctx) : "rate");
ljs_1.appendChild(ljs_3);
var ljs_useCtx = WAVE.isObject(ctx);
var ljs_4 = document.createElement('div');
ljs_4.setAttribute('class', ljs_useCtx ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
ljs_1.appendChild(ljs_4);
var ljs_useCtx = WAVE.isObject(ctx);
var ljs_5 = document.createElement('div');
ljs_5.setAttribute('class', ljs_useCtx ? WAVE.strHTMLTemplate("stub @color@", ctx) : "stub @color@");
ljs_1.appendChild(ljs_5);
var ljs_useCtx = WAVE.isObject(ctx);
var ljs_6 = document.createElement('div');
ljs_6.setAttribute('data-height', ljs_useCtx ? WAVE.strHTMLTemplate("@height@", ctx) : "@height@");
ljs_6.setAttribute('class', ljs_useCtx ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
ljs_1.appendChild(ljs_6);
if (typeof(root) !== 'undefined' && root !== null) {
if (WAVE.isString(root))
root = WAVE.id(root);
if (WAVE.isObject(root))
root.appendChild(ljs_1);
}

}