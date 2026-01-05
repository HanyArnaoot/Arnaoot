using Arnaoot.Core;
using Arnaoot.VectorGraphics.Scene;
using static Arnaoot.VectorGraphics.Abstractions.Abstractions;

namespace Arnaoot.VectorGraphics.Commands
{

        public interface ICommand
        {
            string Name { get; }
            void Execute();
            void Undo();
            bool CanMergeWith(ICommand next);
            void MergeWith(ICommand next); // Add merge implementation
        }

        public sealed class AddRemoveCommand : ICommand
        {
            private readonly IDrawElement _element;
            private readonly ILayer _layer;
            private readonly bool _isAdd;

            public string Name { get; }

            public AddRemoveCommand(IDrawElement element, ILayer layer, bool isAdd)
            {
                _element = element ?? throw new ArgumentNullException(nameof(element));
                _layer = layer ?? throw new ArgumentNullException(nameof(layer));
                _isAdd = isAdd;
                Name = _isAdd ? $"Add {_element.GetType().Name}" : $"Remove {_element.GetType().Name}";
            }

            public void Execute()
            {
                if (_isAdd)
                    _layer.AddElement(_element, false);
                else
                    _layer.RemoveElement(_element);
            }

            public void Undo()
            {
                if (!_isAdd)
                    _layer.AddElement(_element, false);
                else
                    _layer.RemoveElement(_element);
            }

            public bool CanMergeWith(ICommand next) => false;
            public void MergeWith(ICommand next) { } // No-op
        }

        public sealed class PropertyCommand<T> : ICommand
        {
            private readonly IDrawElement _element;
            private readonly string _propertyName;
            private readonly Action<T> _setter;
            private T _oldValue;
            private T _newValue;

            public string Name { get; }

            public PropertyCommand(
                IDrawElement element,
                string propertyName,
                Func<T> getter,
                Action<T> setter,
                T newValue)
            {
                _element = element ?? throw new ArgumentNullException(nameof(element));
                _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
                _setter = setter ?? throw new ArgumentNullException(nameof(setter));

                if (getter == null) throw new ArgumentNullException(nameof(getter));

                _oldValue = getter();
                _newValue = newValue;
                Name = $"Edit {element.GetType().Name}.{propertyName}";
            }

            public void Execute()
            {
                _setter(_newValue);
            }

            public void Undo()
            {
                _setter(_oldValue);
            }

            public bool CanMergeWith(ICommand next)
            {
                return next is PropertyCommand<T> p
                    && p._element == _element
                    && p._propertyName == _propertyName;
            }

            public void MergeWith(ICommand next)
            {
                if (next is PropertyCommand<T> p && CanMergeWith(next))
                {
                    // Keep the original old value, update to the new final value
                    _newValue = p._newValue;
                }
            }
            public void UpdateNewValue(T newValue)
            {
                _newValue = newValue;
            }
        }

        public sealed class CompositeCommand : ICommand
        {
            private readonly List<ICommand> _commands;
            public string Name { get; }

            public CompositeCommand(string name, IEnumerable<ICommand> commands)
            {
                Name = name ?? "Composite Command";
                _commands = commands?.ToList() ?? throw new ArgumentNullException(nameof(commands));
            }

            public void Execute()
            {
                foreach (var cmd in _commands)
                    cmd.Execute();
            }

            public void Undo()
            {
                // Undo in reverse order
                for (int i = _commands.Count - 1; i >= 0; i--)
                    _commands[i].Undo();
            }

            public bool CanMergeWith(ICommand next) => false;
            public void MergeWith(ICommand next) { }
        }

        // Command for modifying line points (vertices)
        public sealed class LinePointCommand : ICommand
        {
            private readonly IDrawElement _line;
            private readonly int _pointIndex;
            private readonly Vector2D _oldPoint; // Could be Point, PointF, Vector2, etc.
            private Vector2D _newPoint;
            private readonly Action<int, Vector2D> _setter;
            private readonly Func<int, Vector2D> _getter;

            public string Name { get; }

            public LinePointCommand(
                IDrawElement line,
                int pointIndex,
                Func<int, Vector2D> getter,
                Action<int, Vector2D> setter,
                 Vector2D newPoint)
            {
                _line = line ?? throw new ArgumentNullException(nameof(line));
                _pointIndex = pointIndex;
                _getter = getter ?? throw new ArgumentNullException(nameof(getter));
                _setter = setter ?? throw new ArgumentNullException(nameof(setter));

                _oldPoint = getter(pointIndex);
                _newPoint = newPoint;
                Name = $"Move {line.GetType().Name} Point {pointIndex}";
            }

