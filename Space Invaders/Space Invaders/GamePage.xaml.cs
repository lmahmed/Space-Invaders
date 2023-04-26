using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Space_Invaders
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {

        public int x = 0;
        public int y = 495;
        public SpaceInvaders SpaceInvaders;

        public CanvasBitmap PlayerHeart;

        private int LastScore;

        public List<MediaElement> PlayerShootElements;
        public List<MediaElement> InvaderDestroyElements;
        public List<Image> PlayerHealthImage;
        public List<Image> ScoreNumbers;
        public List<Image> BulletLoadList;
        public List<FinalScore> FinalScoreList;
        public List<TextBlock> ScoreBlocks;
        public StorageFile ScoreFile;

        public GamePage()
        {
            this.InitializeComponent();

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetDesiredBoundsMode(Windows.UI.ViewManagement.ApplicationViewBoundsMode.UseCoreWindow);
            Play_Again.Visibility = Visibility.Collapsed;
            Home_Page.Visibility = Visibility.Collapsed;
            //NameTextBox.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;

            Window.Current.CoreWindow.KeyDown += Canvas_KeyDown;
            Window.Current.CoreWindow.KeyUp += Canvas_KeyUp;

            LastScore = 0;

            PlayerShootElements =
                new List<MediaElement>() { PlayerShoot1, PlayerShoot2, PlayerShoot3, PlayerShoot4, PlayerShoot5, PlayerShoot6, PlayerShoot7, PlayerShoot8, PlayerShoot9
                , PlayerShoot10, PlayerShoot11, PlayerShoot12, PlayerShoot13, PlayerShoot14, PlayerShoot15, PlayerShoot16, PlayerShoot17, PlayerShoot18, PlayerShoot19, PlayerShoot20 };

            InvaderDestroyElements = new List<MediaElement>() { InvaderDestroyed1, InvaderDestroyed2, InvaderDestroyed3, InvaderDestroyed4, InvaderDestroyed5
                , InvaderDestroyed6, InvaderDestroyed7, InvaderDestroyed8, InvaderDestroyed9, InvaderDestroyed10 };

            PlayerHealthImage = new List<Image>() { PlayerHealth1, PlayerHealth2, PlayerHealth3, PlayerHealth4, PlayerHealth5, PlayerHealth6 };

            ScoreNumbers = new List<Image>() { Score1, Score2, Score3, Score4, Score5, Score6 };

            BulletLoadList = new List<Image>() { BulletLoad1, BulletLoad2, BulletLoad3, BulletLoad4, BulletLoad5, BulletLoad6, BulletLoad7, BulletLoad8, BulletLoad9, BulletLoad10 };

            ScoreBlocks = new List<TextBlock>() { HighScore1, HighScore2, HighScore3, HighScore4, HighScore5, HighScore6 };

            // File stuff
            FinalScoreList = new List<FinalScore>();
            File.Open("Scores.txt", FileMode.Open, FileAccess.Read);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            ScoreFile = localFolder.CreateFileAsync("Scores.txt", CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();

            var fileData = FileIO.ReadLinesAsync(ScoreFile).GetAwaiter().GetResult().ToList();
            FinalScoreList = fileData.Select(fd =>
            {
                var splitData = fd.Split(':');
                return new FinalScore(splitData[0], int.Parse(splitData[1]));
            }).ToList();

            UpdateHighScore();
        }

        public void UpdateHighScore()
        {
            for (int i = 0; i < FinalScoreList.Count && i < ScoreBlocks.Count; i++) 
            {
                ScoreBlocks[i].Text = FinalScoreList[i].ToString();
            }
        }

        private void Canvas_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            SpaceInvaders.Draw(args.DrawingSession);

        }
        private void Canvas_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            SpaceInvaders.Update();
        }

        // If the down key is pressed by player
        private void Canvas_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (e.VirtualKey == Windows.System.VirtualKey.Left)
            {
                SpaceInvaders.PlayerSpaceship.IsTravelingLeft= true;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Right)
            {
                SpaceInvaders.PlayerSpaceship.IsTravelingRight = true;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Space)
            {
                SpaceInvaders.PlayerSpaceship.IsShooting = true;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.C)
            {
                // toggle of using interval system or just one bullet at a time
                SpaceInvaders.PlayerSpaceship.BulletIntervalSystemUsed = !SpaceInvaders.PlayerSpaceship.BulletIntervalSystemUsed;
            }

        }
        
        // If the up key is pressed by player
        private void Canvas_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (e.VirtualKey == Windows.System.VirtualKey.Left)
            {
                SpaceInvaders.PlayerSpaceship.IsTravelingLeft = false;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Right)
            {
                SpaceInvaders.PlayerSpaceship.IsTravelingRight = false;
            }
            else if (e.VirtualKey == Windows.System.VirtualKey.Space)
            {
                SpaceInvaders.PlayerSpaceship.IsShooting = false;
            }
        }

        private void Canvas_CreateResources(CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResources(sender).AsAsyncAction());
        }

        private void BulletFired(object sender, EventArgs e)
        {
            var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                var player = PlayerShootElements.FirstOrDefault(f => f.CurrentState != MediaElementState.Playing);
                if (player != null)
                {
                    player.Play();
                }
            });
        }

        private void InvaderDestroyed(object sender, EventArgs e)
        {
            var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                var player = InvaderDestroyElements.FirstOrDefault(f => f.CurrentState != MediaElementState.Playing);
                if (player != null)
                {
                    player.Play();
                }
            });
        }
        private void UpdateUi(object sender, EventArgs e) // Specifically score and player shoot bar if on interval mode
        {
           SpaceInvaders spaceInvaders = sender as SpaceInvaders;
           var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {

                if (LastScore != (sender as SpaceInvaders).TotalPoints)
                {
                    LastScore = (sender as SpaceInvaders).TotalPoints;
                    string totalScore = (LastScore != 0) ? LastScore.ToString() : "000000";
                    for (int i = ScoreNumbers.Count - 1, j = totalScore.Length - 1; i >= 0 && j >= 0; i--, j--)
                    {
                        ScoreNumbers[i].Source = new BitmapImage(new Uri($"ms-appx:///Assets/Aspect Ratio and Trimmed Sprites/{totalScore[j]}Number.png"));
                    }
                }

                if (spaceInvaders.PlayerSpaceship.BulletIntervalSystemUsed)
                {
                    BulletLoadList.ForEach(c => c.Visibility = Visibility.Collapsed);                   
                    int ratio = (int)Math.Round((((double)spaceInvaders.PlayerSpaceship.BulletCooldown) / spaceInvaders.PlayerSpaceship.BulletInterval * BulletLoadList.Count),MidpointRounding.AwayFromZero);
                    for(int i = 0; i < BulletLoadList.Count && i <= ratio; i++)
                    {
                        BulletLoadList[i].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    BulletLoadList.ForEach(c => c.Visibility = Visibility.Collapsed );
                }


            });
        }

        private void UpdateHealth(object sender, EventArgs e)
        {
            int healthCount   = (sender as PlayerSpaceship).Health;  // get health
            var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                PlayerHealthImage.ForEach(c => c.Visibility = Visibility.Collapsed);   

                for (int i = 0; i < healthCount && i < PlayerHealthImage.Count; i++)
                {
                    PlayerHealthImage[i].Visibility = Visibility.Visible;
                }
            });
        }
        private void PlayerHit(object sender, EventArgs e)
        {
            var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                PlayerHitMedia.Play();
            });
        }
        private void UfoSpawn(object sender, EventArgs e)
        {
            var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                UfoSpawnMedia.Play();
            });
        }

        private void UfoHit(object sender, EventArgs e)
        {
           var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                UfoHitMedia.Play();
            });
        }
        private void GameOver(object sender, EventArgs e)
        {
            var ignored = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                BackgroundMusic.Pause();
                GameOverMedia.Play();

                Play_Again.Visibility = Visibility.Visible;
                Home_Page.Visibility = Visibility.Visible;
                NameTextBox.Visibility = Visibility.Visible;
                AddButton.Visibility = Visibility.Visible;
            });
        }

        async Task CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender)
        {
            CanvasBitmap PlayerSprite = await CanvasBitmap.LoadAsync(sender, "Assets/playersprite2_40x45.png");

            CanvasBitmap SquidSprite1 = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/thirtypoint1_24x24.png");
            CanvasBitmap SquidSprite2 = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/thirtypoint2_24x24.png");

            CanvasBitmap CrabSprite1 = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/twentypoint1_30x22.png");
            CanvasBitmap CrabSprite2 = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/twentypoint2_30x22.png");

            CanvasBitmap OctopusSprite1 = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/tenpoint1_32x21.png");
            CanvasBitmap OctopusSprite2 = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/tenpoint2_32x21.png");

            CanvasBitmap UfoSprite = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/specialpoint_36x17.png");

            CanvasBitmap DestroyInvaderSprite = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/invaderdestroyed_30x23.png");

            SpriteCollection spriteCollection = new SpriteCollection(PlayerSprite, SquidSprite1, SquidSprite2, CrabSprite1,CrabSprite2,OctopusSprite1,OctopusSprite2,UfoSprite);

            CanvasBitmap FiftyScore =  await CanvasBitmap.LoadAsync(sender, "Assets/+50.png");
            CanvasBitmap HundredScore = await CanvasBitmap.LoadAsync(sender, "Assets/+100.png");
            CanvasBitmap HundredFiftyScore = await CanvasBitmap.LoadAsync(sender, "Assets/+150.png");
            CanvasBitmap TwoHundredScore = await CanvasBitmap.LoadAsync(sender, "Assets/+200.png");
            CanvasBitmap ThreeHundredScore = await CanvasBitmap.LoadAsync(sender, "Assets/+300.png");

            UfoScoreCollection ufoScoreCollection = new UfoScoreCollection(FiftyScore, HundredScore, HundredFiftyScore, TwoHundredScore, ThreeHundredScore);

            PlayerHeart = await CanvasBitmap.LoadAsync(sender, "Assets/Aspect Ratio and Trimmed Sprites/playerhealthheart_35x35.png");

            List<CanvasBitmap> playerGifHit = new List<CanvasBitmap>();
            for (int i = 1; i <= 12; i++)
            {
                playerGifHit.Add(await CanvasBitmap.LoadAsync(sender, $"Assets/Player Hit Gif/playergif{i}.gif"));
            }


            SpaceInvaders = new SpaceInvaders(spriteCollection, DestroyInvaderSprite, playerGifHit, ufoScoreCollection);
            SpaceInvaders.BulletFired += BulletFired;
            SpaceInvaders.InvaderDestroyed += InvaderDestroyed;
            SpaceInvaders.UpdateUi += UpdateUi;
            SpaceInvaders.UfoSpawnSound += UfoSpawn;
            SpaceInvaders.UfoHitSound += UfoHit;
            SpaceInvaders.PlayerHitSound += PlayerHit;
            SpaceInvaders.GameOver += GameOver;

            SpaceInvaders.PlayerSpaceship.PlayerHealth += UpdateHealth;
            UpdateHealth(SpaceInvaders.PlayerSpaceship,EventArgs.Empty); // Update health once after subscription

        }

        private void Play_Again_Click(object sender, RoutedEventArgs e)
        {
            SpaceInvaders = new SpaceInvaders(SpaceInvaders.SpriteCollection, SpaceInvaders.DestroyInvaderSprite, SpaceInvaders.PlayerHitGif, SpaceInvaders.UfoScoreCollection);
            SpaceInvaders.BulletFired += BulletFired;
            SpaceInvaders.InvaderDestroyed += InvaderDestroyed;
            SpaceInvaders.UpdateUi += UpdateUi;
            SpaceInvaders.UfoSpawnSound += UfoSpawn;
            SpaceInvaders.UfoHitSound += UfoHit;
            SpaceInvaders.PlayerHitSound += PlayerHit;
            SpaceInvaders.GameOver += GameOver;

            SpaceInvaders.PlayerSpaceship.PlayerHealth += UpdateHealth;
            UpdateHealth(SpaceInvaders.PlayerSpaceship, EventArgs.Empty); // Update health once after subscription

            BackgroundMusic.Position = TimeSpan.Zero;
            BackgroundMusic.Play();

            Play_Again.Visibility = Visibility.Collapsed;
            Home_Page.Visibility = Visibility.Collapsed;
            NameTextBox.Text = string.Empty;
            NameTextBox.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;
        }

        private void Home_Page_Click(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NameTextBox.MaxHeight = NameTextBox.ActualHeight;
            NameTextBox.MaxWidth = NameTextBox.ActualWidth;
            NameTextBox.MaxLength = 8;

            NameTextBox.Visibility = Visibility.Collapsed;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            if (string.IsNullOrEmpty(name)) 
            {
                return;
            }
            int points = SpaceInvaders.TotalPoints;

            FinalScore newScore = new FinalScore(name, points);
            if (FinalScoreList.Count == 0)
            {
                FinalScoreList.Add(newScore);
            }
            else if (newScore.Score > FinalScoreList.First().Score)
            {
                FinalScoreList.Insert(0, newScore);
            }
            else if (newScore.Score <= FinalScoreList.Last().Score)
            {
                FinalScoreList.Add(newScore);
            }
            else
            {
                for (int i = 0; i < FinalScoreList.Count - 1; i++)
                {
                    if (newScore.Score <= FinalScoreList[i].Score && newScore.Score > FinalScoreList[i + 1].Score)
                    {
                        FinalScoreList.Insert(i + 1, newScore);
                        break;
                    }
                }
            }


            FileIO.WriteLinesAsync(ScoreFile, FinalScoreList.Select(fs => fs.ToString())).GetAwaiter().GetResult();

            NameTextBox.Text = string.Empty;
            NameTextBox.Visibility = Visibility.Collapsed;
            AddButton.Visibility = Visibility.Collapsed;

            // Update Leaderboard
            UpdateHighScore();
        }

    }
}
