function hello(text) {
  alert(WAVE.strDefault(text, "Hello"));
}

function inline() {
  return "*** div{ id=someText } ***";
}

function skip() {
  /*** #skIp#**********

  div {
  }

  ***/
}

function skip2() {
  /*** #skIp#    **********

  div {
  }

  ***/
}

function skip2() {
  /*** #skIp# #skIp#    **********

  div {
  }

  ***/
}

/***@ _include.js ***/

function ex1(root, ctx) {
  /***
    div= $"
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nibh mauris maecenas ullamcorper faucibus facilisi torquent mauris,
        facilisis interdum fermentum porta mus non pretium; Erat pretium placerat ut congue per suscipit...
      "
    {
      id=sect
      ljsid=rootNode
      class=sect
      $"?var a = ""pupkin"";"{}

      "?if(ctx.pzd || ?this || rootNode)" {
        div="?ctx.error" {
          class="?ctx.ec"
        }
      }
    }
  ***/
}

function ex2(root) {
  /***
    "?if (1 === 1)" {
      "?for(var i = 0, l = 10; i < l; i++)" {
        div="?i" {
          class=counter
          id="?i+1"
        }
      }
    }
  ***/
}

function ex3(root, ctx){
  /***
  div{
    on-click="function() { console.log('kaka') }"
    div{
      class=title
      data-alert="alert('just data')"
      data-alert-script='<script>alert("script")</script>'
    }
    div{ id=rate }
    div{ class=?ctx.color }
    div{ class="?'stub ' + ctx.color" }

    div="<script>alert(\"'<script>alert('?ctx.color');</script>'text\")</script>"{}
    div{
      div{
        class=?ctx.color
        div{
          div{
            class=?ctx.color
            div{
              div{
                class=?ctx.color
                div{
                  div{
                    class=?ctx.color
                    div=",./[\\]{}|!@#$%^&*()_+=-~`'"{}
                  }
                }
              }
            }
          }
        }
      }
    }

    div{
      data-height=?ctx.height
      class=?ctx.color
    }

    div{
      id=controls
      input{
        value="2013-06-06"
        type="date"
      }
      input{
        value=234.11
        type="text"
      }
      input{
        value=234.11
        type=number
      }
    }

    div{
      id=container
      h1="Animation Test"{}
      button=Highlight { class=highlight on-click="function() { hello('highlight'); }" }
      button=Fade { class=fade on-click="function() { hello('fade'); }" }
      button=Rizzle { class=rizzle on-click=hello }
      button=Knit { class=knit on-click=hello }
      button=Shrink { class=shrink on-click=hello }
      button=Rotate { class=rotate on-click=hello }
      button=Boom { class=boom on-click=hello }
      button=Squeeze { class=squeeze on-click=hello }
      button=Deform { class=deform on-click=hello }
    }

    h1="Compiler output example"{}

    code=$"
          function noRoot() {
            var ljs_useCtx_2 = WAVE.isObject(arguments[1]);
            var ljs_2_1 = document.createElement('section');
            var ljsv_2_2 = 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nibh mauris maecenas ullamcorper faucibus facilisi torquent mauris, facilisis interdum fermentum porta mus non pretium; Erat pretium placerat ut congue per suscipit...';
            ljsv_2_2 = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_2, ctx) : ljsv_2_2;
            ljs_2_1.innerText = ljsv_2_2;
            var ljsv_2_3 = 'sect';
            ljsv_2_3 = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_3, ctx) : ljsv_2_3;
            ljs_2_1.setAttribute('id', ljsv_2_3);
            var ljsv_2_4 = 'sect';
            ljsv_2_4 = ljs_useCtx_2 ? WAVE.strHTMLTemplate(ljsv_2_4, ctx) : ljsv_2_4;
            ljs_2_1.setAttribute('class', ljsv_2_4);

            var ljs_r_2 = arguments[0];
            if (typeof(ljs_r_2) !== 'undefined' && ljs_r_2 !== null) {
              if (WAVE.isString(ljs_r_2))
                ljs_r_2 = WAVE.id(ljs_r_2);
              if (WAVE.isObject(ljs_r_2))
                ljs_r_2.appendChild(ljs_2_1);
            }
          }
      " {}
  }
  ***/
}