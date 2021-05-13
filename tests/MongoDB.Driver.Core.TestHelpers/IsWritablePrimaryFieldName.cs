namespace MongoDB.Driver.TestHelpers
{
    public static class IsWritablePrimaryFieldName
    {
        public static string Legacy => "ismaster";
        public static string Modern => "isWritablePrimary";
    }
}
