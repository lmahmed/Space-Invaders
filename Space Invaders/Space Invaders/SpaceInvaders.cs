using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;

// Main logic of the game written here 



namespace Space_Invaders
{
    public class SpaceInvaders : IDrawable
    {
        // Lists
        public List<IDrawable> Drawables;
        public List<List<Invader>> Invaders; // Nested list is Rows
        public List<Invader> BottomRowInvaders; // Keep track of invaders on bottom for shooting purposes
        public List<Level> Levels; // List of different level and their attributes
        public List<CanvasBitmap> PlayerHitGif;

        public Level CurrentLevel;

        public PlayerSpaceship PlayerSpaceship;

        public int TotalPoints;
        public int InvaderCount; // Count of destroyed invaders

        // Invader gaps and spawn position, x location always starts at 0
        public int xGapOfOctopus; // xGap of octopus because its the biggest
        public int yGap;
        public int InvaderStunDuration;

        public Invader UfoInvader;
        public int UfoSpawnTime;
        public int UfoCount;
        public bool UfoGoRight;

        // Images
        public SpriteCollection SpriteCollection;
        public UfoScoreCollection UfoScoreCollection;
        public CanvasBitmap DestroyInvaderSprite;

        public DestroySprite PlayerDestroySprite;

        public event EventHandler BulletFired;
        public event EventHandler InvaderDestroyed;
        public event EventHandler UpdateUi;
        public event EventHandler PlayerHitSound;
        public event EventHandler UfoSpawnSound;
        public event EventHandler UfoHitSound;
        public event EventHandler GameOver;

        public SpaceInvaders(SpriteCollection spriteCollection, CanvasBitmap destroyInvaderSprite, List <CanvasBitmap> playerHitGif, UfoScoreCollection ufoScoreCollection)
        {
            Random random = new Random();

            CreateLevels();
            CurrentLevel = Levels.First();

            SpriteCollection = spriteCollection;
            UfoScoreCollection = ufoScoreCollection;
            DestroyInvaderSprite = destroyInvaderSprite;
            PlayerHitGif = playerHitGif;
            PlayerDestroySprite = null;

            Drawables = new List<IDrawable> { };

            TotalPoints = 0;
            InvaderCount = 0;

            UfoInvader = null;
            UfoSpawnTime = 1200 + random.Next(600);

            UfoCount = 0;

            // Creating invaders starting at 100 height and gap between invaders
            Invaders = new List<List<Invader>>();


            // Create Spaceship spawns at bottom left always
            PlayerSpaceship = new PlayerSpaceship(SpriteCollection.PlayerSprite, CurrentLevel.PlayerBulletSpeed, CurrentLevel.PlayerMovementSpeed, CurrentLevel.PlayerBulletCooldown);
            Drawables.Add(PlayerSpaceship);

            Invader.IsTravelingRight = true;
            Invader.BulletSpeed = CurrentLevel.InvaderBulletSpeed;
            Invader.MinimumTickBeforeShoot = CurrentLevel.InvaderMinimumTickBeforeShoot;
            Invader.ShootCount = (Invader.MinimumTickBeforeShoot < 50) ? Invader.MinimumTickBeforeShoot - 50 : 0;  // For first invader shot, wait at least 50 ticks
            Invader.MovementHorizontalSpeed = CurrentLevel.InvaderHorizontalSpeed;
            Invader.MovementVerticalSpeed = CurrentLevel.InvaderVerticalSpeed;
            Invader.MovementInterval = CurrentLevel.InvaderMovementInterval;
            xGapOfOctopus = CurrentLevel.InvaderXGap;
            yGap = CurrentLevel.InvaderYGap;
            InvaderStunDuration = CurrentLevel.InvaderStunDuration;
            CreateInvader(Drawables, SpriteCollection, CurrentLevel.InvaderStartYPosition);
        }

