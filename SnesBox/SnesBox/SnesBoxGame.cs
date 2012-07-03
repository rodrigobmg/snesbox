using System.IO;
using Microsoft.Xna.Framework;
using Snes;
using SnesBox.Components;

namespace SnesBox
{
    public class SnesBoxGame : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public string CartridgePath { get; set; }
        public bool Paused { get; set; }
        public Filter Filter
        {
            get
            {
                var video = (IVideoService)Services.GetService(typeof(IVideoService));
                return video.Filter;
            }
            set
            {
                var video = (IVideoService)Services.GetService(typeof(IVideoService));
                video.Filter = value;
            }
        }

        public SnesBoxGame()
        {
            this.IsFixedTimeStep = false;
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            GraphicsDeviceManager.PreferredBackBufferWidth = 800;
            GraphicsDeviceManager.PreferredBackBufferHeight = 600;
            GraphicsDeviceManager.ApplyChanges();

            Content.RootDirectory = "Content";

            Components.Add(new FrameRate(this));
            Components.Add(new Audio(this));
            Components.Add(new Video(this));
            Components.Add(new Input(this));
        }

        public void LoadCartridge(string cartridge)
        {
            using (var fs = new FileStream(cartridge, FileMode.Open))
            {
                var rom = new byte[fs.Length];
                fs.Read(rom, 0, (int)fs.Length);
                LibSnes.LoadCartridgeNormal(null, rom, (uint)rom.Length);
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            LibSnes.Init();

            if (!string.IsNullOrEmpty(CartridgePath))
            {
                LoadCartridge(CartridgePath);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (!Paused)
            {
                base.Update(gameTime);

                LibSnes.Run();

                var frameRate = (IFrameRateService)Services.GetService(typeof(IFrameRateService));
                Window.Title = string.Format("{0:##} FPS", frameRate.FPS);
            }
        }

        protected override void OnExiting(object sender, System.EventArgs args)
        {
           LibSnes.Exit();
        }
    }
}
