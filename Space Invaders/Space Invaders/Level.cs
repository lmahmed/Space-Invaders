using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.AllJoyn;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Gaming.Input;
using Windows.Media.Devices.Core;
using Windows.UI;
using Windows.UI.Xaml.Documents;

namespace Space_Invaders
{
    // Edges of canvas 
    static public class Edges
    {
        public static int LEFT = 0;
        public static int RIGHT = 550;
        public static int TOP = 0;
        public static int BOTTOM = 540;
    }

    // Canvas 
    public interface IDrawable
    {
        void Draw(CanvasDrawingSession canvas);
    }

    // Changes due to destroying invaders
    public class InvaderChange 
    {
        public int InvaderDestroyed;
        public int? InvaderBulletSpeed;
        public int? InvaderHorizontalSpeed;
        public int? InvaderMovementInterval;
        public int? InvaderMinimumTickBeforeShoot;
        public int? InvaderStunDuration;
        public InvaderChange(int invaderDestroyed, int? invaderBulletSpeed, int? invaderHorizontalSpeed, int? invaderMovementInterval, int? invaderMinimumTickBeforeShoot, int? invaderStunDuration) 
        {
            InvaderDestroyed = invaderDestroyed;
            InvaderBulletSpeed = invaderBulletSpeed;
            InvaderHorizontalSpeed = invaderHorizontalSpeed;
            InvaderMovementInterval = invaderMovementInterval;
            InvaderMinimumTickBeforeShoot = invaderMinimumTickBeforeShoot;
            InvaderStunDuration = invaderStunDuration;
        }
    }

    public class Level
    {
        // Invader
        public int InvaderBulletSpeed;
        public int InvaderHorizontalSpeed;
        public int InvaderVerticalSpeed;
        public int InvaderMinimumTickBeforeShoot;
        public int InvaderMovementInterval;
        public int InvaderStunDuration;
        public int InvaderStartYPosition;
        public int InvaderXGap;
        public int InvaderYGap;

        public int PlayerBulletSpeed;
        public int PlayerMovementSpeed;
        public int PlayerBulletCooldown;

        public List<InvaderChange> InvaderChanges;

        public Level(int invaderBulletSpeed, int invaderHorizontalSpeed, 
            int invaderVerticalSpeed, int invaderMinimumTickBeforeShoot, 
            int invaderMovementInterval, int invaderStunDuration, 
            int invaderStartYPosition, int invaderXGap, int invaderYGap, int playerBulletSpeed, 
            int playerMovementSpeed, int playerBulletCooldown, List<InvaderChange> invaderChanges)
        {
            InvaderBulletSpeed = invaderBulletSpeed;
            InvaderHorizontalSpeed = invaderHorizontalSpeed;
            InvaderVerticalSpeed = invaderVerticalSpeed;
            InvaderMinimumTickBeforeShoot = invaderMinimumTickBeforeShoot;
            InvaderMovementInterval = invaderMovementInterval;
            InvaderStartYPosition = invaderStartYPosition;
            InvaderStunDuration = invaderStunDuration;
            InvaderXGap = invaderXGap;
            InvaderYGap = invaderYGap;

            PlayerBulletSpeed = playerBulletSpeed;
            PlayerMovementSpeed = playerMovementSpeed;
            PlayerBulletCooldown = playerBulletCooldown;

            InvaderChanges = new List<InvaderChange> (invaderChanges);
        }
    }


    public class DestroySprite : IDrawable
    {
        public List <CanvasBitmap> Sprites;
        public int LocX;
        public int LocY;
        public int Duration;
        private int Count;
        public DestroySprite(List <CanvasBitmap> sprites, int locXMiddle, int locYMiddle, int duration) // Give the middle point of where destruction sprite goes
        {
            Sprites = sprites;
            LocX = locXMiddle - (int)(Sprites[0].SizeInPixels.Width / 2);
            LocY = locYMiddle - (int)(Sprites[0].SizeInPixels.Height / 2);
            Duration = duration;
            Count = 0;
        }
        public DestroySprite(CanvasBitmap sprite, int locXMiddle, int locYMiddle, int duration) // Give the middle point of where destruction sprite goes
        {
            Sprites = new List<CanvasBitmap>();
            Sprites.Add (sprite);
            LocX = locXMiddle - (int)(Sprites[0].SizeInPixels.Width / 2);
            LocY = locYMiddle - (int)(Sprites[0].SizeInPixels.Height / 2);
            Duration = duration;
            Count = 0;
        }
        public void Draw(CanvasDrawingSession canvas)
        {
            int ratio = (int)(((float)Count / Duration) * (Sprites.Count - 1));
            canvas.DrawImage(Sprites[ratio], LocX, LocY);
        }

        public bool Update() // returns true when time to get deleted
        {
            Count++;
            if (Count >= Duration)
            {
                return true;
            }
            return false;
        }
    }

}
