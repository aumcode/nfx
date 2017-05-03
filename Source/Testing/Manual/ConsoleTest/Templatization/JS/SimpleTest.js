function simpleTest(parent, ctx){
/***
div{
  on-click = "alert('kaka')"
  div{ class=title }
  div{ id=rate }
}
***/
}

function simpleTest(parent, ctx){
/***
div{
  on-click = "function() { console.log('do it'); }"
  div{ class='alert "alert" <script>alert()</script>>' }
  div{ id=@sid@rate }
}
***/
}
