using System;
using System.Collections.Generic;
using System.Text;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Platform;
using static SDL2.SDL;

namespace ShaderTranslator.Demo
{
    class SimpleRenderForm
    {
        public Color4 ClearColor { get; } = new Color4(0.1f, 0.8f, 0.2f, 1);

        IWindowInfo window;

        public SimpleRenderForm()
        {
            SDL_Init(SDL_INIT_VIDEO);
            var sdlWindow = SDL_CreateWindow("ShaderTranslator Demo", 100, 100, 800, 480, SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_SHOWN);

            SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
            SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG);

            window = Utilities.CreateSdl2WindowInfo(sdlWindow);
            var sdlContext = SDL_GL_CreateContext(sdlWindow);
            int result = SDL_GL_MakeCurrent(sdlWindow, sdlContext);

            //context = new GraphicsContext(GraphicsMode.Default, window);
            var context = new GraphicsContext(new OpenTK.ContextHandle(sdlContext),
                SDL_GL_GetProcAddress,
                () => new OpenTK.ContextHandle(SDL_GL_GetCurrentContext()));
            context.LoadAll();
            context.MakeCurrent(window);

            SDL_GL_SetSwapInterval(1);

        }

        public void DoRenderLoop(Action loopBody)
        {
            bool exit = false;
            while (!exit)
            {
                GL.ClearColor(ClearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                loopBody();

                SDL_GL_SwapWindow(window.Handle);

                while (SDL_PollEvent(out var e) != 0)
                {
                    if (e.type == SDL_EventType.SDL_QUIT)
                        exit = true;
                }
            }
        }
    }
}
