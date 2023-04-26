using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Space_Invaders
{
   
    public class Invader : IDrawable
    {
        public static bool IsTravelingRight = true;
        // This means every 20 ticks the invaders will shift upto 8 pixels horizontally (left or right) or 21 pixels (downward)
        public static int MovementInterval; // A movement every 20 ticks
        public static int MovementCount = 0; // To count up to MovementInterval
        public static int MovementHorizontalSpeed;
        public static int MovementVerticalSpeed;
        public static int MovementVerticalToDo = 1; // How many times to do vertical jump when hitting wall
        public static int MovementVerticalToDoCount = 0; // Will be used to count back down to 0 from MovementVerticalToDo


        public static int MinimumTickBeforeShoot; // The more lower, the more they shoot
        public static int ShootCount; // To count up to MinimumTickBeforeShoot
        public static int BulletSpeed; // Bullet goes down 8 every tick


        public int LocX; // x location
        public int LocY; // y location 
        public int Point; // How many points u get when u destroy it

        public CanvasBitmap InvaderSprite1; // First image of sprite
        public CanvasBitmap InvaderSprite2; // Second image of sprite 
        public CanvasBitmap SpriteToDraw; // Image to draw currently

        // Constructor 
        public Invader(int locX, int locY, CanvasBitmap invaderSprite, CanvasBitmap invaderSprite2, int point)
        {
            LocX = locX;
            LocY = locY;
            Point = point;
            InvaderSprite1 = invaderSprite;
            InvaderSprite2 = invaderSprite2;
            SpriteToDraw = InvaderSprite1;
        }
        
        // Draw invader onto screen 
        public void Draw(CanvasDrawingSession canvas)
        {
            canvas.DrawImage(SpriteToDraw, LocX, LocY);
        }

        // Move the invaders either right or left 
        static private bool UpdateInvaderHorizontalMovement(List<List<Invader>> invaders)
        {
            MovementCount++;

            // return if not ready to move
            if (MovementCount <= MovementInterval)
            {
                return false;
            }
            MovementCount = 0;

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

                            if (tempInvader.LocX + width + movementSpeed >= Edges.RIGHT)
                            {
                                //update movement speed temp of sprite to the distance between the sprite and right edge
                                movementSpeed = Edges.RIGHT - (tempInvader.LocX + width);
                                updateYandSwitch = true;
                                isFound = true; // we found one of the most right sprites
                                break;
                            }
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
                            if (tempInvader.LocX - movementSpeed <= Edges.LEFT)
                            {
                                //update movement speed temp of sprite to the distance between the sprite and left edge
                                movementSpeed = tempInvader.LocX - Edges.LEFT;
                                updateYandSwitch = true;
                                isFound = true;
                                break;
                            }
                        }
                    }
                    //break once the speed has been updated
                    if (isFound)
                    {
                        break;
                    }
                }
            }
            //move invaders either left or right 
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
        } // return true if hit wall
        
        // Move the invaders downward
        static private void UpdateInvaderVerticalMovement(List<List<Invader>> invaders)
        {
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
        }


        static public void Update(List<IDrawable> drawables, List<List<Invader>> invaders, List<Invader> bottomRowInvaders)
        {
            Random random = new Random();

            MovementCount++;
            ShootCount++;
            if (MovementCount > MovementInterval)
            {
                if (MovementVerticalToDoCount == 0)
                {
                    if (UpdateInvaderHorizontalMovement(invaders)) // if hit wall while moving horizontally
                    {
                        MovementVerticalToDoCount += MovementVerticalToDo;
                    }
                }
                else
                {
                    UpdateInvaderVerticalMovement(invaders);
                    MovementVerticalToDoCount--;
                }
                MovementCount = 0;
            }
            // Invader shoot logic
            if (ShootCount >= MinimumTickBeforeShoot && random.Next((int)(.25 * MinimumTickBeforeShoot)) == 0)
            {
                List<Invader> availableBottomRowInvaders = bottomRowInvaders.Where(c => c != null).ToList();
                if (availableBottomRowInvaders.Count != 0)
                {
                    int length = 8;
                    Invader invaderShoot = availableBottomRowInvaders[random.Next(availableBottomRowInvaders.Count)];
                    drawables.Add(new Bullet(invaderShoot.LocX + ((int)invaderShoot.SpriteToDraw.SizeInPixels.Width / 2), invaderShoot.LocY + (int)invaderShoot.SpriteToDraw.SizeInPixels.Height, BulletSpeed, length, Bullet.BulletType.Invader));
                }
                ShootCount = 0;
            }
        }
    }
}
