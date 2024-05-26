namespace TaskbarSharp
{
    public class Settings
    {
        public static bool Pause{ get; set; }

        public static int TaskbarStyle{ get; set; }
        public static int SecondaryTaskbarStyle{ get; set; }
        public static int PrimaryTaskbarOffset{ get; set; }
        public static int SecondaryTaskbarOffset{ get; set; }
        public static int CenterPrimaryOnly{ get; set; }
        public static int CenterSecondaryOnly{ get; set; }
        public static string AnimationStyle{ get; set; }
        public static int AnimationSpeed{ get; set; }
        public static int LoopRefreshRate{ get; set; }
        public static int CenterInBetween{ get; set; }
        public static int FixToolbarsOnTrayChange{ get; set; }
        public static int SkipResolution{ get; set; }
        public static int SkipResolution2{ get; set; }
        public static int SkipResolution3{ get; set; }
        public static int CheckFullscreenApp{ get; set; }
        public static int DefaultTaskbarStyleOnWinMax{ get; set; }
        public static int DontCenterTaskbar{ get; set; }
        public static int HidePrimaryStartButton{ get; set; }
        public static int HideSecondaryStartButton{ get; set; }
        public static int HidePrimaryNotifyWnd{ get; set; }
        public static int HideSecondaryNotifyWnd{ get; set; }
        public static int ShowTrayIcon{ get; set; }
        public static int TaskbarStyleOnMax{ get; set; }
        public static int TaskbarStyleRed{ get; set; }
        public static int TaskbarStyleGreen{ get; set; }
        public static int TaskbarStyleBlue{ get; set; }
        public static int TaskbarStyleAlpha{ get; set; }
        public static int ConsoleEnabled{ get; set; }
        public static int StickyStartButton{ get; set; }
        public static int TotalPrimaryOpacity{ get; set; }
        public static int TotalSecondaryOpacity{ get; set; }
        public static int RevertZeroBeyondTray{ get; set; }
        public static int TaskbarRounding{ get; set; }
        public static int TaskbarSegments{ get; set; }

        public static int UseUIA{ get; set; }

        // If on battery TaskbarSharp will override with these
        public static string OnBatteryAnimationStyle{ get; set; }

        public static int OnBatteryLoopRefreshRate{ get; set; }

    }
}