function hello() {
  alert("Hello");
}

function render(root, ctx){
/***
div{
  on-click = "function() { console.log('kaka') }"
  div{ class=title data-alert="alert('just data')" data-alert-script='<script>alert("script")</script>' }
  div{ id=rate }
  div{ class=@color@ }
  div{ class="stub @color@" }

  div{ -content="<script>alert(\"'<script>alert();</script>'text\")</script>" }
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
    h1{ -content="Animation Test" }
    button { class=highlight -content=Highlight on-click=hello }
    button { class=fade -content=Fade on-click=hello }
    button { class=rizzle -content=Rizzle on-click=hello }
    button { class=knit -content=Knit on-click=hello }
    button { class=shrink -content=Shrink on-click=hello }
    button { class=rotate -content=Rotate on-click=hello }
    button { class=boom -content=Boom on-click=hello }
    button { class=squeeze -content=Squeeze on-click=hello }
    button { class=deform -content=Deform on-click=hello }
  }

  h1 { -content="Compiler output example" }

  code {
    -content=$"
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
    "
  }


}
***/
}


function noRoot() {
/***
  section{
    id=sect
    class=sect
    -content = $"
      Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cum sollicitudin interdum,
      sollicitudin condimentum montes nulla bibendum aliquam velit? Fermentum mattis aenean nec...
      Orci proin litora nec ullamcorper?
    "
  }
***/
}
