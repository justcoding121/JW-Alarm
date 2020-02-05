using Xamarin.Forms;

namespace Bible.Alarm.UI.ViewHelpers
{
    public class AnimateUtils
    {
        public static void FlickUponTouched(View view, uint duration, string hexColorInitial,
            string hexColorFinal, int repeatCountMax)
        {
            var repeatCount = 0;
            view.Animate("changedBG", new Animation((val) =>
            {
                if (repeatCount == 0)
                {
                    view.BackgroundColor = Color.FromHex(hexColorInitial);
                }
                else
                {
                    view.BackgroundColor = Color.FromHex(hexColorFinal);
                }
            }), duration, finished: (val, b) =>
            {
                repeatCount++;
            }, repeat: () =>
            {
                return repeatCount < repeatCountMax;
            });
        }
    }
}
