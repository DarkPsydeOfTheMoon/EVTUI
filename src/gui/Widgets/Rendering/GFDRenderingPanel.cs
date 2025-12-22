using System;
using System.Diagnostics;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using static Avalonia.OpenGL.GlConsts;

using OpenTK.Graphics.OpenGL;

using EVTUI.ViewModels;

namespace EVTUI.Views
{

    public partial class GFDRenderingPanel : OpenGlControlBase
    {
        /////////////////////////////
        // *** PRIVATE MEMBERS *** //
        /////////////////////////////
        private Window topLevel;
        private AvaloniaOpenTKWrapper? avaloniaTkContext;
        private readonly DispatcherTimer redrawTimer = new DispatcherTimer();

        private GFDRenderingPanelViewModel getVM()
        {
            return (GFDRenderingPanelViewModel)this.DataContext;
        }

        ////////////////////////////
        // *** PUBLIC METHODS *** //
        ////////////////////////////
        public GFDRenderingPanel()
        {
            // Want to render at 30fps, but the TimeSpan needs to be initialised
            // with ticks... so calculate how many ticks correspond to each frame.
            // Ticks per Second / Frames per Second = Ticks per Frame
            redrawTimer.Interval = new TimeSpan(TimeSpan.TicksPerSecond/30);      
            redrawTimer.Tick += this.RequestRedrawEventHandler;

            // Also redraw if the window resizes.
            this.Loaded += GetTopLevel;
            MainWindow.ClientSizeProperty.Changed.Subscribe(size => RequestNextFrameRendering());
        }

        ////////////////////////////
        // *** PRIVATE METHODS *** //
        ////////////////////////////
        public void GetTopLevel(object? sender, RoutedEventArgs e)
        {
            try
            {
                var tl = TopLevel.GetTopLevel(this);
                if (tl is null) throw new NullReferenceException();
                this.topLevel = (Window)tl;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        private async void RequestRedrawEventHandler(object? sender, EventArgs e)
        {
            try
            {
                RequestNextFrameRendering();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                if (!(this.topLevel is null))
                    await Utils.RaiseModal(this.topLevel, $"Failed to redraw render due to unhandled exception:\n{ex.ToString()}");
                getVM().ReadyToRender = false;
            }
        }

        private static void CheckError(GlInterface gl)
        {
            int err;
            while ((err = gl.GetError()) != GL_NO_ERROR)
            {
                SceneManager.ReportError(err);
            }
        }

        //protected override unsafe async void OnOpenGlInit(GlInterface gl)
        protected override async void OnOpenGlInit(GlInterface gl)
        {
            try
            {
                // Bind OpenTK to the panel
                CheckError(gl);
                this.avaloniaTkContext = new(gl);
                GL.LoadBindings(avaloniaTkContext);

                // Start the signal to redraw the scene @ 30fps
                redrawTimer.Start();
            
                // Init test data if it exists and has not already been
                // initialised (should be removed when real hooks exist).
                var ctx = getVM();
                ctx.ReadyToRender = true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                if (!(this.topLevel is null))
                    await Utils.RaiseModal(this.topLevel, $"Failed to initialize render due to unhandled exception:\n{ex.ToString()}");
                getVM().ReadyToRender = false;
            }
        }

        protected override async void OnOpenGlDeinit(GlInterface GL)
        {
            try
            {
                redrawTimer.Stop();
                this.avaloniaTkContext = null;
                var ctx = getVM();
                ctx.ReadyToRender = false;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                if (!(this.topLevel is null))
                    await Utils.RaiseModal(this.topLevel, $"Failed to de-initialize render due to unhandled exception:\n{ex.ToString()}");
                getVM().ReadyToRender = false;
            }
        }

        //protected override unsafe async void OnOpenGlRender(GlInterface gl, int framebuffer)
        protected override async void OnOpenGlRender(GlInterface gl, int framebuffer)
        {
            try
            {
                var vm = getVM();
                vm.width  = Bounds.Width;
                vm.height = Bounds.Height;

                vm.RefreshSceneState();
                vm.DrawScene();

                CheckError(gl);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                if (!(this.topLevel is null))
                    await Utils.RaiseModal(this.topLevel, $"Failed to draw render due to unhandled exception:\n{ex.ToString()}");
                getVM().ReadyToRender = false;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
        }
    }
}
