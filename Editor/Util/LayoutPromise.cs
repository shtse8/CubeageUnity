using System;
using System.Linq.Expressions;

namespace Cubeage.Avatar.Editor.Util
{
    public class LayoutPromise<TValue>
    {

        private TValue OldValue { get; set; }
        private TValue NewValue { get; set; }

        public LayoutPromise(TValue oldValue, Func<TValue, TValue> func)
        {
            OldValue = oldValue;
            NewValue = func(OldValue);
        }


        public LayoutPromise<TValue> OnChanged(Action<TValue, TValue> action)
        {
            if (!Equals(NewValue, OldValue))
                action(NewValue, OldValue);
            return this;
        }

        public LayoutPromise<TValue> OnChanged(Action<TValue> action)
        {
            return OnChanged((x, _) => action(x));
        }

        public LayoutPromise<TValue> OnChanged(Action action)
        {
            return OnChanged(_ => action());
        }
    }
    public class LayoutPromise<TTarget, TValue>
    {

        private TTarget Target { get; }
        private Expression<Func<TTarget, TValue>> Expression { get; }
        private TValue OldValue { get; set; }
        private TValue NewValue { get; set; }

        public LayoutPromise(TTarget target, Expression<Func<TTarget, TValue>> expression, Func<TValue, TValue> func)
        {
            Target = target;
            Expression = expression;
            OldValue = target.GetValue(expression);
            NewValue = func(OldValue);
        }


        public LayoutPromise<TTarget, TValue> OnChanged(Action<TValue, TValue> action)
        {
            if (!NewValue.Equals(OldValue))
                action(NewValue, OldValue);
            return this;
        }

        public LayoutPromise<TTarget, TValue> OnChanged(Action<TValue> action)
        {
            return OnChanged((x, _) => action(x));
        }

        public LayoutPromise<TTarget, TValue> OnChanged(Action action)
        {
            return OnChanged(_ => action());
        }

        public LayoutPromise<TTarget, TValue> ApplyChange()
        {
            if (!NewValue.Equals(OldValue))
                Target.SetValue(Expression, NewValue);
            return this;
        }

        public LayoutPromise<TTarget, TValue> ApplyChange(Action action)
        {
            return OnChanged(action).ApplyChange();
        }
        public LayoutPromise<TTarget, TValue> ApplyChange(Action<TValue> action)
        {
            return OnChanged(action).ApplyChange();
        }
        public LayoutPromise<TTarget, TValue> ApplyChange(Action<TValue, TValue> action)
        {
            return OnChanged(action).ApplyChange();
        }

    }
}
