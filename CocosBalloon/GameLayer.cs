using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CocosDenshion;
using CocosSharp;

namespace CocosBalloon
{
    public class GameLayer : CCLayerGradient
    {
        private int _currWave;  // current 'wave' number that we are on
        private float _currTimeToReachTop; // current duration for a balloon to reach the top of the screen, 
        private int _maxAtOnce; // current max number of balloons that can be launched
        
        private int increaseMaxBalloonCountEvery = 5; // increase ballon count after n waves
        private float durationMultiplier = .975f; // reduces _currTimeToReachTop each wave
        
        private int _numLives;  // current lives left
        private int _currScore; // current score
        private bool _gameOver; // is game over

        private float _maxLivesAchieved; // highest number of lives the player has had

        private CCLabel _scoreLabel;
        private CCLabel _livesLabel;

        private CCParticleSun _sun;
        private CCParticleRain _rain;

        List<CCTexture2D> PeopleTextures { get; set; }

        public static new CCScene Scene(List<CCTexture2D> textures, CCWindow window)
        {
            var hl = new GameLayer(textures);
            var scene = new CCScene(window);
            scene.AddChild(hl);

            return scene;
        }

        public GameLayer(List<CCTexture2D> textures)
        {
            this.PeopleTextures = textures;
            _currWave = 1;
            _numLives = 5;
            _maxLivesAchieved = _numLives;
            _currTimeToReachTop = 2f;
            _maxAtOnce = 2;
            _currScore = 0;
            _gameOver = false;
        }

        protected async override void AddedToScene()
        {
            base.AddedToScene();

            // add sun and rain particle effects
            var topOfscreen = this.VisibleBoundsWorldspace.Center.Offset(0f, this.VisibleBoundsWorldspace.MaxY/2f);
            _sun = new CCParticleSun(topOfscreen.Offset(0f, 20f));
            _rain = new CCParticleRain(topOfscreen) { Scale = 0 };

            this.AddChild(_sun);
            this.AddChild(_rain);

            SetBackgroundColour();
            
            // init labels
            _scoreLabel = new CCLabel(String.Format("Score: {0}", _currScore), "Consolas", 18f) { Color = CCColor3B.Black }
                .PlaceAt(.1f, .05f, this)
                .WithTextAlignment(CCTextAlignment.Left);

            _livesLabel = new CCLabel(String.Format("Lives: {0}", _numLives), "Consolas", 18f) { Color = CCColor3B.Black }
                .PlaceAt(1f - .1f, .05f, this)
                .WithTextAlignment(CCTextAlignment.Right);

            // track game over state
            var enteredGameOver = false;
            var completedGameOver = false;

            // launch waves of balloons while not game over
            var r = new Random();
            while (!(enteredGameOver && completedGameOver))
            {
                if (_gameOver)
                    enteredGameOver = true;

                // wait before launching next set of balloons
                if(!_gameOver)
                    await Task.Delay(TimeSpan.FromSeconds(_currTimeToReachTop));

                // determine number of balloons to fire
                // if game over, fire heaps for effect
                var numBalloons = !_gameOver ? r.Next(1, _maxAtOnce + 1) : 500;

                // generate the balloons
                var balloons = Enumerable.Range(0, numBalloons)
                    .Select(_ =>
                    {
                        // pick a texture at random for the new balloon
                        var randomIndex = r.Next(0, this.PeopleTextures.Count - 1);
                        var randomTexture = this.PeopleTextures[randomIndex];

                        // create sprite with touch handler 
                        return new CCSprite(randomTexture)
                            .WithTouchHelper(h => h.TouchBegan
                                                    .Where(__=> !_gameOver)                        
                                                    .Subscribe(___ => BalloonPopped(h)));
                    }).ToList();


                // launch the balloons from the bottom of the screen
                var i = 0;
                balloons.ForEach(async b =>
                {
                    // wait a little so that each balloons are slightly staggered
                    await Task.Delay(TimeSpan.FromMilliseconds(50 * (++i)));

                    // place under the screen
                    var randomXPos = r.NextFloat().Between(.2f, .8f);

                    b.PlaceAt(randomXPos, 1.1f, this);

                    // create the launch action
                    var timeToReachTop = _currTimeToReachTop.VaryBy(.15f); // use current duration with some variability
                    var targetPoint = new CCPoint(b.PositionX, b.VisibleBoundsWorldspace.MaxY + 50); // move to a point above the top of the screen;
                    
                    var moveToTop = new CCMoveTo(timeToReachTop, targetPoint);

                    // create the fail action; this runs after the balloon reaches the top of the screen 
                    // if the move action completes then this ballon was not tapped in time
                    var failAction = new CCCallFuncN(MissedBalloon);

                    // combine the move and fail in sequence
                    var seq = new CCSequence(moveToTop, failAction);

                    // launch the balloon
                    b.RunAction(seq);
                });

                if (_gameOver)
                    completedGameOver = true;
                
                // after each round, increase the difficulty by reducing the time 
                // for balloons to reach the top
                _currTimeToReachTop = _currTimeToReachTop * durationMultiplier;
                _currWave++;

                // if there have been enough waves, increase max balloon count 
                if (_currWave % increaseMaxBalloonCountEvery == 0)
                    _maxAtOnce = (int)(_maxAtOnce + 1);
            }

        }

