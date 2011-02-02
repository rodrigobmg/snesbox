using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace SnesBox
{
    public class SnesBoxGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Snes _snes = new Snes();
        FrameRateComponent _frameRate;

        Texture2D _videoFrame;
        Color[] _videoBuffer;
        Rectangle _videoRect;

        DynamicSoundEffectInstance _audioFrame;

        public SnesBoxGame()
        {
            IsFixedTimeStep = false;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;

            _frameRate = new FrameRateComponent(this);
            Components.Add(_frameRate);
        }

        protected override void Initialize()
        {
            _audioFrame = new DynamicSoundEffectInstance(32040, AudioChannels.Stereo);
            _audioFrame.Play();

            _snes.VideoUpdated += new VideoUpdatedEventHandler(Snes_VideoUpdated);
            _snes.AudioUpdated += new AudioUpdatedEventHandler(Snes_AudioUpdated);

            _videoFrame = new Texture2D(GraphicsDevice, 512, 512, false, SurfaceFormat.Color);
            _videoBuffer = new Color[512 * 512];

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            using (FileStream fs = new FileStream("SMW.smc", FileMode.Open))
            {
                var rom = new byte[fs.Length];
                fs.Read(rom, 0, (int)fs.Length);
                _snes.LoadCartridge(new NormalCartridge() { RomData = rom });
            }
        }

        protected override void Update(GameTime gameTime)
        {
            _snes.RunToFrame();
            Window.Title = string.Format("{0:##} FPS", _frameRate.FrameRate);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var vp = GraphicsDevice.Viewport;
            spriteBatch.Begin();
            spriteBatch.Draw(_videoFrame, new Rectangle(0, 0, vp.Width, vp.Height), _videoRect, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        void Snes_AudioUpdated(object sender, AudioUpdatedEventArgs e)
        {
            var audioBuffer = new byte[e.SampleCount * 4];
            int bufferIndex = 0;

            for (int i = 0; i < e.AudioBuffer.Length; i++)
            {
                var samples = BitConverter.GetBytes(e.AudioBuffer[i]);
                audioBuffer[bufferIndex++] = samples[0];
                audioBuffer[bufferIndex++] = samples[1];
                audioBuffer[bufferIndex++] = samples[2];
                audioBuffer[bufferIndex++] = samples[3];
            }

            if (audioBuffer.Length > 0)
            {
                _audioFrame.SubmitBuffer(audioBuffer, 0, audioBuffer.Length);
            }
        }

        void Snes_VideoUpdated(object sender, VideoUpdatedEventArgs e)
        {
            bool interlace = (e.Height >= 240);
            uint pitch = interlace ? 1024U : 2048U;
            pitch >>= 1;

            for (int y = 0; y < e.Height; y++)
            {
                for (int x = 0; x < e.Width; x++)
                {
                    ushort color = e.VideoBuffer.Array[e.VideoBuffer.Offset + (y * pitch) + x];
                    int b;

                    b = ((color >> 10) & 31) * 8;
                    var red = (byte)(b + b / 35);
                    b = ((color >> 5) & 31) * 8;
                    var green = (byte)(b + b / 35);
                    b = ((color >> 0) & 31) * 8;
                    var blue = (byte)(b + b / 35);
                    var alpha = (byte)255;

                    _videoBuffer[y * _videoFrame.Width + x] = new Color() { R = red, G = green, B = blue, A = alpha };
                }
            }

            GraphicsDevice.Textures[0] = null;
            _videoFrame.SetData<Color>(_videoBuffer);
            _videoRect = new Rectangle(0, 0, e.Width, e.Height);
        }
    }
}
