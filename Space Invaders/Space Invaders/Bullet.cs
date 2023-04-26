using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Space_Invaders
{
    public class Bullet : IDrawable
    {
        // Different bullet types 
        public enum BulletType
        {
            Player = 0,
            Invader = 1
        }
        public int LocX;
        public int LocY;
        public int Speed;
        public int Length;
        public BulletType Type;
        
        // Constructor 
        public Bullet(int locX, int locY, int speed, int length, BulletType type)
        {
            LocX = locX;
            LocY = locY;
            Speed = speed;
            Length = length;
            Type = type;
        }
        
        // Draw bullet onto scren
        public void Draw(CanvasDrawingSession canvas)
        {
            Color color;
            if (Type == BulletType.Player)
            {
                color = Colors.Gold;
            }
            else if (Type == BulletType.Invader)
            {
                color = Colors.Red;
            }    
            canvas.DrawLine(LocX, LocY, LocX, LocY + Length, color);
        }

        // If bullet hits an invader 
        public bool CollidesWith(Invader invader)
        {
            int horizontalHitboxCutoff = 0;
            int width = (int)invader.SpriteToDraw.SizeInPixels.Width;
            int height = (int)invader.SpriteToDraw.SizeInPixels.Height;
            if (invader.LocX + horizontalHitboxCutoff < LocX && LocX < invader.LocX + (width - horizontalHitboxCutoff) && invader.LocY + height >= LocY && invader.LocY <= LocY + Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // If bullet hits another bullet 
        public bool CollidesWith(Bullet other)
        {
            if (Type == other.Type) // return if both same
            {
                return false;
            }

            int maxXDifference = 5;

            Bullet playerBullet;
            Bullet invaderBullet;

            if (Type == BulletType.Player && other.Type == BulletType.Invader)
            {
                playerBullet = this;
                invaderBullet = other;
            }
            else
            {
                playerBullet = other;
                invaderBullet = this;
            }

            if (Math.Abs(LocX - other.LocX) <= maxXDifference && invaderBullet.LocY + invaderBullet.Length - playerBullet.LocY <= playerBullet.Speed - invaderBullet.Speed && invaderBullet.LocY + invaderBullet.Length - playerBullet.LocY >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // If bullet hits the players spaceship 
        public bool CollidesWith(PlayerSpaceship playerSpaceship)
        {
            // Playerspaceship on bottom
            int horizontalHitboxCutoff = 12; // Narrows hitbox to nose of spaceship
            int heightToSolid = 20; // Height downwards from LocY of spaceship where spaceship is all solid, no bezel

            int width = (int)playerSpaceship.PlayerSprite.SizeInPixels.Width;
            int height = (int)playerSpaceship.PlayerSprite.SizeInPixels.Height;

            if (playerSpaceship.LocX + horizontalHitboxCutoff < LocX && LocX < playerSpaceship.LocX + (width - horizontalHitboxCutoff) && (playerSpaceship.LocY <= LocY + Length) && (playerSpaceship.LocY + height > LocY))
            {
                return true;
            }
            else if (playerSpaceship.LocX < LocX && LocX < playerSpaceship.LocX + width && playerSpaceship.LocY + heightToSolid <= LocY + Length && playerSpaceship.LocY + height - 6 > LocY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Update location of bullet (sending bullet downwards)
        public bool Update() 
        {
            if (LocY <= Edges.TOP || LocY + Length >= Edges.BOTTOM) // If bullet is out of frame
            {
                return true;
            }
            else
            {
                LocY -= Speed;
                return false;
            }
        }
    }

}
