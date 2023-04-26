using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

    static public class Edges
    {
        public static int LEFT = 0;
        public static int RIGHT = 520;
        public static int TOP = 0;
        public static int BOTTOM = 540;
    }

    public interface IDrawable
    {
        void Draw(CanvasDrawingSession canvas);
    }

    public class SpaceInvaders : IDrawable
    {

        public List<IDrawable> drawables;
        public PlayerSpaceship PlayerSpaceship;
        public List<List<Invader>> Invaders; // Nested list is Rows
        public int InvaderCoolDown;
        public int InvaderCount;
        public int xGap;
        public int yGap;
        public int UpdateVerticalCount;
        public Gamepad controller;

        public List<Invader> BottomRowInvaders;
        public int TotalPoints;

        public CanvasBitmap SquidSprite;
        public CanvasBitmap CrabSprite;
        public CanvasBitmap OctopusSprite;
        public CanvasBitmap SquidSprite2;
        public CanvasBitmap CrabSprite2;
        public CanvasBitmap OctopusSprite2;
        public CanvasBitmap UfoSprite;


        public event EventHandler BulletFired;
        public event EventHandler InvaderDestroyed;
        public int InvaderLocYStart;

        public SpaceInvaders(CanvasBitmap playerSprite, CanvasBitmap squidSprite, CanvasBitmap crabSprite, CanvasBitmap octopusSprite, CanvasBitmap ufoSprite, CanvasBitmap squidSprite2, CanvasBitmap crabSprite2, CanvasBitmap octopusSprite2)
        {
            SquidSprite = squidSprite;
            CrabSprite = crabSprite;
            OctopusSprite = octopusSprite;
            SquidSprite2 = squidSprite2;
            CrabSprite2 = crabSprite2;
            OctopusSprite2 = octopusSprite2;

            UfoSprite = ufoSprite;

            drawables = new List<IDrawable> { };
            TotalPoints = 0;
            InvaderCoolDown = -50;
            InvaderCount = 0;
            xGap = 6;
            yGap = 7;
            UpdateVerticalCount = 0;

            Invaders = new List<List<Invader>>();
            InvaderLocYStart = 100;

            CreateInvader(drawables, SquidSprite, CrabSprite, OctopusSprite, SquidSprite2, CrabSprite2, OctopusSprite2, InvaderLocYStart);

            PlayerSpaceship = new PlayerSpaceship(playerSprite);


            drawables.Add(PlayerSpaceship);
        }

        public void CreateInvader(List<IDrawable> drawables, CanvasBitmap squidSprite, CanvasBitmap crabSprite, CanvasBitmap octopusSprite, CanvasBitmap squidSprite2, CanvasBitmap crabSprite2, CanvasBitmap octopusSprite2, int locY)
        {
            int locX = 0;


            List<Invader> invaders = new List<Invader>();
            for (int i = 0; i < 11; i++)
            {
                invaders.Add(new Invader(locX + i * 35 + xGap, locY, squidSprite, squidSprite2, 30));
                drawables.Add(invaders.Last());
            }
            Invaders.Add(new List<Invader>(invaders));

            for (int j = 0; j < 2; j++)
            {
                locY += 35 + yGap; // Image height plus gap
                invaders = new List<Invader> { };
                for (int i = 0; i < 11; i++)
                {
                    invaders.Add(new Invader(locX + i * 35 + xGap, locY, crabSprite, crabSprite2, 20));
                    drawables.Add(invaders.Last());
                }
                Invaders.Add(new List<Invader>(invaders));
            }

            for (int j = 0; j < 2; j++)
            {
                locY += 35 + yGap; // Image height plus gap
                invaders = new List<Invader> { };
                for (int i = 0; i < 11; i++)
                {
                    invaders.Add(new Invader(locX + i * 35 + xGap, locY, octopusSprite, octopusSprite2, 10));
                    drawables.Add(invaders.Last());
                }
                Invaders.Add(new List<Invader>(invaders));

                if (j == 1)
                {
                    BottomRowInvaders = new List<Invader>(invaders);
                }
            }

        }

        public void Draw(CanvasDrawingSession canvas)
        {
            foreach (var drawable in drawables)
            {
                drawable.Draw(canvas);
            }
        }

        public void Update()
        {

            // Reset game with delay
            //check if 55 invaders have died (player wins)
            if (InvaderCount >= 55)
            {
                //reset game 
                if (InvaderCount == 55)
                {
                    drawables = new List<IDrawable> { };
                    InvaderLocYStart += 35 + yGap;
                    drawables.Add(PlayerSpaceship);
                    PlayerSpaceship.LocX = 0;
                    InvaderCoolDown = -50;
                    PlayerSpaceship.PlayerBullet = null;
                    PlayerSpaceship.Health++;
                    Invader.IsTravelingRight = true;
                    CreateInvader(drawables, SquidSprite, CrabSprite, OctopusSprite, SquidSprite2, CrabSprite2, OctopusSprite2, InvaderLocYStart);
                    InvaderCount++;
                    return;
                }
                InvaderCount = 0;
                Thread.Sleep(1500);
            }

            InvaderCoolDown++;
            Random random = new Random();
            List<IDrawable> drawablesToErase = new List<IDrawable>();



            foreach (var drawable in drawables)
            {
                if (drawablesToErase.Contains(drawable))
                {
                    continue;
                }

                var bullet = drawable as Bullet;

                if (bullet != null)
                {
                    foreach (var drawable1 in drawables)
                    {
                        if (drawablesToErase.Contains(drawable1)) // drawable1 already being deleted
                        {
                            continue;
                        }

                        if (drawable1 is Bullet && drawable1 as Bullet != bullet && bullet.CollidesWith(drawable1 as Bullet))
                        {
                            drawablesToErase.Add(bullet);
                            drawablesToErase.Add(drawable1);
                            break;
                        }


                        if (bullet.Type == Bullet.BulletType.Player && drawable1 is Invader && bullet.CollidesWith(drawable1 as Invader))
                        {
                            var invader = drawable1 as Invader;
                            int rowIndex = 0;
                            for (; rowIndex < Invaders.Count; rowIndex++)
                            {
                                int collumnIndex = Invaders[rowIndex].FindIndex(c => c == invader);
                                if (collumnIndex != -1)
                                {
                                    Invaders[rowIndex][collumnIndex] = null;
                                    break;
                                }
                            }
                            if (BottomRowInvaders.Contains(invader))
                            {
                                int collumnIndex = BottomRowInvaders.FindIndex(c => c == invader);
                                bool replaced = false;
                                for (int i = Invaders.Count - 1; i >= 0; i--)
                                {
                                    if (Invaders[i][collumnIndex] != null)
                                    {
                                        BottomRowInvaders[collumnIndex] = Invaders[i][collumnIndex];
                                        replaced = true;
                                        break;
                                    }

                                }
                                if (!replaced)
                                {
                                    BottomRowInvaders[collumnIndex] = null;
                                }
                            }

                            drawablesToErase.Add(invader);
                            drawablesToErase.Add(bullet);
                            InvaderDestroyed?.Invoke(this, EventArgs.Empty);
                            InvaderCount++;
                            TotalPoints += invader.Point;
                            break;
                        }

                    }

                    if (bullet.Update()) // if it goes out of map, delete
                    {
                        drawablesToErase.Add(bullet);
                    }
                }

            }

            foreach (var drawableToErase in drawablesToErase)
            {
                if (drawableToErase == PlayerSpaceship.PlayerBullet)
                {
                    PlayerSpaceship.PlayerBullet = null;
                }
                drawables.Remove(drawableToErase);
            }

            if (InvaderCoolDown >= 30 && random.Next(12) == 0)
            {
                List<Invader> availableBottomRowInvaders = BottomRowInvaders.Where(c => c != null).ToList();
                if (availableBottomRowInvaders.Count != 0)
                {
                    Invader invaderShoot = availableBottomRowInvaders[random.Next(availableBottomRowInvaders.Count)];
                    drawables.Add(new Bullet(invaderShoot.LocX + 17 + random.Next(2), invaderShoot.LocY + 36, -8, Bullet.BulletType.Invader));
                }
                InvaderCoolDown = 0;
            }


            if (Gamepad.Gamepads.Count > 0)
            {
                controller = Gamepad.Gamepads.First();
                var reading = controller.GetCurrentReading();
                PlayerSpaceship.LocX += (int)(reading.LeftThumbstickX * 5);

                if (reading.Buttons.HasFlag(GamepadButtons.A))
                {
                    if (PlayerSpaceship.PlayerBullet == null)
                    {
                        PlayerSpaceship.PlayerBullet = new Bullet(PlayerSpaceship.LocX + 20, PlayerSpaceship.LocY, 8, Bullet.BulletType.Player);
                        BulletFired?.Invoke(this, EventArgs.Empty);
                        drawables.Add(PlayerSpaceship.PlayerBullet);
                    }
                }
            }


            if (UpdateVerticalCount == 0)
            {
                if (Invader.UpdateInvaderHorizontalMovement(Invaders))
                {
                    UpdateVerticalCount += 1;
                }
            }
            else
            {
                if (Invader.UpdateInvaderVerticalMovement(Invaders))
                {
                    UpdateVerticalCount--;
                }
            }

            PlayerSpaceship.Update(drawables, BulletFired);


        }
    }
    public class PlayerSpaceship : IDrawable
    {

        public CanvasBitmap PlayerSprite;
        public int Health;
        public int LocX;
        public int LocY;
        public int BulletCooldown;
        public Bullet PlayerBullet;

        public bool IsTravelingLeft;
        public bool IsTravelingRight;
        public bool IsShooting;

        public PlayerSpaceship(CanvasBitmap playerSprite)
        {
            PlayerSprite = playerSprite;
            Health = 3;
            LocX = 0;
            LocY = 495;
            BulletCooldown = 0;
            PlayerBullet = null;

            IsTravelingLeft = false;
            IsTravelingRight = false;
            IsShooting = false;
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            canvas.DrawImage(PlayerSprite, LocX, LocY);
        }


        public void Update(List<IDrawable> drawables, EventHandler bulletFired)
        {

            BulletCooldown++;

            //check if player is moving left and update if so
            if (IsTravelingLeft)
            {
                if (LocX - 3 < Edges.LEFT)
                {
                    LocX = Edges.LEFT;
                }
                else
                {
                    LocX -= 3;
                }

            }
            //check if player is moving right and update if so
            else if (IsTravelingRight)
            {
                if (LocX + 40 + 3 > Edges.RIGHT)
                {
                    LocX = Edges.RIGHT - 40;
                }
                else
                {
                    LocX += 3;
                }
            }

            //checking if player can shoot a bullet
            if (PlayerBullet == null && IsShooting)
            {
                PlayerBullet = new Bullet(LocX + 20, LocY, 8, Bullet.BulletType.Player);
                bulletFired?.Invoke(this, EventArgs.Empty);
                drawables.Add(PlayerBullet);
                BulletCooldown = 0;
            }
        }
    }



    public class Invader : IDrawable
    {
        public static bool IsTravelingRight = true;
        static int MovementCountdown = 0;
        public static int MovementInterval = 20;
        static int MovementHorizontalSpeed = 8;
        static int MovementVerticalSpeed = 21;



        public int LocX;
        public int LocY;
        public int Point; // How many points u get when u destroy it
        public CanvasBitmap SpriteToDraw;
        public CanvasBitmap InvaderSprite1;
        public CanvasBitmap InvaderSprite2;

        public Invader(int locX, int locY, CanvasBitmap invaderSprite, CanvasBitmap invaderSprite2, int point)
        {
            LocX = locX;
            LocY = locY;
            Point = point;
            InvaderSprite1 = invaderSprite;
            InvaderSprite2 = invaderSprite2;
            SpriteToDraw = InvaderSprite1;
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            canvas.DrawImage(SpriteToDraw, LocX, LocY);
        }

        //move the invader sprites right or left 
        static public bool UpdateInvaderHorizontalMovement(List<List<Invader>> invaders)
        {
            MovementCountdown++;

            // return if not ready to move
            if (MovementCountdown <= MovementInterval)
            {
                return false;
            }
            MovementCountdown = 0;

            int movementSpeed = MovementHorizontalSpeed;
            bool updateYandSwitch = false;

            //temporarily change movement speed if sprite is gonna clip out of frame 
            if (IsTravelingRight)
            {
                //look for one of the most right sprites
                for (int collumnIndex = invaders[0].Count - 1; collumnIndex >= 0; collumnIndex--)
                {
                    bool isFound = false;
                    for (int rowIndex = 0; rowIndex < invaders.Count; rowIndex++)
                    {
                        if (invaders[rowIndex][collumnIndex] != null)
                        {
                            Invader tempInvader = invaders[rowIndex][collumnIndex];
                            int width = (int)tempInvader.SpriteToDraw.SizeInPixels.Width;
                            isFound = true; // we found one of the most right sprites
                            if (tempInvader.LocX + width + movementSpeed >= Edges.RIGHT)
                            {
                                //update movement speed temp of sprite to the distance between the sprite and right edge
                                movementSpeed = Edges.RIGHT - (tempInvader.LocX + width);
                                updateYandSwitch = true;
                            }
                            break;
                        }

                    }
                    //break once the speed has been updated
                    if (isFound)
                    {
                        break;
                    }
                }
            }
            else
            {
                for (int collumnIndex = 0; collumnIndex < invaders[0].Count; collumnIndex++)
                {
                    bool isFound = false;
                    for (int rowIndex = 0; rowIndex < invaders.Count; rowIndex++)
                    {
                        if (invaders[rowIndex][collumnIndex] != null)
                        {
                            Invader tempInvader = invaders[rowIndex][collumnIndex];
                            isFound = true;
                            if (tempInvader.LocX - movementSpeed <= Edges.LEFT)
                            {
                                //update movement speed temp of sprite to the distance between the sprite and left edge
                                movementSpeed = tempInvader.LocX - Edges.LEFT;
                                updateYandSwitch = true;
                            }
                            break;
                        }
                    }
                    //break once the speed has been updated
                    if (isFound)
                    {
                        break;
                    }
                }
            }

            foreach (var invaderList in invaders)
            {
                foreach (var invader in invaderList)
                {
                    if (invader != null)
                    {
                        if (IsTravelingRight)
                        {
                            invader.LocX += movementSpeed;
                        }
                        else
                        {
                            invader.LocX -= movementSpeed;
                        }
                        invader.SpriteToDraw = (invader.SpriteToDraw == invader.InvaderSprite1) ? invader.InvaderSprite2 : invader.InvaderSprite1;
                    }
                }

            }
            if (updateYandSwitch)
            {
                IsTravelingRight = !IsTravelingRight;
                return true;
            }
            return false;
        }
        static public bool UpdateInvaderVerticalMovement(List<List<Invader>> invaders)
        {
            MovementCountdown++;

            // return if not ready to move
            if (MovementCountdown <= MovementInterval)
            {
                return false;
            }
            MovementCountdown = 0;

            foreach (var invaderList in invaders)
            {
                foreach (var invader in invaderList)
                {
                    if (invader != null)
                    {
                        invader.LocY += MovementVerticalSpeed;
                        invader.SpriteToDraw = (invader.SpriteToDraw == invader.InvaderSprite1) ? invader.InvaderSprite2 : invader.InvaderSprite1;
                    }
                }
            }
            return true;
        }

    }




    public class Bullet : IDrawable
    {
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

        public Bullet(int locX, int locY, int speed, BulletType type)
        {
            LocX = locX;
            LocY = locY;
            Speed = speed;
            Length = 8;
            Type = type;
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            canvas.DrawLine(LocX, LocY, LocX, LocY - Length, Colors.Red);
        }

        public bool CollidesWith(Invader invader)
        {
            int bottomVerticalHitbox = 20;
            if (invader.LocX + 5 < LocX && LocX < invader.LocX + 30 && invader.LocY + 35 >= LocY && invader.LocY + (35 - bottomVerticalHitbox) < LocY + Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CollidesWith(Bullet other)
        {
            if (Speed == other.Speed)
            {
                return false;
            }

            Bullet top;
            Bullet bottom;

            if (LocY > other.LocY)
            {
                top = this;
                bottom = other;
            }
            else
            {
                top = other;
                bottom = this;
            }

            if (Math.Abs(LocX - other.LocX) <= 5 && top.LocY + top.Length - bottom.LocY >= 0 && top.LocY <= bottom.LocY + bottom.Length)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Update()
        {
            if (LocY + Length < Edges.TOP || LocY > Edges.BOTTOM)
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
