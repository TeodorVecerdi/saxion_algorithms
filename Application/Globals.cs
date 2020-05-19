namespace root {
    public static class Globals {
        public const bool FULLSCREEN = false;
        public const bool PIXEL_ART = false;
        public const bool USE_ASPECT_RATIO = false;
        public const bool VSYNC = false;
        
        public const int WIDTH = 1280;
        private const int H_MAIN = 720;
        private const int H_ASPECT = (int) (WIDTH / ASPECT_RATIO);
        public static int HEIGHT => USE_ASPECT_RATIO ? H_ASPECT : H_MAIN;
        
        public const float ASPECT_RATIO = 1.7777777f;
        public const string WINDOW_TITLE = "Example Project";
    }
}