            public void Execute()
            {
                _setter(_pointIndex, _newPoint);
            }

            public void Undo()
            {
                _setter(_pointIndex, _oldPoint);
            }

            public bool CanMergeWith(ICommand next)
            {
                return next is LinePointCommand lpc
                    && lpc._line == _line
                    && lpc._pointIndex == _pointIndex;
            }

            public void MergeWith(ICommand next)
            {
                if (next is LinePointCommand lpc && CanMergeWith(next))
                {
                    // Keep original old point, update to new final point
                    _newPoint = lpc._newPoint;
                }
            }
        }

        // Generic typed version for type safety
        public sealed class LinePointCommand<TPoint> : ICommand
        {
            private readonly IDrawElement _line;
            private readonly int _pointIndex;
            private readonly TPoint _oldPoint;
            private TPoint _newPoint;
            private readonly Action<int, TPoint> _setter;

            public string Name { get; }

            public LinePointCommand(
                IDrawElement line,
                int pointIndex,
                Func<int, TPoint> getter,
                Action<int, TPoint> setter,
                TPoint newPoint)
            {
                _line = line ?? throw new ArgumentNullException(nameof(line));
                _pointIndex = pointIndex;
                _setter = setter ?? throw new ArgumentNullException(nameof(setter));

                if (getter == null) throw new ArgumentNullException(nameof(getter));

                _oldPoint = getter(pointIndex);
                _newPoint = newPoint;
                Name = $"Move {line.GetType().Name} Point {pointIndex}";
            }

            public void Execute()
            {
                _setter(_pointIndex, _newPoint);
            }

            public void Undo()
            {
                _setter(_pointIndex, _oldPoint);
            }

            public bool CanMergeWith(ICommand next)
            {
                return next is LinePointCommand<TPoint> lpc
                    && lpc._line == _line
                    && lpc._pointIndex == _pointIndex;
            }

            public void MergeWith(ICommand next)
            {
                if (next is LinePointCommand<TPoint> lpc && CanMergeWith(next))
                {
                    _newPoint = lpc._newPoint;
                }
            }
        }

        // Command for batch adding/removing multiple elements
        public sealed class BatchAddRemoveCommand : ICommand
        {
            private readonly List<IDrawElement> _elements;
            private readonly Layer _layer;
            private readonly bool _isAdd;

            public string Name { get; }

            public BatchAddRemoveCommand(
                IEnumerable<IDrawElement> elements,
                Layer layer,
                bool isAdd)
            {
                _elements = elements?.ToList() ?? throw new ArgumentNullException(nameof(elements));
                _layer = layer ?? throw new ArgumentNullException(nameof(layer));
                _isAdd = isAdd;

                Name = _isAdd
                    ? $"Add {_elements.Count} Element(s)"
                    : $"Remove {_elements.Count} Element(s)";
            }

            public void Execute()
            {
                if (_isAdd)
                {
                    foreach (var element in _elements)
                        _layer.AddElement(element, false);
                }
                else
                {
                    foreach (var element in _elements)
                        _layer.RemoveElement(element);
                }
            }

            public void Undo()
            {
                if (!_isAdd)
                {
                    // When undoing removal, add back in original order
                    foreach (var element in _elements)
                        _layer.AddElement(element, false);
                }
                else
                {
                    // When undoing addition, remove in reverse order
                    for (int i = _elements.Count - 1; i >= 0; i--)
                        _layer.RemoveElement(_elements[i]);
                }
            }

            public bool CanMergeWith(ICommand next) => false;
            public void MergeWith(ICommand next) { }
        }

        // Command for moving multiple elements at once
        public sealed class BatchMoveCommand : ICommand
        {
            private readonly List<IDrawElement> _elements;
            private readonly List<object> _oldPositions;
            private readonly List<object> _newPositions;
            private readonly Action<IDrawElement, object> _setter;

            public string Name { get; }

