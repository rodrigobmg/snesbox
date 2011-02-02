using System;
using Microsoft.Xna.Framework;

namespace SnesBox
{
    public class FrameRateComponent : DrawableGameComponent
    {
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        public string FrameRate { get; private set; }

        public FrameRateComponent(Game game)
            : base(game)
        {
        }


        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }
        }


        public override void Draw(GameTime gameTime)
        {
            frameCounter++;

            FrameRate = frameRate.ToString();
        }
    }
}
