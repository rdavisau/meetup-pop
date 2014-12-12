using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CocosBalloon.Core;
using CocosSharp;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CocosBalloon
{
    public class IntroLayer : CCLayer
    {
        public static new CCScene Scene(CCWindow window)
        {
            var introLayer = new IntroLayer();
            var scene = new CCScene(window);
            scene.AddChild(introLayer);

            return scene;
        }

        protected async override void AddedToScene()
        {
            base.AddedToScene();

            // add a title label
            var titleLabel = new CCLabel("Balloon Pop!", "arial", 36f)
                .PlaceAt(.5f, .35f, this)
                .WithTextCentered();

            // add a status label 
            var statusLabel = new CCLabel("Loading..", "arial", 24f)
                .PlaceAt(.5f, .7f, this)
                .WithTextCentered();

			// add a spinner
			var spinner = new CCLabel ("+", "arial", 48f)
				.PlaceAt (.5f, .75f, this)
				.WithTextCentered ()
				.WithOngoingActions (new CCRotateBy (.1f, 180f));

			// add an error label, initially invisible
			var errorLabel = new CCLabel ("", "arial", 18f) { Opacity = 0, Color = CCColor3B.Red }
				.PlaceAt (.5f, .85f, this)
				.WithTextCentered ();

			errorLabel.AnchorPoint = CCPoint.AnchorMiddle;

			List<CCTexture2D> textures = null;
			while (textures == null) 
			{
				// show the spinner
				spinner.RunAction (new CCScaleTo (.5f, 1f));

				try { 
				        var validRsvpImageUrls = await GetValidRsvpImageUrls();
							
						// download the photos
						var imageStreams = await Task.Run(() =>
							validRsvpImageUrls
							.Select(url => new MemoryStream(new HttpClient().GetByteArrayAsync(url).Result))
							.ToList());

						// have to load textures on the main thread
						textures = imageStreams.Select(img => new CCTexture2D(img)).ToList();

				} catch (Exception e) { errorLabel.Text = String.Format ("There was an error getting meetup data: {0}\n\n, Tap to retry", e.Message); }

				// hide the spinner;
				spinner.RunAction (new CCScaleTo (.5f, 0f));

				// if no textures, there was an error. show and let user tap to retry
				if (textures == null) {
					errorLabel.Opacity = 255;
					var labelTapped = new TaskCompletionSource<bool> ();

					errorLabel.RemoveAllListeners ();
					errorLabel.OnTapped(_=> labelTapped.SetResult(true));

					await labelTapped.Task;

					errorLabel.Opacity = 0;
				}

			}

            // place images on the screen them around the screen
            PlaceImages(textures);

            // show suprise banner
            ShowSurpriseBanner();

            // ready to play
            statusLabel.Text = "TAP TO PLAY!";
            statusLabel.ZOrder = 100;

            statusLabel.RunAction(new CCRepeatForever(new CCBlink(1f, 1)));

            // add touch listener to proceed to game scene
            var touchListener = new CCEventListenerTouchAllAtOnce()
            {
                OnTouchesBegan = (touches, args) =>
                {
                    titleLabel.RemoveFromParent();

                    var gameScene = GameLayer.Scene(textures, this.Window);
                    var transition = new CCTransitionRotoZoom(.5f, gameScene);

                    this.Director.PushScene(transition);
                }
            };

            this.AddEventListener(touchListener);
        }

        private void ShowSurpriseBanner()
        {
            var container = new CCNode()
            {
                ContentSize = new CCSize(this.VisibleBoundsWorldspace.MaxX*.75f, this.VisibleBoundsWorldspace.MaxY*.1f),
                Scale = 5f,
                Rotation = -22.5f,
            }.PlaceAt(.5f, .35f, this);

            var filledBanner = new CCDrawNode() {ContentSize = container.ContentSize}
                .FillWith(CCColor3B.Red)
                .PlaceAt(.5f, .5f, container);

            var newLabel = new CCLabel("MEETUP POP", "consolas", 48f)
                .WithTextCentered()
                .PlaceAt(.5f, .5f, container);

            // bounce into the screen
            container.ZOrder = 1000;
            container.RunAction(new CCEaseBounceOut(new CCScaleTo(1f, 1f)));
        }

        private void PlaceImages(List<CCTexture2D> textures)
        {
            var r = new Random();
            var i = 0;
            textures.ForEach(async tex =>
            {
                // stagger the appearence of images
                await Task.Delay(TimeSpan.FromMilliseconds(i++ * 50f));

                // place the sprite on the screen at at random location, starting scaled to zero
                var sprite = new CCSprite(tex) { Scale = 0f }
                    .PlaceAt(r.NextFloat(), r.NextFloat()*.6, this);

                // scale up to normal size
                await sprite.RunActionsWithTask(new CCEaseOut(new CCScaleTo(.25f, 1f), .3f));

                // shrink and grow forever
                var duration = .75f.VaryBy(.1f);
                var grow = new CCScaleTo(duration, 1.5f);
                var shrink = new CCScaleTo(duration, 1f);
                var forever = new CCRepeatForever(grow, shrink);

                sprite.RunAction(forever);
            });
        }

        private static async Task<List<string>> GetValidRsvpImageUrls()
        {
            // if the api key is not set, use cached list
            if (String.IsNullOrEmpty(MeetupConfig.ApiKey))
                return MeetupConfig.CachedImageUrls;
            
			var url = String.Format("https://api.meetup.com/2/rsvps?&sign=true&photo-host=public&event_id={0}&page=100&key={1}", MeetupConfig.MeetupEventId, MeetupConfig.ApiKey);

			var rsvpsData =	await new HttpClient ().GetStringAsync (url);

			var validRsvps = JsonConvert.DeserializeObject<MeetupRSVPResponse> (rsvpsData)
                .results
                .Where (rsvp => rsvp.member_photo != null && !String.IsNullOrEmpty (rsvp.member_photo.thumb_link))
				.Select (rsvp => rsvp.member_photo.thumb_link)
                .ToList();

            return validRsvps;
        }
    }
}