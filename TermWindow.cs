using System;
using UnityEngine;

namespace kOS
{
    // Blockotronix 550 Computor Monitor
    public class TermWindow : MonoBehaviour
    {
        public Core Core;
        private readonly string root = KSPUtil.ApplicationRootPath.Replace("\\", "/");
        private Rect windowRect = new Rect(60, 50, 470, 395);
        private Texture2D fontImage = new Texture2D(0, 0, TextureFormat.DXT1, false);
        private Texture2D terminalImage = new Texture2D(0, 0, TextureFormat.DXT1, false);
        private bool isOpen;
        private CameraManager cameraManager;
        private bool isLocked;
        private float cursorBlinkTime;
        private const int CHARSIZE = 8;
        private const int CHARS_PER_ROW = 16;
        private readonly Color color = new Color(1,1,1,1);
        private readonly Color colorAlpha = new Color(0.9f, 0.9f, 0.9f, 0.2f);
        private readonly Color textcolor = new Color(0.45f, 0.92f, 0.23f, 0.9f);
        private readonly Color textcolorAlpha = new Color(0.45f, 0.92f, 0.23f, 0.5f);
        private Rect closebuttonRect = new Rect(398, 359, 59, 30);
        private bool allTexturesFound = true;
        private CPU cpu;
        private CameraManager.CameraMode cameraModeWhenOpened;
        private bool showPilcrows;


        public void Awake()
        {
            LoadTexture("GameData/kOS/GFX/font_sml.png", ref fontImage);
            LoadTexture("GameData/kOS/GFX/monitor_minimal.png", ref terminalImage);
        }

        public void LoadTexture(String relativePath, ref Texture2D targetTexture)
        {
            var imageLoader = new WWW("file://" + root + relativePath);
            imageLoader.LoadImageIntoTexture(targetTexture);

            if (imageLoader.isDone && imageLoader.size == 0) allTexturesFound = false;
        }

        public void Open()
        {
            isOpen = true;

            Lock();
        }

        public void Close()
        {
            // Diable GUI and release all locks
            isOpen = false;

            Unlock();
        }

        public void Toggle()
        {
            if (isOpen) Close();
            else Open();
        }

        private void Lock()
        {
            if (isLocked) return;
            isLocked = true;

            cameraManager = CameraManager.Instance;
            cameraModeWhenOpened = cameraManager.currentCameraMode;
            cameraManager.enabled = false;

            InputLockManager.SetControlLock("kOSTerminal");

            // Prevent editor keys from being pressed while typing
            var editor = EditorLogic.fetch;
            if (editor != null && !EditorLogic.softLock) editor.Lock(true, true, true);
        }

        private void Unlock()
        {
            if (!isLocked) return;
            isLocked = false;

            InputLockManager.RemoveControlLock("kOSTerminal");

            cameraManager.enabled = true;

            var editor = EditorLogic.fetch;
            if (editor != null) editor.Unlock();
        }

        void OnGUI()
        {
            if (isOpen && isLocked) ProcessKeyStrokes();
            
            try
            {
                if (PauseMenu.isOpen || FlightResultsDialog.isDisplaying) return;
            }
            catch(NullReferenceException)
            {
            }

            if (!isOpen) return;
            
            GUI.skin = HighLogic.Skin;
            GUI.color = isLocked ? color : colorAlpha;

            windowRect = GUI.Window(0, windowRect, TerminalGui, "");
        }

        void Update()
        {
            if (cpu == null || cpu.Vessel == null || cpu.Vessel.parts.Count == 0)
            {
                // Holding onto a vessel instance that no longer exists?
                Close();
            }

            if (!isOpen || !isLocked) return;

            cursorBlinkTime += Time.deltaTime;
            if (cursorBlinkTime > 1) cursorBlinkTime -= 1;
        }

        public class KeyEvent
        {
            public KeyCode code;
            public float duration = 0;
        }
        
        void ProcessKeyStrokes()
        {
            if (Event.current.type != EventType.KeyDown) return;

            if (Event.current.character != 0 && Event.current.character != 13 && Event.current.character != 10)
            {
                Type(Event.current.character);
            }
            else if (Event.current.keyCode != KeyCode.None) 
            {
                Keydown(Event.current.keyCode);
            }

            cursorBlinkTime = 0.0f;
        }

