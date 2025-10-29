using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GooberHelper {
    public static class Utils {
        public class InputState {
            private bool Jump;
            private bool Grab;
            // private static Dictionary<string, VirtualInput> watchedInputs = new Dictionary<string, VirtualInput>() {
            //     { "Jump", Input.Jump },
            //     { "Dash", Input.Dash },
            //     { "CrouchDash", Input.CrouchDash },
            //     { "Grab", Input.Grab },
            //     { "Pause", Input.Pause },
            //     { "MenuJournal", Input.MenuJournal },
            //     // { "MoveX", Input.MoveX },
            //     // { "MoveY", Input.MoveY },
            //     { "Aim", Input.Aim },
            // };
            // public Dictionary<string, object> State = [];

            public InputState() {
                Jump = Input.Jump.Check;
                Grab = Input.Grab.Check;
                // foreach(var pair in watchedInputs) {
                //     this.State[pair.Key] = 
                //         (pair.Value is VirtualButton button) ? button.Check :
                //         (pair.Value is VirtualIntegerAxis axis) ? axis.Value :
                //         (pair.Value is VirtualJoystick joystick) ? joystick.Value :
                //         new UnreachableException();
                // }
            }

            public bool FarEnoughFrom(InputState other) {
                // foreach(var pair in watchedInputs) {
                //     object a = this.State[pair.Key];
                //     object b = other.State[pair.Key];

                //     if(watchedInputs[pair.Key] is VirtualJoystick) {
                //         // if(Vector2.Dot(((Vector2)a).SafeNormalize(), ((Vector2)b).SafeNormalize()) < 0.5f) return true;
                //     } else if(watchedInputs[pair.Key] is VirtualButton) {
                //         if((bool)a != (bool)b || (watchedInputs[pair.Key] as VirtualButton).Pressed) return true;
                //     } else if(watchedInputs[pair.Key] is VirtualIntegerAxis) {
                //         if((int)a != (int)b) return true;
                //     }
                // }

                if(Jump != other.Jump || Input.Jump.Pressed) return true;
                if(Grab != other.Grab || Input.Grab.Pressed) return true;
                if(Input.Dash.Pressed) return true;
                if(Input.CrouchDash.Pressed) return true;
                if(Input.MenuJournal.Pressed) return true;
                if(Input.Pause.Pressed) return true;

                return false;
            }
        }

        public static string NumberCaptureRegex = @"(?<num>(\d+(\.\d+)?))";
        private static Regex zingleRegex = new Regex(@$"\s\((?<num>{NumberCaptureRegex})\)$");
        private static Regex bingleRegex = new Regex(@$"\(copy( (?<num>{NumberCaptureRegex}))?\)$");

        public static Action<Action<string>, Action, string> OpenTextInputField;
        public static Action IncreaseCombo;

        

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

        public static string CreateCopyName<T>(string name, Dictionary<string, T> dict, out string lastNameCollision) {
            string newName = name;

            lastNameCollision = newName;

            while(dict.ContainsKey(newName)) {    
                lastNameCollision = newName;

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
            TextMenuExt.TextBox textBox = new TextMenuExt.TextBox() { Container = menu, };
            TextMenuExt.Modal modal = new TextMenuExt.Modal(textBox, null, 85) { Visible = false };

            Action<string> finishCallback = null;
            Action cancelCallback = null;

            void exitTextBox() {
                textBox.StopTyping();
                modal.Visible = false;
                Input.Pause.ConsumePress();
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

            OpenTextInputField = (Action<string> finish, Action cancel, string placeholder) => {
                textBox.PlaceholderText = placeholder;
                textBox.ClearText();
                textBox.StartTyping();
                modal.Visible = true;

                finishCallback = finish;
                cancelCallback = cancel;
            };
        }

        public static void CreateComboModal(TextMenu menu, float expireTime = 1) {
            TextMenu.Header label = new TextMenu.Header("") { Container = menu };
            TextMenuExt.Modal modal = new TextMenuExt.Modal(label, 85, 500) { Visible = false };

            float timeSinceLastInput = 0;
            int counter = 0;

            menu.Add(modal);

            modal.OnUpdate = () => {
                if(counter == 0) return;

                timeSinceLastInput += Engine.DeltaTime;

                if(timeSinceLastInput > expireTime) {
                    counter = 0;

                    modal.Visible = false;
                }
            };

            IncreaseCombo = () => {
                counter++;
                modal.Visible = true;
                label.Title = "x" + counter;
            };
        }

        public class BetterCoroutine : Coroutine {
            private float sum = 0f;
            private int count = 0;

            public BetterCoroutine(IEnumerator functionCall, bool removeOnComplete = false) : base(functionCall, removeOnComplete) {}

            public override void Update() {
                float oldWaitTimer = waitTimer;

                base.Update();

                // sum += Engine.DeltaTime;

                if(oldWaitTimer <= 0 && oldWaitTimer != waitTimer) {
                    waitTimer += oldWaitTimer - Engine.DeltaTime;

                    // Console.WriteLine($"ratio: {sum/count}, waitTimer: {waitTimer}, oldWaitTimer: {oldWaitTimer}");

                    // sum += (MathF.Floor(waitTimer / Engine.DeltaTime) + 1) * Engine.DeltaTime;
                    // count++;
                }
            }
        }
    }
}