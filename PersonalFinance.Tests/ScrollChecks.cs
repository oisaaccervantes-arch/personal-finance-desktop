using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PersonalFinance.Views;

internal static class ScrollChecks
{
    public static void Run()
    {
        Exception? failure=null;
        var thread=new Thread(()=>
        {
            Window? window=null;
            try
            {
                var scroller=new SmoothScrollViewer { Height=240, Content=new Border {Height=1600}, VerticalScrollBarVisibility=ScrollBarVisibility.Auto };
                window=new Window {Width=400,Height=300,Content=scroller,ShowInTaskbar=false};
                window.Show();window.UpdateLayout();
                void Pump(int milliseconds)
                {
                    var frame=new DispatcherFrame();
                    var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(milliseconds)};
                    timer.Tick+=(_,_)=>{timer.Stop();frame.Continue=false;};timer.Start();Dispatcher.PushFrame(frame);
                }
                void Wheel(int delta)=>scroller.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice,Environment.TickCount,delta){RoutedEvent=Mouse.MouseWheelEvent});
                void Check(bool success,string label){if(!success)throw new Exception(label);Console.WriteLine("PASS "+label);}
                Wheel(-120);Pump(450);
                double offset=scroller.VerticalOffset;
                Check(offset>0&&offset<=60,"Un paso de rueda avanza como máximo 60 píxeles");
                Wheel(120);Pump(450);
                Check(scroller.VerticalOffset<1,"Invertir la rueda vuelve al inicio sin sobrepasarlo");
                for(int i=0;i<30;i++)Wheel(-120);
                Pump(450);Check(scroller.VerticalOffset<=181,"Una ráfaga no acumula más de 180 píxeles");
                Wheel(-120);scroller.ScrollToVerticalOffset(700);scroller.UpdateLayout();Pump(450);
                Check(Math.Abs(scroller.VerticalOffset-700)<1,"El desplazamiento manual cancela la animación pendiente");
                scroller.ScrollToEnd();scroller.UpdateLayout();Wheel(-120);Pump(250);
                Check(scroller.VerticalOffset<=scroller.ScrollableHeight,"El desplazamiento respeta el límite inferior");
            }
            catch(Exception ex){failure=ex;}
            finally {window?.Close();}
        });
        thread.SetApartmentState(ApartmentState.STA);thread.Start();thread.Join();
        if(failure!=null)throw failure;
    }
}