        public void CreateLevels()
        {
            Levels = new List<Level>();
            List<InvaderChange> invaderChanges = new List<InvaderChange>();

            // 29 may be the maximum player bullet speed without bullet clipping through invaders 21 + 8 

            // Level 1 Creation
            invaderChanges.Add(new InvaderChange(20, null, null, 20, null, 20)); // 20
            invaderChanges.Add(new InvaderChange(40, null, null, 10, null, 10)); // 40
            invaderChanges.Add(new InvaderChange(54, null, null, 1, null, null)); // 45
            Levels.Add(new Level(-4, 8, 21, 30, 30, 30, 100, 8, 10, 10, 3, 20, invaderChanges));


            invaderChanges = new List<InvaderChange>();
            // Level 2 Creation
            invaderChanges.Add(new InvaderChange(20, null, null, 17, null, 15)); // 20
            invaderChanges.Add(new InvaderChange(40, null, null, 7, 25, 5)); // 40
            invaderChanges.Add(new InvaderChange(54, null, null, 1, null, null)); // 45
            Levels.Add(new Level(-6, 8, 21, 30, 27, 25, 121, 8, 10, 10, 3, 30, invaderChanges));

            invaderChanges = new List<InvaderChange>();
            // Level 3 Creation
            invaderChanges.Add(new InvaderChange(20, null, null, 17, null, 10)); // 20
            invaderChanges.Add(new InvaderChange(40, null, null, 7, 20, null)); // 40
            invaderChanges.Add(new InvaderChange(54, null, null, 1, null, null)); // 45
            Levels.Add(new Level(-8, 8, 21, 25, 27, 20, 142, 8, 10, 10, 4, 40, invaderChanges));

            invaderChanges = new List<InvaderChange>();
            // Level 4 Creation
            invaderChanges.Add(new InvaderChange(10, null, null, 10, null, null)); // 30
            invaderChanges.Add(new InvaderChange(30, null, null, 5, 40, null)); // 40
            invaderChanges.Add(new InvaderChange(54, null, null, 1, null, null)); // 45
            Levels.Add(new Level(-12, 9, 21, 45, 15, 15, 100, 8, 10, 10, 8, 40, invaderChanges));

        }

        // Creating the invaders
        public void CreateInvader(List<IDrawable> drawables, SpriteCollection s, int locY)
        {
            CreateInvader(drawables, s.SquidSprite1, s.CrabSprite1, s.OctopusSprite1, s.SquidSprite2, s.CrabSprite2, s.OctopusSprite2, locY);
        }

        // Drawing invaders to screen
        public void CreateInvader(List<IDrawable> drawables, CanvasBitmap squidSprite, CanvasBitmap crabSprite, CanvasBitmap octopusSprite, CanvasBitmap squidSprite2, CanvasBitmap crabSprite2, CanvasBitmap octopusSprite2, int locY)
        {
            int locX = 0;

            List<Invader> invaders = new List<Invader>();

            // all invaders are aligned on the left
            // squid and crab get more gap to account for being (possibly) smaller 
            int squidExtraGap = (int)(octopusSprite.SizeInPixels.Width - squidSprite.SizeInPixels.Width);
            int crabExtraGap = (int)(octopusSprite.SizeInPixels.Width - crabSprite.SizeInPixels.Width);

            // row of squid
            for (int i = 0; i < 11; i++)
            {
                invaders.Add(new Invader(locX + i * ((int)squidSprite.SizeInPixels.Width + xGapOfOctopus + squidExtraGap), locY, squidSprite, squidSprite2, 30));
                drawables.Add(invaders.Last());
            }
            Invaders.Add(new List<Invader>(invaders));
            locY += (int)squidSprite.SizeInPixels.Height + yGap;

            // 2 rows of crab
            for (int j = 0; j < 2; j++)
            {
                invaders = new List<Invader> { };
                for (int i = 0; i < 11; i++)
                {
                    invaders.Add(new Invader(locX + i * ((int)crabSprite.SizeInPixels.Width + xGapOfOctopus + crabExtraGap), locY, crabSprite, crabSprite2, 20));
                    drawables.Add(invaders.Last());
                }
                Invaders.Add(new List<Invader>(invaders));
                locY += (int)crabSprite.SizeInPixels.Height + yGap; // Image height plus gap
            }

            // 2 rows of octopus
            for (int j = 0; j < 2; j++)
            {
                invaders = new List<Invader> { };
                for (int i = 0; i < 11; i++)
                {
                    invaders.Add(new Invader(locX + i * ((int)octopusSprite.SizeInPixels.Width + xGapOfOctopus), locY, octopusSprite, octopusSprite2, 10));
                    drawables.Add(invaders.Last());
                }
                Invaders.Add(new List<Invader>(invaders));
                locY += (int)octopusSprite.SizeInPixels.Height + yGap; // Image height plus gap
            }
            BottomRowInvaders = new List<Invader>(Invaders.Last());
    

            Drawables.Reverse();
        }

