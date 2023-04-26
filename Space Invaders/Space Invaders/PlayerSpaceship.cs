using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Gaming.Input;

namespace Space_Invaders
{
    public class PlayerSpaceship : IDrawable
    {
        private int _health;
        public event EventHandler PlayerHealth;

        public CanvasBitmap PlayerSprite;
        public int Health
        {
            get => _health;
            set
            {
                if (value < 5)
                {
                    _health = value;
                    PlayerHealth?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public int LocX;
        public int LocY;
        public int MovementSpeed;

        public Bullet PlayerBullet;
        public int BulletCooldown;
        public int BulletInterval;
        public int BulletSpeed;

        public bool IsTravelingLeft;
        public bool IsTravelingRight;
        public bool IsShooting;
        public bool BulletIntervalSystemUsed; // Sets if gamemode is shooting one at a time or by interval, toggle with c

        public PlayerSpaceship(CanvasBitmap playerSprite, int bulletSpeed, int movementSpeed, int bulletInterval)
        {
            PlayerSprite = playerSprite;
            Health = 3;

            LocX = Edges.LEFT;
            LocY = Edges.BOTTOM - (int)PlayerSprite.SizeInPixels.Height;
            MovementSpeed = movementSpeed; // Pixel movement per tick if moving left or right. OVERWRITTEN BY LEVEL

            PlayerBullet = null;
            BulletInterval = bulletInterval;
            BulletCooldown = 0; // If using a tick system
            BulletSpeed = bulletSpeed;


            IsTravelingLeft = false;
            IsTravelingRight = false;
            IsShooting = false;
            BulletIntervalSystemUsed = false;
        }

        // enter null 
        public void UpdateStats(int? bulletSpeed, int? movementSpeed, int? bulletInterval)
        {
            if (bulletSpeed != null)
            {
                BulletSpeed = bulletSpeed.Value;
            }
            if (movementSpeed != null)
            {
                MovementSpeed = movementSpeed.Value;
            }
            if (bulletInterval != null)
            {
                BulletInterval = bulletInterval.Value;
            }
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            canvas.DrawImage(PlayerSprite, LocX, LocY);
        }


        public void Update(List<IDrawable> drawables, EventHandler bulletFired)
        {
            BulletCooldown++;

            bool pressedBbutton = false;
            if (Gamepad.Gamepads.Count > 0) // Gamepad input
            {
                var reading = Gamepad.Gamepads.First().GetCurrentReading(); // Only uses first gamepad input

                int movementAmount = (int)(reading.LeftThumbstickX * MovementSpeed);
                LocX += movementAmount;

                if (LocX < Edges.LEFT)
                {
                    LocX = Edges.LEFT;
                }
                if (LocX + PlayerSprite.SizeInPixels.Width > Edges.RIGHT)
                {
                    LocX = Edges.RIGHT - (int)PlayerSprite.SizeInPixels.Width;
                }

                pressedBbutton = reading.Buttons.HasFlag(GamepadButtons.RightShoulder) || reading.Buttons.HasFlag(GamepadButtons.A);

                if (reading.Buttons.HasFlag(GamepadButtons.Y))
                {
                    BulletIntervalSystemUsed = !BulletIntervalSystemUsed;
                }
            }


            // Keyboard logic 
            // check if player is moving left and update if so
            if (IsTravelingLeft)
            {
                if (LocX - MovementSpeed < Edges.LEFT)
                {
                    LocX = Edges.LEFT;
                }
                else
                {
                    LocX -= MovementSpeed;
                }

            }
            //check if player is moving right and update if so
            else if (IsTravelingRight)
            {
                if (LocX + PlayerSprite.SizeInPixels.Width + MovementSpeed > Edges.RIGHT)
                {
                    LocX = Edges.RIGHT - (int)PlayerSprite.SizeInPixels.Width;
                }
                else
                {
                    LocX += MovementSpeed;
                }
            }

            // Keyboard and Gamepad stack for movement


            //checking if player can shoot a bullet
            if (((BulletIntervalSystemUsed && BulletCooldown >= BulletInterval) || (!BulletIntervalSystemUsed && PlayerBullet == null)) && (IsShooting || pressedBbutton))
            {
                int length = 8;
                PlayerBullet = new Bullet(LocX + (int)(PlayerSprite.SizeInPixels.Width / 2), LocY - length, BulletSpeed, length, Bullet.BulletType.Player);
                drawables.Add(PlayerBullet);

                bulletFired?.Invoke(this, EventArgs.Empty); // Sound effect

                BulletCooldown = 0;
            }
        }

        // Resets x location and makes playerbullet null
        public void Reset()
        {
            LocX = Edges.LEFT;
            PlayerBullet = null;
        }
    }

}
