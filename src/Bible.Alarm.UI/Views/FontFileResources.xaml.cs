using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace FontNameResources
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FontFileResources : ResourceDictionary
    {
        private static readonly FontFileResources _instance = new FontFileResources();
        public FontFileResources()
        {
            InitializeComponent();
        }

        public static string FontAwesomeRegular => _instance.GetStringResourceForPlatform("FontAwesomeRegularId");
        public static string FontAwesomeSolid => _instance.GetStringResourceForPlatform("FontAwesomeSolidId");

        private string GetStringResourceForPlatform(string resourceKey)
        {
            if (!_instance.ContainsKey(resourceKey)) return null;
            var label = new Label();
            if (!(_instance[resourceKey] is OnPlatform<string> resource)) return string.Empty;

            var retString = resource.Platforms.Where(c => c.Platform.Contains(Device.RuntimePlatform))
                .Select(c => c.Value).FirstOrDefault() as string;

            return retString ?? "NOFONT";
        }

    }

    public class GlyphNames
    {
        public static string Plus = "\uf067";
        public static string Search = "\uf002";
        public static string Play = "\uf04b";
        public static string Pause = "\uf04c";
        public static string Left = "\uf053";
        public static string Right = "\uf054";
        public static string Language = "\uf1ab";
    }
}