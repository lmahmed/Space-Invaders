using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.UI.Xaml.Controls;

namespace Space_Invaders
{
    // Collection of photos for all the sprites used
    public class SpriteCollection
    {
        public CanvasBitmap PlayerSprite;

        public CanvasBitmap SquidSprite1;
        public CanvasBitmap SquidSprite2;

        public CanvasBitmap CrabSprite1;
        public CanvasBitmap CrabSprite2;

        public CanvasBitmap OctopusSprite1;
        public CanvasBitmap OctopusSprite2;

        public CanvasBitmap UfoSprite;

        public SpriteCollection(CanvasBitmap playerSprite, CanvasBitmap squidSprite1, CanvasBitmap squidSprite2, CanvasBitmap crabSprite1, CanvasBitmap crabSprite2, CanvasBitmap octopusSprite1, CanvasBitmap octopusSprite2, CanvasBitmap ufoSprite) 
        {
            PlayerSprite = playerSprite;
            SquidSprite1 = squidSprite1;
            SquidSprite2 = squidSprite2;
            CrabSprite1 = crabSprite1;
            CrabSprite2 = crabSprite2;
            OctopusSprite1 = octopusSprite1;
            OctopusSprite2 = octopusSprite2;
            UfoSprite = ufoSprite;
        }
    }

    public class UfoScoreCollection
    {
        public CanvasBitmap FiftyScore;
        public CanvasBitmap HundredScore;
        public CanvasBitmap HundredFiftyScore;
        public CanvasBitmap TwoHundredScore;
        public CanvasBitmap ThreeHundredScore;

        public UfoScoreCollection(CanvasBitmap fiftyScore, CanvasBitmap hundredScore, CanvasBitmap hundredFiftyScore, CanvasBitmap twoHundredScore, CanvasBitmap threeHundredScore)
        {
            FiftyScore = fiftyScore;
            HundredScore = hundredScore;
            HundredFiftyScore = hundredFiftyScore;
            TwoHundredScore = twoHundredScore;
            ThreeHundredScore = threeHundredScore;
        }
    }

    public class FinalScore
    {
        public string Name;
        public int Score;

        public FinalScore(string name, int score) 
        {
            Name = name;
            Score = score;
        }

        public override string ToString()
        {
            return $"{Name}:{Score}";
        }
    }
}
