using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GlitchCompiler.Rendering
{
    public sealed class CanvasRenderer : MonoBehaviour
    {
        [SerializeField] private RawImage preview; [SerializeField] private int resolution=512; public Texture2D CanvasTexture { get; private set; }
        public void Render(IReadOnlyList<DrawCommand> commands) { CanvasTexture=new Texture2D(resolution,resolution,TextureFormat.RGBA32,false); Clear(); var state=TurtleState.Default; foreach(var command in commands) Apply(command,ref state); CanvasTexture.Apply(); if(preview!=null) preview.texture=CanvasTexture; }
        private void Clear() { var pixels=new Color[resolution*resolution]; CanvasTexture.SetPixels(pixels); }
        private void Apply(DrawCommand command,ref TurtleState state) { switch(command.Type) { case DrawCommandType.Color:state.Color=command.Color;break; case DrawCommandType.Width:state.Width=Mathf.Max(1,Mathf.RoundToInt(command.A));break; case DrawCommandType.Turn:state.Angle+=command.A;break; case DrawCommandType.Move:var next=state.Position+new Vector2(Mathf.Cos(state.Angle*Mathf.Deg2Rad),Mathf.Sin(state.Angle*Mathf.Deg2Rad))*command.A; Line(state.Position,next,state);state.Position=next;break; case DrawCommandType.Circle:Circle(state.Position,command.A,state);break; case DrawCommandType.Rect:Rect(state.Position,command.A,command.B,state);break; } }
        private void Line(Vector2 a,Vector2 b,TurtleState state) { int steps=Mathf.CeilToInt(Vector2.Distance(a,b)); for(int i=0;i<=steps;i++) Dot(Vector2.Lerp(a,b,steps==0?0:(float)i/steps),state); }
        private void Circle(Vector2 center,float radius,TurtleState state) { int steps=Mathf.Max(12,Mathf.CeilToInt(radius*6)); for(int i=0;i<steps;i++) { float a=i*Mathf.PI*2/steps; Dot(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,state); } }
        private void Rect(Vector2 origin,float width,float height,TurtleState state) { var a=origin;var b=a+new Vector2(width,0);var c=b+new Vector2(0,height);var d=a+new Vector2(0,height);Line(a,b,state);Line(b,c,state);Line(c,d,state);Line(d,a,state); }
        private void Dot(Vector2 point,TurtleState state) { int r=Mathf.Max(0,state.Width/2); for(int x=-r;x<=r;x++) for(int y=-r;y<=r;y++) { int px=Mathf.RoundToInt(point.x)+x,py=Mathf.RoundToInt(point.y)+y; if(px>=0&&py>=0&&px<resolution&&py<resolution) CanvasTexture.SetPixel(px,py,state.Color); } }
    }
}
