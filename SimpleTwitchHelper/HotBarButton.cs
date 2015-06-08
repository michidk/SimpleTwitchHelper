namespace SimpleTwitchHelper
{
    public class HotbarButton
    {
        public string Text { get; set; }
        public string Executable { get; set; }

        public HotbarButton() : this("", "")
        {
        }

        public HotbarButton(string text, string executable)
        {
            Text = text;
            Executable = executable;
        }
    }
}