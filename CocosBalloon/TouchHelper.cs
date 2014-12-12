using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CocosSharp;

namespace CocosBalloon
{
	/// <summary>
	/// Touch helper class to do basic one by one touch hit detection.
	/// Optionally sends TouchesHeld (not provided by the framework),
	/// if an interval is provided. 
	/// *** DOES NOT ACCOUNT FOR SCALE OR OTHER TRANSFORMS ***
	/// </summary>
    public class TouchHelper<TNode> where TNode : CCNode
    {
        private TNode _node;
        public TNode Node { get { return this._node; } set { this._node = value; } }
        private TimeSpan _touchesHeldInterval;


        private Subject<CCTouch> _touchBegan = new Subject<CCTouch>();
        private Subject<CCTouch> _touchHeld = new Subject<CCTouch>();
        private Subject<CCTouch> _touchMoved = new Subject<CCTouch>();
        private Subject<CCTouch> _touchEnded = new Subject<CCTouch>();
        private Subject<CCTouch> _touchCancelled = new Subject<CCTouch>();

        public IObservable<CCTouch> TouchBegan { get; set; }
        public IObservable<CCTouch> TouchHeld { get; set; }
        public IObservable<CCTouch> TouchMoved { get; set; }
        public IObservable<CCTouch> TouchEnded { get; set; }
        public IObservable<CCTouch> TouchCancelled { get; set; }

        public TouchHelper(TNode targetNode, TimeSpan touchesHeldInterval = default(TimeSpan))
            : base()
        {
            this._node = targetNode;
            this._touchesHeldInterval = touchesHeldInterval;

            this.TouchBegan = _touchBegan.AsObservable();
            this.TouchHeld = _touchHeld.AsObservable();
            this.TouchMoved = _touchMoved.AsObservable();
            this.TouchEnded = _touchEnded.AsObservable();
            this.TouchCancelled = _touchCancelled.AsObservable();

            var tl = new CCEventListenerTouchOneByOne()
            {
                IsSwallowTouches = true,
                OnTouchBegan = (touch, evt) =>
                {
					// don't accept touches if invisible
                    if (this._node.Opacity == (byte)0)
                        return false;

					// little hack to check size of first child when a sprite is in a node and the bounding box was not updated
                    if (this._node.BoundingBoxTransformedToWorld.ContainsPoint(touch.Location)
                        || this._node.ChildrenCount > 0 && this._node.Children.First().BoundingBoxTransformedToWorld.ContainsPoint(touch.Location))
                    {
                        this._touchBegan.OnNext(touch);

						// pump 'touches held' messages if required
                        if (this._touchesHeldInterval > default(TimeSpan))
                        {
                            var holding = Observable.Interval(this._touchesHeldInterval)
                                .Subscribe(_ => this._touchHeld.OnNext(touch));

                            this.TouchEnded
                                .Merge(this.TouchCancelled)
                                .Take(1)
                                .Subscribe(_ => holding.Dispose());

                        }

			            return true;
                    }

					// let someone else handle the touch 
                    return false;
                },
                OnTouchMoved = (touch, evt) => this._touchMoved.OnNext(touch),
                OnTouchEnded = (touch, evt) => this._touchEnded.OnNext(touch),
                OnTouchCancelled = (touch, evt) => this._touchCancelled.OnNext(touch),
            };

            this._node.AddEventListener(tl, this._node);
        }
    }
}