using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace Nikse.SubtitleEdit.Controls
{
    public class TimeCodeUpDown : TemplatedControl
    {
        private TextBox? _textBox;
        private ButtonSpinner? _spinner;
        private string _textBuffer = "00:00:00:000";

        public static readonly StyledProperty<TimeSpan> ValueProperty =
            AvaloniaProperty.Register<TimeCodeUpDown, TimeSpan>(
                nameof(Value),
                defaultValue: TimeSpan.Zero);

        public TimeSpan Value
        {
            get => GetValue(ValueProperty);
            set
            {
                SetValue(ValueProperty, Clamp(value));
                UpdateText();
            }
        }

        public TimeCodeUpDown()
        {
            Template = CreateTemplate();
            _textBuffer = FormatTime(Value);

            this.GetObservable(ValueProperty).Subscribe(newValue =>
            {
                if (_textBox != null)
                {
                    _textBuffer = FormatTime(newValue);
                    _textBox.Text = _textBuffer;
                }
            });
        }

        private static FuncControlTemplate<TimeCodeUpDown> CreateTemplate()
        {
            return new FuncControlTemplate<TimeCodeUpDown>((control, scope) =>
            {
                var textBox = new TextBox
                {
                    Name = "PART_TextBox",
                    IsReadOnly = false,
                    Padding = new Thickness(9,2,2,2),
                    Margin = new Thickness(0),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Width = double.NaN,
                    BorderBrush = Brushes.Transparent,
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = double.NaN,
                };
                grid.Children.Add(textBox);

                var spinner = new ButtonSpinner
                {
                    Name = "PART_Spinner",
                    ButtonSpinnerLocation = Location.Right,
                    ShowButtonSpinner = true,
                    Content = grid,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = double.NaN,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                };

                scope.Register("PART_Spinner", spinner);
                scope.Register("PART_TextBox", textBox);

                return spinner;
            });
        }


        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _textBox = e.NameScope.Find<TextBox>("PART_TextBox");
            _spinner = e.NameScope.Find<ButtonSpinner>("PART_Spinner");

            if (_spinner != null)
            {
                _spinner.Spin += OnSpin;
            }

            if (_textBox != null)
            {
                _textBox.Text = FormatTime(Value);

                _textBox.AddHandler(TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
                _textBox.KeyDown += OnTextBoxKeyDown;

                _textBox.IsReadOnly = true;
            }
        }

        private void OnTextInput(object? sender, TextInputEventArgs e)
        {
            if (_textBox == null || string.IsNullOrEmpty(e.Text)) return;

            var c = e.Text[0];
            if (!char.IsDigit(c))
            {
                e.Handled = true;
                return;
            }

            var caret = _textBox.CaretIndex;
            var pos = GetEditableIndex(caret);
            if (pos < 0 || pos >= _textBuffer.Length)
            {
                e.Handled = true;
                return;
            }

            char[] chars = _textBuffer.ToCharArray();
            chars[pos] = c;
            _textBuffer = new string(chars);
            _textBox.Text = _textBuffer;
            _textBox.CaretIndex = GetNextEditableIndex(pos + 1);

            Value = ParseTime(_textBuffer);
            e.Handled = true;
        }

        private int GetEditableIndex(int caret)
        {
            // Skip colons
            if (caret == 2 || caret == 5 || caret == 8)
            {
                return caret + 1;
            }

            return caret;
        }

        private int GetNextEditableIndex(int caret)
        {
            if (caret == 2 || caret == 5 || caret == 8)
            {
                return caret + 1;
            }

            return caret;
        }

        private TimeSpan ParseTime(string text)
        {
            if (TimeSpan.TryParseExact(text, @"hh\:mm\:ss\:fff", null, out var result))
            {
                return result;
            }

            return TimeSpan.Zero;
        }

        private void OnSpin(object? sender, SpinEventArgs e)
        {
            ChangeValue(e.Direction == SpinDirection.Increase ? +1 : -1);
        }

        private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                ChangeValue(+1);
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ChangeValue(-1);
                e.Handled = true;
            }
        }

        private void ChangeValue(int delta)
        {
            if (_textBox == null)
            {
                return;
            }

            var caret = _textBox.CaretIndex;
            TimeSpan newVal = Value;

            if (caret <= 2)
            {
                newVal = newVal.Add(TimeSpan.FromHours(delta));
            }
            else if (caret <= 5)
            {
                newVal = newVal.Add(TimeSpan.FromMinutes(delta));
            }
            else if (caret <= 8)
            {
                newVal = newVal.Add(TimeSpan.FromSeconds(delta));
            }
            else
            {
                newVal = newVal.Add(TimeSpan.FromMilliseconds(delta));
            }

            Value = newVal;
        }

        private void UpdateText()
        {
            _textBuffer = FormatTime(Value);
            if (_textBox != null)
            {
                _textBox.Text = _textBuffer;
            }
        }

        private string FormatTime(TimeSpan time)
        {
            var hours = time.TotalHours;
            if (hours > 99)
            {
                hours = 99;
            }

            return $"{hours:00}:{time.Minutes:00}:{time.Seconds:00}:{time.Milliseconds:000}";
        }

        private TimeSpan Clamp(TimeSpan time)
        {
            return time.TotalMilliseconds < 0 ? TimeSpan.Zero : time;
        }
    }
}