        private async void BalloonPopped(TouchHelper<CCSprite> h)
        {
            var poppedBalloon = h.Node;

            // points scored relative to the speed of the balloons
            _currScore += (int)(2000f - _currTimeToReachTop * 1000f);
            _scoreLabel.Text = String.Format("Score: {0:N0}", _currScore);

            // get an extra life each success
            _numLives += 1;
            _livesLabel.Text = String.Format("Lives: {0}", _numLives);

            // update the background colour
            SetBackgroundColour();

            // stop the balloon from moving
            poppedBalloon.StopAllActions();

            // make it spin away
            poppedBalloon.RunAction(new CCRepeatForever(new CCRotateBy(.1f, 360f)));
            await poppedBalloon.RunActionsWithTask(new CCScaleTo(.5f, 0f));

            //cleanup
            poppedBalloon.RemoveFromParent();
        }

        private void MissedBalloon(CCNode balloon)
        {
            // if already game over, don't worry
            if (_gameOver)
                return;

            // decrement lives and update label
            _numLives -= 1;
            _livesLabel.Text = String.Format("Lives: {0}", _numLives);

            // update the background colour
            SetBackgroundColour();

            // if we are out of lives, it's game over time
            if (_numLives <= 0)
                GameOver();

            // remove the balloon from the game
            balloon.RemoveFromParent();
        }

        

        private async void GameOver()
        {
            _gameOver = true;

            UpdateSun();
            UpdateRain();

            // switch the colour of the score labels against dark background
            this._scoreLabel.Color = CCColor3B.White;
            this._livesLabel.Color = CCColor3B.White;

            // red means bad
            this.StartColor = new CCColor3B(70, 0, 0);
            this.EndColor = new CCColor3B(10, 0, 0);

            // add background panel and game over label
            var container = new CCNode
            {
                ContentSize = new CCSize(.85f*this.VisibleBoundsWorldspace.MaxX, .25f*this.VisibleBoundsWorldspace.MaxY),
            }.PlaceAt(.5f, .5f, this);

            var gameOverBG = new CCDrawNode() { ContentSize = container.ContentSize }
                .FillWith(CCColor3B.DarkGray)
                .PlaceAt(.5f, .5f, container);

            var gameOverLabel = new CCLabel("GAME OVER", "Arial", 48f)
                .PlaceAt(.5f, .5f, container)
                .WithTextCentered();

            var tapToRestartLabel = new CCLabel("[tap to restart]", "Arial", 12f) { Opacity = 0 }
                .PlaceAt(.5f, .7f, container)
                .WithTextCentered();

            // blink the restart label
            tapToRestartLabel.RunAction(new CCRepeatForever(new CCBlink(1f, 1)));

            gameOverBG.ZOrder = 998;
            tapToRestartLabel.ZOrder = 999;
            gameOverLabel.ZOrder = 1000;
            container.ZOrder = 1001;

            // wait a few seconds before adding the touch listener so that you don't tap out of the game before seeing game over
            await Task.Delay(2000);

            tapToRestartLabel.Opacity = (byte) 255;

            // add touch listener, restart game on touch
            var touchListener = new CCEventListenerTouchAllAtOnce()
            {
                OnTouchesBegan = (touches, args) =>
                {
                    // make sure the while loop terminates

                    // reload this scene 
                    this.Director.ReplaceScene(new CCTransitionRotoZoom(.5f, GameLayer.Scene(this.PeopleTextures, this.Window)));
                    this.RemoveAllChildren(true);
                }
            };
            
            this.AddEventListener(touchListener);
        }

        private void SetBackgroundColour()
        {
            var startVal = 150;
            var endVal = 50;

            var greenStart = (byte)(_numLives / _maxLivesAchieved * startVal);
            var greenEnd = (byte)(_numLives / _maxLivesAchieved * endVal);

            this.StartColor = new CCColor3B(0, greenStart, 0);
            this.EndColor = new CCColor3B(0, greenEnd, 0);

            UpdateSun();
            UpdateRain();
        }

        private void UpdateSun()
        {
            var sunScale = (_numLives/_maxLivesAchieved)*10f;
            _sun.StopAllActions();
            _sun.RunAction(new CCScaleTo(.2f, sunScale));
        }

        private void UpdateRain()
        {
            if (_numLives / _maxLivesAchieved < .4f)
            {
                _rain.Scale = 1;
                _rain.EmissionRate *= 2f;
                _rain.Speed *= 2f;
            }
            else
                _rain.Scale = 0;
        }
    }
}