        private void Keydown(KeyCode code)
        {
            var shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var control = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            switch (code)
            {
                case (KeyCode.C):
                    if(control) SpecialKey(kOSKeys.BREAK);
                    return;
                case (KeyCode.Break):
                    SpecialKey(kOSKeys.BREAK);
                    break;
                case (KeyCode.F1):
                    SpecialKey(kOSKeys.F1);
                    break;
                case KeyCode.F2:
                    SpecialKey(kOSKeys.F2);
                    break;
                case KeyCode.F3:
                    SpecialKey(kOSKeys.F3);
                    break;
                case KeyCode.F4:
                    SpecialKey(kOSKeys.F4);
                    break;
                case KeyCode.F5:
                    SpecialKey(kOSKeys.F5);
                    break;
                case KeyCode.F6:
                    SpecialKey(kOSKeys.F6);
                    break;
                case KeyCode.F7:
                    SpecialKey(kOSKeys.F7);
                    break;
                case KeyCode.F8:
                    SpecialKey(kOSKeys.F8);
                    break;
                case KeyCode.F9:
                    SpecialKey(kOSKeys.F9);
                    break;
                case KeyCode.F10:
                    SpecialKey(kOSKeys.F10);
                    break;
                case KeyCode.F11:
                    SpecialKey(kOSKeys.F11);
                    break;
                case KeyCode.F12:
                    SpecialKey(kOSKeys.F12);
                    break;
                case (KeyCode.UpArrow):
                    SpecialKey(kOSKeys.UP);
                    break;
                case (KeyCode.DownArrow):
                    SpecialKey(kOSKeys.DOWN);
                    break;
                case (KeyCode.LeftArrow):
                    SpecialKey(kOSKeys.LEFT);
                    break;
                case (KeyCode.RightArrow):
                    SpecialKey(kOSKeys.RIGHT);
                    break;
                case (KeyCode.Home):
                    SpecialKey(kOSKeys.HOME);
                    break;
                case (KeyCode.End):
                    SpecialKey(kOSKeys.END);
                    break;
                case (KeyCode.Backspace):
                    Type((char)8);
                    break;
                case (KeyCode.Delete):
                    SpecialKey(kOSKeys.DEL);
                    break;
                case (KeyCode.KeypadEnter):
                case (KeyCode.Return):
                    Type((char)13);
		    break;
            }
        }
        
        public void ClearScreen()
        {
            
        }
        
        void Type(char ch)
        {
            if (cpu != null) cpu.KeyInput(ch);
        }

        void SpecialKey(kOSKeys key)
        {
            if (cpu != null) cpu.SpecialKey(key);
        }

        void TerminalGui(int windowId)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var mousePos = new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y);

                if (closebuttonRect.Contains(mousePos))
                {
                    Close();
                }
                else if (new Rect(0,0,terminalImage.width, terminalImage.height).Contains(mousePos))
                {
                    Lock();
                }
                else
                {
                    Unlock();
                }
            }

            if (!allTexturesFound)
            {
                GUI.Label(new Rect(15, 15, 450, 300), "Error: Some or all kOS textures were not found. Please " +
                           "go to the following folder: \n\n<Your KSP Folder>\\GameData\\kOS\\GFX\\ \n\nand ensure that the png texture files are there.");

                GUI.Label(closebuttonRect, "Close");

                return;
            }

            if (cpu == null) return;

            GUI.color = isLocked ? color : colorAlpha;
            GUI.DrawTexture(new Rect(10, 10, terminalImage.width, terminalImage.height), terminalImage);

            if (GUI.Button(new Rect(580, 10, 80, 30), "Close"))
            {
                isOpen = false;
                Close();
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 500));

            if (cpu == null || cpu.Mode != CPU.Modes.READY || !cpu.IsAlive()) return;

            var textColor = isLocked ? textcolor : textcolorAlpha;

            GUI.BeginGroup(new Rect(31, 38, 420, 340));

            if (cpu != null)
            {
                var buffer = cpu.GetBuffer();

                for (var x = 0; x < buffer.GetLength(0); x++)
                    for (var y = 0; y < buffer.GetLength(1); y++)
                    {
                        var c = buffer[x, y];

                        if (c != 0 && c != 9 && c != 32) ShowCharacterByAscii(buffer[x, y], x, y, textColor);
                    }

                var blinkOn = cursorBlinkTime < 0.5f;
                if (blinkOn && cpu.GetCursorX() > -1)
                {
                    ShowCharacterByAscii((char)1, cpu.GetCursorX(), cpu.GetCursorY(), textColor);
                }
            }

            GUI.EndGroup();
        }

        void ShowCharacterByAscii(char ch, int x, int y, Color textColor)
        {
            var tx = ch % CHARS_PER_ROW;
            var ty = ch / CHARS_PER_ROW;

            ShowCharacterByXY(x, y, tx, ty, textColor);
        }

        void ShowCharacterByXY(int x, int y, int tx, int ty, Color textColor)
        {
            GUI.BeginGroup(new Rect((x * CHARSIZE), (y * CHARSIZE), CHARSIZE, CHARSIZE));
            GUI.color = textColor;
            GUI.DrawTexture(new Rect(tx * -CHARSIZE, ty * -CHARSIZE, fontImage.width, fontImage.height), fontImage);
            GUI.EndGroup();
        }

        public void SetOptionPilcrows(bool val)
        {
            showPilcrows = val;
        }

        internal void AttachTo(CPU cpu)
        {
            this.cpu = cpu;
        }

        internal void PrintLine(string line)
        {
            //if (Cpu != null) Cpu.PrintLine(line);
        }
    }
}
