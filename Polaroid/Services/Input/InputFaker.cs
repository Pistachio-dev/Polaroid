
using ECommons.Automation;
using ECommons.Automation.UIInput;

namespace InputInjection
{
    public static class InputFaker
    {
        static InputFaker()
        {
        }

        public static void PressHideHudKey()
        {
            WindowsKeypress.SendKeypress(ECommons.Interop.LimitedKeys.Scroll, null);
        }
        public static void PressScreenshotKey()
        {
            WindowsKeypress.SendKeypress(ECommons.Interop.LimitedKeys.PrintScreen, null);
            // Press it a few times 'cause sometimes it gets eaten. Or press it, check for picture, press it again, until you get picture or it's been too many already
        }
    }
}
