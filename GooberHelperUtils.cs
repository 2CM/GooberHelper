using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste.Mod.GooberHelper {
    public static class Utils {
        public static string NumberCaptureRegex = @"(?<num>(\d+(\.\d+)?))";
        private static Regex zingleRegex = new Regex(@$"\s\((?<num>{NumberCaptureRegex})\)$");
        private static Regex bingleRegex = new Regex(@$"\(copy( (?<num>{NumberCaptureRegex}))?\)$");

        public static Action<Action<string>, Action, string> OpenTextInputField;

        //this can probably be improved with something similar to a binary search 
        public static string PreventNameCollision<T>(string name, Dictionary<string, T> dict) {
            string newName = name;

            while(dict.ContainsKey(newName)) {                
                Match match = zingleRegex.Match(newName);
                
                if(match.Success) {
                    match.Groups.TryGetValue("num", out Group num);

                    newName = newName[0..num.Index] + $"{float.Parse(num.Value) + 1})";
                } else {
                    newName += " (2)";
                }
            }

            return newName;
        }

        public static string CreateCopyName<T>(string name, Dictionary<string, T> dict) {
            string newName = name;

            while(dict.ContainsKey(newName)) {                
                Match match = bingleRegex.Match(newName);
                
                if(match.Success) {
                    if(match.Groups.TryGetValue("num", out Group num) && num.Success) {
                        newName = newName[0..num.Index] + $"{float.Parse(num.Value) + 1})";
                    } else {
                        newName = newName[0..(newName.Length - 1)] + " 2)";
                    }
                } else {
                    newName += " (copy)";
                }
            }

            return newName;
        }

        public static void CreateTextInputField(TextMenu menu) {
            TextMenuExt.TextBox textBox = new TextMenuExt.TextBox() { Container = menu };
            TextMenuExt.Modal modal = new TextMenuExt.Modal(textBox, null, 85) { Visible = false };

            Action<string> finishCallback = null;
            Action cancelCallback = null;

            void exitTextBox() {
                textBox.StopTyping();
                modal.Visible = false;
            }

            textBox.OnTextInputCharActions['\n'] = (_) => {};
            textBox.OnTextInputCharActions['\r'] = (_) => {
                exitTextBox();

                if(textBox.Text.Length > 0) {
                    finishCallback?.Invoke(textBox.Text);
                } else {
                    cancelCallback?.Invoke();
                }
            };

            textBox.AfterInputConsumed = () => {
                if(textBox.Typing) {
                    if(Input.ESC.Pressed) {
                        exitTextBox();

                        Input.ESC.ConsumePress();

                        cancelCallback?.Invoke();
                    }
                }
            };

            menu.Add(modal);

            OpenTextInputField = (Action<string> finish, Action cancel, string placeholder = "") => {
                textBox.PlaceholderText = placeholder;
                textBox.ClearText();
                textBox.StartTyping();
                modal.Visible = true;

                finishCallback = finish;
                cancelCallback = cancel;
            };
        }
    }
}