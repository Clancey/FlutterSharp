using System;
using System.Collections.Generic;

namespace Flutter
{
    /// <summary>
    /// A ChangeNotifier that holds a single value.
    /// </summary>
    /// <remarks>
    /// When value is replaced with something that is not equal to the old value as evaluated by the equality operator ==,
    /// this class notifies its listeners.
    ///
    /// Because this class only notifies listeners when the value's identity changes, listeners will not be notified when
    /// mutable state within the value itself changes.
    ///
    /// For example, a ValueNotifier&lt;List&lt;int&gt;&gt; will not notify its listeners when the contents of the list are changed.
    /// As a result, it should never be used with a mutable object. Consider subclassing ChangeNotifier directly instead.
    /// </remarks>
    /// <typeparam name="T">The type of value stored.</typeparam>
    public class ValueNotifier<T> : ChangeNotifier, ValueListenable<T>
    {
        private T _value;

        /// <summary>
        /// Creates a ChangeNotifier that wraps the given value.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public ValueNotifier(T value)
        {
            _value = value;
        }

        /// <summary>
        /// The current value stored in this notifier.
        /// </summary>
        /// <remarks>
        /// When the value is replaced with something that is not equal to the old value as evaluated by the
        /// equality operator ==, this class notifies its listeners.
        /// </remarks>
        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    NotifyListeners();
                }
            }
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name}({_value})";
        }
    }
}
