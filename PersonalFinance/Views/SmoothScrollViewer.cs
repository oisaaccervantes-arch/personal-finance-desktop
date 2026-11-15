using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PersonalFinance.Views;

/// <summary>Bounded pixel scrolling. Nested tables keep their own wheel handling.</summary>
public sealed class SmoothScrollViewer : ScrollViewer
{
    private double target;
    private bool running, applying;
    private long previousFrame;

    public SmoothScrollViewer()
    {
        Unloaded+=(_,_)=>Stop();
        ScrollChanged+=(_,e)=>
        {
            if(e.OriginalSource!=this) return;
            if(!applying && e.VerticalChange!=0) Stop(); // thumb, keyboard or touch
            target=Math.Clamp(target,0,ScrollableHeight);
        };
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if(e.Handled || ScrollableHeight<=0) return;
        var lines=SystemParameters.WheelScrollLines;
        if(lines==0) { e.Handled=true; return; }
        double step=lines<0?ViewportHeight*.85:Math.Clamp(lines*12,12,60);
        double origin=running?target:VerticalOffset;
        // Do not queue a long fling when a wheel sends a burst of events.
        target=Math.Clamp(origin-e.Delta/120d*step,Math.Max(0,VerticalOffset-180),Math.Min(ScrollableHeight,VerticalOffset+180));
        e.Handled=true;
        if(!SystemParameters.ClientAreaAnimation)
        {
            ScrollToVerticalOffset(target);return;
        }
        if(running)return;
        running=true;previousFrame=Stopwatch.GetTimestamp();
        CompositionTarget.Rendering+=OnFrame;
    }

    private void OnFrame(object? sender,EventArgs e)
    {
        var now=Stopwatch.GetTimestamp();
        double dt=Math.Min(.05,(now-previousFrame)/(double)Stopwatch.Frequency);
        previousFrame=now;
        target=Math.Clamp(target,0,ScrollableHeight);
        double distance=target-VerticalOffset;
        applying=true;
        try
        {
            ScrollToVerticalOffset(Math.Abs(distance)<.5?target:VerticalOffset+distance*(1-Math.Exp(-26*dt)));
            // Apply the pending offset while applying=true so ScrollChanged can
            // distinguish this update from a scrollbar/keyboard interaction.
            UpdateLayout();
        }
        finally { applying=false; }
        if(Math.Abs(target-VerticalOffset)<.5)Stop();
    }

    private void Stop()
    {
        if(running)CompositionTarget.Rendering-=OnFrame;
        running=false;target=VerticalOffset;
    }
}
