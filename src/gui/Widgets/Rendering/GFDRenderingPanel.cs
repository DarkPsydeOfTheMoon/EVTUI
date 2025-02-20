using System;
using Avalonia;
using Avalonia.Controls;
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
            var topLevel = (MainWindow)TopLevel.GetTopLevel(this);
            MainWindow.ClientSizeProperty.Changed.Subscribe(size => RequestNextFrameRendering());
        }

        ////////////////////////////
        // *** PRIVATE METHODS *** //
        ////////////////////////////
        private void RequestRedrawEventHandler(object? sender, EventArgs e)
        {
            RequestNextFrameRendering();
        }

        private static void CheckError(GlInterface gl)
        {
            int err;
            while ((err = gl.GetError()) != GL_NO_ERROR)
            {
                SceneManager.ReportError(err);
            }
        }

        protected override unsafe void OnOpenGlInit(GlInterface gl)
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

        protected override void OnOpenGlDeinit(GlInterface GL)
        {
            redrawTimer.Stop();
            this.avaloniaTkContext = null;
            var ctx = getVM();
            ctx.ReadyToRender = false;
        }

        protected override unsafe void OnOpenGlRender(GlInterface gl, int framebuffer)
        {
            var vm = getVM();
            vm.width  = Bounds.Width;
            vm.height = Bounds.Height;

            vm.RefreshSceneState();
            vm.DrawScene();

            CheckError(gl);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
        }
    }
}
