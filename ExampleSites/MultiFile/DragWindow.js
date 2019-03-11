var TopWindow = null;

function MoveableWindow(elem)
{
    const Elem = elem;
    const Title = elem.getElementsByClassName("titlebar")[0];
    const Window = this;
    this.Width = elem.offsetWidth;
    this.Height = elem.offsetHeight;
    this.UnFocus = function(){
        Title.classList.remove("titlebar-focused");
    }
    this.LowerZ = function(){
        Elem.style.zIndex--;
    }
    function onMouseMove(e){
        e = e || window.event;
        e.preventDefault();
        Elem.style.top=(e.pageY-(Title.offsetHeight/2))+"px";
        Elem.style.left=(e.pageX-(Title.offsetWidth/2))+"px";
    }
    function onTouchMove(e){
        e = e || window.event;
        var t = e.touches[0]
        Elem.style.top=(t.pageY-(Title.offsetHeight/2))+"px";
        Elem.style.left=(t.pageX-(Title.offsetWidth/2))+"px";
    }
    function onMouseUp(e) {
        e = e || window.event;
        e.preventDefault();
        document.onmouseup = null;
        document.onmousemove = null;
    }
    function onTouchEnd(e) {
        e = e || window.event;
        document.removeEventListener("touchend",onTouchEnd,false);
        document.removeEventListener("touchmove",onTouchMove,false);
    }
    function changeFocus() {
       Elem.style.zIndex = 100;
      if(TopWindow!=null && TopWindow != Window)
      {
          TopWindow.LowerZ();
          TopWindow.UnFocus();
      }
      TopWindow = Window;
      Title.classList.add("titlebar-focused"); 
    }
    
    Title.onmousedown = function(e) {
      e = e || window.event;
      e.preventDefault();
      changeFocus();
      document.onmouseup = onMouseUp;
      document.onmousemove = onMouseMove;
    }
    
    Title.addEventListener("touchstart",function(e) {
      e = e || window.event;
      
      changeFocus();
      document.addEventListener("touchend",onTouchEnd,false);
      document.addEventListener("touchmove",onTouchMove,false);
    },false);
    
}

WindowList = []
var movelist = document.getElementsByClassName("moveable");
for(var w=0;w<movelist.length;++w)
{
    WindowList.push(new MoveableWindow(movelist[w]));
}