        public void Draw(CanvasDrawingSession canvas)
        {
            foreach (var drawable in Drawables)
            {
                drawable.Draw(canvas);
            }
        }

        public void Update()
        {
            if (PlayerSpaceship.Health == 0)
            {
                return;
            }

            if (BottomRowInvaders.Where(c=> c != null).Any(c => c.LocY + (int)c.SpriteToDraw.SizeInPixels.Height >= PlayerSpaceship.LocY))
            {
                PlayerSpaceship.Health = 0;
                GameOver?.Invoke(this, EventArgs.Empty);
                return;
            }
            // If player has been hit by a bullet
            if (PlayerDestroySprite != null)
            {
                // gif taking place, at end of gif updates happen
                if (PlayerDestroySprite.Update())
                {
                    PlayerSpaceship.Health--;
                    Invader.ShootCount = 0;

                    Drawables.Remove(PlayerDestroySprite);
                    PlayerDestroySprite = null;
                    PlayerSpaceship.Reset();

                    if (PlayerSpaceship.Health == 0)
                    {
                        Drawables.Remove(PlayerSpaceship);
                        GameOver?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }
                return;
            }
           

            // Reset game in case of winning round with delay
            // check if 55 invaders have died (player wins)
            if (InvaderCount >= 55)
            {
                //reset game 
                if (InvaderCount == 55)
                {
                    WinLevel();
                    InvaderCount++; // Go to next tick for reset to appear
                    return;
                }
                InvaderCount = 0;
                Thread.Sleep(1500); // Delay for the new round
            }

            // Check to see if UfoInvader went out of bounds
            if (UfoInvader != null && (UfoInvader.LocX + (int)UfoInvader.InvaderSprite1.SizeInPixels.Width < Edges.LEFT || UfoInvader.LocX > Edges.RIGHT))
            {
                Drawables.Remove(UfoInvader);
                UfoInvader = null;
            }

            if (CurrentLevel.InvaderChanges.Any(ic => ic.InvaderDestroyed == InvaderCount)) // Changes due to destroying invaders
            {
                var invaderChanges = CurrentLevel.InvaderChanges.First(ic => ic.InvaderDestroyed == InvaderCount);
                if (invaderChanges.InvaderBulletSpeed != null)
                {
                    Invader.BulletSpeed = invaderChanges.InvaderBulletSpeed.Value;
                }
                if (invaderChanges.InvaderMovementInterval != null)
                {
                    Invader.MovementInterval = invaderChanges.InvaderMovementInterval.Value;
                }
                if (invaderChanges.InvaderHorizontalSpeed != null)
                {
                    Invader.MovementHorizontalSpeed = invaderChanges.InvaderHorizontalSpeed.Value;
                }
                if (invaderChanges.InvaderMinimumTickBeforeShoot != null)
                {
                    Invader.MinimumTickBeforeShoot = invaderChanges.InvaderMinimumTickBeforeShoot.Value;
                }
                if (invaderChanges.InvaderStunDuration != null)
                {
                    InvaderStunDuration = invaderChanges.InvaderStunDuration.Value;
                }
            }

            Random random = new Random();
            List<IDrawable> drawablesToErase = new List<IDrawable>();

            // Checking is bullets have hit anything
            // Mainly for bullet collision checking and logic
            for (int i = 0; i < Drawables.Count; i++)
            {
                IDrawable drawable = Drawables[i];
                if (drawablesToErase.Contains(drawable))
                {
                    continue;
                }

                // bullet collision logic
                var bullet = drawable as Bullet;
                if (bullet != null)
                {
                    if (bullet.Type == Bullet.BulletType.Invader && bullet.CollidesWith(PlayerSpaceship))
                    {
                        drawablesToErase.Add(bullet);
                        PlayerDestroySprite = new DestroySprite(PlayerHitGif,
                            PlayerSpaceship.LocX + ((int)PlayerSpaceship.PlayerSprite.SizeInPixels.Width / 2),
                            PlayerSpaceship.LocY + ((int)PlayerSpaceship.PlayerSprite.SizeInPixels.Height / 2),
                            100);
                        Drawables.Add(PlayerDestroySprite);

                        PlayerHitSound?.Invoke(this, EventArgs.Empty);
                        continue;
                    }

                    if (bullet.Type == Bullet.BulletType.Player && UfoInvader != null && bullet.CollidesWith(UfoInvader))
                    {
                        int points = UfoInvader.Point;
                        TotalPoints += points;
                        int locX = UfoInvader.LocX + (int)(UfoInvader.InvaderSprite1.SizeInPixels.Width / 2);
                        int locY = UfoInvader.LocY + (int)(UfoInvader.InvaderSprite1.SizeInPixels.Height / 2);
                        CanvasBitmap ufoScore;
                        if (points == 50)
                        {
                            ufoScore = UfoScoreCollection.FiftyScore;
                        }    
                        else if (points == 100)
                        {
                            ufoScore = UfoScoreCollection.HundredScore;
                        }
                        else if (points == 150)
                        {
                            ufoScore = UfoScoreCollection.HundredFiftyScore;
                        }    
                        else if (points == 200)
                        {
                            ufoScore = UfoScoreCollection.TwoHundredScore;
                        }
                        else 
                        {
                            ufoScore = UfoScoreCollection.ThreeHundredScore;
                        }
                        Drawables[Drawables.FindIndex(c => c == UfoInvader)] = new DestroySprite(ufoScore, locX, locY, 150);
                        UfoInvader = null;
                        drawablesToErase.Add(bullet);

                        UfoHitSound?.Invoke(this, EventArgs.Empty);
                        continue;
                    }

                    for (int j = 0; j < Drawables.Count; j++)
                    {
                        IDrawable drawable1 = Drawables[j];
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
                                for (int k = Invaders.Count - 1; k >= 0; k--)
                                {
                                    if (Invaders[k][collumnIndex] != null)
                                    {
                                        BottomRowInvaders[collumnIndex] = Invaders[k][collumnIndex];
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

                            InvaderCount++;
                            TotalPoints += invader.Point;

                            Invader.MovementCount = Invader.MovementInterval - InvaderStunDuration; // Delay for destroying an invader, and cooldown
                            Invader.ShootCount = Invader.MinimumTickBeforeShoot - InvaderStunDuration;

                            InvaderDestroyed?.Invoke(this, EventArgs.Empty); // sound effect

                            // Replace invader drawable with destroy sprite drawable
                            Drawables[j] = new DestroySprite(DestroyInvaderSprite, invader.LocX + (int)(invader.SpriteToDraw.SizeInPixels.Width / 2), invader.LocY + (int)(invader.SpriteToDraw.SizeInPixels.Height / 2), InvaderStunDuration);
                            break;
                        }

                    }

                    if (bullet.Update()) // if it goes out of map, delete
                    {
                        drawablesToErase.Add(bullet);
                    }
                }
                // end bullet collision and bound checking

                if (drawable is DestroySprite)
                {
                    if ((drawable as DestroySprite).Update()) // returns when time to delete
                    {
                        drawablesToErase.Add(drawable);

                        // Reset player finally
                    }
                }
            }

            foreach (var drawableToErase in drawablesToErase)
            {
                if (drawableToErase == PlayerSpaceship.PlayerBullet)
                {
                    PlayerSpaceship.PlayerBullet = null;
                }
                Drawables.Remove(drawableToErase);
            }
            // End of bullet collision logic. (collision), (out of bounds), and (destroySprite running out) and deletions;

            // Tick player is hit
            if (PlayerDestroySprite != null)
            {
                Drawables.RemoveAll(c => c as Bullet != null);
                return;
            }


            // Invader logic
            Invader.Update(Drawables, Invaders, BottomRowInvaders);
            // Invader logic end


            // Player movement and shooting logic
            PlayerSpaceship.Update(Drawables, BulletFired); // Keyboard input and gamepad logic in function
            // Player movement and shooting logic end

            // Creating UFO and setting random value to UFO points 
            if (UfoInvader == null)
            {
                UfoCount++;
                if (UfoSpawnTime <= UfoCount)
                {
                    UfoCount = 0;
                    UfoSpawnTime = 1200 + random.Next(600); // new spawn time
                    int points;
                    int n = random.Next(18);
                    if (n <= 6)
                    {
                        points = 50;
                    }
                    else if (n <= 11)
                    {
                        points = 100;
                    }
                    else if (n <= 14)
                    {
                        points = 150;
                    }
                    else if (n <= 16)
                    {
                        points = 200;
                    }
                    else
                    {
                        points = 300;
                    }
                    if (random.Next(2) == 0)
                    {
                        UfoInvader = new Invader(Edges.LEFT, Edges.TOP + 30, SpriteCollection.UfoSprite, SpriteCollection.UfoSprite, points);
                        UfoGoRight = true;
                    }
                    else
                    {
                        UfoInvader = new Invader(Edges.RIGHT - (int)SpriteCollection.UfoSprite.SizeInPixels.Width, Edges.TOP + 30, SpriteCollection.UfoSprite, SpriteCollection.UfoSprite, points);
                        UfoGoRight = false;
                    }
                    Drawables.Add(UfoInvader);

                    UfoSpawnSound?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                int speed = 1;
                int velocity = UfoGoRight ? speed : -1 * speed;
                UfoInvader.LocX += velocity;
            }

            // Update score and bullet progress
            UpdateUi?.Invoke(this, EventArgs.Empty);
        }

        // If the player beats the level and kills all invaders 
        public void WinLevel()
        {
            // Level equals next level in list or loops back to first when al levels are completed
            CurrentLevel = (Levels.FindIndex(c => c == CurrentLevel) + 1 != Levels.Count)
                ? Levels[Levels.FindIndex(c => c == CurrentLevel) + 1] : Levels.First();

            PlayerSpaceship.Health++;

            // Reset drawables and add player and invaders
            Drawables = new List<IDrawable> { PlayerSpaceship };
            
            // Resetting all game logic 
            Invader.IsTravelingRight = true;
            Invader.BulletSpeed = CurrentLevel.InvaderBulletSpeed;
            Invader.MinimumTickBeforeShoot = CurrentLevel.InvaderMinimumTickBeforeShoot;
            Invader.ShootCount = (Invader.MinimumTickBeforeShoot < 50) ? Invader.MinimumTickBeforeShoot - 50 : 0;  // For first invader shot, wait at least 50 ticks
            Invader.MovementHorizontalSpeed = CurrentLevel.InvaderHorizontalSpeed;
            Invader.MovementVerticalSpeed = CurrentLevel.InvaderVerticalSpeed;
            Invader.MovementInterval = CurrentLevel.InvaderMovementInterval;
            xGapOfOctopus = CurrentLevel.InvaderXGap;
            yGap = CurrentLevel.InvaderYGap;
            InvaderStunDuration = CurrentLevel.InvaderStunDuration;
            CreateInvader(Drawables, SpriteCollection, CurrentLevel.InvaderStartYPosition);

            // Resetting the the score 
            PlayerSpaceship.UpdateStats(CurrentLevel.PlayerBulletSpeed, CurrentLevel.PlayerMovementSpeed, CurrentLevel.PlayerBulletCooldown);
            PlayerSpaceship.Reset();
            UfoInvader = null;

        }
    }
}
