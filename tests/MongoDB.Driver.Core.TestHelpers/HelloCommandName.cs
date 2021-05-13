namespace MongoDB.Driver.TestHelpers
{
    public static class HelloCommandName
    {
        public static string Legacy => "isMaster";
        public static string Modern => "hello";
    }
}
