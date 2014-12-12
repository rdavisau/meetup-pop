using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CocosSharp;

namespace CocosBalloon
{
    public static class CocoExtensions
    {
        private static Random _r = new Random();

        /// <summary>
        /// Execute an action before returning the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="action">The action to be performed</param>
        /// <returns></returns>
        public static T Do<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }

        /// <summary>
        /// Apply an action to each item in the List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void ForEach<T>(this List<T> items, Action<T> action)
        {
            foreach (var item in items)
                action(item);
        }

        /// <summary>
        /// Returns a task that will complete after the provided actions complete.
        /// Will not complete if the action list contains a CCRepeatForever.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="actions">The actions to perform</param>
        /// <returns></returns>
        public static Task<bool> RunActionsWithTask(this CCNode node, params CCFiniteTimeAction[] actions)
        {
            var t = new TaskCompletionSource<bool>();
            node.RunAction(new CCSequence(actions.Concat(new[] { new CCCallFunc(() => t.SetResult(true)) }).ToArray()));

            return t.Task;
        }

        /// <summary>
        /// Centers label text vertically and horizontally
        /// </summary>
        /// <typeparam name="TLabel"></typeparam>
        /// <param name="label"></param>
        /// <returns></returns>
        public static TLabel WithTextCentered<TLabel>(this TLabel label) where TLabel : CCLabel
        {
            label.HorizontalAlignment = CCTextAlignment.Center;
            label.VerticalAlignment = CCVerticalTextAlignment.Center;

            return label;
        }

        /// <summary>
        /// Applies the specified horizontal text aligment
        /// </summary>
        /// <typeparam name="TLabel"></typeparam>
        /// <param name="label"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        public static TLabel WithTextAlignment<TLabel>(this TLabel label, CCTextAlignment alignment) where TLabel : CCLabel
        {
            label.HorizontalAlignment = alignment;

            return label;
        }

        /// <summary>
        /// Draws a rectangle the size of the provided node's BoundingBox in the provided colour
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="colour"></param>
        /// <returns></returns>
        public static TNode FillWith<TNode>(this TNode node, CCColor3B colour) where TNode : CCDrawNode
        {
            node.Color = colour;
            node.DrawRect(node.BoundingBox);

            return node;
        }

        /// <summary>
        /// Loads a sprite into the provided CCnode 
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static TNode WithSprite<TNode>(this TNode node, string fileName)
            where TNode : CCNode
        {
            node.AddChild(new CCSprite(new CCTexture2D(fileName)));

            return node;
        }

        /// <summary>
        /// Places the target node within the provided parent node (or its existing node) according to the xPct and yPct factors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="xPct">A value from 0f to 1f (left to right) indicating how far along the x-axis the node should sit within the parent.</param>
        /// <param name="yPct">A value from 0f to 1f (top to bottom) indicating how far along the y-axis the node should sit within the parent.</param>
        /// <param name="parent">The parent node within which the target should sit. If null, the target must already have a parent.</param>
        /// <returns></returns>
        public static T PlaceAt<T>(this T node, double xPct, double yPct, CCNode parent = null) where T : CCNode
        {
            if (parent == null)
                parent = node.Parent.AssertNotNull("No parent container for node requiring PlaceAt");
            else
            {
                // check whether target already has a parent, if it does we need to remove it before placing in new parent
                if (node.Parent != null)
                    node.RemoveFromParent(false);

                parent.AddChild(node);
            }
            var parentSize = parent.BoundingBox.Size;

            var targetX = (float)(parentSize.Width * xPct);
            var targetY = (float)(parentSize.Height - (parentSize.Height * yPct));

            node.AnchorPoint = CCPoint.AnchorMiddle;

            node.Position = new CCPoint(targetX, targetY);

            return node;
        }

        /// <summary>
        /// Returns the target node with the provided actions set to repeat indefinitely
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
		public static TNode WithOngoingActions<TNode>(this TNode node, params CCFiniteTimeAction[] actions) where TNode : CCNode
		{
			var repeat = new CCRepeatForever (actions);
			node.RunAction (repeat);

			return node;
		}

        /// <summary>
        /// Returns the target node with a TouchHelper applied that can be configured via the configure parameter
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="configure">Provides and opportunity to subscribe to the TouchHelper events</param>
        /// <param name="touchHeldNotifyInterval">The interval at which the TouchHeld events should be fired</param>
        /// <returns></returns>
        public static TNode WithTouchHelper<TNode>(this TNode node, Action<TouchHelper<TNode>> configure, TimeSpan touchHeldNotifyInterval = default(TimeSpan)) where TNode : CCNode
        {
            var helper = new TouchHelper<TNode>(node, touchHeldNotifyInterval);
            configure(helper);

            return node;
        }

        /// <summary>
        /// Returns the target node after attaching a TouchHelper that peforms the provided action on TouchBegan
        /// </summary>
        /// <typeparam name="TNode"></typeparam>
        /// <param name="node"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TNode OnTapped<TNode>(this TNode node, Action<TNode> action) where TNode : CCNode
        {
            var helper = new TouchHelper<TNode>(node);
            helper.TouchBegan.Subscribe(_ => action(node));

            return node;
        }

        /// <summary>
        /// Return a new CCSize with the dimensions of factor * the target CCSize
        /// </summary>
        /// <param name="size"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static CCSize MultiplyBy(this CCSize size, float factor)
        {
            return new CCSize(size.Width * factor, size.Height * factor);
        }

        /// <summary>
        /// Return a new CCSize with the dimensions of xFactor * the target CCSize width, yFactor * the target CCSize height
        /// </summary>
        /// <param name="size"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static CCSize MultiplyBy(this CCSize size, float xFactor = 1, float yFactor = 1)
        {
            return new CCSize(size.Width * xFactor, size.Height * yFactor);
        }

        /// <summary>
        /// Returns target after asserting not null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static T AssertNotNull<T>(this T obj, string msg = null)
        {
            Debug.Assert(obj != null, msg ?? "AssertNotNull failed");
            return obj;
        }

        /// <summary>
        /// Randomly applies a variation within the provided percentage to the target value
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pct">The percentage at which to vary the value in both directions. To vary by +- 10%, provide .1f.</param>
        /// <returns></returns>
        public static float VaryBy(this float f, float pct)
        {
            var modifier = _r.NextFloat()*pct*2 - (pct*2);
            return f* (1+modifier);
        }

        /// <summary>
        /// Clamps the provided value to within the provided range
        /// </summary>
        /// <param name="f"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Between(this float f, float min, float max)
        {
            return Math.Max(Math.Min(max, f), min);
        }

        /// <summary>
        /// Returns a random float between 0f and 1f;
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static float NextFloat(this Random r)
        {
            return (float)r.NextDouble();
        }

        /// <summary>
        /// Returns true if the provided value is positive
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool IsPositive(this float f)
        {
            return f >= 0;
        }

        /// <summary>
        /// Returns true if the provided value is negative
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static bool IsNegative(this float f)
        {
            return !IsPositive(f);
        }

        /// <summary>
        /// Returns true if the provided values have different signs
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool HasDifferentSignTo(this float f1, float f2)
        {
            return !f1.HasSameSignAs(f2);
        }

        /// <summary>
        /// Returns true if the provided values have the same sign (both positive, both negative)
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <returns></returns>
        public static bool HasSameSignAs(this float f1, float f2)
        {
            return (f1.IsPositive() && f2.IsPositive()) || (f1.IsNegative() && f2.IsNegative());
        }

    }
}