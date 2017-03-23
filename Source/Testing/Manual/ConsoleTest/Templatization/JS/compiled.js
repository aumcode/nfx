function hello(text) {
  alert(WAVE.strDefault(text, "Hello"));
}

function render(){
  var ljs_useCtx_1 = WAVE.isObject(arguments[1]);
  var ljs_1_1 = document.createElement('div');
  ljs_1_1.addEventListener('click', function() { console.log('kaka') }, false);
  var ljs_1_2 = document.createElement('div');
  ljs_1_2.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("title", ctx) : "title");
  var ljsv_1_4 = "alert(\x27just data\x27)";
  ljs_1_2.setAttribute('data-alert', ljs_useCtx_1 ? WAVE.strHTMLTemplate(ljsv_1_4, ctx) : ljsv_1_4);
  var ljsv_1_5 = "\x3Cscript\x3Ealert(\x22script\x22)\x3C\x2Fscript\x3E";
  ljs_1_2.setAttribute('data-alert-script', ljs_useCtx_1 ? WAVE.strHTMLTemplate(ljsv_1_5, ctx) : ljsv_1_5);
  ljs_1_1.appendChild(ljs_1_2);

  var ljs_1_6 = document.createElement('div');
  ljs_1_6.setAttribute('id', ljs_useCtx_1 ? WAVE.strHTMLTemplate("rate", ctx) : "rate");
  ljs_1_1.appendChild(ljs_1_6);

  var ljs_1_8 = document.createElement('div');
  ljs_1_8.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
  ljs_1_1.appendChild(ljs_1_8);

  var ljs_1_10 = document.createElement('div');
  ljs_1_10.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("stub @color@", ctx) : "stub @color@");
  ljs_1_1.appendChild(ljs_1_10);

  var ljs_1_12 = document.createElement('div');
  var ljsv_1_13 = "\x3Cscript\x3Ealert(\x22\x27\x3Cscript\x3Ealert(\x27@color@\x27);\x3C\x2Fscript\x3E\x27text\x22)\x3C\x2Fscript\x3E";
  ljs_1_12.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate(ljsv_1_13, ctx) : ljsv_1_13;
  ljs_1_1.appendChild(ljs_1_12);

  var ljs_1_14 = document.createElement('div');
  var ljs_1_15 = document.createElement('div');
  ljs_1_15.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
  var ljs_1_17 = document.createElement('div');
  var ljs_1_18 = document.createElement('div');
  ljs_1_18.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
  var ljs_1_20 = document.createElement('div');
  var ljs_1_21 = document.createElement('div');
  ljs_1_21.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
  var ljs_1_23 = document.createElement('div');
  var ljs_1_24 = document.createElement('div');
  ljs_1_24.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
  var ljs_1_26 = document.createElement('div');
  var ljsv_1_27 = ",.\x2F[\x5C]{}|!@#$%^\x26*()_+=-~`\x27";
  ljs_1_26.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate(ljsv_1_27, ctx) : ljsv_1_27;
  ljs_1_24.appendChild(ljs_1_26);

  ljs_1_23.appendChild(ljs_1_24);

  ljs_1_21.appendChild(ljs_1_23);

  ljs_1_20.appendChild(ljs_1_21);

  ljs_1_18.appendChild(ljs_1_20);

  ljs_1_17.appendChild(ljs_1_18);

  ljs_1_15.appendChild(ljs_1_17);

  ljs_1_14.appendChild(ljs_1_15);

  ljs_1_1.appendChild(ljs_1_14);

  var ljs_1_28 = document.createElement('div');
  ljs_1_28.setAttribute('data-height', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@height@", ctx) : "@height@");
  ljs_1_28.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("@color@", ctx) : "@color@");
  ljs_1_1.appendChild(ljs_1_28);

  var ljs_1_31 = document.createElement('div');
  ljs_1_31.setAttribute('id', ljs_useCtx_1 ? WAVE.strHTMLTemplate("controls", ctx) : "controls");
  var ljs_1_33 = document.createElement('input');
  ljs_1_33.setAttribute('value', ljs_useCtx_1 ? WAVE.strHTMLTemplate("2013-06-06", ctx) : "2013-06-06");
  ljs_1_33.setAttribute('type', ljs_useCtx_1 ? WAVE.strHTMLTemplate("date", ctx) : "date");
  ljs_1_31.appendChild(ljs_1_33);

  var ljs_1_36 = document.createElement('input');
  ljs_1_36.setAttribute('value', ljs_useCtx_1 ? WAVE.strHTMLTemplate("234.11", ctx) : "234.11");
  ljs_1_36.setAttribute('type', ljs_useCtx_1 ? WAVE.strHTMLTemplate("text", ctx) : "text");
  ljs_1_31.appendChild(ljs_1_36);

  var ljs_1_39 = document.createElement('input');
  ljs_1_39.setAttribute('value', ljs_useCtx_1 ? WAVE.strHTMLTemplate("234.11", ctx) : "234.11");
  ljs_1_39.setAttribute('type', ljs_useCtx_1 ? WAVE.strHTMLTemplate("number", ctx) : "number");
  ljs_1_31.appendChild(ljs_1_39);

  ljs_1_1.appendChild(ljs_1_31);

  var ljs_1_42 = document.createElement('div');
  ljs_1_42.setAttribute('id', ljs_useCtx_1 ? WAVE.strHTMLTemplate("container", ctx) : "container");
  var ljs_1_44 = document.createElement('h1');
  ljs_1_44.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Animation Test", ctx) : "Animation Test";
  ljs_1_42.appendChild(ljs_1_44);

  var ljs_1_46 = document.createElement('button');
  ljs_1_46.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Highlight", ctx) : "Highlight";
  ljs_1_46.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("highlight", ctx) : "highlight");
  ljs_1_46.addEventListener('click', function() { hello('highlight'); }, false);
  ljs_1_42.appendChild(ljs_1_46);

  var ljs_1_49 = document.createElement('button');
  ljs_1_49.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Fade", ctx) : "Fade";
  ljs_1_49.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("fade", ctx) : "fade");
  ljs_1_49.addEventListener('click', function() { hello('fade'); }, false);
  ljs_1_42.appendChild(ljs_1_49);

  var ljs_1_52 = document.createElement('button');
  ljs_1_52.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Rizzle", ctx) : "Rizzle";
  ljs_1_52.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("rizzle", ctx) : "rizzle");
  ljs_1_52.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_52);

  var ljs_1_55 = document.createElement('button');
  ljs_1_55.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Knit", ctx) : "Knit";
  ljs_1_55.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("knit", ctx) : "knit");
  ljs_1_55.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_55);

  var ljs_1_58 = document.createElement('button');
  ljs_1_58.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Shrink", ctx) : "Shrink";
  ljs_1_58.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("shrink", ctx) : "shrink");
  ljs_1_58.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_58);

  var ljs_1_61 = document.createElement('button');
  ljs_1_61.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Rotate", ctx) : "Rotate";
  ljs_1_61.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("rotate", ctx) : "rotate");
  ljs_1_61.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_61);

  var ljs_1_64 = document.createElement('button');
  ljs_1_64.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Boom", ctx) : "Boom";
  ljs_1_64.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("boom", ctx) : "boom");
  ljs_1_64.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_64);

  var ljs_1_67 = document.createElement('button');
  ljs_1_67.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Squeeze", ctx) : "Squeeze";
  ljs_1_67.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("squeeze", ctx) : "squeeze");
  ljs_1_67.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_67);

  var ljs_1_70 = document.createElement('button');
  ljs_1_70.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate("Deform", ctx) : "Deform";
  ljs_1_70.setAttribute('class', ljs_useCtx_1 ? WAVE.strHTMLTemplate("deform", ctx) : "deform");
  ljs_1_70.addEventListener('click', hello, false);
  ljs_1_42.appendChild(ljs_1_70);

  ljs_1_1.appendChild(ljs_1_42);

  var ljs_1_73 = document.createElement('h1');
  var ljsv_1_74 = "Compiler output example";
  ljs_1_73.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate(ljsv_1_74, ctx) : ljsv_1_74;
  ljs_1_1.appendChild(ljs_1_73);

  var ljs_1_75 = document.createElement('code');
  var ljsv_1_76 = "\x0D\x0A          function noRoot() {\x0D\x0A            var ljs_useCtx_2 = WAVE.isObject(arguments[1]);\x0D\x0A            var ljs_2_1 = document.createElement(\x27section\x27);\x0D\x0A            var ljsv_2_2 = \x27Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nibh mauris maecenas ullamcorper faucibus facilisi torquent mauris, facilisis interdum fermentum porta mus non pretium; Erat pretium placerat ut congue per suscipit...\x27;\x0D\x0A            ljsv_2_2 = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_2, ctx) : ljsv_2_2;\x0D\x0A            ljs_2_1.innerText = ljsv_2_2;\x0D\x0A            var ljsv_2_3 = \x27sect\x27;\x0D\x0A            ljsv_2_3 = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_3, ctx) : ljsv_2_3;\x0D\x0A            ljs_2_1.setAttribute(\x27id\x27, ljsv_2_3);\x0D\x0A            var ljsv_2_4 = \x27sect\x27;\x0D\x0A            ljsv_2_4 = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_4, ctx) : ljsv_2_4;\x0D\x0A            ljs_2_1.setAttribute(\x27class\x27, ljsv_2_4);\x0D\x0A\x0D\x0A            var ljs_r_2 = arguments[0];\x0D\x0A            if (typeof(ljs_r_2) !== \x27undefined\x27 \x26\x26 ljs_r_2 !== null) {\x0D\x0A              if (WAVE.isString(ljs_r_2))\x0D\x0A                ljs_r_2 = WAVE.id(ljs_r_2);\x0D\x0A              if (WAVE.isObject(ljs_r_2))\x0D\x0A                ljs_r_2.appendChild(ljs_2_1);\x0D\x0A            }\x0D\x0A          }\x0D\x0A      ";
  ljs_1_75.innerText = ljs_useCtx_1 ? WAVE.strHTMLTemplate(ljsv_1_76, ctx) : ljsv_1_76;
  ljs_1_1.appendChild(ljs_1_75);

  var ljs_r_1 = arguments[0];
  if (typeof(ljs_r_1) !== 'undefined' && ljs_r_1 !== null) {
    if (WAVE.isString(ljs_r_1))
      ljs_r_1 = WAVE.id(ljs_r_1);
    if (WAVE.isObject(ljs_r_1))
      ljs_r_1.appendChild(ljs_1_1);
  }
}


function noRoot() {
  var ljs_useCtx_2 = WAVE.isObject(arguments[1]);
  var ljs_2_1 = document.createElement('section');
  var ljsv_2_2 = "\x0D\x0A        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nibh mauris maecenas ullamcorper faucibus facilisi torquent mauris,\x0D\x0A        facilisis interdum fermentum porta mus non pretium; Erat pretium placerat ut congue per suscipit...\x0D\x0A      ";
  ljs_2_1.innerText = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_2, ctx) : ljsv_2_2;
  ljs_2_1.setAttribute('id', ljs_useCtx_2 ? WAVE.strHTMLTemplate("sect", ctx) : "sect");
  ljs_2_1.setAttribute('class', ljs_useCtx_2 ? WAVE.strHTMLTemplate("sect", ctx) : "sect");
  var ljs_r_2 = arguments[0];
  if (typeof(ljs_r_2) !== 'undefined' && ljs_r_2 !== null) {
    if (WAVE.isString(ljs_r_2))
      ljs_r_2 = WAVE.id(ljs_r_2);
    if (WAVE.isObject(ljs_r_2))
      ljs_r_2.appendChild(ljs_2_1);
  }
}