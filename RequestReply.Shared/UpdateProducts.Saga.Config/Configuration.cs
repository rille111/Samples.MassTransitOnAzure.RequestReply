// ReSharper disable once CheckNamespace
public static class Configuration
{
    // Used both in the Sender and Receiver. Press F12 or Shift-F12 to see where this queue-name is used.
    public static string QueueNameForStartingTheSaga { get; } = "update_products_saga";
}