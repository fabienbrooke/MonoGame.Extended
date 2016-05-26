﻿using System;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Animations.Tweens
{
    public class PropertyTween<T> : Animation
        where T : struct 
    {
        static PropertyTween()
        {
            var a = Expression.Parameter(typeof(T));
            var b = Expression.Parameter(typeof(T));
            var c = Expression.Parameter(typeof(float));
            Add = Expression.Lambda<Func<T, T, T>>(Expression.Add(a, b), a, b).Compile();
            Subtract = Expression.Lambda<Func<T, T, T>>(Expression.Subtract(a, b), a, b).Compile();
            Multiply = Expression.Lambda<Func<T, float, T>>(Expression.Multiply(a, c), a, c).Compile();
        } 

        public PropertyTween(Func<T> getValue, Action<T> setValue, T targetValue, float duration, EasingFunction easingFunction) 
            : base(null, true)
        {
            _getValue = getValue;
            _setValue = setValue;
            TargetValue = targetValue;
            Duration = duration;
            EasingFunction = easingFunction;
        }

        private readonly Func<T> _getValue;
        private readonly Action<T> _setValue;
        private float _currentMultiplier;
        private T? _initialValue;
        private T? _differenceValue;

        public T TargetValue { get; }
        public float Duration { get; }
        public EasingFunction EasingFunction { get; set; }

        protected static Func<T, T, T> Add;
        protected static Func<T, T, T> Subtract;
        protected static Func<T, float, T> Multiply;

        protected override bool OnUpdate(float deltaTime)
        {
            if (!_initialValue.HasValue || !_differenceValue.HasValue)
            {
                _initialValue = _getValue();
                _differenceValue = Subtract(TargetValue, _initialValue.Value);
            }

            var isComplete = CurrentTime >= Duration;

            if (isComplete)
            {
                CurrentTime = Duration;
                _currentMultiplier = 1.0f;
            }
            else
            {
                _currentMultiplier = EasingFunction(CurrentTime / Duration);
            }

            var multiply = Multiply(_differenceValue.Value, _currentMultiplier);
            var newValue = Add(_initialValue.Value, multiply);
            _setValue(newValue);
            return isComplete;
        }
    }
}