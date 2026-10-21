using UnityEngine;

namespace ZeroMenu.UI
{
    public class TextField
    {
        public bool IsFocused => _focused;
        public bool Submitted { get; private set; }

        public string Content
        {
            get => _content;
            set => _content = value;
        }

        private string _content = "";
        private bool _focused = false;
        private float _lastBlinkTime = 0f;
        private bool _cursorVisible = true;
        private Rect _fieldRect = Rect.zero;
        private float _cursorBlinkTime = 0.5f;

        public TextField(string initialContent = "")
        {
            _content = initialContent;
        }

        public void ClearSubmitted()
        {
            Submitted = false;
        }

        public void Draw(int width = 200, int height = 20)
        {
            GUILayout.Box("", new GUILayoutOption[]
            {
                GUILayout.Width(width),
                GUILayout.Height(height)
            });

            if (Event.current.type == EventType.Repaint)
            {
                _fieldRect = GUILayoutUtility.GetLastRect();
            }

            if (Event.current.type == EventType.MouseDown)
            {
                if (_fieldRect.Contains(Event.current.mousePosition))
                {
                    _focused = true;
                    _lastBlinkTime = Time.time;
                    _cursorVisible = true;
                    Event.current.Use();
                }
                else
                {
                    _focused = false;
                }
            }

            if (_focused && Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Backspace)
                {
                    if (_content.Length > 0)
                    {
                        _content = _content.Substring(0, _content.Length - 1);
                        Event.current.Use();
                    }
                }
                else if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    Submitted = true;
                    Event.current.Use();
                }
                else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
                {
                    _content += Event.current.character.ToString();
                    Event.current.Use();
                }
            }

            GUI.Label(new Rect(_fieldRect.x + 5f, _fieldRect.y + 2f, _fieldRect.width - 10f, _fieldRect.height), _content);

            if (_focused)
            {
                if (Time.time - _lastBlinkTime > _cursorBlinkTime)
                {
                    _cursorVisible = !_cursorVisible;
                    _lastBlinkTime = Time.time;
                }

                if (_cursorVisible)
                {
                    Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(_content));
                    GUI.Label(new Rect(_fieldRect.x + textSize.x + 7f, _fieldRect.y + 2f, 10f, _fieldRect.height - 4f), "|");
                }
            }
        }

        public void Unfocus()
        {
            _focused = false;
        }
    }
}