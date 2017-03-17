function hello(text) {
  alert(WAVE.strDefault(text, "Hello"));
}

function render(root, ctx){
/***
div{
  on-click = "function() { console.log('kaka') }"
  div{ class=title data-alert="alert('just data')" data-alert-script='<script>alert("script")</script>' }
  div{ id=rate }
  div{ class=@color@ }
  div{ class="stub @color@" }

  div="<script>alert(\"'<script>alert();</script>'text\")</script>"{}
  div{
    div{
      class=@color@
      div{
        div{
          class=@color@
          div{
            div{
              class=@color@
              div{
                div{
                  class=@color@
                  div{
                  }
                }
              }
            }
          }
        }
      }
    }
  }

  div{ data-height=@height@ class=@color@ }

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
    button=Fade { class=fade on-click=hello }
    button=Rizzle { class=rizzle on-click=hello }
    button=Knit { class=knit on-click=hello }
    button=Shrink { class=shrink on-click=hello }
    button=Rotate { class=rotate on-click=hello }
    button=Boom { class=boom on-click=hello }
    button=Squeeze { class=squeeze on-click=hello }
    button=Deform { class=deform on-click=hello }
  }

  h1="Compiler output example"{}

  code =$"
      function noRoot() {
        var ljs_useCtx = WAVE.isObject(ctx);
        var ljs_1 = document.createElement('section');
        var ljs_2 = 'sect';
        ljs_1.setAttribute('id', ljs_useCtx ? WAVE.strHTMLTemplate(ljs_2, ctx) : WAVE.strEscapeHTML(ljs_2));
        var ljs_3 = 'sect';
        ljs_1.setAttribute('class', ljs_useCtx ? WAVE.strHTMLTemplate(ljs_3, ctx) : WAVE.strEscapeHTML(ljs_3));
        var ljs_4 = ' Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cum sollicitudin interdum,      sollicitudin condimentum montes nulla bibendum aliquam velit? Fermentum mattis aenean nec...      Orci proin litora nec ullamcorper?    ';
        ljs_1.innerText = ljs_useCtx ? WAVE.strHTMLTemplate(ljs_4, ctx) : WAVE.strEscapeHTML(ljs_4);
        if (typeof(root) !== 'undefined' && root !== null) {
        if (WAVE.isString(root))
        root = WAVE.id(root);
        if (WAVE.isObject(root))
        root.appendChild(ljs_1);
      }
    " {}
}
***/
}


function noRoot() {
/***
  section= $"
      Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cum sollicitudin interdum,
      sollicitudin condimentum montes nulla bibendum aliquam velit? Fermentum mattis aenean nec...
      Orci proin litora nec ullamcorper?
    "
  {
    id=sect
    class=sect
  }
***/
}
