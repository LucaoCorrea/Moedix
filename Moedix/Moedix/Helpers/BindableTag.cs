namespace Moedix.Helpers
{
    public static class BindableTag
    {
        public static readonly BindableProperty TagProperty =
            BindableProperty.CreateAttached(
                "Tag",
                typeof(object),
                typeof(BindableTag),
                default(object));

        public static object GetTag(BindableObject view) =>
            view.GetValue(TagProperty);

        public static void SetTag(BindableObject view, object value) =>
            view.SetValue(TagProperty, value);
    }
}