            public BatchMoveCommand(
                IEnumerable<IDrawElement> elements,
                Func<IDrawElement, object> getter,
                Action<IDrawElement, object> setter,
                Func<object, object> positionDelta) // Function to calculate new position
            {
                _elements = elements?.ToList() ?? throw new ArgumentNullException(nameof(elements));
                _setter = setter ?? throw new ArgumentNullException(nameof(setter));

                if (getter == null) throw new ArgumentNullException(nameof(getter));
                if (positionDelta == null) throw new ArgumentNullException(nameof(positionDelta));

                _oldPositions = new List<object>(_elements.Count);
                _newPositions = new List<object>(_elements.Count);

                foreach (var element in _elements)
                {
                    var oldPos = getter(element);
                    _oldPositions.Add(oldPos);
                    _newPositions.Add(positionDelta(oldPos));
                }

                Name = $"Move {_elements.Count} Element(s)";
            }

            public void Execute()
            {
                for (int i = 0; i < _elements.Count; i++)
                    _setter(_elements[i], _newPositions[i]);
            }

            public void Undo()
            {
                for (int i = 0; i < _elements.Count; i++)
                    _setter(_elements[i], _oldPositions[i]);
            }

            public bool CanMergeWith(ICommand next)
            {
                if (next is BatchMoveCommand bmc)
                {
                    // Can merge if moving the same set of elements
                    if (bmc._elements.Count != _elements.Count) return false;
                    return _elements.SequenceEqual(bmc._elements);
                }
                return false;
            }

            public void MergeWith(ICommand next)
            {
                if (next is BatchMoveCommand bmc && CanMergeWith(next))
                {
                    // Keep original old positions, update to new final positions
                    for (int i = 0; i < _newPositions.Count; i++)
                        _newPositions[i] = bmc._newPositions[i];
                }
            }
        }

    public interface ICommandManager
    {
        bool CanUndo { get; }
        bool CanRedo { get; }
        int UndoCount { get; }
        int RedoCount { get; }

        event EventHandler HistoryChanged;

        void ExecuteCommand(ICommand command);
        void Undo();
        void Redo();
        void Clear();

        IEnumerable<string> GetUndoHistory();
        IEnumerable<string> GetRedoHistory();

        void BeginMergeBlock();
        void EndMergeBlock();
    }
    public class CommandManager: ICommandManager
    {
            private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
            private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();
            private readonly int _maxHistorySize;
            private bool _isMergingEnabled = true;

            public bool CanUndo
            {
                get { return _undoStack.Count > 0; }
            }
            public bool CanRedo
            {
                get { return _redoStack.Count > 0; }
            }
            public int UndoCount
            {
                get { return _undoStack.Count; }
            }
            public int RedoCount
            {
                get { return _redoStack.Count; }
            }
            public event EventHandler HistoryChanged;

            public CommandManager(int maxHistorySize = 100)
            {
                _maxHistorySize = maxHistorySize;
            }

            public void ExecuteCommand(ICommand command)
            {
                if (command == null) throw new ArgumentNullException(nameof(command));

                command.Execute();

                // Try to merge with the last command if enabled
                if (_isMergingEnabled && _undoStack.Count > 0)
                {
                    var last = _undoStack.Peek();
                    if (last.CanMergeWith(command))
                    {
                        last.MergeWith(command);
                        HistoryChanged?.Invoke(this, EventArgs.Empty);
                        return;
                    }
                }

                _undoStack.Push(command);
                _redoStack.Clear(); // Clear redo stack on new command

                // Limit history size
                if (_undoStack.Count > _maxHistorySize)
                {
                    var temp = new Stack<ICommand>(_undoStack.Reverse().Skip(_undoStack.Count - _maxHistorySize));
                    _undoStack.Clear();
                    foreach (var cmd in temp.Reverse())
                        _undoStack.Push(cmd);
                }

                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Undo()
            {
                if (!CanUndo) return;

                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);

                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Redo()
            {
                if (!CanRedo) return;

                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);

                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }

            public void Clear()
            {
                _undoStack.Clear();
                _redoStack.Clear();
                HistoryChanged?.Invoke(this, EventArgs.Empty);
            }

            public IEnumerable<string> GetUndoHistory()
            {
                return _undoStack.Select(c => c.Name);
            }

            public IEnumerable<string> GetRedoHistory()
            {
                return _redoStack.Select(c => c.Name);
            }

            public void BeginMergeBlock()
            {
                _isMergingEnabled = true;
            }

            public void EndMergeBlock()
            {
                _isMergingEnabled = false;
            }
        }
    }
 
