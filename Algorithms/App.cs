using System;
using System.Collections;
using System.Collections.Generic;
using game.utils;
using GXPEngine;
using Debug = game.utils.Debug;

namespace algorithms {
    public class App : Game {
        public App() : base(Globals.WIDTH, Globals.HEIGHT, Globals.FULLSCREEN, Globals.VSYNC, pPixelArt: Globals.PIXEL_ART, windowTitle: Globals.WINDOW_TITLE) {
            Debug.EnableFileLogger(true);
            ShowMouse(true);
            SetupInput();

            var list = new LinkedList<string>();
            Debug.Log($"list.Count (should be 0) => {list.Count}");
            list.Add("Hello, world!");
            Debug.Log($"Adding one item. list.Count should be 1 => {list.Count}");
            Debug.Log($"First item should be 'Hello, world!' => {list[0]}");
            try {
                Debug.Log($"Accessing non-existant value. Should throw exception => {list[1]}");
            } catch {
                Debug.Log("Exception thrown.");
            }

            Debug.Log("Adding more items:");
            list.Add("Hello");
            list.Add("World");
            list.Add("My name");
            list.Add("is Teo");
            Debug.Log("Printing all items:");
            var idx = 0;
            foreach (var val in list) {
                Debug.Log($"{idx}:{val}");
                idx++;
            }
            Debug.Log($"Removing at index 3 => {list[3]}");
            list.RemoveAt(3);
            Debug.Log("Printing all items:");
            idx = 0;
            foreach (var val in list) {
                Debug.Log($"{idx}:{val}");
                idx++;
            }

            var l = new List<string>();
            Environment.Exit(0);
        }

        private void SetupInput() {
            // Input.AddAxis("Horizontal", new List<int>{Key.LEFT, Key.A}, new List<int>{Key.RIGHT, Key.D});
            // Input.AddAxis("Vertical", new List<int>{Key.DOWN, Key.S}, new List<int>{Key.UP, Key.W}, true);
        }

        public static void Main(string[] args) {
            try {
                new App().Start();
            } catch (Exception e) {
                Debug.LogError($"An error occured: {e.Message}.\nYou can contact the developer Teodor Vecerdi (475884@student.saxion.nl).\nStacktrace:\n{e.StackTrace}");
                throw;
            } finally {
                Debug.FinalizeLogger();
            }
        }
    }
}